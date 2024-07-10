using System.Collections.Generic;
using JetBrains.Annotations;
using Konfus.Systems.AI;

namespace Konfus.Systems.Modular_Agents
{
    public class ModularAgent : Agent
    {
        private IAgentInputModule[] _inputModules;
        private IAgentModule[] _modules;

        public override bool OnInput(IAgentInput input)
        {
            foreach (IAgentInputModule inputModule in _inputModules)
            {
                if (inputModule.OnInputFromAgent(input)) return true;
            }

            return false;
        }

        public bool TryGetModule<T>([CanBeNull] out T module) where T : class, IAgentModule
        {
            foreach (IAgentModule moduleOnAgent in _modules)
            {
                if (moduleOnAgent is not T t) continue;
                module = t;
                return true;
            }
            
            foreach (IAgentInputModule moduleOnAgent in _inputModules)
            {
                if (moduleOnAgent is not T t) continue;
                module = t;
                return true;
            }
            
            module = null;
            return false;
        }

        private void Start()
        {
            var inputModules = new List<IAgentInputModule>();
            var modules = new List<IAgentModule>();
            
            IAgentModule[] modulesOnAgent = GetComponents<IAgentModule>();
            foreach (IAgentModule module in modulesOnAgent)
            {
                module.Initialize(this);
                if (module is IAgentInputModule inputModule)
                    inputModules.Add(inputModule);
                else
                    modules.Add(module);
            }
            
            _inputModules = inputModules.ToArray();
            _modules = modules.ToArray();
        }
    }
}