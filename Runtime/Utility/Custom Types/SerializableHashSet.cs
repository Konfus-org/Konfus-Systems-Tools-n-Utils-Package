using System;
using System.Collections.Generic;
using UnityEngine;

namespace Konfus.Utility.Custom_Types
{
    [Serializable]
    public class SerializableHashSet<T> : HashSet<T>, ISerializationCallbackReceiver
    {
        [SerializeField]
        T[] keys;

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (keys == null) return;
            Clear();
            
            foreach (var key in keys)
            {
                Add(key);
            }

            keys = null;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            keys = new T[Count];

            int i = 0;
            foreach (var value in this)
            {
                keys[i] = value;
                ++i;
            }
        }
    }
}