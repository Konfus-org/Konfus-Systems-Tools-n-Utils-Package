using UnityEngine;

namespace Konfus.Systems.AI
{
    /// <summary>
    /// Abstract agent class that simply takes input, the inheriting class decides how to deal with the input.
    /// </summary>
    public abstract class Agent : MonoBehaviour, IAgent
    {
        /// <summary>
        /// Takes an input to process.
        /// </summary>
        /// <param name="input">The input to be handled</param>
        public abstract void OnInput(IAgentInput input);
    }
}