using Konfus.Sensor_Toolkit;
using UnityEngine;

namespace Konfus.Input
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public class RigidbodyJumping : MonoBehaviour
    {
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

        private float _coyoteUntil;
        private float _jumpBufferedUntil;
        private bool _jumping;
        private float _peakY;
        private Rigidbody? _rb;
        private float _startY;
        private bool _wasGrounded;

        private void FixedUpdate()
        {
            if (!_rb) return;

            bool grounded = IsGrounded();

            // Coyote bookkeeping
            if (grounded)
                _coyoteUntil = Time.unscaledTime + coyoteTime;
            else if (_wasGrounded && !grounded)
            {
                // Just left ground this frame
                _coyoteUntil = Time.unscaledTime + coyoteTime;
            }
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
            if (!_rb) return;
            _rb.useGravity = false;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        /// <summary>Call to perform a jump.</summary>
        public void StartJump()
        {
            // Always buffer the press.
            BufferJumpPress();

            // If we can jump right now (grounded or within coyote), do it immediately.
            if (_rb && !_jumping && CanJumpNow(IsGrounded()))
            {
                ConsumeBufferedJump();
                PerformJump();
            }
        }

        /// <summary>Immediately cancels a jump.</summary>
        public void StopJump()
        {
            if (!_rb) return;
            if (!_jumping) return;

            // Immediately cancel jump and kill any upward velocity
            _jumping = false;

            Vector3 v = _rb.linearVelocity;
            if (v.y > 0f)
                v.y = 0f;
            _rb.linearVelocity = v;
        }

        private bool CanJumpNow(bool grounded)
        {
            if (grounded) return true;
            return Time.unscaledTime <= _coyoteUntil;
        }

        private void PerformJump()
        {
            if (!_rb) return;

            _jumping = true;

            _startY = _rb.position.y;
            _peakY = _startY + maxJumpHeight;

            float baseGMag = Mathf.Abs(Physics.gravity.y * gravityMultiplier);
            float v0 = Mathf.Sqrt(2f * baseGMag * maxJumpHeight);

            Vector3 v = _rb.linearVelocity;
            v.y = v0;
            _rb.linearVelocity = v;

            // After we jump, coyote is effectively spent until we touch ground again.
            _coyoteUntil = 0f;
        }

        private void BufferJumpPress()
        {
            if (jumpBufferTime <= 0f)
            {
                _jumpBufferedUntil = 0f;
                return;
            }

            _jumpBufferedUntil = Time.unscaledTime + jumpBufferTime;
        }

        private bool HasBufferedJump()
        {
            return Time.unscaledTime <= _jumpBufferedUntil;
        }

        private void ConsumeBufferedJump()
        {
            _jumpBufferedUntil = 0f;
        }

        private void ApplyCurveGravity()
        {
            if (!_rb) return;

            float baseG = Physics.gravity.y * gravityMultiplier; // negative

            // If we’re not in a jump (e.g., fell off a ledge), just apply scaled gravity.
            if (!_jumping)
            {
                AddVerticalAcceleration(baseG);
                return;
            }

            // Compute jump phase from height (no explicit time):
            float height01 = Mathf.InverseLerp(_startY, _peakY, _rb.position.y);
            height01 = Mathf.Clamp01(height01);

            float phase;
            if (_rb.linearVelocity.y >= 0f)
                phase = height01 * 0.5f; // rising: 0..0.5
            else
            {
                float descent01 = 1f - height01; // 0 at peak, 1 near start height
                phase = 0.5f + descent01 * 0.5f; // falling: 0.5..1
            }

            float curveValue = Mathf.Clamp01(jumpCurve.Evaluate(phase));
            float gThisFrame = baseG * curveValue;
            AddVerticalAcceleration(gThisFrame);
        }

        private void AddVerticalAcceleration(float accelY)
        {
            if (!_rb) return;

            Vector3 v = _rb.linearVelocity;
            v.y += accelY * Time.fixedDeltaTime;
            _rb.linearVelocity = v;
        }

        private bool IsGrounded()
        {
            return groundSensor?.Scan() ?? false;
        }
    }
}