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

        [Header("Settings")]
        [SerializeField]
        private float maxJumpHeight = 0.5f;
        [Tooltip("Scales overall gravity strength (1 = normal Unity gravity).")]
        [SerializeField]
        private float gravityMultiplier = 2.0f;
        [Tooltip("Optional extra gravity right after releasing jump early (snappier short hop).")]
        [SerializeField]
        private float earlyReleaseGravityBoost = 2.0f;
        [Tooltip("Interpreted as a gravity-shaping curve over the jump phase (0..1). " +
                 "We map curveValue (0..1) to gravityScale between min/max via an inversion: " +
                 "gravityScale = Lerp(max, min, curveValue). Default 0-1-0 gives hang-time near the middle.")]
        [SerializeField]
        private AnimationCurve jumpCurve = new(
            new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f));

        private bool _jumpHeld;
        private bool _jumping;
        private float _peakY;
        private Rigidbody? _rb;
        private bool _releasedEarly;
        private float _startY;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (!_rb) return;

            // Always apply custom gravity when not grounded, so falling also respects gravityMultiplier.
            bool grounded = IsGrounded();

            if (!_jumping && grounded)
                return;

            // Hard cap peak height (prevents “held jump” from exceeding maxJumpHeight).
            if (_jumping && _rb.position.y >= _peakY && _rb.linearVelocity.y > 0f)
            {
                Vector3 v = _rb.linearVelocity;
                v.y = 0f;
                _rb.linearVelocity = v;
            }

            ApplyCurveGravity();

            // End jump state once we see we’re falling and we’ve passed the peak-ish region.
            if (_jumping && _rb.linearVelocity.y <= 0f && _rb.position.y >= _peakY - 0.01f)
                _jumping = false;

            // If we land, reset.
            if (grounded && _rb.linearVelocity.y <= 0f)
            {
                _jumping = false;
                _releasedEarly = false;
            }
        }

        private void OnValidate()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        /// <summary>Call on jump button pressed (performed).</summary>
        public void StartJump()
        {
            if (!_rb) return;

            _jumpHeld = true;

            if (_jumping)
                return;

            if (!IsGrounded())
                return;

            _jumping = true;
            _releasedEarly = false;

            _startY = _rb.position.y;
            _peakY = _startY + maxJumpHeight;

            // Choose initial velocity so that (under *base* gravity) we’d reach maxJumpHeight.
            // The curve will then stretch/squash time by changing gravity scale over the arc.
            float baseGMag = Mathf.Abs(Physics.gravity.y * gravityMultiplier);
            float v0 = Mathf.Sqrt(2f * baseGMag * maxJumpHeight);

            Vector3 v = _rb.linearVelocity;
            v.y = v0;
            _rb.linearVelocity = v;
        }

        /// <summary>Call on jump button released (canceled).</summary>
        public void StopJump()
        {
            _jumpHeld = false;

            // We don’t instantly zero velocity here; we let gravity shaping do the cut.
            // The moment you release while rising, _releasedEarly triggers extra gravity.
        }

        private void ApplyCurveGravity()
        {
            if (!_rb) return;

            // Base gravity magnitude (Unity’s gravity is negative Y).
            float baseG = Physics.gravity.y * gravityMultiplier; // negative

            // If we’re not in a jump (e.g., fell off a ledge), just apply scaled gravity.
            if (!_jumping)
            {
                AddVerticalAcceleration(baseG);
                return;
            }

            // Compute jump phase from height (no explicit time):
            // phase 0..0.5 while rising (start->peak), 0.5..1 while falling (peak->start height).
            float height01 = Mathf.InverseLerp(_startY, _peakY, _rb.position.y);
            height01 = Mathf.Clamp01(height01);

            float phase;
            if (_rb.linearVelocity.y >= 0f)
            {
                // Rising: phase 0..0.5
                phase = height01 * 0.5f;
            }
            else
            {
                // Falling: phase 0.5..1 (based on descent progress)
                float descent01 = 1f - height01; // 0 at peak, 1 near start height
                phase = 0.5f + descent01 * 0.5f;
            }

            float curveValue = Mathf.Clamp01(jumpCurve.Evaluate(phase));

            // If player released early *while still rising*, increase gravity to cut the jump short.
            if (_releasedEarly && _rb.linearVelocity.y > 0f)
                curveValue *= earlyReleaseGravityBoost;

            float gThisFrame = baseG * curveValue;
            AddVerticalAcceleration(gThisFrame);

            // If jump is no longer held and we’re rising, we consider it an early release cut.
            if (!_jumpHeld && _rb.linearVelocity.y > 0f)
                _releasedEarly = true;
        }

        private void AddVerticalAcceleration(float accelY)
        {
            if (!_rb) return;

            // Velocity += a * dt
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