using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Konfus.Systems.Graph
{
    /// <summary>
    ///     The graph data model split into are minimalistic runtime part and editor specific extensions.
    ///     See everything under #if UNITY_EDITOR for all editor specific parts of this class.
    ///     The compilation tags allow us to strip away any unwated data for a runtime scenario.
    ///     Source: https://docs.unity3d.com/2022.2/Documentation/Manual/script-Serialization.html
    ///     Discussion: https://forum.unity.com/threads/serialize-fields-only-in-editor.433422/
    /// </summary>
    public class Graph : ScriptableObject
    {
        /// <summary>
        ///     List of all the nodes we want to work on.
        /// </summary>
        [SerializeReference] public List<Node> nodes = new();
        
#if UNITY_EDITOR
        // FROM HERE BE DRAGONS...
        [NonSerialized] public SerializedObject serializedGraphData;
        [NonSerialized] private SerializedProperty _nodesProperty;
        [NonSerialized] private SerializedProperty _utilityNodesProperty;

        [HideInInspector] [SerializeField] private string tmpNameProperty;

        [SerializeField] private bool viewportInitiallySet;
        public bool ViewportInitiallySet => viewportInitiallySet;
        private SerializedProperty _viewportInitiallySetProperty;

        private SerializedProperty ViewportInitiallySetProperty
        {
            get
            {
                if (_viewportInitiallySetProperty == null)
                    _viewportInitiallySetProperty = serializedGraphData.FindProperty(nameof(viewportInitiallySet));
                return _viewportInitiallySetProperty;
            }
        }

        [SerializeField] private Vector3 viewPosition;
        public Vector3 ViewPosition => viewPosition;
        [NonSerialized] private SerializedProperty _viewPositionProperty;

        [SerializeField] private Vector3 viewScale;
        [SerializeReference] public List<Node> utilityNodes = new();

        public Vector3 ViewScale => viewScale;
        [NonSerialized] private SerializedProperty _viewScaleProperty;

        private SerializedProperty ViewPositionProperty
        {
            get
            {
                if (_viewPositionProperty == null)
                    _viewPositionProperty = serializedGraphData.FindProperty(nameof(viewPosition));
                return _viewPositionProperty;
            }
        }

        private SerializedProperty ViewScaleProperty
        {
            get
            {
                if (_viewScaleProperty == null) _viewScaleProperty = serializedGraphData.FindProperty(nameof(viewScale));
                return _viewScaleProperty;
            }
        }

        public void SetViewport(Vector3 position, Vector3 scale)
        {
            if (position != viewPosition || scale != viewScale)
            {
                ViewportInitiallySetProperty.boolValue = true;
                ViewScaleProperty.vector3Value = scale;
                ViewPositionProperty.vector3Value = position;
                serializedGraphData.ApplyModifiedProperties();
            }
        }

        public Node AddNode(INode node, bool isUtilityNode)
        {
            var baseNodeItem = new Node(node);
            baseNodeItem.isUtilityNode = isUtilityNode;
            if (!isUtilityNode)
                nodes.Add(baseNodeItem);
            else
                utilityNodes.Add(baseNodeItem);
            ForceSerializationUpdate();
            return baseNodeItem;
        }

        public void AddNode(Node nodeItem)
        {
            if (!nodeItem.isUtilityNode)
                nodes.Add(nodeItem);
            else
                utilityNodes.Add(nodeItem);
        }

        public void RemoveNode(Node node)
        {
            if (!node.isUtilityNode)
                nodes.Remove(node);
            else
                utilityNodes.Remove(node);
        }

        public void RemoveNodes(List<Node> nodesToRemove)
        {
            if (nodesToRemove.Count > 0)
            {
                Undo.RecordObject(this, "Remove Nodes");
                foreach (Node node in nodesToRemove) RemoveNode(node);
            }
        }

        public void ForceSerializationUpdate()
        {
            serializedGraphData.Update();
            EditorUtility.SetDirty(this);
            serializedGraphData.ApplyModifiedProperties();
        }

        public void CreateSerializedObject()
        {
            serializedGraphData = new SerializedObject(this);
            _nodesProperty = null;
            _utilityNodesProperty = null;
            _viewPositionProperty = null;
            _viewScaleProperty = null;
            _viewportInitiallySetProperty = null;
        }

        public SerializedProperty GetNodesProperty(bool isUtilityNode)
        {
            if (!isUtilityNode)
            {
                if (_nodesProperty == null) _nodesProperty = serializedGraphData.FindProperty(nameof(nodes));
                return _nodesProperty;
            }

            if (_utilityNodesProperty == null)
                _utilityNodesProperty = serializedGraphData.FindProperty(nameof(utilityNodes));
            return _utilityNodesProperty;
        }

        public SerializedProperty GetLastAddedNodeProperty(bool isUtilityNode)
        {
            if (isUtilityNode) return GetNodesProperty(true).GetArrayElementAtIndex(utilityNodes.Count - 1);
            return GetNodesProperty(false).GetArrayElementAtIndex(nodes.Count - 1);
        }
#endif
    }
}