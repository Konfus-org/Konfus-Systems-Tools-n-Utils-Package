using UnityEngine;
using Konfus.Sensor_Toolkit;
using UnityEngine.InputSystem;
using SensorHit = Konfus.Sensor_Toolkit.Sensor.Hit;

namespace Konfus.Vehicles
{
    public enum VehicleDriveMode
    {
        CarLike,
        TankLike
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public class VehicleMovement : MonoBehaviour
    {
        private const float InputDeadZone = 0.001f;

        [Header("Drive")]
        [SerializeField]
        private VehicleDriveMode driveMode = VehicleDriveMode.TankLike;
        [SerializeField]
        [Min(0f)]
        private float maxForwardSpeed = 10f;
        [SerializeField]
        [Min(0f)]
        private float maxReverseSpeed = 6f;
        [SerializeField]
        [Min(0f)]
        private float acceleration = 10f;
        [SerializeField]
        [Min(0f)]
        private float braking = 14f;
        [SerializeField]
        [Min(0f)]
        private float coasting = 6f;
        [SerializeField]
        [Min(0f)]
        private float movingTurnSpeed = 90f;
        [SerializeField]
        [Min(0f)]
        private float pivotTurnSpeed = 120f;
        [SerializeField]
        [Min(0f)]
        private float reverseTurnMultiplier = 0.75f;
        [SerializeField]
        [Min(0f)]
        private float sprintSpeedMultiplier = 1.35f;
        [SerializeField]
        [Min(0f)]
        private float inputResponsiveness = 4f;

        [Header("Stability")]
        [SerializeField]
        private Rigidbody? body;
        [SerializeField]
        private ScanSensor? groundSensor;
        [SerializeField]
        [Tooltip("Fallback normal used when no ground sensor hit is available.")]
        private Vector3 defaultGroundNormal = Vector3.up;
        [SerializeField]
        [Min(0f)]
        private float gravity = 30f;
        [SerializeField]
        [Min(0f)]
        private float terminalFallSpeed = 30f;
        [SerializeField]
        [Min(0f)]
        private float groundSnapSpeed = 20f;
        [SerializeField]
        [Min(0f)]
        private float groundAlignmentSharpness = 10f;
        [SerializeField]
        [Range(0f, 1f)]
        private float airborneSteeringMultiplier = 0.35f;
        [SerializeField]
        private bool lockPitchAndRoll = true;
        [SerializeField]
        private bool overrideCenterOfMass = true;
        [SerializeField]
        private Vector3 centerOfMass = new(0f, -1.25f, 0f);

        private Vector2 _moveInput;
        private Vector2 _smoothedMoveInput;
        private bool _sprinting;
        private bool _isGrounded;
        private Vector3 _groundNormal = Vector3.up;
        private Vector3 _planarVelocity;
        private Vector3 _desiredPlanarVelocity;
        private float _currentForwardSpeed;
        private float _targetSpeed;
        private float _currentTurnSpeed;
        private float _currentYawDelta;
        private float _verticalSpeed;
        private float _targetGroundProbeDistance;
        private bool _hasTargetGroundProbeDistance;

        public VehicleDriveMode DriveMode
        {
            get => driveMode;
            set => driveMode = value;
        }

        public Rigidbody? Body => body;
        public ScanSensor? GroundSensor => groundSensor;
        public Vector2 MoveInput => _moveInput;
        public Vector2 SmoothedMoveInput => _smoothedMoveInput;
        public bool IsSprinting => _sprinting;
        public bool IsGrounded => _isGrounded;
        public Vector3 GroundNormal => _groundNormal;
        public Vector3 PlanarVelocity => _planarVelocity;
        public Vector3 DesiredPlanarVelocity => _desiredPlanarVelocity;
        public Vector3 KinematicVelocity => _planarVelocity + Vector3.up * _verticalSpeed;
        public float CurrentForwardSpeed => _currentForwardSpeed;
        public float TargetSpeed => _targetSpeed;
        public float CurrentTurnSpeed => _currentTurnSpeed;
        public float CurrentYawDelta => _currentYawDelta;

        private void Awake()
        {
            body ??= GetComponent<Rigidbody>();
            groundSensor ??= GetComponentInChildren<ScanSensor>();

            if (body == null)
            {
                return;
            }

            ConfigureRigidbody();
            CacheGroundProbeDistance();
        }

        private void ConfigureRigidbody()
        {
            if (body == null)
            {
                return;
            }

            body.isKinematic = true;
            body.useGravity = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            if (overrideCenterOfMass)
            {
                body.centerOfMass = centerOfMass;
            }

            if (lockPitchAndRoll)
            {
                body.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }
        }

        private void CacheGroundProbeDistance()
        {
            if (!TryGetGroundHit(out _, out _, out float hitDistance))
            {
                if (groundSensor)
                {
                    _targetGroundProbeDistance = groundSensor.SensorLength;
                    _hasTargetGroundProbeDistance = true;
                }

                return;
            }

            _targetGroundProbeDistance = hitDistance;
            _hasTargetGroundProbeDistance = true;
        }

        private void FixedUpdate()
        {
            if (body == null)
            {
                return;
            }

            _smoothedMoveInput = Vector2.MoveTowards(
                _smoothedMoveInput,
                _moveInput,
                inputResponsiveness * Time.fixedDeltaTime);

            Vector3 groundNormal = Vector3.up;
            Vector3 groundPoint = body.position;
            bool isGrounded = TryGetGroundHit(out groundNormal, out groundPoint, out _);
            _isGrounded = isGrounded;
            _groundNormal = groundNormal;

            Vector3 nextPlanarVelocity = CalculateDriveVelocity(groundNormal);
            body.MovePosition(body.position + CalculateKinematicDisplacement(nextPlanarVelocity, groundNormal, groundPoint, isGrounded));
            ApplyRotation(groundNormal, isGrounded);
        }

        public void Move(Vector2 input)
        {
            _moveInput = Vector2.ClampMagnitude(input, 1f);
        }

        public void StartSprint()
        {
            _sprinting = true;
        }

        public void StopSprint()
        {
            _sprinting = false;
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

        private Vector3 CalculateDriveVelocity(Vector3 groundNormal)
        {
            if (body == null)
            {
                return Vector3.zero;
            }

            Vector3 planarVelocity = Vector3.ProjectOnPlane(_planarVelocity, groundNormal);
            float throttle = Mathf.Abs(_moveInput.y) > InputDeadZone ? _smoothedMoveInput.y : 0f;
            float speedMultiplier = _sprinting ? sprintSpeedMultiplier : 1f;
            float targetSpeed = throttle >= 0f
                ? throttle * maxForwardSpeed * speedMultiplier
                : throttle * maxReverseSpeed;

            Vector3 forwardOnPlane = Vector3.ProjectOnPlane(transform.forward, groundNormal);
            if (forwardOnPlane.sqrMagnitude <= InputDeadZone)
            {
                forwardOnPlane = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            }

            forwardOnPlane = forwardOnPlane.sqrMagnitude > InputDeadZone
                ? forwardOnPlane.normalized
                : transform.forward;

            Vector3 desiredPlanarVelocity = forwardOnPlane * targetSpeed;
            float currentForwardSpeed = Vector3.Dot(planarVelocity, forwardOnPlane);

            float rate = Mathf.Abs(throttle) <= InputDeadZone
                ? coasting
                : Mathf.Sign(currentForwardSpeed) != Mathf.Sign(targetSpeed) && Mathf.Abs(currentForwardSpeed) > InputDeadZone
                    ? braking
                    : acceleration;

            Vector3 nextPlanarVelocity = Vector3.MoveTowards(
                planarVelocity,
                desiredPlanarVelocity,
                rate * Time.fixedDeltaTime);

            _desiredPlanarVelocity = desiredPlanarVelocity;
            _currentForwardSpeed = currentForwardSpeed;
            _targetSpeed = targetSpeed;
            _planarVelocity = nextPlanarVelocity;

            return nextPlanarVelocity;
        }

        private Vector3 CalculateKinematicDisplacement(
            Vector3 planarVelocity,
            Vector3 groundNormal,
            Vector3 groundPoint,
            bool isGrounded)
        {
            Vector3 displacement = planarVelocity * Time.fixedDeltaTime;

            if (isGrounded)
            {
                _verticalSpeed = 0f;
                displacement += CalculateGroundSnapCorrection(groundNormal, groundPoint);
                return displacement;
            }

            _verticalSpeed = Mathf.MoveTowards(_verticalSpeed, -terminalFallSpeed, gravity * Time.fixedDeltaTime);
            displacement += Vector3.up * (_verticalSpeed * Time.fixedDeltaTime);
            return displacement;
        }

        private Vector3 CalculateGroundSnapCorrection(Vector3 groundNormal, Vector3 groundPoint)
        {
            if (!groundSensor || !_hasTargetGroundProbeDistance || groundSnapSpeed <= 0f)
            {
                return Vector3.zero;
            }

            Vector3 sensorForward = groundSensor.transform.forward.normalized;
            if (sensorForward.sqrMagnitude <= InputDeadZone)
            {
                sensorForward = -groundNormal;
            }

            Vector3 targetSensorPosition = groundPoint - sensorForward * _targetGroundProbeDistance;
            Vector3 correction = Vector3.Project(targetSensorPosition - groundSensor.transform.position, groundNormal);
            return Vector3.ClampMagnitude(correction, groundSnapSpeed * Time.fixedDeltaTime);
        }

        private void ApplyRotation(Vector3 groundNormal, bool isGrounded)
        {
            if (body == null)
            {
                return;
            }

            float steer = Mathf.Abs(_moveInput.x) > InputDeadZone ? _smoothedMoveInput.x : 0f;
            float throttle = Mathf.Abs(_moveInput.y) > InputDeadZone ? _smoothedMoveInput.y : 0f;

            float turnSpeed = driveMode == VehicleDriveMode.TankLike
                ? Mathf.Abs(throttle) > 0.05f ? movingTurnSpeed : pivotTurnSpeed
                : Mathf.Abs(throttle) > 0.05f ? movingTurnSpeed : movingTurnSpeed * 0.35f;

            if (driveMode == VehicleDriveMode.CarLike && throttle < -0.05f)
            {
                turnSpeed *= reverseTurnMultiplier;
                steer *= -1f;
            }

            float steeringAuthority = isGrounded ? 1f : airborneSteeringMultiplier;
            float yawDelta = steer * turnSpeed * steeringAuthority * Time.fixedDeltaTime;
            _currentTurnSpeed = turnSpeed * steeringAuthority;
            _currentYawDelta = yawDelta;

            Vector3 rotatedForward = Quaternion.AngleAxis(yawDelta, groundNormal) * transform.forward;
            Vector3 forwardOnPlane = Vector3.ProjectOnPlane(rotatedForward, groundNormal);

            if (forwardOnPlane.sqrMagnitude <= InputDeadZone)
            {
                forwardOnPlane = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            }

            if (forwardOnPlane.sqrMagnitude <= InputDeadZone)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(forwardOnPlane.normalized, groundNormal);
            float lerpFactor = 1f - Mathf.Exp(-groundAlignmentSharpness * Time.fixedDeltaTime);
            body.MoveRotation(Quaternion.Slerp(body.rotation, targetRotation, lerpFactor));
        }

        private bool TryGetGroundHit(out Vector3 groundNormal, out Vector3 groundPoint, out float hitDistance)
        {
            if (!groundSensor || !groundSensor.Scan() || groundSensor.Hits == null)
            {
                groundNormal = defaultGroundNormal.normalized;
                groundPoint = body != null ? body.position : transform.position;
                hitDistance = 0f;
                return false;
            }

            bool found = false;
            float bestUpDot = float.NegativeInfinity;
            Vector3 bestNormal = defaultGroundNormal;
            Vector3 bestPoint = body != null ? body.position : transform.position;
            float bestDistance = 0f;
            Vector3 sensorOrigin = groundSensor.transform.position;
            Vector3 sensorForward = groundSensor.transform.forward.normalized;

            foreach (SensorHit hit in groundSensor.Hits)
            {
                if (!hit.GameObject || hit.GameObject == gameObject || hit.GameObject.transform.IsChildOf(transform))
                {
                    continue;
                }

                Vector3 candidateNormal = hit.Normal.normalized;
                float upDot = Vector3.Dot(candidateNormal, Vector3.up);
                if (upDot <= 0f || upDot <= bestUpDot)
                {
                    continue;
                }

                bestUpDot = upDot;
                bestNormal = candidateNormal;
                bestPoint = hit.Point;
                bestDistance = Vector3.Dot(hit.Point - sensorOrigin, sensorForward);
                if (bestDistance < 0f)
                {
                    bestDistance = Vector3.Distance(sensorOrigin, hit.Point);
                }

                found = true;
            }

            groundNormal = bestNormal.normalized;
            groundPoint = bestPoint;
            hitDistance = bestDistance;
            return found;
        }
    }
}
