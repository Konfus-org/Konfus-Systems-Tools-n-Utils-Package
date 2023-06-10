using System;
using Konfus.Systems.AI;
using UnityEngine;

namespace Konfus.Systems.Modular_Agents
{
    public abstract class AgentInputModule<TInput> : MonoBehaviour, IAgentInputModule
    {
        public Type AssociatedInputType => typeof(TInput);

        public void OnInput(IAgentInput input)
        {
            if (input.GetType() != typeof(TInput)) return;
            ProcessInput((TInput)input);
        }

        public abstract void Initialize(ModularAgent modularAgent);
        protected abstract void ProcessInput(TInput input);
    }
}