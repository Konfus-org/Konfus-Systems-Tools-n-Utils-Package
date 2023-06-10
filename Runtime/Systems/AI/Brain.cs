using UnityEngine;

namespace Konfus.Systems.AI
{
    /// <summary>
    /// Abstract class that houses one reference to an <see cref="Agent"/>.
    /// This class is meant to be inherited by a 'Controller' of sorts that will control
    /// the agent. The controlled agent can be changed at runtime.
    /// </summary>
    public abstract class Brain : MonoBehaviour
    {
        [SerializeField] 
        private Agent controlledAgent;
        public Agent ControlledAgent 
        { 
            get => controlledAgent;
            protected set => controlledAgent = value;
        }
    }
}