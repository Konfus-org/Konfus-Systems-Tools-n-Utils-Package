using UnityEngine;

namespace Konfus.AI
{
    /// <summary>
    /// Generic implementation of the IBrain class.
    /// This class is meant to be inherited by a 'Controller' of sorts that will control
    /// the agent. The controlled agent can be changed at runtime.
    /// </summary>
    public abstract class Brain : MonoBehaviour, IBrain
    {
        [SerializeField]
        private Agent controlledAgent;
        public IAgent ControlledAgent => controlledAgent;
    }
}