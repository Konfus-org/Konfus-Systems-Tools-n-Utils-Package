using System;
using System.Collections.Generic;
using Konfus.Systems.Graph.Attributes;
using Konfus.Systems.Graph.Enums;
using UnityEditor;

namespace Konfus.Tools.Graph_Editor.Editor.Serialization.PropertyInfo
{
    public class PortInfo : PropertyInfo
    {
        public PortBaseAttribute portDisplay;
        public Type fieldType;
        public string fieldName = null;
        public List<Type> connectableTypes = new();

        public PortInfo(string relativePropertyPath, Type fieldType, PortBaseAttribute portDisplay,
            string portName = null) : base(relativePropertyPath)
        {
            this.portDisplay = portDisplay;
            this.fieldType = fieldType;
            fieldName = portName;

            if (portDisplay.connectionPolicy == ConnectionPolicy.IdenticalOrSubclass)
            {
                TypeCache.TypeCollection typeCollection = TypeCache.GetTypesDerivedFrom(fieldType);
                foreach (Type type in typeCollection) connectableTypes.Add(type);
            }

            if (!connectableTypes.Contains(fieldType)) connectableTypes.Add(fieldType);
        }
    }
}