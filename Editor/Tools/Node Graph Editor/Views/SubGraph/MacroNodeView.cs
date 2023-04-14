using Konfus.Systems.Node_Graph;
using UnityEditor;

namespace Konfus.Tools.NodeGraphEditor.View
{
    [NodeCustomEditor(typeof(MacroNode))]
    public class MacroNodeView : NodeView
    {
        private MacroNode Target => nodeTarget as MacroNode;
        private SubGraph SubGraph => Target.SubGraph;

        private SubGraphGUIUtility SubGraphSerializer => SubGraph ? new SubGraphGUIUtility(SubGraph) : null;

        protected override void DrawDefaultInspector(bool fromInspector = false)
        {
        }

        public override void OnDoubleClicked()
        {
            if (SubGraph == null) return;

            EditorWindow.GetWindow<SubGraphWindow>().InitializeGraph(SubGraph);
        }
    }
}