using System;
using UnityEngine;

namespace Konfus.Attribute_Table
{
    [Serializable]
    public class AttributeTableEntry
    {
        [SerializeField]
        private string name;
        [SerializeField]
        private Attribute attribute;
        
        public string Name => name;
        public Attribute Attribute => attribute;
    }
}