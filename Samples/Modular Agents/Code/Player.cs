using Konfus.Systems.AI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Konfus_Systems_Tools_n_Utils.Samples.Modular_Agents
{
    public class Player : Brain
    {
        public void OnInput(InputAction.CallbackContext context)
        {
            // TODO: deal with other inputs...
            ControlledAgent.OnInput(new MovementInput(context.ReadValue<Vector2>()));
        }
    }
}
