using Konfus.Systems.Node_Graph;
using UnityEngine;

namespace Konfus.Tools.NodeGraphEditor
{
    public class SubGraphWindow : GraphWindow
    {
        protected override void InitializeWindow(Graph graph)
        {
            titleContent = new GUIContent("Default Graph");

            if (graphView == null)
                graphView = new GraphView(this);

            rootView.Add(graphView);
        }

        protected override void OnDestroy()
        {
            graphView?.Dispose();
        }
    }
}