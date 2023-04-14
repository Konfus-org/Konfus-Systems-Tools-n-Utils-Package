using System;
using System.Collections.Generic;
using System.Reflection;
using Konfus.Systems.Node_Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Konfus.Tools.NodeGraphEditor
{
    public class PortView : Port
    {
        public const string VerticalClass = "Vertical";

        public string fieldName => fieldInfo.Name;
        public Type fieldType => fieldInfo.GetUnderlyingType();
        public MemberInfo MemberInfo => fieldInfo;
        public new Type portType;
        public NodeView owner { get; private set; }
        public PortData portData;

        public event Action<PortView, Edge> OnConnected;
        public event Action<PortView, Edge> OnDisconnected;

        protected MemberInfo fieldInfo;
        protected BaseEdgeConnectorListener listener;

        private readonly string userPortStyleFile = "PortViewTypes";

        private readonly List<EdgeView> edges = new();

        public int connectionCount => edges.Count;

        private readonly string portStyle = "GraphProcessorStyles/PortView";

        protected PortView(Direction direction, MemberInfo fieldInfo, PortData portData,
            BaseEdgeConnectorListener edgeConnectorListener)
            : base(portData.vertical ? Orientation.Vertical : Orientation.Horizontal, direction, Capacity.Multi,
                portData.DisplayType ?? fieldInfo.GetUnderlyingType())
        {
            this.fieldInfo = fieldInfo;
            listener = edgeConnectorListener;
            portType = portData.DisplayType ?? fieldInfo.GetUnderlyingType();
            this.portData = portData;
            portName = fieldName;

            styleSheets.Add(Resources.Load<StyleSheet>(portStyle));

            UpdatePortSize();

            var userPortStyle = Resources.Load<StyleSheet>(userPortStyleFile);
            if (userPortStyle != null)
                styleSheets.Add(userPortStyle);

            if (portData.vertical)
                AddToClassList(VerticalClass);

            tooltip = portData.tooltip;
        }

        public static PortView CreatePortView(Direction direction, MemberInfo fieldInfo, PortData portData,
            BaseEdgeConnectorListener edgeConnectorListener)
        {
            var pv = new PortView(direction, fieldInfo, portData, edgeConnectorListener);
            pv.m_EdgeConnector = new BaseEdgeConnector(edgeConnectorListener);
            pv.AddManipulator(pv.m_EdgeConnector);

            // Force picking in the port label to enlarge the edge creation zone
            VisualElement portLabel = pv.Q("type");
            if (portLabel != null)
            {
                portLabel.pickingMode = PickingMode.Position;
                portLabel.style.flexGrow = 1;
            }

            // hide label when the port is vertical
            if (portData.vertical && portLabel != null)
                portLabel.style.display = DisplayStyle.None;

            // Fixup picking mode for vertical top ports
            if (portData.vertical)
                pv.Q("connector").pickingMode = PickingMode.Position;

            return pv;
        }

        /// <summary>
        ///     Update the size of the port view (using the portData.sizeInPixel property)
        /// </summary>
        public void UpdatePortSize()
        {
            int size = portData.sizeInPixel == 0 ? 8 : portData.sizeInPixel;
            VisualElement connector = this.Q("connector");
            VisualElement cap = connector.Q("cap");
            connector.style.width = size;
            connector.style.height = size;
            cap.style.width = size - 4;
            cap.style.height = size - 4;

            // Update connected edge sizes:
            edges.ForEach(e => e.UpdateEdgeSize());
        }

        public virtual void Initialize(NodeView nodeView, string name)
        {
            owner = nodeView;
            AddToClassList(fieldName);

            // Correct port type if port accept multiple values (and so is a container)
            if (direction == Direction.Input && portData.acceptMultipleEdges &&
                portType == fieldType) // If the user haven't set a custom field type
                if (fieldType.GetGenericArguments().Length > 0)
                    portType = fieldType.GetGenericArguments()[0];

            if (name != null)
                portName = name;
            visualClass = "Port_" + portType.Name;
            tooltip = portData.tooltip;
        }

        public override void Connect(Edge edge)
        {
            OnConnected?.Invoke(this, edge);

            base.Connect(edge);

            NodeView inputNode = (edge.input as PortView).owner;
            NodeView outputNode = (edge.output as PortView).owner;

            edges.Add(edge as EdgeView);

            inputNode.OnPortConnected(edge.input as PortView);
            outputNode.OnPortConnected(edge.output as PortView);
        }

        public override void Disconnect(Edge edge)
        {
            OnDisconnected?.Invoke(this, edge);

            base.Disconnect(edge);

            if (!(edge as EdgeView).isConnected)
                return;

            NodeView inputNode = (edge.input as PortView)?.owner;
            NodeView outputNode = (edge.output as PortView)?.owner;

            inputNode?.OnPortDisconnected(edge.input as PortView);
            outputNode?.OnPortDisconnected(edge.output as PortView);

            edges.Remove(edge as EdgeView);
        }

        public void UpdatePortView(PortData data)
        {
            if (data.DisplayType != null)
            {
                base.portType = data.DisplayType;
                portType = data.DisplayType;
                visualClass = "Port_" + portType.Name;
            }

            if (!string.IsNullOrEmpty(data.displayName))
                portName = data.displayName;

            VisualElement portLabel = this.Q("type");
            if (portLabel != null)
            {
                if (data.vertical)
                {
                    portLabel.style.display = DisplayStyle.None;
                    AddToClassList(VerticalClass);

                    // Allows the port to pick up mouse events
                    this.Q("connector").pickingMode = PickingMode.Position;
                }
                else
                {
                    portLabel.style.display = DisplayStyle.Flex;
                    RemoveFromClassList(VerticalClass);

                    this.Q("connector").pickingMode = PickingMode.Ignore;
                }
            }

            portData = data;

            // Update the edge in case the port color have changed
            schedule.Execute(() =>
            {
                foreach (EdgeView edge in edges)
                {
                    edge.UpdateEdgeControl();
                    edge.MarkDirtyRepaint();
                }
            }).ExecuteLater(50); // Hummm

            UpdatePortSize();
        }

        public List<EdgeView> GetEdges()
        {
            return edges;
        }
    }
}