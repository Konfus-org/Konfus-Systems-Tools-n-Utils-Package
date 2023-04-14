using System;
using UnityEngine;

namespace Konfus.Systems.Node_Graph
{
    [Serializable]
    public class SerializableEdge : ISerializationCallbackReceiver
    {
        [NonSerialized] public Node inputNode;

        [NonSerialized] public NodePort inputPort;

        [NonSerialized] public Node outputNode;

        [NonSerialized] public NodePort outputPort;

        //temporary object used to send port to port data when a custom input/output function is used.
        [NonSerialized] public object passThroughBuffer;

        [SerializeField] private PropertyName _guid;

        [SerializeField] private PropertyName inputNodeGUID;

        [SerializeField] private PropertyName outputNodeGUID;

        [SerializeField] private Graph owner;

        public string inputFieldName;
        public string outputFieldName;

        // Use to store the id of the field that generate multiple ports
        public string inputPortIdentifier;
        public string outputPortIdentifier;

        public PropertyName GUID
        {
            get
            {
                if (PropertyName.IsNullOrEmpty(_guid))
                    _guid = Guid.NewGuid().ToString();

                return _guid;
            }
        }

        public void OnAfterDeserialize()
        {
        }

        public void OnBeforeSerialize()
        {
            if (outputNode == null || inputNode == null)
                return;

            outputNodeGUID = outputNode.GUID;
            inputNodeGUID = inputNode.GUID;
        }

        public static SerializableEdge CreateNewEdge(Graph graph, NodePort inputPort, NodePort outputPort)
        {
            var edge = new SerializableEdge();

            edge.owner = graph;
            edge._guid = Guid.NewGuid().ToString();
            edge.inputNode = inputPort.owner;
            edge.inputFieldName = inputPort.fieldName;
            edge.outputNode = outputPort.owner;
            edge.outputFieldName = outputPort.fieldName;
            edge.inputPort = inputPort;
            edge.outputPort = outputPort;
            edge.inputPortIdentifier = inputPort.portData.Identifier;
            edge.outputPortIdentifier = outputPort.portData.Identifier;

            return edge;
        }

        public override string ToString()
        {
            return $"{outputNode.name}:{outputPort.fieldName} -> {inputNode.name}:{inputPort.fieldName}";
        }

        //here our owner have been deserialized
        public void Deserialize()
        {
            if (!owner.nodesPerGUID.ContainsKey(outputNodeGUID) || !owner.nodesPerGUID.ContainsKey(inputNodeGUID))
                return;

            outputNode = owner.nodesPerGUID[outputNodeGUID];
            inputNode = owner.nodesPerGUID[inputNodeGUID];
            inputPort = inputNode.GetPort(inputFieldName, inputPortIdentifier);
            outputPort = outputNode.GetPort(outputFieldName, outputPortIdentifier);
        }
    }
}