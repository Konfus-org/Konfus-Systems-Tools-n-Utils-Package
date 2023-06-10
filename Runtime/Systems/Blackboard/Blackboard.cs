using System;
using System.Linq;
using Konfus.Systems.Blackboard.Variables;
using Konfus.Utility.Custom_Types;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Konfus.Systems.Blackboard
{
    [Serializable]
    public class Blackboard
    {
        [ShowInInspector] [DictionaryDrawerSettings(KeyLabel = "Var Name", ValueLabel = "Var Type", DisplayMode = DictionaryDisplayOptions.OneLine)]
        private SerializableDict<string, BlackboardVar> _entries = new SerializableDict<string, BlackboardVar>();
        
        public bool Contains(string key) => _entries.ContainsKey(key);
        
        public void Add(string key, BlackboardVar value)
        {
            _entries.Add(key, value);
        }
        
        public T Get<T>(string key)
        {
            if (!_entries.ContainsKey(key))
            {
                Debug.LogError($"ERROR on getting {key}. The blackboard does not contain the key {key}.");
                return default;
            }   
            return ((BlackboardVar<T>)_entries[key]).Value();
        }
        
        public BlackboardVar[] GetAll() => _entries.Values.ToArray();
        
        public void Set<T>(string key, T value)
        {
            if (!_entries.ContainsKey(key))
            {
                Debug.LogError($"ERROR on setting {key}. The blackboard does not contain the key {key}.");
                return;
            }
            ((BlackboardVar<T>)_entries[key]).Set(value);
        }
    }
}