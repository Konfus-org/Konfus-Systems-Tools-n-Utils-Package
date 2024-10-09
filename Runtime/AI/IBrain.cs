namespace Konfus.Systems.AI
{
    public interface IBrain
    {
        /// <summary>
        /// Houses one reference to an <see cref="Agent"/>.
        /// This class is meant to be implemented by a 'Controller' of sorts that will control
        /// the agent. The controlled agent can be changed at runtime.
        /// </summary>
        public IAgent ControlledAgent { get; }
    }
}