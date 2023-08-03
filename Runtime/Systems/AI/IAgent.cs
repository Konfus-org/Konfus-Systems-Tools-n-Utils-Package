namespace Konfus.Systems.AI
{
    /// <summary>
    /// Agent interface that enforces an agent takes input, the class implementing this decides how to deal with the input.
    /// </summary>
    public interface IAgent
    {
        void OnInput(IAgentInput input);
    }
}