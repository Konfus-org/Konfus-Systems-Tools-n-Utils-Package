using Konfus.Systems.Modular_Agents;
using Konfus.Systems.Sensor_Toolkit;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Konfus_Systems_Tools_n_Utils.Samples.Modular_Agents
{
    [RequireComponent(typeof(Rigidbody))] 
    [RequireComponent(typeof(UprightSpring))]
    public class RigidbodyBasicLocomotion : AgentInputModule<BasicLocomotionInput>, IAgentPhysicsModule, IAgentUpdateModule
    {
        [Header("Rotation")]
        [SerializeField] 
        private RotateMode rotateMode;
        [SerializeField, DisableIf(nameof(rotateMode), RotateMode.Disabled)] 
        private float rotationSpeed = 5f;
        
        [Header("Leaning")] 
        [SerializeField] 
        private float leanAmount = 1f;

        [Header("Movement")]
        [SerializeField] 
        private MoveMode moveMode;
        [SerializeField, EnableIf(nameof(moveMode), MoveMode.RelativeToCamera)]
        private Transform cameraTransform;
        [SerializeField]
        private AnimationCurve accelerationCurve;
        [SerializeField] 
        private float maxSpeed = 5f;
        [SerializeField] 
        private float acceleration = 10f;
        [SerializeField] 
        private float deceleration = 10f;
        
        [Header("Jumping")]
        [SerializeField] 
        private float jumpForce = 5f;
        [SerializeField] 
        private float coyoteTime = 0.1f;
        [SerializeField]
        private float jumpTime = 0.5f;
        [SerializeField]
        private AnimationCurve jumpHeightCurve;

        [Header("Sensors")] 
        [SerializeField]
        private ScanSensor groundSensor;
        
        private float _currentSpeed;
        private float _jumpAirTime;
        private float _coyoteTimer;
        
        private bool _isJumping;
        private bool _jumpInput;
        
        private Vector2 _moveInput;
        private Vector3 _moveDir;
        private Vector3 _targetVelocity;
        private Rigidbody _rb;
        
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
        }

        public void OnAgentUpdate()
        {
            // Update move direction based off move input
            _moveDir = CalculateMoveDirection(_moveInput);

            // Run jump logic
            bool canJump = CanJump();
            if (canJump) StartJump();
            else if (groundSensor.isTriggered) StopJump();
        }

        public void OnAgentFixedUpdate()
        {
            Move(_moveDir);
            Rotate(_moveDir);
            if (_isJumping) PerformJump();
        }

        protected override void ProcessInputFromAgent(BasicLocomotionInput input)
        {
            _moveInput = input.MoveInput;
            _jumpInput = input.JumpInput;
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

            // Apply deceleration to gradually slow down when there is no input
            if (dir.magnitude < 0.01f)
            {
                Vector3 decelerationForce = -currentVelocity.normalized * deceleration;
                decelerationForce.y = 0f; // Ignore deceleration in the vertical direction
                // Using AddForceAtPosition in order to both move the player and cause the play to lean in the direction of input.
                _rb.AddForce(decelerationForce, ForceMode.Acceleration);
                /*_rb.AddForceAtPosition(
                    decelerationForce, 
                    transform.position + new Vector3(0f, transform.localScale.y * leanAmount, 0f)
                ); */
            }
            // Apply acceleration with input
            else
            {
                // Using AddForceAtPosition in order to both move the player and cause the play to lean in the direction of input.
                _rb.AddTorque(new Vector3(-dir.y, 0, -dir.x) * leanAmount);
                _rb.AddForce(velocityChange.normalized * currentAcceleration, ForceMode.Acceleration);
                /*_rb.AddForceAtPosition(
                    velocityChange.normalized * currentAcceleration, 
                    transform.position + new Vector3(0f, transform.localScale.y * leanAmount, 0f)
                ); */
            }

            // Clamp the velocity to the maximum speed
            _rb.velocity = Vector3.ClampMagnitude(_rb.velocity, maxSpeed);
        }
        
        private void StartJump()
        {
            _isJumping = true;
            _rb.AddForce(Vector3.up * jumpForce + _moveDir/2, ForceMode.VelocityChange);
        }

        private void PerformJump()
        {
            float normalizedTime = _jumpAirTime / jumpTime;
            float jumpHeightMultiplier = jumpHeightCurve.Evaluate(normalizedTime);
            float jumpForceApplied = jumpForce * jumpHeightMultiplier;
            
            _jumpAirTime += Time.fixedDeltaTime;
                
            _rb.AddForce(Vector3.up * jumpForceApplied + _moveDir/2, ForceMode.Acceleration);
        }

        private void StopJump()
        {
            _isJumping = false;
            _jumpAirTime = 0f;
            _coyoteTimer = coyoteTime;
        }

        private bool CanJump()
        {
            groundSensor.Scan();
            return !_isJumping && _jumpInput && _jumpAirTime < jumpTime && (groundSensor.isTriggered || _coyoteTimer > 0f);
        }
    }
}