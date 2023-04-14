using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Node = Konfus.Systems.Node_Graph.Node;

namespace Konfus.Tools.NodeGraphEditor
{
    public class GroupView : Group
    {
        public GraphView owner;
        public Systems.Node_Graph.Group group;

        private Label titleLabel;
        private ColorField colorField;

        private readonly string groupStyle = "GraphProcessorStyles/GroupView";

        public GroupView()
        {
            styleSheets.Add(Resources.Load<StyleSheet>(groupStyle));
        }

        private static void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
        }

        public void Initialize(GraphView graphView, Systems.Node_Graph.Group block)
        {
            group = block;
            owner = graphView;

            title = block.title;
            SetPosition(block.position);

            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));

            headerContainer.Q<TextField>().RegisterCallback<ChangeEvent<string>>(TitleChangedCallback);
            titleLabel = headerContainer.Q<Label>();

            colorField = new ColorField {value = group.color, name = "headerColorPicker"};
            colorField.RegisterValueChangedCallback(e => { UpdateGroupColor(e.newValue); });
            UpdateGroupColor(group.color);

            headerContainer.Add(colorField);

            InitializeInnerNodes();
        }

        private void InitializeInnerNodes()
        {
            foreach (PropertyName nodeGUID in group.innerNodeGUIDs.ToList())
            {
                if (!owner.graph.nodesPerGUID.ContainsKey(nodeGUID))
                {
                    Debug.LogWarning("Node GUID not found: " + nodeGUID);
                    group.innerNodeGUIDs.Remove(nodeGUID);
                    continue;
                }

                Node node = owner.graph.nodesPerGUID[nodeGUID];
                NodeView nodeView = owner.nodeViewsPerNode[node];

                AddElement(nodeView);
            }
        }

        protected override void OnElementsAdded(IEnumerable<GraphElement> elements)
        {
            foreach (GraphElement element in elements)
            {
                var node = element as NodeView;

                // Adding an element that is not a node currently supported
                if (node == null)
                    continue;

                if (!group.innerNodeGUIDs.Contains(node.nodeTarget.GUID))
                    group.innerNodeGUIDs.Add(node.nodeTarget.GUID);
            }

            base.OnElementsAdded(elements);
        }

        protected override void OnElementsRemoved(IEnumerable<GraphElement> elements)
        {
            // Only remove the nodes when the group exists in the hierarchy
            if (parent != null)
                foreach (GraphElement elem in elements)
                    if (elem is NodeView nodeView)
                        group.innerNodeGUIDs.Remove(nodeView.nodeTarget.GUID);

            base.OnElementsRemoved(elements);
        }

        public void UpdateGroupColor(Color newColor)
        {
            group.color = newColor;
            style.backgroundColor = newColor;
        }

        private void TitleChangedCallback(ChangeEvent<string> e)
        {
            group.title = e.newValue;
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            group.position = newPos;
        }
    }
}