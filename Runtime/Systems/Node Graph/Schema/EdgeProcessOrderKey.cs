using System;
using UnityEngine;

namespace Konfus.Systems.Node_Graph.Schema
{
    [Serializable]
    public class EdgeProcessOrderKey : IEquatable<string>, IEquatable<EdgeProcessOrderKey>
    {
        public const string ValueFieldName = nameof(_value);

        [SerializeField] [HideInInspector] private string _value;

        public EdgeProcessOrderKey(string key)
        {
            _value = key;
        }

        public string Value => _value;

        public bool Equals(EdgeProcessOrderKey other)
        {
            return Value == other.Value;
        }

        public bool Equals(string other)
        {
            return string.Equals(_value, other);
        }

        public static bool operator ==(EdgeProcessOrderKey lhs, EdgeProcessOrderKey rhs)
        {
            return lhs.Equals(rhs);
        }

        public static implicit operator string(EdgeProcessOrderKey edgeProcessOrderKey)
        {
            return edgeProcessOrderKey.Value;
        }

        public static implicit operator EdgeProcessOrderKey(string key)
        {
            return new(key);
        }

        public static bool operator !=(EdgeProcessOrderKey lhs, EdgeProcessOrderKey rhs)
        {
            return !lhs.Equals(rhs);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}