using System;
using Konfus.Utility.Custom_Types;
using UnityEngine;

namespace Konfus.Systems.AttribTable
{
    public class AttributeTable : MonoBehaviour
    {
        [SerializeField]
        private SerializableDict<Type, ActorAttribute> attributes;

        public T GetAttribute<T>(Type key) where T : ActorAttribute
        {
            return attributes.TryGetValue(key, out ActorAttribute attribute) ? (T) attribute : null;
        }
        
        public ActorAttribute GetAttribute(Type key)
        {
            return attributes.TryGetValue(key, out ActorAttribute attribute) ? attribute : null;
        }
    }
}