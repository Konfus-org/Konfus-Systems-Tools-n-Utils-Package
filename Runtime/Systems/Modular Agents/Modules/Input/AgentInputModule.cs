using Konfus.Systems.AI;
using UnityEngine;

namespace Konfus.Systems.Modular_Agents
{
    public abstract class AgentInputModule<TInput> : MonoBehaviour, IAgentInputModule
    {
        public void OnInputFromAgent(IAgentInput input)
        {
            if (input.GetType() != typeof(TInput)) return;
            ProcessInputFromAgent((TInput)input);
        }

        public abstract void Initialize(ModularAgent modularAgent);
        protected abstract void ProcessInputFromAgent(TInput input);
    }
}