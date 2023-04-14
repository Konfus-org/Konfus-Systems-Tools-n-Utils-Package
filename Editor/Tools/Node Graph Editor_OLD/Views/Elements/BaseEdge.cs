// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using Konfus.Tools.Graph_Editor.Editor.Utils;
using Konfus.Tools.Graph_Editor.Editor.Enums;
using Konfus.Tools.Graph_Editor.Editor.Interfaces;
using Konfus.Tools.Graph_Editor.Editor.Manipulators;
using UnityEngine;
using UnityEngine.UIElements;

namespace Konfus.Tools.Graph_Editor.Views.Elements
{
    public abstract class BaseEdge : GraphElement
    {
        protected BasePort m_InputPort;
        private Vector2 m_InputPortPosition;
        protected bool m_InputPositionOverridden;
        protected Vector2 m_InputPositionOverride;

        protected BasePort m_OutputPort;
        private Vector2 m_OutputPortPosition;
        protected bool m_OutputPositionOverridden;
        protected Vector2 m_OutputPositionOverride;

        #region Constructor

        public BaseEdge()
        {
            Capabilities
                |= Capabilities.Selectable
                   | Capabilities.Deletable
                   | Capabilities.Movable;
        }

        #endregion

        public BasePort GetInputPort()
        {
            return m_InputPort;
        }

        public BasePort GetOutputPort()
        {
            return m_OutputPort;
        }

        #region Properties

        public Vector2 From
        {
            get
            {
                if (m_OutputPositionOverridden) return m_OutputPositionOverride;
                if (Output != null && BaseGraph != null) return m_OutputPortPosition;
                return Vector2.zero;
            }
        }

        public Vector2 To
        {
            get
            {
                if (m_InputPositionOverridden) return m_InputPositionOverride;
                if (Input != null && BaseGraph != null) return m_InputPortPosition;
                return Vector2.zero;
            }
        }

        public Orientation InputOrientation => Input?.Orientation ?? (Output?.Orientation ?? Orientation.Horizontal);
        public Orientation OutputOrientation => Output?.Orientation ?? (Input?.Orientation ?? Orientation.Horizontal);

        public BasePort Output
        {
            get => m_OutputPort;
            set => SetPort(ref m_OutputPort, value);
        }

        public BasePort Input
        {
            get => m_InputPort;
            set => SetPort(ref m_InputPort, value);
        }

        private void SetPort(ref BasePort portToSet, BasePort newPort)
        {
            if (newPort != portToSet)
            {
                // Clean Up Old Connection
                if (portToSet != null)
                {
                    UntrackPort(portToSet);
                    portToSet.Disconnect(this);
                }

                // Setup New Connection
                portToSet = newPort;
                if (portToSet != null)
                {
                    TrackPort(portToSet);
                    portToSet.Connect(this);
                }

                // Mark Dirty
                OnEdgeChanged();
            }
        }

        #endregion

        #region Drag Status

        public bool IsRealEdge()
        {
            return Input != null && Output != null;
        }

        public bool IsCandidateEdge()
        {
            return m_InputPositionOverridden || m_OutputPositionOverridden;
        }

        #endregion

        #region Event Handlers

        protected virtual void OnGeometryChanged(GeometryChangedEvent e)
        {
            UpdateCachedInputPortPosition();
            UpdateCachedOutputPortPosition();
            OnEdgeChanged();
        }

        protected override void OnAddedToGraphView()
        {
            base.OnAddedToGraphView();
            UpdateCachedInputPortPosition();
            UpdateCachedOutputPortPosition();
        }

        protected override void OnRemovedFromGraphView()
        {
            base.OnRemovedFromGraphView();
            Disconnect();
            UnsetPositionOverrides();
        }

        #endregion

        #region Ports

        public void SetPortByDirection(BasePort port)
        {
            if (port.Direction == Direction.Input)
                Input = port;
            else
                Output = port;
        }

        public void Disconnect()
        {
            Input = null;
            Output = null;
        }

        private void TrackPort(BasePort port)
        {
            port.OnPositionChange += OnPortPositionChanged;

            VisualElement current = port.hierarchy.parent;
            while (current != null)
            {
                if (current is BaseGraphView.Layer) break;

                // if we encounter our node ignore it but continue in the case there are nodes inside nodes
                if (current != port.ParentNode) current.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

                current = current.hierarchy.parent;
            }

            if (port.ParentNode != null) port.ParentNode.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            OnPortPositionChanged(new PositionData {element = port});
        }

        private void UntrackPort(BasePort port)
        {
            port.OnPositionChange -= OnPortPositionChanged;

            VisualElement current = port.hierarchy.parent;
            while (current != null)
            {
                if (current is BaseGraphView.Layer) break;

                // if we encounter our node ignore it but continue in the case there are nodes inside nodes
                if (current != port.ParentNode) port.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);

                current = current.hierarchy.parent;
            }

            if (port.ParentNode != null) port.ParentNode.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void OnPortPositionChanged(PositionData changeData)
        {
            var p = changeData.element as BasePort;
            if (p != null && BaseGraph != null)
            {
                if (p == m_InputPort)
                    UpdateCachedInputPortPosition();
                else if (p == m_OutputPort) UpdateCachedOutputPortPosition();
                OnEdgeChanged();
            }
        }

        private void UpdateCachedInputPortPosition()
        {
            if (BaseGraph == null || Input == null) return;
            m_InputPortPosition = BaseGraph.ContentContainer.WorldToLocal(Input.GetGlobalCenter());
            if (!m_InputPositionOverridden) m_InputPositionOverride = m_InputPortPosition;
        }

        private void UpdateCachedOutputPortPosition()
        {
            if (BaseGraph == null || Output == null) return;
            m_OutputPortPosition = BaseGraph.ContentContainer.WorldToLocal(Output.GetGlobalCenter());
            if (!m_OutputPositionOverridden) m_OutputPositionOverride = m_OutputPortPosition;
        }

        #endregion

        #region Position

        protected abstract void OnEdgeChanged();

        public override void SetPosition(Vector2 newPosition)
        {
            if (IsInputPositionOverriden())
                SetInputPositionOverride(newPosition);
            else if (IsOutputPositionOverriden()) SetOutputPositionOverride(newPosition);
            // If there are no overrides, ignore this because we aren't dragging yet.
        }

        public override Vector2 GetPosition()
        {
            if (IsInputPositionOverriden()) return GetInputPositionOverride();
            if (IsOutputPositionOverriden()) return GetOutputPositionOverride();
            return Vector2.zero;
        }

        public override void ApplyDeltaToPosition(Vector2 delta)
        {
            SetPosition(GetPosition() + delta);
        }

        public Vector2 GetInputPositionOverride()
        {
            return m_InputPositionOverride;
        }

        public Vector2 GetOutputPositionOverride()
        {
            return m_OutputPositionOverride;
        }

        public bool IsInputPositionOverriden()
        {
            return m_InputPositionOverridden;
        }

        public bool IsOutputPositionOverriden()
        {
            return m_OutputPositionOverridden;
        }

        public void SetInputPositionOverride(Vector2 position)
        {
            SetPositionOverride(
                position, out m_InputPositionOverride, ref m_InputPositionOverridden, ref m_InputPort);
        }

        public void SetOutputPositionOverride(Vector2 position)
        {
            SetPositionOverride(
                position, out m_OutputPositionOverride, ref m_OutputPositionOverridden, ref m_OutputPort);
        }

        private void SetPositionOverride(Vector2 position, out Vector2 overrideValueToSet,
            ref bool overrideFlagToSet, ref BasePort portToUpdate)
        {
            // Order here is important
            // TODO - position is being used as a flag for "drag mode". Should be refactored.
            overrideValueToSet = position;
            if (overrideFlagToSet != true)
            {
                overrideFlagToSet = true;
                portToUpdate?.UpdateCapColor();
            }

            OnEdgeChanged();
        }

        public void UnsetPositionOverrides()
        {
            m_InputPositionOverridden = false;
            m_OutputPositionOverridden = false;
            m_InputPort?.UpdateCapColor();
            m_OutputPort?.UpdateCapColor();
            OnEdgeChanged();
        }

        public override Vector2 GetCenter()
        {
            if (m_InputPositionOverridden) return m_InputPositionOverride;
            if (m_OutputPositionOverridden) return m_OutputPositionOverride;
            return base.GetCenter();
        }

        #endregion

        #region Drag Events

        [EventInterest(typeof(DragOfferEvent))]
        protected override void ExecuteDefaultActionAtTarget(EventBase evt)
        {
            base.ExecuteDefaultActionAtTarget(evt);
            if (evt.eventTypeId == DragOfferEvent.TypeId()) OnDragOffer((DragOfferEvent) evt);
        }

        private void OnDragOffer(DragOfferEvent e)
        {
            if (BaseGraph != null && BaseGraph.IsViewDrag(e))
            {
                BaseGraph.OnDragOffer(e, true);
            }
            else
            {
                // Check if this is a edge drag event 
                if (!IsEdgeDrag(e) || !IsMovable()) return;

                // Accept Drag
                e.AcceptDrag(this);

                // Set Drag Threshold
                e.SetDragThreshold(10);
            }
        }

        private bool IsEdgeDrag<T>(DragAndDropEvent<T> e) where T : DragAndDropEvent<T>, new()
        {
            if ((MouseButton) e.button != MouseButton.LeftMouse) return false;
            if (!e.modifiers.IsNone()) return false;
            return true;
        }

        #endregion
    }
}