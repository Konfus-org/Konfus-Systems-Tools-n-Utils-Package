using System;
using System.Collections.Generic;
using System.Linq;
using Konfus.Blackboard.Variables;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Konfus.Blackboard
{
    [Serializable]
    public class Blackboard
    {
        [ShowInInspector]
        [OdinSerialize]
        [DictionaryDrawerSettings(KeyLabel = "Name", ValueLabel = "Variable",
            DisplayMode = DictionaryDisplayOptions.OneLine)]
        private Dictionary<string, BlackboardVar> entries = new();

        public bool Contains(string key)
        {
            return entries.ContainsKey(key);
        }

        public void Add(string key, BlackboardVar value)
        {
            entries.Add(key, value);
        }

        public T? Get<T>(string key)
        {
            if (entries.TryGetValue(key, out BlackboardVar? entry)) return ((BlackboardVar<T>)entry).Value();
            Debug.LogError($"ERROR on getting {key}. The blackboard does not contain the key {key}.");
            return default;
        }

        public BlackboardVar[] GetAll()
        {
            return entries.Values.ToArray();
        }

        public void Set<T>(string key, T value)
        {
            if (!entries.TryGetValue(key, out BlackboardVar? entry))
            {
                Debug.LogError($"ERROR on setting {key}. The blackboard does not contain the key {key}.");
                return;
            }

            ((BlackboardVar<T>)entry).Set(value);
        }
    }
}