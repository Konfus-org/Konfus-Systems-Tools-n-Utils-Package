using Konfus.Systems.AI;

namespace Konfus.Systems.Modular_Agents
{
    public interface IAgentInputModule : IAgentModule
    {
        bool OnInputFromAgent(IAgentInput input);
    }
}