using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Konfus.Systems.Node_Graph
{
    [Serializable]
    [NodeMenuItem("Utils/Relay")]
    public class RelayNode : Node
    {
        private const int k_MaxPortSize = 14;
        private const string packIdentifier = "_Pack";

        private static List<(Type, string)> s_empty = new();

        [Input(name = "In")] public PackedRelayData input;

        [Output(name = "Out")] public PackedRelayData output;

        private SerializableType inputType = new(typeof(object));

        [NonSerialized] private int outputIndex;

        public bool unpackOutput;
        public bool packInput;
        public int inputEdgeCount;

        public override string layoutStyle => "GraphProcessorStyles/RelayNode";

        [CustomPortInput(nameof(input), typeof(object))]
        public void GetInputs(List<SerializableEdge> edges)
        {
            inputEdgeCount = edges.Count;

            // If the relay is only connected to another relay:
            if (edges.Count == 1 && edges.First().outputNode.GetType() == typeof(RelayNode))
            {
                if (edges.First().passThroughBuffer != null)
                    input = (PackedRelayData) edges.First().passThroughBuffer;
            }
            else
            {
                input.values = edges.Select(e => e.passThroughBuffer).ToList();
                input.names = edges.Select(e => e.outputPort.portData.displayName).ToList();
                input.types = edges.Select(e =>
                    e.outputPort.portData.DisplayType ?? e.outputPort.fieldInfo.GetUnderlyingType()).ToList();
            }
        }

        public List<SerializableEdge> GetNonRelayEdges()
        {
            List<SerializableEdge> inputEdges = inputPorts?[0]?.GetEdges();

            // Iterate until we don't have a relay node in input
            while (inputEdges.Count == 1 && inputEdges.First().outputNode.GetType() == typeof(RelayNode))
                inputEdges = inputEdges.First().outputNode.inputPorts[0]?.GetEdges();

            return inputEdges;
        }

        public List<(Type type, string name)> GetUnderlyingPortDataList()
        {
            // get input edges:
            if (inputPorts.Count == 0)
                return s_empty;

            List<SerializableEdge> inputEdges = GetNonRelayEdges();

            if (inputEdges != null)
                return inputEdges.Select(e => (
                    e.outputPort.portData.DisplayType ?? e.outputPort.fieldInfo.GetUnderlyingType(),
                    e.outputPort.portData.displayName)).ToList();

            return s_empty;
        }

        [CustomPortOutput(nameof(output), typeof(object))]
        public void PushOutputs(List<SerializableEdge> edges, NodePort outputPort)
        {
            if (inputPorts.Count == 0)
                return;

            List<SerializableEdge> inputPortEdges = inputPorts[0].GetEdges();

            if (outputPort.portData.Identifier != packIdentifier && outputIndex >= 0 &&
                (unpackOutput || inputPortEdges.Count == 1))
            {
                if (output.values == null)
                    return;

                // When we unpack the output, there is one port per type of data in output
                // That means that this function will be called the same number of time than the output port count
                // Thus we use a class field to keep the index.
                object data = output.values[outputIndex++];

                foreach (SerializableEdge edge in edges)
                {
                    var inputRelay = edge.inputNode as RelayNode;
                    edge.passThroughBuffer = inputRelay != null && !inputRelay.packInput ? output : data;
                }
            }
            else
            {
                foreach (SerializableEdge edge in edges)
                    edge.passThroughBuffer = output;
            }
        }

        protected override void Process()
        {
            outputIndex = 0;
            output = input;
        }

        [CustomPortBehavior(nameof(input))]
        private IEnumerable<PortData> InputPortBehavior(List<SerializableEdge> edges)
        {
            // When the node is initialized, the input ports is empty because it's this function that generate the ports
            int sizeInPixel = 0;
            if (inputPorts.Count != 0)
            {
                // Add the size of all input edges:
                List<SerializableEdge> inputEdges = inputPorts[0]?.GetEdges();
                sizeInPixel = inputEdges.Sum(e => Mathf.Max(0, e.outputPort.portData.sizeInPixel - 8));
            }

            if (edges.Count == 1 && !packInput)
                inputType.type = edges[0].outputPort.portData.DisplayType;
            else
                inputType.type = typeof(object);

            yield return new PortData
            {
                displayName = "",
                DisplayType = inputType.type,
                identifier = "0",
                acceptMultipleEdges = true,
                sizeInPixel = Mathf.Min(k_MaxPortSize, sizeInPixel + 8)
            };
        }

        [CustomPortBehavior(nameof(output))]
        private IEnumerable<PortData> OutputPortBehavior(List<SerializableEdge> edges)
        {
            if (inputPorts.Count == 0)
            {
                // Default dummy port to avoid having a relay without any output:
                yield return new PortData
                {
                    displayName = "",
                    DisplayType = typeof(object),
                    identifier = "0",
                    acceptMultipleEdges = true
                };
                yield break;
            }

            List<SerializableEdge> inputPortEdges = inputPorts[0].GetEdges();
            List<(Type type, string name)> underlyingPortData = GetUnderlyingPortDataList();
            if (unpackOutput && inputPortEdges.Count == 1)
            {
                yield return new PortData
                {
                    displayName = "Pack",
                    identifier = packIdentifier,
                    DisplayType = inputType.type,
                    acceptMultipleEdges = true,
                    sizeInPixel = Mathf.Min(k_MaxPortSize, Mathf.Max(underlyingPortData.Count, 1) + 7) // TODO: function
                };

                // We still keep the packed data as output when unpacking just in case we want to continue the relay after unpacking
                for (int i = 0; i < underlyingPortData.Count; i++)
                    yield return new PortData
                    {
                        displayName = underlyingPortData?[i].name ?? "",
                        DisplayType = underlyingPortData?[i].type ?? typeof(object),
                        identifier = i.ToString(),
                        acceptMultipleEdges = true,
                        sizeInPixel = 0
                    };
            }
            else
            {
                yield return new PortData
                {
                    displayName = "",
                    DisplayType = inputType.type,
                    identifier = "0",
                    acceptMultipleEdges = true,
                    sizeInPixel = Mathf.Min(k_MaxPortSize, Mathf.Max(underlyingPortData.Count, 1) + 7)
                };
            }
        }

        [HideInInspector]
        public struct PackedRelayData
        {
            public List<object> values;
            public List<string> names;
            public List<Type> types;
        }
    }
}