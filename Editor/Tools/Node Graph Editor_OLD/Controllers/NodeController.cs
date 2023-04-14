using System;
using System.Collections.Generic;
using Konfus.Systems.Graph;
using Konfus.Tools.Graph_Editor.Editor.Serialization;
using Konfus.Tools.Graph_Editor.Editor.Serialization.PropertyInfo;
using Konfus.Tools.Graph_Editor.Views;
using Konfus.Tools.Graph_Editor.Views.Elements;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Konfus.Tools.Graph_Editor.Editor.Controllers
{
    public class NodeController
    {
        public GraphController graphController;
        public NodeView nodeView;
        public Node nodeItem;
        private Vector2 startPosition = Vector2.zero;
        private PropertyBag propertyBag;
        private SerializedObject serializedObject;
        private SerializedProperty nodeDataProperty;

        public Vector2 GetViewScale()
        {
            return graphController.GetViewScale();
        }

        public void ForEachNode(Action<BaseNode> callback)
        {
            graphController.ForEachNode(callback);
        }

        public NodeController(Node nodeItem, GraphController graphController)
        {
            this.graphController = graphController;
            this.nodeItem = nodeItem;
            nodeDataProperty = nodeItem.GetSpecificSerializedProperty();
            propertyBag = PropertyBag.GetCachedOrCreate(nodeItem.nodeAttribute, nodeItem.nodeType, nodeDataProperty);
            serializedObject = graphController.graphData.serializedGraphData;
            nodeView = new NodeView(this, nodeItem.nodeAttribute.color);

            string name = nodeItem.GetName();
            if (string.IsNullOrEmpty(name)) nodeItem.SetName(nodeItem.nodeAttribute.GetName(nodeItem.nodeType));
        }

        public NodeController(Node node, GraphController graphController, Vector2 startPosition) : this(node,
            graphController)
        {
            this.startPosition = startPosition;
        }

        public SerializedObject GetSerializedObject()
        {
            return serializedObject;
        }

        public void Initialize()
        {
            var customEditor = NodeEditor.CreateEditor(nodeItem.nodeType);
            if (customEditor != null)
            {
                nodeView.shouldSetBackgroundColor = customEditor.ShouldSetBackgroundColor();
                nodeView.InitializeView();
                customEditor.Initialize(this);
            }
            else
            {
                nodeView.InitializeView();
            }

            //initialized = true;
        }

        public void SetPosition(float xMin, float yMin)
        {
            nodeItem.SetPosition(xMin, yMin);
        }

        public Vector2 GetStartPosition()
        {
            if (startPosition == Vector2.zero) return nodeItem.GetPosition();
            return startPosition;
        }

        public void DoForEachPropertyOrGroup(VisualElement[] parents,
            Func<GroupInfo, VisualElement[], SerializedProperty, VisualElement[]> groupCreation,
            Action<VisualElement[], GraphPropertyInfo, SerializedProperty> propCreation)
        {
            DoForEachPropertyOrGroupRecursive(parents, propertyBag.graphPropertiesAndGroups, groupCreation,
                propCreation);
        }

        private void DoForEachPropertyOrGroupRecursive(VisualElement[] parents,
            List<PropertyInfo> propertiesIncludingGroups,
            Func<GroupInfo, VisualElement[], SerializedProperty, VisualElement[]> groupCreation,
            Action<VisualElement[], GraphPropertyInfo, SerializedProperty> propCreation)
        {
            foreach (PropertyInfo groupOrProperty in propertiesIncludingGroups)
                if (groupOrProperty.GetType() == typeof(GroupInfo))
                {
                    var groupInfo = (GroupInfo) groupOrProperty;
                    if (groupInfo.graphProperties.Count > 0)
                    {
                        VisualElement[] groupParents = groupCreation(groupInfo, parents,
                            nodeDataProperty.FindPropertyRelative(groupOrProperty.relativePropertyPath));
                        DoForEachPropertyOrGroupRecursive(groupParents, groupInfo.graphProperties, groupCreation,
                            propCreation);
                    }
                }
                else
                {
                    propCreation(parents, (GraphPropertyInfo) groupOrProperty,
                        nodeDataProperty.FindPropertyRelative(groupOrProperty.relativePropertyPath));
                }
        }

        public void DoForEachPortPropertyBase(ref List<PortInfo> portList, Action<PortInfo, SerializedProperty> action)
        {
            foreach (PortInfo info in portList)
                action(info, nodeDataProperty.FindPropertyRelative(info.relativePropertyPath));
        }

        public void DoForEachPortProperty(Action<PortInfo, SerializedProperty> action)
        {
            DoForEachPortPropertyBase(ref propertyBag.ports, action);
        }

        public void DoForEachPortListProperty(Action<PortInfo, SerializedProperty> action)
        {
            DoForEachPortPropertyBase(ref propertyBag.portLists, action);
        }

        public void DoForInputPortProperty(Action<PortInfo, SerializedProperty> action)
        {
            if (propertyBag.inputPort != null) action(propertyBag.inputPort, nodeDataProperty);
        }

        public void DoForNameProperty(Action<SerializedProperty> action)
        {
            action(nodeItem.GetNameSerializedProperty());
        }
    }
}