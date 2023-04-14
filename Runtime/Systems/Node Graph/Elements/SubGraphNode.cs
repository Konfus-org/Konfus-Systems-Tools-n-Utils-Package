using System;
using System.Collections.Generic;
using Konfus.Systems.Node_Graph.Schema;
using UnityEngine;

namespace Konfus.Systems.Node_Graph
{
    [Serializable]
    [NodeMenuItem("Subgraph")]
    public class SubGraphNode : Node
    {
        public const string EgressPortsField = nameof(_egress);
        public const string IngressPortsField = nameof(_ingress);
        public const string SubGraphField = nameof(subGraph);

        [Output] [CustomBehaviourOnly] protected object _egress;

        [Input] [CustomBehaviourOnly] protected object _ingress;

        protected Dictionary<PortData, object> _passThroughBufferByPort = new();

        [SerializeField] protected SubGraph subGraph;

        public override Color color => new(1, 0, 1, 0.5f);
        public override bool HideNodeInspectorBlock => true;

        public override string name
        {
            get
            {
                if (!SubGraph)
                    return "SubGraphNode";

                if (string.IsNullOrWhiteSpace(SubGraph.Options.DisplayName))
                    return SubGraph.name;

                return SubGraph.Options.DisplayName;
            }
        }

        public override bool needsInspector => true;

        public SubGraph SubGraph => subGraph;

        protected override NodeRenamePolicy DefaultRenamePolicy =>
            SubGraph?.Options.RenamePolicy ?? NodeRenamePolicy.DISABLED;

        protected List<PortData> IngressPortData => SubGraph?.IngressPortData ?? new List<PortData>();
        protected List<PortData> EgressPortData => SubGraph?.EgressPortData ?? new List<PortData>();

        public override void InitializePorts()
        {
            base.InitializePorts();

            _passThroughBufferByPort?.Clear();
            SubGraph?.AddUpdatePortsListener(OnPortsListUpdated);
            SubGraph?.AddOptionsListener(OnSubGraphOptionsChanged);
        }

        protected override void Process()
        {
            base.Process();

            var processor = new ProcessSubGraphProcessor(SubGraph);
            processor.Run(_passThroughBufferByPort);
        }

        [CustomPortBehavior(nameof(_egress), true)]
        protected IEnumerable<PortData> CreateEgressPorts(List<SerializableEdge> edges)
        {
            if (EgressPortData == null) yield break;

            foreach (PortData output in EgressPortData)
            {
                if (string.IsNullOrEmpty(output.Identifier))
                    output.identifier = output.displayName;

                yield return output;
            }
        }

        [CustomPortBehavior(nameof(_ingress), true)]
        protected IEnumerable<PortData> CreateIngressPorts(List<SerializableEdge> edges)
        {
            if (IngressPortData == null) yield break;

            foreach (PortData input in IngressPortData)
            {
                if (string.IsNullOrEmpty(input.Identifier))
                    input.identifier = input.displayName;

                yield return input;
            }
        }

        [CustomPortInput(nameof(_ingress), typeof(object))]
        protected void PullIngress(List<SerializableEdge> connectedEdges)
        {
            if (connectedEdges.Count == 0) return;

            PortData portData = IngressPortData.Find(x => x.Equals(connectedEdges[0].inputPort.portData));
            _passThroughBufferByPort[portData] = connectedEdges[0].passThroughBuffer;
        }

        [CustomPortInput(nameof(_egress), typeof(object))]
        protected void PushEgress(List<SerializableEdge> connectedEdges)
        {
            if (connectedEdges.Count == 0) return;

            PortData portData = EgressPortData.Find(x => x.Equals(connectedEdges[0].outputPort.portData));
            Dictionary<PortData, object> returnedData = SubGraph.EgressNode.PushEgress();

            foreach (SerializableEdge edge in connectedEdges)
                if (returnedData.ContainsKey(portData))
                    edge.passThroughBuffer = returnedData[portData];
        }

        private void OnPortsListUpdated()
        {
            UpdateAllPortsLocal();
        }

        private void OnSubGraphOptionsChanged()
        {
            RepaintTitle();
        }
    }
}