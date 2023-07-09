using System.Collections.Generic;
using Konfus.Systems.AI;

namespace Konfus.Systems.Modular_Agents
{
    public class ModularAgent : Agent
    {
        private IAgentInputModule[] _inputModules;
        private IAgentUpdateModule[] _updateModules;
        private IAgentPhysicsModule[] _physicsModules;

        public override void OnInput(IAgentInput input)
        {
            foreach (IAgentInputModule inputModule in _inputModules)
            {
                inputModule.OnInputFromAgent(input);
            }
        }

        private void Start()
        {
            var inputModules = new List<IAgentInputModule>();
            var updateModules = new List<IAgentUpdateModule>();
            var physicsModules = new List<IAgentPhysicsModule>();
            
            IAgentModule[] modules = GetComponents<IAgentModule>();
            foreach (IAgentModule module in modules)
            {
                module.Initialize(this);
                if (module is IAgentInputModule inputModule)
                    inputModules.Add(inputModule);
                if (module is IAgentUpdateModule updateModule)
                    updateModules.Add(updateModule);
                if (module is IAgentPhysicsModule physicsModule) 
                    physicsModules.Add(physicsModule);
            }
            
            _inputModules = inputModules.ToArray();
            _updateModules = updateModules.ToArray();
            _physicsModules = physicsModules.ToArray();
        }

        private void Update()
        {
            foreach (IAgentUpdateModule module in _updateModules)
            {
                module.OnAgentUpdate();
            }
        }

        private void FixedUpdate()
        {
            foreach (IAgentPhysicsModule module in _physicsModules)
            {
                module.OnAgentFixedUpdate();
            }
        }
    }
}