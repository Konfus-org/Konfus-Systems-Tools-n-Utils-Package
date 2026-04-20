using Konfus.Sensor_Toolkit;
using UnityEngine;

namespace Konfus.Input
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class RigidbodyMovement : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private ScanSensor? groundSensor;
        [SerializeField]
        [Tooltip("Planar facing reference used to convert move input into world space. Defaults to this transform or the FPS camera yaw target when available.")]
        private Transform? movementReference;

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

        private Vector3 _currentHorizontalVelocity;
        private Vector3 _desiredHorizontalVelocity;
        private Vector3 _inputDirectionWorld;
        private Vector3 _resultingHorizontalVelocity;

        public bool IsSprinting => _isSprinting;
        public bool IsGroundedNow => IsGrounded();
        public Vector3 CurrentHorizontalVelocity => _currentHorizontalVelocity;
        public Vector3 DesiredHorizontalVelocity => _desiredHorizontalVelocity;
        public Vector3 InputDirection => _inputDirectionWorld;
        public Vector3 HorizontalVelocity => _resultingHorizontalVelocity;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            ApplyRigidbodyDefaults();
            EnsureCurveHasEndpoints(ref accelerationCurve);
            EnsureCurveHasEndpoints(ref decelerationCurve);
            AutoAssignMovementReference();
        }

        private void Reset()
        {
            _rigidbody = GetComponent<Rigidbody>();
            ApplyRigidbodyDefaults();
            EnsureCurveHasEndpoints(ref accelerationCurve);
            EnsureCurveHasEndpoints(ref decelerationCurve);
            AutoAssignMovementReference();
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

            // Local -> World direction using a planar facing reference so movement matches FPS yaw.
            GetPlanarMovementBasis(out Vector3 referenceRight, out Vector3 referenceForward);
            Vector3 desiredDirWorld = (referenceRight * inputLocal.x) + (referenceForward * inputLocal.z);
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

                    _currentHorizontalVelocity = horiz;
                    _desiredHorizontalVelocity = Vector3.zero;
                    _inputDirectionWorld = Vector3.zero;
                    _resultingHorizontalVelocity = newHoriz;
                    break;
                }
                case true when hasInput: // Grounded + input => move toward desired velocity (direction * targetSpeed)
                {
                    Vector3 desiredVel = desiredDirNorm * (targetSpeed * inputLocal.magnitude);
                    float maxDeltaV = braking ? decelMaxDelta : accelMaxDelta * steerBoost;

                    Vector3 newGroundHoriz = Vector3.MoveTowards(horiz, desiredVel, maxDeltaV);
                    _rigidbody.linearVelocity = new Vector3(newGroundHoriz.x, v.y, newGroundHoriz.z);

                    _currentHorizontalVelocity = horiz;
                    _desiredHorizontalVelocity = desiredVel;
                    _inputDirectionWorld = desiredDirNorm;
                    _resultingHorizontalVelocity = newGroundHoriz;
                    break;
                }
                case false when !hasInput: // Airborne + no input => preserve momentum (no artificial drag)
                {
                    _currentHorizontalVelocity = horiz;
                    _desiredHorizontalVelocity = Vector3.zero;
                    _inputDirectionWorld = Vector3.zero;
                    _resultingHorizontalVelocity = horiz;
                    break;
                }
                default: // Airborne + input => steer toward desired direction using airControl
                {
                    if (airControl <= 0.0001f || desiredDirNorm == Vector3.zero || horiz.sqrMagnitude <= 0.0001f)
                    {
                        // No air control or no usable direction/momentum => preserve momentum
                        _currentHorizontalVelocity = horiz;
                        _desiredHorizontalVelocity = desiredDirNorm * targetSpeed;
                        _inputDirectionWorld = desiredDirNorm;
                        _resultingHorizontalVelocity = horiz;
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

                    _currentHorizontalVelocity = horiz;
                    _desiredHorizontalVelocity = desiredDirNorm * targetSpeed;
                    _inputDirectionWorld = desiredDirNorm;
                    _resultingHorizontalVelocity = newAirHoriz;
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

        private void AutoAssignMovementReference()
        {
            if (movementReference)
                return;

            var fpsCamera = GetComponent<FPSCamera>();
            if (!fpsCamera)
                fpsCamera = GetComponentInChildren<FPSCamera>();

            movementReference = fpsCamera ? fpsCamera.YawTarget : transform;
        }

        private void GetPlanarMovementBasis(out Vector3 right, out Vector3 forward)
        {
            Transform reference = movementReference ? movementReference : transform;

            forward = Vector3.ProjectOnPlane(reference.forward, Vector3.up);
            if (forward.sqrMagnitude <= 0.0001f)
                forward = Vector3.ProjectOnPlane(reference.up, Vector3.up);
            if (forward.sqrMagnitude <= 0.0001f)
                forward = transform.forward;

            forward.Normalize();
            right = Vector3.Cross(Vector3.up, forward);
        }

        private void ApplyRigidbodyDefaults()
        {
            if (!_rigidbody) return;

            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }
}
