using Konfus.Systems.Modular_Agents;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Konfus_Systems_Tools_n_Utils.Samples.Modular_Agents
{
    [RequireComponent(typeof(Rigidbody))] 
    //[RequireComponent(typeof(UprightSpring))]
    public class RigidbodyBasicLocomotion : AgentInputModule<MovementInput>, IAgentPhysicsModule
    {
        [Header("Rotation")]
        [SerializeField] 
        private RotateMode rotateMode;
        [SerializeField, DisableIf(nameof(rotateMode), RotateMode.Disabled)]
        private Transform cameraTransform;
        [SerializeField, DisableIf(nameof(rotateMode), RotateMode.Disabled)] 
        private float rotationSpeed = 5f;
        
        [Header("Leaning")] 
        [SerializeField] 
        private float maxLeanAngle = 45f;
        [SerializeField] 
        private float leanSpeed = 10f;

        [Header("Movement")]
        [SerializeField] 
        private MoveMode moveMode;
        [SerializeField]
        private AnimationCurve accelerationCurve;
        [SerializeField] 
        private float maxSpeed = 5f;
        [SerializeField] 
        private float acceleration = 10f;
        [SerializeField] 
        private float deceleration = 10f;

        private Rigidbody _rb;
        private Quaternion _initialRotation;

        private Vector2 _moveInput;
        private Vector3 _targetVelocity;
        private float _currentSpeed;

        private enum RotateMode
        {
            TowardMoveInput,
            Disabled
        }
        
        private enum MoveMode
        {
            RelativeToWorld,
            RelativeToCamera
        }

        public override void Initialize(ModularAgent modularAgent)
        {
            _rb = modularAgent.GetComponent<Rigidbody>();
            _initialRotation = _rb.transform.rotation;
        }

        public void OnAgentFixedUpdate()
        {
            Vector3 moveDir = CalculateMoveDirection(_moveInput);
            Move(moveDir);
            Rotate(moveDir);
        }

        protected override void ProcessInputFromAgent(MovementInput input)
        {
            _moveInput = input.Value;
        }

        private Vector3 CalculateMoveDirection(Vector2 input)
        {
            return moveMode == MoveMode.RelativeToCamera 
                ? AdjustInputToBeRelativeToCamera(input) 
                : new Vector3(input.x, 0, input.y);
        }

        /// <summary>
        /// Adjusts the input, so that the movement matches input regardless of camera rotation.
        /// </summary>
        /// <param name="moveInput">The player movement input.</param>
        /// <returns>The camera corrected movement input.</returns>
        private Vector3 AdjustInputToBeRelativeToCamera(Vector2 moveInput)
        {
            // Get movement input
            float horizontalInput = moveInput.x;
            float verticalInput = moveInput.y;
            
            // Calculate movement direction relative to the camera
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();
            
            Vector3 movementDirection = (cameraForward * verticalInput) + (cameraRight * horizontalInput);
            return movementDirection;
        } 
        
        /// <summary>
        /// Rotates based on rotation option.
        /// </summary>
        private void Rotate(Vector3 dir)
        {
            if (rotateMode != RotateMode.TowardMoveInput || dir == Vector3.zero) return;
            Quaternion toRotation = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }

        /// <summary>
        /// Apply forces to move the character up to a maximum acceleration, with consideration to acceleration graphs.
        /// </summary>
        private void Move(Vector3 dir)
        {
            // Calculate desired velocity based on input and max speed
            Vector3 desiredVelocity = dir * maxSpeed;

            // Calculate current velocity and speed
            Vector3 currentVelocity = _rb.velocity;
            float currentSpeed = currentVelocity.magnitude;

            // Calculate acceleration based on animation curve
            float normalizedSpeed = Mathf.Clamp01(currentSpeed / maxSpeed);
            float currentAcceleration = acceleration * accelerationCurve.Evaluate(normalizedSpeed);

            // Apply acceleration to reach the desired velocity
            Vector3 velocityChange = desiredVelocity - currentVelocity;
            velocityChange.y = 0f; // Ignore changes in the vertical direction
            _rb.AddForce(velocityChange.normalized * currentAcceleration, ForceMode.Acceleration);

            // Apply deceleration to gradually slow down when there is no input
            if (dir.magnitude < 0.01f)
            {
                Vector3 decelerationForce = -currentVelocity.normalized * deceleration;
                decelerationForce.y = 0f; // Ignore deceleration in the vertical direction
                _rb.AddForce(decelerationForce, ForceMode.Acceleration);
            }

            // Clamp the velocity to the maximum speed
            _rb.velocity = Vector3.ClampMagnitude(_rb.velocity, maxSpeed);
        }

        private void LeanInDirectionOfMovement(Vector3 dir)
        {
            Vector3 velocity = _rb.velocity;
            velocity.y = 0f; // Ignore vertical velocity

            if (velocity != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(velocity, Vector3.up);
                Quaternion leanRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, leanSpeed * Time.fixedDeltaTime);
            
                // Calculate lean angle based on rotation difference
                float leanAngle = Quaternion.Angle(_initialRotation, leanRotation);
                leanAngle = Mathf.Clamp(leanAngle, 0f, maxLeanAngle);
            
                // Apply lean rotation
                transform.rotation = Quaternion.Euler(0f, 0f, leanAngle);
            }
            else
            {
                // Reset to initial rotation if not moving
                transform.rotation = Quaternion.RotateTowards(transform.rotation, _initialRotation, leanSpeed * Time.fixedDeltaTime);
            }
        }
    }
}