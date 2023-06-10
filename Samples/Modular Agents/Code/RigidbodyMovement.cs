using Konfus.Systems.Modular_Agents;
using UnityEngine;

namespace Konfus_Systems_Tools_n_Utils.Samples.Modular_Agents
{
    [RequireComponent(typeof(Rigidbody))] 
    public class RigidbodyMovement : AgentInputModule<MovementInput>, IAgentPhysicsModule
    {
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

        private readonly float _speedFactor = 1f;
        private readonly float _maxAccelForceFactor = 1f;
        
        private Rigidbody _rb;
        private Transform _transform;

        private Vector3 _moveInput;
        private Vector3 _targetVelocity = Vector3.zero;
        
        [SerializeField] 
        private RotateMode rotateMode;
        [SerializeField] 
        private MoveMode moveMode;
        [SerializeField] 
        private float maxSpeed = 8f;
        [SerializeField] 
        private float acceleration = 200f;
        [SerializeField] 
        private float maxAccelForce = 150f;
        [SerializeField] 
        private float leanFactor = 0.25f;
        [SerializeField] 
        private AnimationCurve accelerationFactorFromDot;
        [SerializeField] 
        private AnimationCurve maxAccelerationForceFactorFromDot;
        [SerializeField] 
        private Vector3 moveForceScale = new Vector3(1f, 0f, 1f);

        public override void Initialize(ModularAgent modularAgent)
        {
            _rb = modularAgent.GetComponent<Rigidbody>();
            _transform = modularAgent.GetComponent<Transform>();
        }

        protected override void ProcessInput(MovementInput input)
        {
            Vector2 moveInput2d = input.Value;
            var moveInput = new Vector3(moveInput2d.x, 0, moveInput2d.y);
            _moveInput = moveMode == MoveMode.RelativeToCamera ? AdjustInputToBeRelativeToCamera(moveInput) : moveInput;
        }
        
        public void OnAgentFixedUpdate()
        {
            Move();
            Rotate();
        }

        /// <summary>
        /// Adjusts the input, so that the movement matches input regardless of camera rotation.
        /// </summary>
        /// <param name="moveInput">The player movement input.</param>
        /// <returns>The camera corrected movement input.</returns>
        private Vector3 AdjustInputToBeRelativeToCamera(Vector3 moveInput)
        {
            float facing = Camera.main.transform.eulerAngles.y;
            return Quaternion.Euler(0, facing, 0) * moveInput;
        }
        
        /// <summary>
        /// Rotates based on rotation option.
        /// </summary>
        private void Rotate()
        {
            if (rotateMode == RotateMode.Disabled || _moveInput == Vector3.zero)
            {
                return;
            }

            Vector3 rotateInput = _moveInput;

            // Get target rotation
            Quaternion targetRot = Quaternion.LookRotation(
                new Vector3(rotateInput.x, 0, rotateInput.y),
                _transform.up
            );
                
            // Rotate towards rotation input
            _transform.rotation =
                Quaternion.RotateTowards(_transform.rotation, targetRot,
                    maxSpeed * Time.fixedDeltaTime * 100);
        }

        /// <summary>
        /// Apply forces to move the character up to a maximum acceleration, with consideration to acceleration graphs.
        /// </summary>
        private void Move()
        {
            Vector3 moveInput = _moveInput;
            
            // Calculate target velocity
            Vector3 unitVel = _targetVelocity.normalized;
            float velDot = Vector3.Dot(moveInput, unitVel);
            float accel = acceleration * accelerationFactorFromDot.Evaluate(velDot);
            Vector3 goalVel = moveInput * (maxSpeed * _speedFactor);
            _targetVelocity = Vector3.MoveTowards(_targetVelocity, goalVel, accel * Time.fixedDeltaTime);
            
            // Calculate the acceleration needed to reach the target velocity
            Vector3 neededAccel = (_targetVelocity - _rb.velocity) / Time.fixedDeltaTime;
            float maxAccel = maxAccelForce * maxAccelerationForceFactorFromDot.Evaluate(velDot) * _maxAccelForceFactor;
            neededAccel = Vector3.ClampMagnitude(neededAccel, maxAccel);
            
            // Add force to rigidbody
            // Using AddForceAtPosition in order to both move the player and cause the play to lean in the direction of input.
            _rb.AddForceAtPosition(
                force: Vector3.Scale(neededAccel * _rb.mass, moveForceScale),
                position: _transform.position + new Vector3(0f, _transform.localScale.y * leanFactor, 0f)); 
        }
    }
}