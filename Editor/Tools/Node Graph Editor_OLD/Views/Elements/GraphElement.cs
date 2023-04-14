// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using Konfus.Tools.Graph_Editor.Editor.Enums;
using Konfus.Tools.Graph_Editor.Editor.Interfaces;
using Konfus.Tools.Graph_Editor.Editor.Manipulators;
using UnityEngine;
using UnityEngine.UIElements;

namespace Konfus.Tools.Graph_Editor.Views.Elements
{
    public abstract class GraphElement : VisualElement, ISelectable, IPositionable
    {
        private static readonly CustomStyleProperty<int> s_LayerProperty = new("--layer");
        private BaseGraphView _mBaseGraphView;
        private SelectableManipulator selectableManipulator;
        private int m_Layer;
        private bool m_LayerIsInline;
        private bool m_Selected;

        protected GraphElement()
        {
            ClearClassList();
            AddToClassList("graph-element");
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        protected void Initialize()
        {
            if (selectableManipulator != null)
            {
                this.RemoveManipulator(selectableManipulator);
                selectableManipulator = null;
            }

            // Setup Manipulators
            this.AddManipulator(new SelectableManipulator(BaseGraph.OnActionExecuted));
        }

        public BaseGraphView BaseGraph
        {
            get => _mBaseGraphView;
            internal set
            {
                if (_mBaseGraphView == value) return;

                // We want m_GraphView there whenever these events are call so we can do setup/teardown
                if (value == null)
                {
                    OnRemovedFromGraphView();
                    _mBaseGraphView = null;
                }
                else
                {
                    _mBaseGraphView = value;
                    OnAddedToGraphView();
                }
            }
        }

        public virtual string Title
        {
            get => name;
            set => throw new NotImplementedException();
        }

        public Capabilities Capabilities { get; set; }

        #region Graph Events

        protected virtual void OnAddedToGraphView()
        {
            Initialize();
        }

        protected virtual void OnRemovedFromGraphView()
        {
            pickingMode = PickingMode.Position;
            Selected = false;
            ResetLayer();
        }

        #endregion

        #region Selectable

        public ISelector Selector => BaseGraph.ContentContainer;

        public virtual bool Selected
        {
            get => m_Selected;
            set
            {
                if (m_Selected == value) return;
                if (value && IsSelectable())
                {
                    m_Selected = true;
                    if (IsAscendable() && resolvedStyle.position != Position.Relative) BringToFront();
                }
                else
                {
                    m_Selected = false;
                    if (BaseGraph != null) BaseGraph.ContentContainer.RemoveFromDragSelection(this);
                }
            }
        }

        #endregion

        #region Layer

        public int Layer
        {
            get => m_Layer;
            set
            {
                if (m_Layer == value) return;
                m_Layer = value;
                m_LayerIsInline = true;
                BaseGraph?.ChangeLayer(this);
            }
        }

        public void ResetLayer()
        {
            m_LayerIsInline = false;
            customStyle.TryGetValue(s_LayerProperty, out int styleLayer);
            Layer = styleLayer;
        }

        #endregion

        #region Custom Style

        private void OnCustomStyleResolved(CustomStyleResolvedEvent e)
        {
            OnCustomStyleResolved(e.customStyle);
        }

        protected virtual void OnCustomStyleResolved(ICustomStyle styleOverride)
        {
            if (!m_LayerIsInline) ResetLayer();
        }

        #endregion

        #region Position

        public virtual event Action<PositionData> OnPositionChange;

        public virtual Vector2 GetGlobalCenter()
        {
            return BaseGraph.ContentContainer.LocalToWorld(GetCenter());
        }

        public virtual Vector2 GetCenter()
        {
            return layout.center + (Vector2) transform.position;
        }

        public virtual Vector2 GetPosition()
        {
            return transform.position;
        }

        public virtual void SetPosition(Vector2 newPosition)
        {
            transform.position = newPosition;
            OnPositionChange?.Invoke(new PositionData
            {
                element = this,
                position = newPosition
            });
        }

        public virtual void ApplyDeltaToPosition(Vector2 delta)
        {
            SetPosition((Vector2) transform.position + delta);
        }

        #endregion

        #region Capabilities

        public virtual bool IsMovable()
        {
            return (Capabilities & Capabilities.Movable) == Capabilities.Movable;
        }

        public virtual bool IsDroppable()
        {
            return (Capabilities & Capabilities.Droppable) == Capabilities.Droppable;
        }

        public virtual bool IsAscendable()
        {
            return (Capabilities & Capabilities.Ascendable) == Capabilities.Ascendable;
        }

        public virtual bool IsRenamable()
        {
            return (Capabilities & Capabilities.Renamable) == Capabilities.Renamable;
        }

        public virtual bool IsCopiable()
        {
            return (Capabilities & Capabilities.Copiable) == Capabilities.Copiable;
        }

        public virtual bool IsSnappable()
        {
            return (Capabilities & Capabilities.Snappable) == Capabilities.Snappable;
        }

        public virtual bool IsResizable()
        {
            return false;
        }

        public virtual bool IsGroupable()
        {
            return false;
        }

        public virtual bool IsStackable()
        {
            return false;
        }

        public virtual bool IsSelectable()
        {
            return (Capabilities & Capabilities.Selectable) == Capabilities.Selectable && visible;
        }

        #endregion
    }
}