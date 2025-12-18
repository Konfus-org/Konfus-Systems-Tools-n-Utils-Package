namespace Konfus.AI
{
    /// <summary>
    /// Agent interface that enforces an agent takes input, the class implementing this decides how to deal with the input.
    /// </summary>
    public interface IAgent
    {
        /// <summary>
        /// Attempts to handle input.
        /// </summary>
        /// <param name="input">The input to be handled</param>
        /// <returns>Whether or not the input was handled</returns>
        bool OnInput(IAgentInput input);
    }
}