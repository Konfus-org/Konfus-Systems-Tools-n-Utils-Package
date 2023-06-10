using System;
using Konfus.Systems.AI;

namespace Konfus.Systems.Modular_Agents
{
    public interface IAgentInputModule : IAgentModule
    {
        Type AssociatedInputType { get; }
        void OnInput(IAgentInput input);
    }
}