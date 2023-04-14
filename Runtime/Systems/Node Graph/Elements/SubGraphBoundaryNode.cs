using System;
using System.Collections.Generic;
using Konfus.Systems.Node_Graph.Schema;
using UnityEngine;

namespace Konfus.Systems.Node_Graph
{
    [Serializable]
    [NodeOpacityIfNoPorts(0.25f)]
    public abstract class SubGraphBoundaryNode : Node
    {
        protected Dictionary<PortData, object> passThroughBufferByPort = new();
        public override Color color => Color.grey;
        public override bool deletable => false;
        public override bool HideNodeInspectorBlock => true;
        public override bool needsInspector => true;

        public SubGraph SubGraph => graph as SubGraph;

        protected override NodeRenamePolicy DefaultRenamePolicy => NodeRenamePolicy.DISABLED;

        protected abstract List<PortData> Ports { get; }

        public sealed override void InitializePorts()
        {
            base.InitializePorts();
            SubGraph.AddUpdatePortsListener(OnPortsListUpdated);
            passThroughBufferByPort.Clear();
        }

        protected virtual void OnPortsListUpdated()
        {
            UpdateAllPortsLocal();
        }
    }
}