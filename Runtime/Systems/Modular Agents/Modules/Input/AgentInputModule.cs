using Konfus.Systems.AI;
using UnityEngine;

namespace Konfus.Systems.Modular_Agents
{
    public abstract class AgentInputModule<TInput> : MonoBehaviour, IAgentInputModule
    {
        public bool OnInputFromAgent(IAgentInput input)
        {
            if (input.GetType() != typeof(TInput)) return false;
            return ProcessInputFromAgent((TInput)input);
        }

        public abstract void Initialize(ModularAgent modularAgent);
        protected abstract bool ProcessInputFromAgent(TInput input);
    }
}