using System;
using Konfus.Sensor_Toolkit;
using UnityEngine;
using UnityEngine.Events;

namespace Konfus.Input
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public class RigidbodyJumping : MonoBehaviour
    {
        public event Action<float, float>? Landed;

        [Header("References")]
        [SerializeField]
        private ScanSensor? groundSensor;

        [Header("Feel")]
        [SerializeField]
        [Min(0)]
        [Tooltip("Max jump height in meters")]
        private float maxJumpHeight = 0.5f;
        [Tooltip("Scales overall gravity strength (1 = normal Unity gravity)")]
        [SerializeField]
        [Min(0)]
        private float gravityMultiplier = 2.0f;
        [Tooltip("Gravity shaping curve over jump phase (0..1)")]
        [SerializeField]
        private AnimationCurve jumpCurve = new(
            new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f));

        [Header("Forgiveness")]
        [Tooltip("Press jump up to this many seconds before landing to buffer the jump")]
        [SerializeField]
        [Min(0)]
        private float jumpBufferTime = 0.1f;
        [Tooltip("After leaving the ground, you can still jump for this many seconds")]
        [SerializeField]
        [Min(0)]
        private float coyoteTime = 0.1f;

        [Header("Events")]
        [SerializeField]
        [Tooltip("Explicit landing feedback hook. Wire this to FPSCamera.PlayLandingBounce if the camera should react to landing.")]
        private LandingEvent landed = new();

        private readonly JumpStateMachine _stateMachine = new();

        private float _coyoteUntil;
        private float _jumpBufferedUntil;
        private bool _jumping;
        private float _peakY;
        private Rigidbody? _rb;
        private float _startY;
        private float _airborneTime;
        private bool _wasGrounded;
        private bool _isGroundedNow;

        public bool IsJumping => _jumping;
        public bool IsGroundedNow => _isGroundedNow;
        public bool HasBufferedJumpNow => HasBufferedJump();
        public float CoyoteUntil => _coyoteUntil;
        public float JumpBufferedUntil => _jumpBufferedUntil;
        public float StartY => _startY;
        public float PeakY => _peakY;
        public float JumpHeight01 => _rb ? Mathf.Clamp01(Mathf.InverseLerp(_startY, _peakY, _rb.position.y)) : 0f;
        public float VerticalVelocity => _rb ? _rb.linearVelocity.y : 0f;
        public float AirborneTime => _airborneTime;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            ApplyRigidbodyDefaults();
        }

        private void Reset()
        {
            _rb = GetComponent<Rigidbody>();
            ApplyRigidbodyDefaults();
        }

        private void FixedUpdate()
        {
            if (!_rb) return;

            bool grounded = IsGrounded();
            _isGroundedNow = grounded;
            float verticalVelocityBeforeStep = _rb.linearVelocity.y;

            JumpFrame frame = new()
            {
                // Just left ground this frame
                _coyoteUntil = Time.unscaledTime + coyoteTime;
            }

            if (grounded)
            {
                if (!_wasGrounded)
                    RaiseLanded(verticalVelocityBeforeStep);
                _airborneTime = 0f;
            }
            else
                _airborneTime += Time.fixedDeltaTime;

            _wasGrounded = grounded;

            // Consume buffer when we become allowed to jump (grounded OR within coyote)
            if (!_jumping && HasBufferedJump() && CanJumpNow(grounded))
            {
                ConsumeBufferedJump();
                PerformJump();
                grounded = false; // we just jumped; prevents early returns below
            }

            // If grounded and not jumping, no custom gravity needed.
            if (!_jumping && grounded)
                return;

            // Hard cap peak height.
            if (_jumping && _rb.position.y >= _peakY && _rb.linearVelocity.y > 0f)
            {
                Vector3 v = _rb.linearVelocity;
                v.y = 0f;
                _rb.linearVelocity = v;
            }

            ApplyCurveGravity();

            // End jump state once falling and near peak region.
            if (_jumping && _rb.linearVelocity.y <= 0f && _rb.position.y >= _peakY - 0.01f)
                _jumping = false;

            // If we land, reset.
            if (grounded && _rb.linearVelocity.y <= 0f) _jumping = false;
        }

        private void OnValidate()
        {
            _rb = GetComponent<Rigidbody>();
            ApplyRigidbodyDefaults();
            _isGroundedNow = false;
        }

        /// <summary>
        /// Buffers a jump press and performs it immediately when the current state allows it.
        /// </summary>
        public void StartJump()
        {
            BufferJumpPress();

            if (_rb && !_jumping && CanJumpNow(IsGrounded()))
            {
                ConsumeBufferedJump();
                PerformJump();
                _stateMachine.ForceState(JumpStateId.Jumping);
                _currentState = JumpStateId.Jumping;
            }
        }

        /// <summary>
        /// Cancels the active jump and removes upward velocity for variable-height jumps.
        /// </summary>
        public void StopJump()
        {
            if (!_rb || !_jumping)
                return;

            _jumping = false;

            Vector3 velocity = _rb.linearVelocity;
            if (velocity.y > 0f)
                velocity.y = 0f;
            _rb.linearVelocity = velocity;
        }

        private void ConfigureStateMachine()
        {
            if (_stateMachine.IsConfigured)
                return;

            // Jump states own vertical motion only. Movement and camera effects consume public state/events instead
            // of being queried directly from here.
            _stateMachine.Configure(
                new GroundedJumpState(this),
                new JumpingJumpState(this),
                new AirborneJumpState(this));
        }

        private void UpdateGrounding()
        {
            bool sensorGrounded = IsGrounded();
            bool grounded = sensorGrounded && !_jumping && _rb!.linearVelocity.y <= GroundedVerticalVelocityThreshold;
            _isGroundedNow = grounded;
            float verticalVelocityBeforeStep = _rb.linearVelocity.y;

            if (grounded)
                _coyoteUntil = Time.unscaledTime + coyoteTime;
            else if (_wasGrounded)
                _coyoteUntil = Time.unscaledTime + coyoteTime;

            if (grounded)
            {
                if (!_wasGrounded)
                    RaiseLanded(verticalVelocityBeforeStep);
                _airborneTime = 0f;
            }
            else
            {
                _airborneTime += Time.fixedDeltaTime;
            }

            _wasGrounded = grounded;
        }

        private bool CanJumpNow(bool grounded)
        {
            return grounded || Time.unscaledTime <= _coyoteUntil;
        }

        private void TryConsumeBufferedJump(JumpFrame frame)
        {
            if (_jumping || !frame.HasBufferedJump || !frame.CanJump)
                return;

            ConsumeBufferedJump();
            PerformJump();
            _stateMachine.ForceState(JumpStateId.Jumping);
            _currentState = JumpStateId.Jumping;
        }

        private void PerformJump()
        {
            if (!_rb) return;

            _jumping = true;
            _isGroundedNow = false;

            _startY = _rb.position.y;
            _peakY = _startY + maxJumpHeight;

            float baseGravityMagnitude = Mathf.Abs(Physics.gravity.y * gravityMultiplier);
            float jumpVelocity = Mathf.Sqrt(2f * baseGravityMagnitude * maxJumpHeight);

            Vector3 velocity = _rb.linearVelocity;
            velocity.y = jumpVelocity;
            _rb.linearVelocity = velocity;

            _coyoteUntil = 0f;
        }

        private void BufferJumpPress()
        {
            _jumpBufferedUntil = jumpBufferTime <= 0f
                ? 0f
                : Time.unscaledTime + jumpBufferTime;
        }

        private bool HasBufferedJump()
        {
            return Time.unscaledTime <= _jumpBufferedUntil;
        }

        private void ConsumeBufferedJump()
        {
            _jumpBufferedUntil = 0f;
        }

        private void TickJumping()
        {
            if (!_rb) return;

            if (_rb.position.y >= _peakY && _rb.linearVelocity.y > 0f)
            {
                Vector3 velocity = _rb.linearVelocity;
                velocity.y = 0f;
                _rb.linearVelocity = velocity;
            }

            ApplyCurveGravity();

            if (_rb.linearVelocity.y <= 0f && _rb.position.y >= _peakY - 0.01f)
                _jumping = false;

            if (_isGroundedNow && _rb.linearVelocity.y <= 0f)
                _jumping = false;
        }

        private void TickAirborne(JumpFrame frame)
        {
            TryConsumeBufferedJump(frame);
            if (_jumping)
                return;

            ApplyCurveGravity();
        }

        private void ApplyCurveGravity()
        {
            if (!_rb) return;

            float baseGravity = Physics.gravity.y * gravityMultiplier;
            if (!_jumping)
            {
                AddVerticalAcceleration(baseGravity);
                return;
            }

            float height01 = Mathf.Clamp01(Mathf.InverseLerp(_startY, _peakY, _rb.position.y));
            float phase;
            if (_rb.linearVelocity.y >= 0f)
            {
                phase = height01 * 0.5f;
            }
            else
            {
                float descent01 = 1f - height01;
                phase = 0.5f + descent01 * 0.5f;
            }

            float curveValue = Mathf.Clamp01(jumpCurve.Evaluate(phase));
            AddVerticalAcceleration(baseGravity * curveValue);
        }

        private void AddVerticalAcceleration(float accelY)
        {
            if (!_rb) return;

            Vector3 velocity = _rb.linearVelocity;
            velocity.y += accelY * Time.fixedDeltaTime;
            _rb.linearVelocity = velocity;
        }

        private bool IsGrounded()
        {
            if (!groundSensor || !groundSensor.Scan() || groundSensor.Hits == null)
                return false;

            foreach (Sensor.Hit hit in groundSensor.Hits)
            {
                if (!hit.GameObject || hit.GameObject == gameObject || hit.GameObject.transform.IsChildOf(transform))
                    continue;

                if (Vector3.Dot(hit.Normal.normalized, Vector3.up) > 0f)
                    return true;
            }

            return false;
        }

        private void ApplyRigidbodyDefaults()
        {
            if (!_rb) return;
            _rb.useGravity = false;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void RaiseLanded(float verticalVelocityBeforeStep)
        {
            Landed?.Invoke(_airborneTime, Mathf.Max(0f, -verticalVelocityBeforeStep));
        }
    }
}
