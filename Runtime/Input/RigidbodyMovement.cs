using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Konfus.Input
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class RigidbodyMovement : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField]
        [Tooltip("Maximum speed at which the object moves in meters per second")]
        [Min(0)]
        private float moveSpeed = 6f;

        [SerializeField]
        [Tooltip("Amount to multiply the amount of movement speed when sprinting")]
        [Min(1)]
        private float sprintMod = 1.5f;

        [SerializeField]
        [Tooltip("Amount to multiply the amount of movement speed when moving backwards")]
        [Min(1)]
        private float reverseMod = 1.5f;

        [SerializeField]
        [Tooltip("Amount to multiply the amount of movement speed when steering")]
        [Min(1)]
        private float steeringMod = 1.5f;

        [Tooltip("Max horizontal acceleration rate in m/s²")]
        [SerializeField]
        [Min(1)]
        private float accelerationRate = 35f;

        [Tooltip("Max horizontal deceleration rate in m/s²")]
        [SerializeField]
        [Min(1)]
        private float decelerationRate = 45f;

        [Tooltip("X = normalized speed 0..1, Y = multiplier")]
        [SerializeField]
        private AnimationCurve accelerationCurve =
            AnimationCurve.EaseInOut(0f, 1f, 1f, 0.2f);

        [Tooltip("X = normalized speed 0..1, Y = multiplier")]
        [SerializeField]
        private AnimationCurve decelerationCurve =
            AnimationCurve.EaseInOut(0f, 1f, 1f, 0.6f);

        [Header("Debug")]
        [SerializeField]
        private bool drawDebugGizmos = true;

        [SerializeField]
        [Tooltip("How long the debug vectors are drawn (scaled by velocity magnitude).")]
        private float debugVelocityScale = 0.15f;

        [SerializeField]
        private float debugSphereRadius = 0.05f;

        private bool _isSprinting;
        private Vector2 _moveInput;
        private Rigidbody? _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (!_rigidbody) return;

            Vector3 v = _rigidbody.linearVelocity;
            var horiz = new Vector3(v.x, 0f, v.z);

            // Input -> world direction
            var inputDir = new Vector3(_moveInput.x, 0f, _moveInput.y);
            inputDir = Vector3.ClampMagnitude(inputDir, 1f);
            Vector3 desiredDir = transform.TransformDirection(inputDir);

            float sprintModifier = _isSprinting ? sprintMod : 1f;
            float targetSpeed = moveSpeed * sprintModifier;
            Vector3 desiredHoriz = desiredDir * targetSpeed;

            float speed = horiz.magnitude;
            float normalizedSpeed = moveSpeed > 0.0001f ? Mathf.Clamp01(speed / moveSpeed) : 0f;

            // If no input, just decelerate toward zero.
            if (_moveInput.sqrMagnitude <= 0.0001f)
            {
                float curve = Mathf.Max(0f, decelerationCurve.Evaluate(normalizedSpeed));
                float maxDelta = decelerationRate * curve * Time.fixedDeltaTime;

                Vector3 newHoriz = Vector3.MoveTowards(horiz, Vector3.zero, maxDelta);
                _rigidbody.linearVelocity = new Vector3(newHoriz.x, v.y, newHoriz.z);

#if UNITY_EDITOR
                CacheDebug(
                    horiz,
                    Vector3.zero,
                    Vector3.zero,
                    newHoriz,
                    0f,
                    0f,
                    false,
                    false
                );
#endif
                return;
            }

            // We have input: decide whether we are steering, accelerating, or braking.
            float dot = horiz.sqrMagnitude > 0.0001f
                ? Vector3.Dot(horiz.normalized, desiredDir.normalized)
                : 1f;

            float accelCurveMul = Mathf.Max(0f, accelerationCurve.Evaluate(normalizedSpeed));
            float decelCurveMul = Mathf.Max(0f, decelerationCurve.Evaluate(normalizedSpeed));
            float accelMaxDelta = accelerationRate * accelCurveMul * sprintModifier * Time.fixedDeltaTime;
            float decelMaxDelta = decelerationRate * decelCurveMul * sprintModifier * Time.fixedDeltaTime;

            // 0 when aligned, 1 when perpendicular
            float steeringFactor = 1f - Mathf.Abs(dot);
            float steerBoost = Mathf.Lerp(1f, steeringMod, steeringFactor);
            bool braking = dot < 0f;
            float maxDeltaVelocity = braking ? decelMaxDelta : accelMaxDelta * steerBoost;

            Vector3 newHorizontalVelocity = Vector3.MoveTowards(horiz, desiredHoriz, maxDeltaVelocity);

            _rigidbody.linearVelocity = new Vector3(
                newHorizontalVelocity.x,
                v.y,
                newHorizontalVelocity.z
            );

#if UNITY_EDITOR
            CacheDebug(
                horiz,
                desiredHoriz,
                desiredDir,
                newHorizontalVelocity,
                dot,
                steeringFactor,
                braking,
                true
            );
#endif
        }

        private void OnValidate()
        {
            _rigidbody = GetComponent<Rigidbody>();
            if (_rigidbody)
            {
                _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            }

            EnsureCurveHasEndpoints(ref accelerationCurve);
            EnsureCurveHasEndpoints(ref decelerationCurve);

            debugVelocityScale = Mathf.Max(0.0001f, debugVelocityScale);
            debugSphereRadius = Mathf.Max(0.0001f, debugSphereRadius);
        }

        public void StartSprint()
        {
            _isSprinting = true;
        }

        public void StopSprint()
        {
            _isSprinting = false;
        }

        /// <summary>
        /// Supplies movement input in local X/Z space.
        /// Expected range: (-1..1) per axis.
        /// </summary>
        public void Move(Vector2 input)
        {
            _moveInput = Vector2.ClampMagnitude(input, 1f);
        }

        private static void EnsureCurveHasEndpoints(ref AnimationCurve curve)
        {
            if (curve.length == 0)
            {
                curve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
                return;
            }

            float firstTime = curve.keys[0].time;
            float lastTime = curve.keys[curve.length - 1].time;

            if (firstTime > 0f)
                curve.AddKey(0f, curve.Evaluate(0f));
            if (lastTime < 1f)
                curve.AddKey(1f, curve.Evaluate(1f));

            // NOTE: curve.keys returns a copy; modifying it like this won’t stick.
            // If you actually need to clamp endpoint values, do it by reading keys, modifying array, and assigning back.
        }

#if UNITY_EDITOR
        private Vector3 _dbgHorizVel;
        private Vector3 _dbgDesiredVel;
        private Vector3 _dbgInputDir;
        private Vector3 _dbgNewHorizVel;
        private float _dbgDot;
        private float _dbgSteeringFactor;
        private bool _dbgBraking;
        private bool _dbgHadInput;

        private void CacheDebug(
            Vector3 horizVel,
            Vector3 desiredVel,
            Vector3 inputDirWorld,
            Vector3 newHorizVel,
            float dot,
            float steer,
            bool braking,
            bool hadInput)
        {
            _dbgHorizVel = horizVel;
            _dbgDesiredVel = desiredVel;
            _dbgInputDir = inputDirWorld;
            _dbgNewHorizVel = newHorizVel;
            _dbgDot = dot;
            _dbgSteeringFactor = steer;
            _dbgBraking = braking;
            _dbgHadInput = hadInput;
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawDebugGizmos)
                return;

            if (!Application.isPlaying)
                return;

            Vector3 origin = transform.position + Vector3.up * 0.1f;

            float velScale = debugVelocityScale;
            float r = debugSphereRadius;

            // Current horizontal velocity
            Gizmos.color = Color.green;
            Gizmos.DrawLine(origin, origin + _dbgHorizVel * velScale);
            Gizmos.DrawSphere(origin + _dbgHorizVel * velScale, r);

            // Desired velocity (input * speed)
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(origin, origin + _dbgDesiredVel * velScale);
            Gizmos.DrawSphere(origin + _dbgDesiredVel * velScale, r);

            // Input direction (normalized)
            if (_dbgHadInput)
            {
                Gizmos.color = Color.yellow;
                Vector3 dir = _dbgInputDir.sqrMagnitude > 0.0001f ? _dbgInputDir.normalized : Vector3.zero;
                Gizmos.DrawLine(origin, origin + dir);
                Gizmos.DrawSphere(origin + dir, r * 0.8f);
            }

            // Resulting velocity after steering
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(origin, origin + _dbgNewHorizVel * velScale);
            Gizmos.DrawSphere(origin + _dbgNewHorizVel * velScale, r);

            Handles.color = Color.white;
            Handles.Label(
                origin + Vector3.up * 0.5f,
                $"Dot: {_dbgDot:F2}\n" +
                $"Steer: {_dbgSteeringFactor:F2}\n" +
                $"Braking: {_dbgBraking}\n" +
                $"Sprinting: {_isSprinting}"
            );
        }
#endif
    }
}