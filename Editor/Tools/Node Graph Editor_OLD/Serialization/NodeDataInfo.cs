using System.Collections.Generic;
using Konfus.Systems.Graph;

namespace Konfus.Tools.Graph_Editor.Editor.Serialization
{
    /// <summary>
    /// this helper class holds the original node data as well as a list of "external" references
    /// external in this case means outside of the captured selection.
    /// </summary>
    public class NodeDataInfo
    {
        public List<NodeReference> externalReferences = new();
        public List<NodeReference> internalReferences = new();
        public Node baseNodeItem;
    }
}