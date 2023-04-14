using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Konfus.Tools.NodeGraphEditor
{
    public class MiniMapView : MiniMap
    {
        private new GraphView graphView;
        private Vector2 size;

        public MiniMapView(GraphView graphView)
        {
            this.graphView = graphView;
            SetPosition(new Rect(0, 0, 100, 100));
            size = new Vector2(100, 100);
        }
    }
}