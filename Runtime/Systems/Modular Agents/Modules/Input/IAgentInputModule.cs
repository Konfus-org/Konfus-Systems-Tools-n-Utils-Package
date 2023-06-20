using Konfus.Systems.AI;

namespace Konfus.Systems.Modular_Agents
{
    public interface IAgentInputModule : IAgentModule
    {
        void OnInputFromAgent(IAgentInput input);
    }
}