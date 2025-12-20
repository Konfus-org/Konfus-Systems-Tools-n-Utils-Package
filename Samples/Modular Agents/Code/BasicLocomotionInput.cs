using Konfus.Systems.AI;
using UnityEngine;

namespace Konfus_Systems_Tools_n_Utils.Samples.Modular_Agents
{
    public readonly struct BasicLocomotionInput : IAgentInput
    {
        public BasicLocomotionInput(Vector2 moveInput, bool jumpInput)
        {
            MoveInput = moveInput;
            JumpInput = jumpInput;
        }

        public Vector2 MoveInput { get; }
        public bool JumpInput { get; }
    }
}