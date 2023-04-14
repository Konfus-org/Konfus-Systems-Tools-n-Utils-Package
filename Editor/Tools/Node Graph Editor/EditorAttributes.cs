using System;

namespace Konfus.Tools.NodeGraphEditor
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeCustomEditor : Attribute
    {
        public Type nodeType;

        public NodeCustomEditor(Type nodeType)
        {
            this.nodeType = nodeType;
        }
    }
}