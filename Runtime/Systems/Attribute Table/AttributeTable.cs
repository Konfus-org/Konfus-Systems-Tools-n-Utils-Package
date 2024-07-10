using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Konfus.Systems.Attribute_Table
{
    public class AttributeTable : MonoBehaviour
    {
        [SerializeField]
        private AttributeTableEntry[] entries;
        
        private Dictionary<string, Attribute> _attributeTable;

        [CanBeNull]
        public T GetAttribute<T>(string key) where T : Attribute
        {
            return _attributeTable.TryGetValue(key, out Attribute attribute) ? (T) attribute : null;
        }
        
        [CanBeNull]
        public Attribute GetAttribute(string key)
        {
            return _attributeTable.TryGetValue(key, out Attribute attribute) ? attribute : null;
        }

        private void Start()
        {
            _attributeTable = new Dictionary<string, Attribute>();
            foreach (AttributeTableEntry entry in entries)
            {
                _attributeTable.Add(entry.Name, entry.Attribute);
            }
        }
    }
}