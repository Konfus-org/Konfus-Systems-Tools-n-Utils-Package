using System;
using Konfus.Systems.Graph.Enums;

namespace Konfus.Systems.Graph.Attributes
{
    /// <summary>
    /// Port Attribute attribute, this needs to be added to every SerializeReference field that should show up in the graph as a assignable/ referenceable node.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class PortAttribute : PortBaseAttribute
    {
        /// <summary>
        /// Port Attribute, this needs to be added to every SerializeReference field that should show up in the graph as a assignable node.
        /// </summary>
        public PortAttribute(string name = null) : base(name, Capacity.Single, PortDirection.Output)
        {
        }
    }
}