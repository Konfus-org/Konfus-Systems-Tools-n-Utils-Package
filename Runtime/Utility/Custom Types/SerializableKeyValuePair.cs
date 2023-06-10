using System;
using System.Collections.Generic;
using UnityEngine;

namespace Konfus.Utility.Custom_Types
{
    [Serializable]
    public class SerializableKeyValuePair<TKey, TValue>
    {
        [SerializeField]
        private TKey key;

        [SerializeField]
        private TValue value;

        public SerializableKeyValuePair(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
        }

        public TKey Key => key;

        public TValue Value => value;

        public override string ToString()
        {
            return ((KeyValuePair<TKey, TValue>)this).ToString();
        }

        public static implicit operator KeyValuePair<TKey, TValue>(SerializableKeyValuePair<TKey, TValue> reference)
        {
            return new KeyValuePair<TKey, TValue>(reference.key, reference.value);
        }
    }
}