namespace Konfus.Systems.AI
{
    /// <summary>
    /// Abstract agent class that simply takes input, the inheriting class decides how to deal with the input.
    /// </summary>
    public class Agent : Monobehavior, IAgent
    {
        abstract void OnInput(IAgentInput input);
    }
}