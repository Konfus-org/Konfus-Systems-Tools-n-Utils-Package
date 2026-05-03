using Konfus.Sensor_Toolkit;
using UnityEngine;
using UnityEngine.InputSystem;
using SensorHit = Konfus.Sensor_Toolkit.Sensor.Hit;

namespace Konfus.Input
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class RigidbodyMovement : MonoBehaviour
    {
        private const float InputDeadZone = 0.0001f;
        private const float AscendingGroundReleaseVelocity = 0.05f;
        private const float DynamicSupportContactUpDotThreshold = 0.25f;

        [Header("References")]
        [SerializeField]
        private ScanSensor? groundSensor;
        [SerializeField]
        [Tooltip("Planar facing reference used to convert move input into world space. Defaults to this transform.")]
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
        [Tooltip("Amount of control when in air (0 = no control, 1 = full control). Existing momentum is preserved.")]
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
        [SerializeField]
        [Range(0.01f, 1f)]
        [Tooltip("Scales the player's rigidbody mass while the ground sensor is on a dynamic rigidbody platform to reduce force transfer into movable objects.")]
        private float dynamicGroundMassMultiplier = 0.01f;

        [Header("Acceleration & Deceleration")]
        [SerializeField]
        [Tooltip("Max horizontal acceleration rate in m/s²")]
        [Min(1)]
        private float accelerationRate = 35f;
        [SerializeField]
        [Tooltip("Max horizontal deceleration rate in m/s²")]
        [Min(1)]
        private float decelerationRate = 45f;
        [SerializeField]
        [Tooltip("X = normalized speed 0..1, Y = multiplier")]
        private AnimationCurve accelerationCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0.2f);
        [SerializeField]
        [Tooltip("X = normalized speed 0..1, Y = multiplier")]
        private AnimationCurve decelerationCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0.6f);

        private bool _isSprinting;
        private Vector2 _moveInput;
        private Rigidbody? _rigidbody;
        private CapsuleCollider? _capsuleCollider;

        private Vector3 _currentHorizontalVelocity;
        private Vector3 _desiredHorizontalVelocity;
        private Vector3 _inputDirectionWorld;
        private Vector3 _resultingHorizontalVelocity;
        private float _baseMass = 1f;
        private bool _hasGroundContact;
        private bool _isWalkableGround;
        private bool _isAscendingFromGround;
        private bool _isStandingOnDynamicBody;
        private float _groundSurfaceAngle;
        private Vector3 _groundNormal = Vector3.up;
        private Vector3 _groundPoint;
        private Vector2 _debugMoveInput;
        private MovementStateId _currentState = MovementStateId.AirborneIdle;
        private Vector3 _lastAppliedVelocity;
        private Vector3 _lastPosition;
        private Vector3 _positionDelta;
        private bool _hasDynamicSupportCollision;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _capsuleCollider = GetComponent<CapsuleCollider>();
            _lastPosition = transform.position;
            _baseMass = _rigidbody ? _rigidbody.mass : 1f;

            ApplyRigidbodyDefaults();
            EnsureCurveHasEndpoints(ref accelerationCurve);
            EnsureCurveHasEndpoints(ref decelerationCurve);
            AutoAssignMovementReference();
        }

        private void Reset()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _capsuleCollider = GetComponent<CapsuleCollider>();
            _lastPosition = transform.position;
            _baseMass = _rigidbody ? _rigidbody.mass : 1f;

            ApplyRigidbodyDefaults();
            EnsureCurveHasEndpoints(ref accelerationCurve);
            EnsureCurveHasEndpoints(ref decelerationCurve);
            AutoAssignMovementReference();
        }

        private void OnValidate()
        {
            EnsureCurveHasEndpoints(ref accelerationCurve);
            EnsureCurveHasEndpoints(ref decelerationCurve);
        }

        private void FixedUpdate()
        {
            if (!_rigidbody) return;

            _positionDelta = _rigidbody.position - _lastPosition;
            _lastPosition = _rigidbody.position;

            bool hasDynamicSupportCollision = _hasDynamicSupportCollision;
            _hasDynamicSupportCollision = false;

            MovementFrame frame = BuildFrame();
            UpdateDynamicGroundMass(frame.GroundHit.IsDynamicBody || hasDynamicSupportCollision);
            TickMovement(frame);
        }

        private void OnCollisionEnter(Collision collision)
        {
            CacheDynamicSupportCollision(collision);
        }

        private void OnCollisionStay(Collision collision)
        {
            CacheDynamicSupportCollision(collision);
        }

        public bool IsSprinting => _isSprinting;
        public bool IsGroundedNow => _hasGroundContact && _isWalkableGround && !_isAscendingFromGround;
        public Vector3 CurrentHorizontalVelocity => _currentHorizontalVelocity;
        public Vector3 DesiredHorizontalVelocity => _desiredHorizontalVelocity;
        public Vector3 InputDirection => _inputDirectionWorld;
        public Vector3 HorizontalVelocity => _resultingHorizontalVelocity;
        public bool HasGroundContact => _hasGroundContact;
        public bool IsWalkableGround => _isWalkableGround;
        public bool IsAscendingFromGround => _isAscendingFromGround;
        public bool IsStandingOnDynamicBody => _isStandingOnDynamicBody;
        public float GroundSurfaceAngle => _groundSurfaceAngle;
        public Vector3 GroundNormal => _groundNormal;
        public Vector3 GroundPoint => _groundPoint;
        public Vector2 MoveInput => _debugMoveInput;
        public MovementStateId CurrentState => _currentState;
        public float MaxInclineAngle => maxInclineAngle;
        public Vector3 LastAppliedVelocity => _lastAppliedVelocity;
        public Vector3 RawLinearVelocity => _rigidbody ? _rigidbody.linearVelocity : Vector3.zero;
        public Vector3 PositionDelta => _positionDelta;
        public ScanSensor? GroundSensor => groundSensor;

        public void StartSprint()
        {
            _isSprinting = true;
        }

        public void StopSprint()
        {
            _isSprinting = false;
        }

        public void HandleSprint(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                StartSprint();
            }
            else if (ctx.canceled)
            {
                StopSprint();
            }
        }

        /// <summary>
        /// Supplies movement input in local X/Z space. Expected range is -1..1 per axis.
        /// </summary>
        public void Move(Vector2 input)
        {
            _moveInput = Vector2.ClampMagnitude(input, 1f);
        }

        private MovementFrame BuildFrame()
        {
            Vector3 velocity = _rigidbody ? _rigidbody.linearVelocity : Vector3.zero;
            bool hasGroundContact = TryGetGroundHit(out GroundHit groundHit);
            bool walkableGround = hasGroundContact && groundHit.SurfaceAngle <= maxInclineAngle;
            bool ascendingFromGround = walkableGround && velocity.y > AscendingGroundReleaseVelocity;

            CacheGroundState(hasGroundContact, walkableGround, ascendingFromGround, groundHit);

            Vector3 inputLocal = GetClampedInput();
            bool hasInput = inputLocal.sqrMagnitude > InputDeadZone;
            bool useGroundPlane = walkableGround && !ascendingFromGround;
            Vector3 movementPlaneNormal = useGroundPlane ? groundHit.Normal : Vector3.up;
            Vector3 tangentVelocity = useGroundPlane
                ? Vector3.ProjectOnPlane(velocity, movementPlaneNormal)
                : new Vector3(velocity.x, 0f, velocity.z);

            Vector3 desiredDirectionWorld = GetDesiredDirectionWorld(inputLocal);
            Vector3 desiredDirection = ProjectDesiredDirection(desiredDirectionWorld, movementPlaneNormal, useGroundPlane);
            float targetSpeed = GetTargetSpeed(inputLocal);
            MovementDeltas deltas = CalculateMovementDeltas(tangentVelocity, desiredDirection);

            return new MovementFrame
            {
                Velocity = velocity,
                GroundHit = groundHit,
                HasWalkableGround = walkableGround && !ascendingFromGround,
                CanSlide = hasGroundContact && !walkableGround && !ascendingFromGround,
                HasInput = hasInput,
                InputLocal = inputLocal,
                DesiredDirectionWorld = desiredDirectionWorld,
                DesiredDirection = desiredDirection,
                MovementPlaneNormal = movementPlaneNormal,
                TangentVelocity = tangentVelocity,
                TargetSpeed = targetSpeed,
                AccelMaxDelta = deltas.AccelMaxDelta,
                DecelMaxDelta = deltas.DecelMaxDelta,
                IsBraking = deltas.IsBraking,
                SteerBoost = deltas.SteerBoost
            };
        }

        private void TickMovement(MovementFrame frame)
        {
            _currentState = SelectMovementState(frame);

            switch (_currentState)
            {
                case MovementStateId.GroundedIdle:
                    ApplyGroundedIdle(frame);
                    break;
                case MovementStateId.GroundedMove:
                    ApplyGroundedMove(frame);
                    break;
                case MovementStateId.Sliding:
                    ApplySliding(frame);
                    break;
                case MovementStateId.AirborneIdle:
                case MovementStateId.AirborneMove:
                    ApplyAirborne(frame);
                    break;
            }
        }

        private MovementStateId SelectMovementState(MovementFrame frame)
        {
            if (frame.HasWalkableGround)
                return frame.HasInput ? MovementStateId.GroundedMove : MovementStateId.GroundedIdle;

            if (frame.CanSlide)
                return MovementStateId.Sliding;

            return frame.HasInput ? MovementStateId.AirborneMove : MovementStateId.AirborneIdle;
        }

        private Vector3 GetClampedInput()
        {
            Vector3 inputLocal = new(_moveInput.x, 0f, _moveInput.y);
            inputLocal = Vector3.ClampMagnitude(inputLocal, 1f);
            _debugMoveInput = _moveInput;
            return inputLocal;
        }

        private Vector3 GetDesiredDirectionWorld(Vector3 inputLocal)
        {
            GetPlanarMovementBasis(out Vector3 referenceRight, out Vector3 referenceForward);
            return referenceRight * inputLocal.x + referenceForward * inputLocal.z;
        }

        private static Vector3 ProjectDesiredDirection(Vector3 desiredDirectionWorld, Vector3 movementPlaneNormal, bool useGroundPlane)
        {
            Vector3 projected = useGroundPlane
                ? Vector3.ProjectOnPlane(desiredDirectionWorld, movementPlaneNormal)
                : Vector3.ProjectOnPlane(desiredDirectionWorld, Vector3.up);

            return projected.sqrMagnitude > InputDeadZone ? projected.normalized : Vector3.zero;
        }

        private float GetTargetSpeed(Vector3 inputLocal)
        {
            float sprintModifier = _isSprinting ? sprintMod : 1f;
            float reverseModifier = inputLocal.z < 0f ? reverseMod : 1f;
            return moveSpeed * sprintModifier * reverseModifier;
        }

        private MovementDeltas CalculateMovementDeltas(Vector3 tangentVelocity, Vector3 desiredDirection)
        {
            float normalizedSpeed = moveSpeed > InputDeadZone ? Mathf.Clamp01(tangentVelocity.magnitude / moveSpeed) : 0f;
            float accelCurveMul = Mathf.Max(0f, accelerationCurve.Evaluate(normalizedSpeed));
            float decelCurveMul = Mathf.Max(0f, decelerationCurve.Evaluate(normalizedSpeed));

            float dot = tangentVelocity.sqrMagnitude > InputDeadZone && desiredDirection.sqrMagnitude > InputDeadZone
                ? Vector3.Dot(tangentVelocity.normalized, desiredDirection)
                : 1f;
            float steeringFactor = 1f - Mathf.Abs(dot);

            return new MovementDeltas
            {
                AccelMaxDelta = accelerationRate * accelCurveMul * Time.fixedDeltaTime,
                DecelMaxDelta = decelerationRate * decelCurveMul * Time.fixedDeltaTime,
                IsBraking = dot < 0f,
                SteerBoost = Mathf.Lerp(1f, steeringMod, steeringFactor)
            };
        }

        private bool TryGetGroundHit(out GroundHit groundHit)
        {
            groundHit = default;
            if (!groundSensor || !groundSensor.Scan() || groundSensor.Hits == null)
                return false;

            bool found = false;
            float bestUpDot = float.NegativeInfinity;

            foreach (SensorHit hit in groundSensor.Hits)
            {
                if (!hit.GameObject || hit.GameObject == gameObject || hit.GameObject.transform.IsChildOf(transform))
                    continue;

                float upDot = Vector3.Dot(hit.Normal.normalized, Vector3.up);
                if (upDot <= 0f || upDot <= bestUpDot)
                    continue;

                bestUpDot = upDot;
                groundHit = new GroundHit
                {
                    Point = hit.Point,
                    Normal = hit.Normal.normalized,
                    SurfaceAngle = Vector3.Angle(hit.Normal, Vector3.up),
                    GroundBody = hit.GameObject.GetComponentInParent<Rigidbody>()
                };
                found = true;
            }

            return found;
        }

        private void CacheGroundState(bool hasGroundContact, bool walkableGround, bool ascendingFromGround, GroundHit groundHit)
        {
            _hasGroundContact = hasGroundContact;
            _isWalkableGround = walkableGround;
            _isAscendingFromGround = ascendingFromGround;
            _isStandingOnDynamicBody = hasGroundContact && groundHit.IsDynamicBody;
            _groundSurfaceAngle = hasGroundContact ? groundHit.SurfaceAngle : 0f;
            _groundNormal = hasGroundContact ? groundHit.Normal : Vector3.up;
            _groundPoint = hasGroundContact ? groundHit.Point : transform.position;
        }

        private Vector3 ComposeGroundVelocity(Vector3 tangentVelocity, Vector3 groundNormal, bool applyGroundStickForce)
        {
            float normalVelocity = applyGroundStickForce
                ? -groundStickVelocity * Time.fixedDeltaTime
                : 0f;

            return tangentVelocity + groundNormal * normalVelocity;
        }

        private void ApplyGroundedIdle(MovementFrame frame)
        {
            if (!_rigidbody) return;

            Vector3 newTangent = Vector3.MoveTowards(frame.TangentVelocity, Vector3.zero, frame.DecelMaxDelta);
            _rigidbody.linearVelocity = ComposeGroundVelocity(
                newTangent,
                frame.MovementPlaneNormal,
                applyGroundStickForce: !frame.GroundHit.IsDynamicBody);
            CacheMovementVectors(frame.TangentVelocity, Vector3.zero, Vector3.zero, newTangent);
        }

        private void ApplyGroundedMove(MovementFrame frame)
        {
            if (!_rigidbody) return;

            TryStepUp(frame.DesiredDirection, frame.GroundHit);

            MovementFrame currentFrame = BuildFrame();
            Vector3 desiredVelocity = currentFrame.DesiredDirection * (currentFrame.TargetSpeed * currentFrame.InputLocal.magnitude);
            float maxDeltaV = currentFrame.IsBraking
                ? currentFrame.DecelMaxDelta
                : currentFrame.AccelMaxDelta * currentFrame.SteerBoost;
            Vector3 newTangent = Vector3.MoveTowards(currentFrame.TangentVelocity, desiredVelocity, maxDeltaV);

            _rigidbody.linearVelocity = ComposeGroundVelocity(
                newTangent,
                currentFrame.MovementPlaneNormal,
                applyGroundStickForce: !currentFrame.GroundHit.IsDynamicBody);
            CacheMovementVectors(currentFrame.TangentVelocity, desiredVelocity, currentFrame.DesiredDirection, newTangent);
        }

        private void ApplySliding(MovementFrame frame)
        {
            if (!_rigidbody) return;

            Vector3 slopeTangent = Vector3.ProjectOnPlane(Vector3.down, frame.GroundHit.Normal).normalized;
            Vector3 slideVelocity = frame.TangentVelocity + slopeTangent * (slopeSlideAcceleration * Time.fixedDeltaTime);
            float downwardBias = frame.GroundHit.IsDynamicBody ? 0f : groundStickVelocity * Time.fixedDeltaTime;

            _rigidbody.linearVelocity = new Vector3(slideVelocity.x, Mathf.Min(frame.Velocity.y, -downwardBias), slideVelocity.z);
            CacheMovementVectors(frame.TangentVelocity, Vector3.zero, Vector3.zero, slideVelocity);
        }

        private void ApplyAirborne(MovementFrame frame)
        {
            if (!_rigidbody) return;

            Vector3 airHorizontal = new(frame.Velocity.x, 0f, frame.Velocity.z);
            if (!frame.HasInput)
            {
                CacheMovementVectors(airHorizontal, Vector3.zero, Vector3.zero, airHorizontal);
                return;
            }

            Vector3 airDesiredDir = Vector3.ProjectOnPlane(frame.DesiredDirectionWorld, Vector3.up);
            if (airDesiredDir.sqrMagnitude > InputDeadZone)
                airDesiredDir.Normalize();
            else
                airDesiredDir = frame.DesiredDirection;

            Vector3 desiredAirVelocity = airDesiredDir * (frame.TargetSpeed * frame.InputLocal.magnitude);

            if (airControl <= InputDeadZone || airDesiredDir == Vector3.zero)
            {
                CacheMovementVectors(airHorizontal, desiredAirVelocity, airDesiredDir, airHorizontal);
                return;
            }

            if (airHorizontal.sqrMagnitude <= InputDeadZone)
            {
                Vector3 newAirFromRest = Vector3.MoveTowards(
                    airHorizontal,
                    desiredAirVelocity,
                    frame.AccelMaxDelta * airControl);

                _rigidbody.linearVelocity = new Vector3(newAirFromRest.x, frame.Velocity.y, newAirFromRest.z);
                CacheMovementVectors(airHorizontal, desiredAirVelocity, airDesiredDir, newAirFromRest);
                return;
            }

            Vector3 newAirHorizontal = CalculateAirborneVelocity(frame, airHorizontal, airDesiredDir);
            _rigidbody.linearVelocity = new Vector3(newAirHorizontal.x, frame.Velocity.y, newAirHorizontal.z);
            CacheMovementVectors(airHorizontal, desiredAirVelocity, airDesiredDir, newAirHorizontal);
        }

        private Vector3 CalculateAirborneVelocity(MovementFrame frame, Vector3 airHorizontal, Vector3 airDesiredDir)
        {
            float airHorizontalSpeed = airHorizontal.magnitude;
            float airDot = airHorizontal.sqrMagnitude > InputDeadZone && airDesiredDir.sqrMagnitude > InputDeadZone
                ? Vector3.Dot(airHorizontal.normalized, airDesiredDir)
                : 1f;
            float maxDeltaVAir = (airDot < 0f ? frame.DecelMaxDelta : frame.AccelMaxDelta) * airControl * frame.SteerBoost;
            float maxAngleRad = airHorizontalSpeed > 0.25f ? maxDeltaVAir / airHorizontalSpeed : 999f;

            Vector3 newDirection = Vector3.RotateTowards(airHorizontal.normalized, airDesiredDir, maxAngleRad, 0f);
            float newSpeed = airHorizontalSpeed < frame.TargetSpeed
                ? Mathf.Min(frame.TargetSpeed, airHorizontalSpeed + frame.AccelMaxDelta * airControl)
                : airHorizontalSpeed;
            return newDirection * newSpeed;
        }

        private void CacheMovementVectors(
            Vector3 currentHorizontal,
            Vector3 desiredHorizontal,
            Vector3 inputDirection,
            Vector3 resultingHorizontal)
        {
            _currentHorizontalVelocity = currentHorizontal;
            _desiredHorizontalVelocity = desiredHorizontal;
            _inputDirectionWorld = inputDirection;
            _resultingHorizontalVelocity = resultingHorizontal;
            _lastAppliedVelocity = _rigidbody ? _rigidbody.linearVelocity : Vector3.zero;
        }

        private void TryStepUp(Vector3 moveDirection, GroundHit groundHit)
        {
            if (!_rigidbody || !_capsuleCollider || moveDirection.sqrMagnitude <= InputDeadZone || stepHeight <= 0f)
                return;

            Vector3 probeDirection = Vector3.ProjectOnPlane(moveDirection, Vector3.up);
            if (probeDirection.sqrMagnitude <= InputDeadZone)
                return;
            probeDirection.Normalize();

            GetCapsuleWorld(out _, out Vector3 point2, out float radius);

            float skin = 0.05f;
            float checkDistance = Mathf.Max(stepCheckDistance, radius + skin);
            float footClearance = Mathf.Max(skin, 0.02f);
            Vector3 lowerOrigin = point2 + transform.up * footClearance;
            Vector3 upperOrigin = lowerOrigin + Vector3.up * stepHeight;
            LayerMask mask = groundSensor ? groundSensor.DetectionFilter : Physics.DefaultRaycastLayers;

            bool lowerBlocked = Physics.SphereCast(lowerOrigin, radius * 0.9f, probeDirection, out _, checkDistance, mask, QueryTriggerInteraction.Ignore);
            if (!lowerBlocked)
                return;

            bool upperBlocked = Physics.SphereCast(upperOrigin, radius * 0.9f, probeDirection, out _, checkDistance, mask, QueryTriggerInteraction.Ignore);
            if (upperBlocked)
                return;

            Vector3 stepProbeOrigin = upperOrigin + probeDirection * checkDistance + Vector3.up * skin;
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
            float cylinder = Mathf.Max(0f, height * 0.5f - radius);
            Vector3 center = transform.TransformPoint(_capsuleCollider.center);
            Vector3 axis = transform.up;

            point1 = center + axis * cylinder;
            point2 = center - axis * cylinder;
        }

        private void AutoAssignMovementReference()
        {
            if (movementReference)
                return;

            movementReference = transform;
        }

        private void GetPlanarMovementBasis(out Vector3 right, out Vector3 forward)
        {
            Transform reference = movementReference ? movementReference : transform;

            forward = Vector3.ProjectOnPlane(reference.forward, Vector3.up);
            if (forward.sqrMagnitude <= InputDeadZone)
                forward = Vector3.ProjectOnPlane(reference.up, Vector3.up);
            if (forward.sqrMagnitude <= InputDeadZone)
                forward = transform.forward;

            forward.Normalize();
            right = Vector3.Cross(Vector3.up, forward);
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

        private void ApplyRigidbodyDefaults()
        {
            if (!_rigidbody) return;

            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            _baseMass = _rigidbody.mass;
        }

        private void UpdateDynamicGroundMass(bool isOnDynamicGround)
        {
            if (!_rigidbody)
                return;

            float targetMass = isOnDynamicGround
                ? Mathf.Max(0.01f, _baseMass * dynamicGroundMassMultiplier)
                : _baseMass;

            if (!Mathf.Approximately(_rigidbody.mass, targetMass))
                _rigidbody.mass = targetMass;
        }

        private void CacheDynamicSupportCollision(Collision collision)
        {
            if (!_rigidbody || collision.rigidbody == null || collision.rigidbody.isKinematic)
                return;

            int contactCount = collision.contactCount;
            for (int i = 0; i < contactCount; i++)
            {
                ContactPoint contact = collision.GetContact(i);
                if (Vector3.Dot(contact.normal, Vector3.up) > DynamicSupportContactUpDotThreshold)
                {
                    _hasDynamicSupportCollision = true;
                    return;
                }
            }
        }
        
        public enum MovementStateId
        {
            GroundedIdle,
            GroundedMove,
            Sliding,
            AirborneIdle,
            AirborneMove
        }

        private struct GroundHit
        {
            public Vector3 Point;
            public Vector3 Normal;
            public float SurfaceAngle;
            public Rigidbody? GroundBody;

            public bool IsDynamicBody => GroundBody != null && !GroundBody.isKinematic;
        }

        private struct MovementFrame
        {
            public Vector3 Velocity;
            public GroundHit GroundHit;
            public bool HasWalkableGround;
            public bool CanSlide;
            public bool HasInput;
            public Vector3 InputLocal;
            public Vector3 DesiredDirectionWorld;
            public Vector3 DesiredDirection;
            public Vector3 MovementPlaneNormal;
            public Vector3 TangentVelocity;
            public float TargetSpeed;
            public float AccelMaxDelta;
            public float DecelMaxDelta;
            public bool IsBraking;
            public float SteerBoost;
        }

        private struct MovementDeltas
        {
            public float AccelMaxDelta;
            public float DecelMaxDelta;
            public bool IsBraking;
            public float SteerBoost;
        }
    }
}
