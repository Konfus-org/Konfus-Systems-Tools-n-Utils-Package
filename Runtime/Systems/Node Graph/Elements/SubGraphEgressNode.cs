using System;
using System.Collections.Generic;

namespace Konfus.Systems.Node_Graph
{
    [Serializable]
    public class SubGraphEgressNode : SubGraphBoundaryNode
    {
        [Input] [CustomBehaviourOnly] private object _egress;

        public override string name => "Egress";
        protected override List<PortData> Ports => SubGraph.EgressPortData;

        public Dictionary<PortData, object> PushEgress()
        {
            return passThroughBufferByPort;
        }

        protected override void PreProcess()
        {
            passThroughBufferByPort.Clear();
        }

        [CustomPortBehavior(nameof(_egress), true)]
        protected IEnumerable<PortData> CreatePorts(List<SerializableEdge> edges)
        {
            if (Ports == null) yield break;

            foreach (PortData portData in Ports) yield return portData;
        }

        [CustomPortInput(nameof(_egress), typeof(object))]
        protected void PullEgressPorts(List<SerializableEdge> connectedEdges)
        {
            if (connectedEdges.Count == 0) return;

            passThroughBufferByPort.Add(connectedEdges[0].inputPort.portData, connectedEdges[0].passThroughBuffer);
        }
    }
}