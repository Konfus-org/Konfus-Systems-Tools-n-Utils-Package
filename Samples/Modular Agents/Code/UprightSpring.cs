using Konfus.Systems.Modular_Agents;
using Konfus.Utility.Extensions;
using UnityEngine;

namespace Konfus_Systems_Tools_n_Utils.Samples.Modular_Agents
{
    public class UprightSpring : AgentInputModule<BasicLocomotionInput>, IAgentPhysicsModule
    {
        [SerializeField]
        private float uprightSpringStrength;
        [SerializeField]
        private float uprightSpringDamper;

        private Vector3 _targetLookDir;
        private Rigidbody _rb;

        public override void Initialize(ModularAgent modularAgent)
        {
            _rb = modularAgent.GetComponent<Rigidbody>();
        }

        protected override void ProcessInputFromAgent(BasicLocomotionInput input)
        {
            if (input.MoveInput == Vector2.zero) return;
            _targetLookDir = new Vector3(input.MoveInput.x, 0, input.MoveInput.y);
        }

        public void OnAgentFixedUpdate()
        {
            MaintainUpright();
        }

        /// <summary>
        /// Adds torque to the character to keep the character upright, acting as a torsional oscillator (i.e. vertically flipped pendulum).
        /// </summary>
        /// <param name="lookDir">The input look rotation.</param>
        private void MaintainUpright()
        {
            Vector3 forward = _rb.transform.forward;
            Quaternion uprightTargetRot = Quaternion.LookRotation(new Vector3(forward.x, 0, forward.z), Vector3.up);
            Quaternion currentRot = _rb.transform.rotation;
            Quaternion toGoal = uprightTargetRot.CalculateShortestRotationTo(currentRot);

            toGoal.ToAngleAxis(out float rotDegrees, out Vector3 rotAxis);
            rotAxis.Normalize();

            float rotRadians = rotDegrees * Mathf.Deg2Rad;
            _rb.AddTorque((rotAxis * (rotRadians * uprightSpringStrength)) - (_rb.angularVelocity * uprightSpringDamper));
        }
    }
}