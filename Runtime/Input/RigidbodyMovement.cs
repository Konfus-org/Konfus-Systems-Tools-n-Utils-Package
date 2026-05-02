using Konfus.Sensor_Toolkit;
using UnityEngine;

namespace Konfus.Input
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class RigidbodyMovement : MonoBehaviour
    {
        public enum MovementDebugState
        {
            GroundedIdle,
            GroundedMove,
            Sliding,
            AirIdle,
            AirMove
        }

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

        [Header("Grounding")]
        [SerializeField]
        [Min(0f)]
        [Tooltip("Maximum ledge height the player can step onto while moving.")]
        private float stepHeight = 0.3f;
        [SerializeField]
        [Range(0f, 89f)]
        [Tooltip("Maximum ground angle in degrees before the player starts sliding.")]
        private float maxInclineAngle = 45f;
        [SerializeField]
        [Min(0f)]
        [Tooltip("Extra downward bias applied while grounded to keep the body stuck to walkable ground.")]
        private float groundStickVelocity = 4f;
        [SerializeField]
        [Min(0f)]
        [Tooltip("Additional downhill acceleration applied when standing on slopes steeper than the max incline.")]
        private float slopeSlideAcceleration = 10f;
        [SerializeField]
        [Min(0.05f)]
        [Tooltip("Forward distance used to probe for step-ups.")]
        private float stepCheckDistance = 0.4f;

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
        private CapsuleCollider? _capsuleCollider;
        private RigidbodyJumping? _jumping;

        private Vector3 _currentHorizontalVelocity;
        private Vector3 _desiredHorizontalVelocity;
        private Vector3 _inputDirectionWorld;
        private Vector3 _resultingHorizontalVelocity;
        private bool _hasGroundContact;
        private bool _isWalkableGround;
        private bool _isJumpingNow;
        private float _groundSurfaceAngle;
        private Vector3 _groundNormal;
        private Vector3 _groundPoint;
        private Vector2 _debugMoveInput;
        private MovementDebugState _debugState;
        private Vector3 _lastAppliedVelocity;
        private Vector3 _lastPosition;
        private Vector3 _positionDelta;

        public bool IsSprinting => _isSprinting;
        public bool IsGroundedNow => IsGrounded();
        public Vector3 CurrentHorizontalVelocity => _currentHorizontalVelocity;
        public Vector3 DesiredHorizontalVelocity => _desiredHorizontalVelocity;
        public Vector3 InputDirection => _inputDirectionWorld;
        public Vector3 HorizontalVelocity => _resultingHorizontalVelocity;
        public bool HasGroundContact => _hasGroundContact;
        public bool IsWalkableGround => _isWalkableGround;
        public bool IsJumpingNow => _isJumpingNow;
        public float GroundSurfaceAngle => _groundSurfaceAngle;
        public Vector3 GroundNormal => _groundNormal;
        public Vector3 GroundPoint => _groundPoint;
        public Vector2 MoveInput => _debugMoveInput;
        public MovementDebugState DebugState => _debugState;
        public float MaxInclineAngle => maxInclineAngle;
        public Vector3 LastAppliedVelocity => _lastAppliedVelocity;
        public Vector3 RawLinearVelocity => _rigidbody ? _rigidbody.linearVelocity : Vector3.zero;
        public Vector3 PositionDelta => _positionDelta;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _capsuleCollider = GetComponent<CapsuleCollider>();
            _jumping = GetComponent<RigidbodyJumping>();
            _lastPosition = transform.position;
            ApplyRigidbodyDefaults();
            EnsureCurveHasEndpoints(ref accelerationCurve);
            EnsureCurveHasEndpoints(ref decelerationCurve);
            AutoAssignMovementReference();
        }

        private void Reset()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _capsuleCollider = GetComponent<CapsuleCollider>();
            _jumping = GetComponent<RigidbodyJumping>();
            _lastPosition = transform.position;
            ApplyRigidbodyDefaults();
            EnsureCurveHasEndpoints(ref accelerationCurve);
            EnsureCurveHasEndpoints(ref decelerationCurve);
            AutoAssignMovementReference();
        }

        private void FixedUpdate()
        {
            if (!_rigidbody) return;

            _positionDelta = _rigidbody.position - _lastPosition;
            _lastPosition = _rigidbody.position;

            bool hasGroundContact = TryGetGroundHit(out GroundHit groundHit);
            bool walkableGround = hasGroundContact && groundHit.SurfaceAngle <= maxInclineAngle;
            bool jumping = _jumping && _jumping.IsJumping;
            CacheGroundDebugState(hasGroundContact, walkableGround, jumping, groundHit);
            Vector3 v = _rigidbody.linearVelocity;
            Vector3 movementPlaneNormal = walkableGround ? groundHit.Normal : Vector3.up;
            Vector3 tangentVelocity = walkableGround ? Vector3.ProjectOnPlane(v, movementPlaneNormal) : new Vector3(v.x, 0f, v.z);
            float tangentSpeed = tangentVelocity.magnitude;

            var inputLocal = new Vector3(_moveInput.x, 0f, _moveInput.y);
            inputLocal = Vector3.ClampMagnitude(inputLocal, 1f);
            bool hasInput = inputLocal.sqrMagnitude > 0.0001f;
            _debugMoveInput = _moveInput;

            // Local -> World direction using a planar facing reference so movement matches FPS yaw.
            GetPlanarMovementBasis(out Vector3 referenceRight, out Vector3 referenceForward);
            Vector3 desiredDirWorld = (referenceRight * inputLocal.x) + (referenceForward * inputLocal.z);
            Vector3 desiredDirProjected = walkableGround
                ? Vector3.ProjectOnPlane(desiredDirWorld, movementPlaneNormal)
                : desiredDirWorld;
            Vector3 desiredDirNorm = desiredDirProjected.sqrMagnitude > 0.0001f ? desiredDirProjected.normalized : Vector3.zero;

            // Speed modifiers
            float sprintModifier = _isSprinting ? sprintMod : 1f;
            float reverseModifier = inputLocal.z < 0f ? reverseMod : 1f;
            float targetSpeed = moveSpeed * sprintModifier * reverseModifier;

            // Curves -> per-tick delta-V budgets
            float normalizedSpeed = moveSpeed > 0.0001f ? Mathf.Clamp01(tangentSpeed / moveSpeed) : 0f;

            float accelCurveMul = Mathf.Max(0f, accelerationCurve.Evaluate(normalizedSpeed));
            float decelCurveMul = Mathf.Max(0f, decelerationCurve.Evaluate(normalizedSpeed));

            float accelMaxDelta = accelerationRate * accelCurveMul * Time.fixedDeltaTime;
            float decelMaxDelta = decelerationRate * decelCurveMul * Time.fixedDeltaTime;

            // Steering context (used by both branches when input exists)
            float dot = tangentVelocity.sqrMagnitude > 0.0001f && desiredDirNorm.sqrMagnitude > 0.0001f
                ? Vector3.Dot(tangentVelocity.normalized, desiredDirNorm)
                : 1f;

            bool braking = dot < 0f;
            float steeringFactor = 1f - Mathf.Abs(dot); // 0 aligned, 1 perpendicular
            float steerBoost = Mathf.Lerp(1f, steeringMod, steeringFactor);

            if (walkableGround && !jumping && hasInput)
            {
                TryStepUp(desiredDirNorm, groundHit);
                hasGroundContact = TryGetGroundHit(out groundHit);
                walkableGround = hasGroundContact && groundHit.SurfaceAngle <= maxInclineAngle;
                CacheGroundDebugState(hasGroundContact, walkableGround, jumping, groundHit);
                movementPlaneNormal = walkableGround ? groundHit.Normal : Vector3.up;
                v = _rigidbody.linearVelocity;
                tangentVelocity = walkableGround ? Vector3.ProjectOnPlane(v, movementPlaneNormal) : new Vector3(v.x, 0f, v.z);
                desiredDirProjected = walkableGround ? Vector3.ProjectOnPlane(desiredDirWorld, movementPlaneNormal) : desiredDirWorld;
                desiredDirNorm = desiredDirProjected.sqrMagnitude > 0.0001f ? desiredDirProjected.normalized : Vector3.zero;
                dot = tangentVelocity.sqrMagnitude > 0.0001f && desiredDirNorm.sqrMagnitude > 0.0001f
                    ? Vector3.Dot(tangentVelocity.normalized, desiredDirNorm)
                    : 1f;
                braking = dot < 0f;
                steeringFactor = 1f - Mathf.Abs(dot);
                steerBoost = Mathf.Lerp(1f, steeringMod, steeringFactor);
            }

            switch (walkableGround && !jumping)
            {
                case true when !hasInput: // Grounded + no input => friction/brake to stop
                {
                    Vector3 newTangent = Vector3.MoveTowards(tangentVelocity, Vector3.zero, decelMaxDelta);
                    _rigidbody.linearVelocity = ComposeGroundVelocity(newTangent, v, movementPlaneNormal);
                    _lastAppliedVelocity = _rigidbody.linearVelocity;

                    _currentHorizontalVelocity = tangentVelocity;
                    _desiredHorizontalVelocity = Vector3.zero;
                    _inputDirectionWorld = Vector3.zero;
                    _resultingHorizontalVelocity = newTangent;
                    _debugState = MovementDebugState.GroundedIdle;
                    break;
                }
                case true when hasInput: // Grounded + input => move toward desired velocity (direction * targetSpeed)
                {
                    Vector3 desiredVel = desiredDirNorm * (targetSpeed * inputLocal.magnitude);
                    float maxDeltaV = braking ? decelMaxDelta : accelMaxDelta * steerBoost;

                    Vector3 newGroundTangent = Vector3.MoveTowards(tangentVelocity, desiredVel, maxDeltaV);
                    _rigidbody.linearVelocity = ComposeGroundVelocity(newGroundTangent, v, movementPlaneNormal);
                    _lastAppliedVelocity = _rigidbody.linearVelocity;

                    _currentHorizontalVelocity = tangentVelocity;
                    _desiredHorizontalVelocity = desiredVel;
                    _inputDirectionWorld = desiredDirNorm;
                    _resultingHorizontalVelocity = newGroundTangent;
                    _debugState = MovementDebugState.GroundedMove;
                    break;
                }
                case false when hasGroundContact && !jumping: // Too steep => slide down slope while preserving player steering lockout
                {
                    Vector3 slopeTangent = Vector3.ProjectOnPlane(Vector3.down, groundHit.Normal).normalized;
                    Vector3 slideVelocity = tangentVelocity + (slopeTangent * (slopeSlideAcceleration * Time.fixedDeltaTime));
                    float downwardBias = groundStickVelocity * Time.fixedDeltaTime;
                    _rigidbody.linearVelocity = new Vector3(slideVelocity.x, Mathf.Min(v.y, -downwardBias), slideVelocity.z);
                    _lastAppliedVelocity = _rigidbody.linearVelocity;

                    _currentHorizontalVelocity = tangentVelocity;
                    _desiredHorizontalVelocity = Vector3.zero;
                    _inputDirectionWorld = Vector3.zero;
                    _resultingHorizontalVelocity = slideVelocity;
                    _debugState = MovementDebugState.Sliding;
                    break;
                }
                case false when !hasInput: // Airborne + no input => preserve momentum (no artificial drag)
                {
                    _currentHorizontalVelocity = tangentVelocity;
                    _desiredHorizontalVelocity = Vector3.zero;
                    _inputDirectionWorld = Vector3.zero;
                    _resultingHorizontalVelocity = tangentVelocity;
                    _lastAppliedVelocity = _rigidbody.linearVelocity;
                    _debugState = MovementDebugState.AirIdle;
                    break;
                }
                default: // Airborne + input => steer toward desired direction using airControl
                {
                    Vector3 airHorizontal = new Vector3(v.x, 0f, v.z);
                    float airHorizontalSpeed = airHorizontal.magnitude;
                    Vector3 airDesiredDir = Vector3.ProjectOnPlane(desiredDirWorld, Vector3.up);
                    if (airDesiredDir.sqrMagnitude > 0.0001f)
                        airDesiredDir.Normalize();
                    else
                        airDesiredDir = desiredDirNorm;
                    Vector3 desiredAirVelocity = airDesiredDir * (targetSpeed * inputLocal.magnitude);

                    if (airControl <= 0.0001f || airDesiredDir == Vector3.zero)
                    {
                        _currentHorizontalVelocity = airHorizontal;
                        _desiredHorizontalVelocity = desiredAirVelocity;
                        _inputDirectionWorld = airDesiredDir;
                        _resultingHorizontalVelocity = airHorizontal;
                        _lastAppliedVelocity = _rigidbody.linearVelocity;
                        _debugState = MovementDebugState.AirIdle;
                        return;
                    }

                    if (airHorizontal.sqrMagnitude <= 0.0001f)
                    {
                        Vector3 newAirHorizFromRest = Vector3.MoveTowards(
                            airHorizontal,
                            desiredAirVelocity,
                            accelMaxDelta * airControl);

                        _rigidbody.linearVelocity = new Vector3(newAirHorizFromRest.x, v.y, newAirHorizFromRest.z);
                        _lastAppliedVelocity = _rigidbody.linearVelocity;

                        _currentHorizontalVelocity = airHorizontal;
                        _desiredHorizontalVelocity = desiredAirVelocity;
                        _inputDirectionWorld = airDesiredDir;
                        _resultingHorizontalVelocity = newAirHorizFromRest;
                        _debugState = MovementDebugState.AirMove;
                        break;
                    }

                    // Airborne steering:
                    // - preserve horizontal speed
                    // - rotate current velocity direction toward desired input direction
                    // - allow a bit of acceleration up to targetSpeed, but never force decel in air
                    float airDot = airHorizontal.sqrMagnitude > 0.0001f && desiredDirNorm.sqrMagnitude > 0.0001f
                        ? Vector3.Dot(airHorizontal.normalized, desiredDirNorm)
                        : 1f;
                    bool airBraking = airDot < 0f;
                    float maxDeltaVAir = (airBraking ? decelMaxDelta : accelMaxDelta) * airControl * steerBoost;
                    float maxAngleRad = airHorizontalSpeed > 0.25f ? maxDeltaVAir / airHorizontalSpeed : 999f;

                    Vector3 newDir = Vector3.RotateTowards(airHorizontal.normalized, airDesiredDir, maxAngleRad, 0f);

                    float newSpeed = airHorizontalSpeed;
                    if (airHorizontalSpeed < targetSpeed)
                        newSpeed = Mathf.Min(targetSpeed, airHorizontalSpeed + accelMaxDelta * airControl);

                    Vector3 newAirHoriz = newDir * newSpeed;
                    _rigidbody.linearVelocity = new Vector3(newAirHoriz.x, v.y, newAirHoriz.z);
                    _lastAppliedVelocity = _rigidbody.linearVelocity;

                    _currentHorizontalVelocity = airHorizontal;
                    _desiredHorizontalVelocity = desiredAirVelocity;
                    _inputDirectionWorld = airDesiredDir;
                    _resultingHorizontalVelocity = newAirHoriz;
                    _debugState = MovementDebugState.AirMove;
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
            return TryGetGroundHit(out _);
        }

        private bool TryGetGroundHit(out GroundHit groundHit)
        {
            groundHit = default;
            if (!groundSensor || !groundSensor.Scan() || groundSensor.Hits == null)
                return false;

            bool found = false;
            float bestUpDot = float.NegativeInfinity;

            foreach (Sensor.Hit hit in groundSensor.Hits)
            {
                if (!hit.GameObject || hit.GameObject == gameObject || hit.GameObject.transform.IsChildOf(transform))
                    continue;

                float upDot = Vector3.Dot(hit.Normal.normalized, Vector3.up);
                if (upDot <= 0f || upDot <= bestUpDot) continue;

                bestUpDot = upDot;
                groundHit = new GroundHit
                {
                    Point = hit.Point,
                    Normal = hit.Normal.normalized,
                    SurfaceAngle = Vector3.Angle(hit.Normal, Vector3.up)
                };
                found = true;
            }

            return found;
        }

        private void CacheGroundDebugState(bool hasGroundContact, bool walkableGround, bool jumping, GroundHit groundHit)
        {
            _hasGroundContact = hasGroundContact;
            _isWalkableGround = walkableGround;
            _isJumpingNow = jumping;
            _groundSurfaceAngle = hasGroundContact ? groundHit.SurfaceAngle : 0f;
            _groundNormal = hasGroundContact ? groundHit.Normal : Vector3.up;
            _groundPoint = hasGroundContact ? groundHit.Point : transform.position;
        }

        private Vector3 ComposeGroundVelocity(Vector3 tangentVelocity, Vector3 currentVelocity, Vector3 groundNormal)
        {
            float normalVelocity = Vector3.Dot(currentVelocity, groundNormal);
            normalVelocity = Mathf.Min(normalVelocity, 0f) - (groundStickVelocity * Time.fixedDeltaTime);
            return tangentVelocity + (groundNormal * normalVelocity);
        }

        private void TryStepUp(Vector3 moveDirection, GroundHit groundHit)
        {
            if (!_rigidbody || !_capsuleCollider || moveDirection.sqrMagnitude <= 0.0001f || stepHeight <= 0f)
                return;

            GetCapsuleWorld(out Vector3 point1, out Vector3 point2, out float radius);

            float skin = 0.05f;
            float checkDistance = Mathf.Max(stepCheckDistance, radius + skin);
            Vector3 lowerOrigin = point2 + (transform.up * (radius + Mathf.Max(skin, 0.02f)));
            Vector3 upperOrigin = lowerOrigin + Vector3.up * stepHeight;
            LayerMask mask = groundSensor ? groundSensor.DetectionFilter : Physics.DefaultRaycastLayers;

            bool lowerBlocked = Physics.SphereCast(lowerOrigin, radius * 0.9f, moveDirection, out _, checkDistance, mask, QueryTriggerInteraction.Ignore);
            if (!lowerBlocked)
                return;

            bool upperBlocked = Physics.SphereCast(upperOrigin, radius * 0.9f, moveDirection, out _, checkDistance, mask, QueryTriggerInteraction.Ignore);
            if (upperBlocked)
                return;

            Vector3 stepProbeOrigin = upperOrigin + (moveDirection * checkDistance) + Vector3.up * skin;
            float downDistance = stepHeight + skin + 0.5f;
            if (!Physics.Raycast(stepProbeOrigin, Vector3.down, out RaycastHit stepHit, downDistance, mask, QueryTriggerInteraction.Ignore))
                return;

            float stepUp = stepHit.point.y - groundHit.Point.y;
            if (stepUp <= 0f || stepUp > stepHeight)
                return;

            float stepAngle = Vector3.Angle(stepHit.normal, Vector3.up);
            if (stepAngle > maxInclineAngle)
                return;

            Vector3 newPosition = _rigidbody.position;
            newPosition.y += stepUp + skin;
            _rigidbody.MovePosition(newPosition);
        }

        private void GetCapsuleWorld(out Vector3 point1, out Vector3 point2, out float radius)
        {
            if (!_capsuleCollider)
            {
                point1 = transform.position;
                point2 = transform.position;
                radius = 0.25f;
                return;
            }

            Vector3 scale = transform.lossyScale;
            float radiusScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
            radius = _capsuleCollider.radius * radiusScale;

            float height = Mathf.Max(_capsuleCollider.height * Mathf.Abs(scale.y), radius * 2f);
            float cylinder = Mathf.Max(0f, (height * 0.5f) - radius);
            Vector3 center = transform.TransformPoint(_capsuleCollider.center);
            Vector3 axis = transform.up;

            point1 = center + (axis * cylinder);
            point2 = center - (axis * cylinder);
        }

        private struct GroundHit
        {
            public Vector3 Point;
            public Vector3 Normal;
            public float SurfaceAngle;
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
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
    }
}
