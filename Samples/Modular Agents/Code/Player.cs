using Konfus.Systems.AI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Konfus_Systems_Tools_n_Utils.Samples.Modular_Agents
{
    public class Player : Brain
    {
        private Vector2 _lastMoveInput = Vector2.zero;
        
        public void OnMove(InputAction.CallbackContext context)
        {
            _lastMoveInput = context.ReadValue<Vector2>();
            ControlledAgent.OnInput(new BasicLocomotionInput(_lastMoveInput, false));
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            ControlledAgent.OnInput(new BasicLocomotionInput(_lastMoveInput, context.performed));
        }
    }
}
