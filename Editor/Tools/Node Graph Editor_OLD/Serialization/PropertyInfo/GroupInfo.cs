using System.Collections.Generic;
using Konfus.Systems.Graph.Attributes;

namespace Konfus.Tools.Graph_Editor.Editor.Serialization.PropertyInfo
{
    public class GroupInfo : GraphPropertyInfo
    {
        public string groupName;
        public List<PropertyInfo> graphProperties = new();
        public string[] levels = new string[] { };
        public bool hasEmbeddedManagedReference = false;

        public GroupInfo(string groupName, string relativePropertyPath, GraphDisplayAttribute graphDisplayAttribute) :
            base(relativePropertyPath, graphDisplayAttribute)
        {
            this.groupName = groupName;
            levels = relativePropertyPath.Split('.');
        }
    }
}