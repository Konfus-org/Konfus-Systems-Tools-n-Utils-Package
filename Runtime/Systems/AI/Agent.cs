using UnityEngine;

namespace Konfus.Systems.AI
{
    /// <summary>
    /// Abstract agent class that simply takes input, the inheriting class decides how to deal with the input.
    /// </summary>
    public abstract class Agent : MonoBehaviour
    {
        public abstract void OnInput(IAgentInput input);
    }
}