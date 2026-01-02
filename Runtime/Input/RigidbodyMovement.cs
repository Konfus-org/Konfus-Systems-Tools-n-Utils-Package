using Konfus.Sensor_Toolkit;
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
        private const float DebugSphereRadius = 0.05f;
        private const float DebugVelocityScale = 0.15f;

        [Header("References")]
        [SerializeField]
        private ScanSensor? groundSensor;

        [Header("Speed & Modifiers")]
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
        [Min(0.1f)]
        private float reverseMod = 0.5f;
        [SerializeField]
        [Tooltip("Amount to multiply the amount of movement speed when steering")]
        [Min(1)]
        private float steeringMod = 1.5f;
        [SerializeField]
        [Range(0, 1)]
        [Tooltip("Amount of control when in air (0 = no control, 1 = full control). " +
                 "This ONLY affects how much new input can steer you; existing momentum is preserved.")]
        private float airControl = 0.5f;

        [Header("Acceleration & Deceleration")]
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

            bool grounded = IsGrounded();
            Vector3 v = _rigidbody.linearVelocity;
            var horiz = new Vector3(v.x, 0f, v.z);
            float horizSpeed = horiz.magnitude;

            var inputLocal = new Vector3(_moveInput.x, 0f, _moveInput.y);
            inputLocal = Vector3.ClampMagnitude(inputLocal, 1f);
            bool hasInput = inputLocal.sqrMagnitude > 0.0001f;

            // Local -> World direction (player facing)
            Vector3 desiredDirWorld = transform.TransformDirection(inputLocal);
            Vector3 desiredDirNorm = desiredDirWorld.sqrMagnitude > 0.0001f ? desiredDirWorld.normalized : Vector3.zero;

            // Speed modifiers
            float sprintModifier = _isSprinting ? sprintMod : 1f;
            float reverseModifier = inputLocal.z < 0f ? reverseMod : 1f;
            float targetSpeed = moveSpeed * sprintModifier * reverseModifier;

            // Curves -> per-tick delta-V budgets
            float normalizedSpeed = moveSpeed > 0.0001f ? Mathf.Clamp01(horizSpeed / moveSpeed) : 0f;

            float accelCurveMul = Mathf.Max(0f, accelerationCurve.Evaluate(normalizedSpeed));
            float decelCurveMul = Mathf.Max(0f, decelerationCurve.Evaluate(normalizedSpeed));

            float accelMaxDelta = accelerationRate * accelCurveMul * Time.fixedDeltaTime;
            float decelMaxDelta = decelerationRate * decelCurveMul * Time.fixedDeltaTime;

            // Steering context (used by both branches when input exists)
            float dot = horiz.sqrMagnitude > 0.0001f && desiredDirNorm.sqrMagnitude > 0.0001f
                ? Vector3.Dot(horiz.normalized, desiredDirNorm)
                : 1f;

            bool braking = dot < 0f;
            float steeringFactor = 1f - Mathf.Abs(dot); // 0 aligned, 1 perpendicular
            float steerBoost = Mathf.Lerp(1f, steeringMod, steeringFactor);

            switch (grounded)
            {
                case true when !hasInput: // Grounded + no input => friction/brake to stop
                {
                    Vector3 newHoriz = Vector3.MoveTowards(horiz, Vector3.zero, decelMaxDelta);
                    _rigidbody.linearVelocity = new Vector3(newHoriz.x, v.y, newHoriz.z);

                    CacheDebug(horiz, Vector3.zero, Vector3.zero, newHoriz);
                    break;
                }
                case true when hasInput: // Grounded + input => move toward desired velocity (direction * targetSpeed)
                {
                    Vector3 desiredVel = desiredDirNorm * (targetSpeed * inputLocal.magnitude);
                    float maxDeltaV = braking ? decelMaxDelta : accelMaxDelta * steerBoost;

                    Vector3 newGroundHoriz = Vector3.MoveTowards(horiz, desiredVel, maxDeltaV);
                    _rigidbody.linearVelocity = new Vector3(newGroundHoriz.x, v.y, newGroundHoriz.z);

                    CacheDebug(horiz, desiredVel, desiredDirNorm, newGroundHoriz);
                    break;
                }
                case false when !hasInput: // Airborne + no input => preserve momentum (no artificial drag)
                {
                    CacheDebug(horiz, Vector3.zero, Vector3.zero, horiz);
                    break;
                }
                default: // Airborne + input => steer toward desired direction using airControl
                {
                    if (airControl <= 0.0001f || desiredDirNorm == Vector3.zero || horiz.sqrMagnitude <= 0.0001f)
                    {
                        // No air control or no usable direction/momentum => preserve momentum
                        CacheDebug(horiz, desiredDirNorm * targetSpeed, desiredDirNorm, horiz);
                        return;
                    }

                    // Airborne steering:
                    // - preserve horizontal speed
                    // - rotate current velocity direction toward desired input direction
                    // - allow a bit of acceleration up to targetSpeed, but never force decel in air
                    float maxDeltaVAir = (braking ? decelMaxDelta : accelMaxDelta) * airControl * steerBoost;
                    float maxAngleRad = horizSpeed > 0.25f ? maxDeltaVAir / horizSpeed : 999f;

                    Vector3 newDir = Vector3.RotateTowards(horiz.normalized, desiredDirNorm, maxAngleRad, 0f);

                    float newSpeed = horizSpeed;
                    if (horizSpeed < targetSpeed)
                        newSpeed = Mathf.Min(targetSpeed, horizSpeed + accelMaxDelta * airControl);

                    Vector3 newAirHoriz = newDir * newSpeed;
                    _rigidbody.linearVelocity = new Vector3(newAirHoriz.x, v.y, newAirHoriz.z);

                    CacheDebug(horiz, desiredDirNorm * targetSpeed, desiredDirNorm, newAirHoriz);
                    break;
                }
            }
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
        }

        private bool IsGrounded()
        {
            return groundSensor?.Scan() ?? false;
        }

        private void CacheDebug(
            Vector3 horizVel,
            Vector3 desiredVel,
            Vector3 inputDirWorld,
            Vector3 newHorizVel)
        {
#if UNITY_EDITOR
            _dbgHorizVel = horizVel;
            _dbgDesiredVel = desiredVel;
            _dbgInputDir = inputDirWorld;
            _dbgNewHorizVel = newHorizVel;
#endif
        }

#if UNITY_EDITOR
        private Vector3 _dbgDesiredVel;
        private Vector3 _dbgHorizVel;
        private Vector3 _dbgInputDir;
        private Vector3 _dbgNewHorizVel;

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying)
                return;

            Vector3 origin = transform.position + Vector3.up * 0.1f;

            float velScale = DebugVelocityScale;
            float r = DebugSphereRadius;

            Gizmos.color = Color.green; // current horizontal velocity
            Gizmos.DrawLine(origin, origin + _dbgHorizVel * velScale);
            Gizmos.DrawSphere(origin + _dbgHorizVel * velScale, r);

            Gizmos.color = Color.blue; // desired velocity
            Gizmos.DrawLine(origin, origin + _dbgDesiredVel * velScale);
            Gizmos.DrawSphere(origin + _dbgDesiredVel * velScale, r);

            Gizmos.color = Color.yellow; // input direction
            Vector3 dir = _dbgInputDir.sqrMagnitude > 0.0001f ? _dbgInputDir.normalized : Vector3.zero;
            Gizmos.DrawLine(origin, origin + dir);
            Gizmos.DrawSphere(origin + dir, r * 0.8f);

            Gizmos.color = Color.cyan; // resulting velocity
            Gizmos.DrawLine(origin, origin + _dbgNewHorizVel * velScale);
            Gizmos.DrawSphere(origin + _dbgNewHorizVel * velScale, r);

            Handles.color = Color.white;
            Handles.Label(
                origin + Vector3.up * 0.5f,
                $"Sprinting: {_isSprinting}\n" +
                $"Grounded: {IsGrounded()}\n"
            );
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
        }
#endif
    }
}