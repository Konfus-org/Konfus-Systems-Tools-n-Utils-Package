using System.Collections.Generic;

namespace Konfus.Systems.Node_Graph
{
    public static class SerializedEdgeExtension
    {
        public static IList<SerializableEdge> GetNonRelayEdges(this IList<SerializableEdge> edges)
        {
            var nonrelayEdges = new List<SerializableEdge>();
            foreach (SerializableEdge edge in edges)
                if (edge.outputNode is RelayNode)
                {
                    var relay = edge.outputNode as RelayNode;
                    foreach (SerializableEdge relayEdge in relay.GetNonRelayEdges()) nonrelayEdges.Add(relayEdge);
                }
                else
                {
                    nonrelayEdges.Add(edge);
                }

            return nonrelayEdges;
        }
    }
}