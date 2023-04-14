using System;
using System.Collections.Generic;

namespace Konfus.Systems.Node_Graph
{
    [Serializable]
    public class SubGraphIngressNode : SubGraphBoundaryNode
    {
        [Output] [CustomBehaviourOnly] private object _ingress;

        public override string name => "Ingress";
        protected override List<PortData> Ports => SubGraph.IngressPortData;

        public void PullIngress(Dictionary<PortData, object> ingress)
        {
            passThroughBufferByPort = ingress;
        }

        protected override void PostProcess()
        {
            passThroughBufferByPort.Clear();
        }

        [CustomPortBehavior(nameof(_ingress), true)]
        protected IEnumerable<PortData> CreatePorts(List<SerializableEdge> edges)
        {
            if (Ports == null) yield break;

            foreach (PortData portData in Ports) yield return portData;
        }

        [CustomPortOutput(nameof(_ingress), typeof(object))]
        protected void PushIngress(List<SerializableEdge> connectedEdges)
        {
            if (connectedEdges.Count == 0) return;

            bool keyExists = passThroughBufferByPort.ContainsKey(connectedEdges[0].outputPort.portData);
            object value = keyExists ? passThroughBufferByPort[connectedEdges[0].outputPort.portData] : default;
            foreach (SerializableEdge edge in connectedEdges) edge.passThroughBuffer = value;
        }
    }
}