using Konfus.Systems.AI;
using UnityEngine;

namespace Konfus_Systems_Tools_n_Utils.Samples.Modular_Agents
{
    public readonly struct MovementInput : IAgentInput
    {
        private readonly Vector2 _value;

        public MovementInput(Vector2 value)
        {
            _value = value;
        }

        public Vector2 Value => _value;
    }
}