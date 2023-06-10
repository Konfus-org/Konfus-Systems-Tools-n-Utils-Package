using System;
using System.Collections.Generic;
using UnityEngine;

namespace Konfus.Utility.Custom_Types
{
    [Serializable]
    public class SerializableDict<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] 
        private SerializableKeyValuePair<TKey, TValue>[] pairs;

        public SerializableDict() : base() { }
        public SerializableDict(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }
        public SerializableDict(IEqualityComparer<TKey> comparer) : base(comparer) { }
        public SerializableDict(int capacity) : base(capacity) { }
        public SerializableDict(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary, comparer) { }
        public SerializableDict(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer) { }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (pairs == null) return;
            Clear();
            
            foreach (SerializableKeyValuePair<TKey, TValue> pair in pairs)
            {
                if (!ContainsKey(pair.Key))
                {
                    this[pair.Key] = pair.Value;
                }
            }
            
            pairs = null;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            pairs = new SerializableKeyValuePair<TKey, TValue>[Count];

            int pairIndex = 0;
            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                pairs[pairIndex++] = new SerializableKeyValuePair<TKey, TValue>(pair.Key, pair.Value);
            }
        }
    }
}