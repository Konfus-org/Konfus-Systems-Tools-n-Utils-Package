using System;
using System.Linq;
using Konfus.Blackboard.Variables;
using Konfus.Utility.Custom_Types;
using UnityEngine;

namespace Konfus.Blackboard
{
    // TODO: editor script for blackboard
    [Serializable]
    public class Blackboard
    {
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