using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Konfus.Tools.NodeGraphEditor
{
    /// <summary>
    ///     Custom editor of the node inspector, you can inherit from this class to customize your node inspector.
    /// </summary>
    [CustomEditor(typeof(NodeInspectorObject))]
    public class NodeInspectorObjectEditor : Editor
    {
        protected VisualElement placeholder;
        protected VisualElement root;
        protected VisualElement selectedNodeList;
        private NodeInspectorObject inspector;

        public override VisualElement CreateInspectorGUI()
        {
            return root;
        }

        protected virtual void OnDisable()
        {
            inspector.nodeSelectionUpdated -= UpdateNodeInspectorList;
        }

        protected virtual void OnEnable()
        {
            inspector = target as NodeInspectorObject;
            inspector.nodeSelectionUpdated += UpdateNodeInspectorList;
            root = new VisualElement();
            selectedNodeList = new VisualElement();
            selectedNodeList.styleSheets.Add(Resources.Load<StyleSheet>("GraphProcessorStyles/InspectorView"));
            root.Add(selectedNodeList);
            placeholder = new Label("Select a node to show it's settings in the inspector");
            placeholder.AddToClassList("PlaceHolder");
            UpdateNodeInspectorList();
        }

        protected virtual void UpdateNodeInspectorList()
        {
            selectedNodeList.Clear();

            if (inspector.selectedNodes.Count == 0)
                selectedNodeList.Add(placeholder);

            foreach (NodeView nodeView in inspector.selectedNodes)
                selectedNodeList.Add(CreateNodeBlock(nodeView));
        }

        protected VisualElement CreateNodeBlock(NodeView nodeView)
        {
            var view = new VisualElement();

            view.Add(new Label(nodeView.nodeTarget.name));

            VisualElement tmp = nodeView.controlsContainer;
            nodeView.controlsContainer = view;
            nodeView.Enable(true);
            nodeView.controlsContainer.AddToClassList("NodeControls");
            VisualElement block = nodeView.controlsContainer;
            nodeView.controlsContainer = tmp;

            return block;
        }
    }

    /// <summary>
    ///     Node inspector object, you can inherit from this class to customize your node inspector.
    /// </summary>
    public class NodeInspectorObject : ScriptableObject
    {
        /// <summary>Previously selected object by the inspector</summary>
        public Object previouslySelectedObject;

        /// <summary>Triggered when the selection is updated</summary>
        public event Action nodeSelectionUpdated;

        /// <summary>List of currently selected nodes</summary>
        public HashSet<NodeView> selectedNodes { get; private set; } = new();

        public virtual void NodeViewRemoved(NodeView view)
        {
            selectedNodes.Remove(view);
            nodeSelectionUpdated?.Invoke();
        }

        public virtual void RefreshNodes()
        {
            nodeSelectionUpdated?.Invoke();
        }

        /// <summary>Updates the selection from the graph</summary>
        public virtual void UpdateSelectedNodes(HashSet<NodeView> views)
        {
            selectedNodes = views;
            nodeSelectionUpdated?.Invoke();
        }
    }
}