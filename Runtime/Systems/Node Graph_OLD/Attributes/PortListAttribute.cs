using System;

namespace Konfus.Systems.Graph.Attributes
{
    /// <summary>
    /// Attribute to flag an array or List field as an assignable port field in the graph
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class PortListAttribute : PortAttribute
    {
        public PortListAttribute() : base()
        {
        }
    }
}