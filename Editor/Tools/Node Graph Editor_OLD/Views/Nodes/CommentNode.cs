using Konfus.Systems.Graph.Attributes;
using Konfus.Systems.Graph;
using Konfus.Tools.Graph_Editor.Editor.Controllers;
using Konfus.Tools.Graph_Editor.Editor.Interfaces;
using UnityEngine.UIElements;

namespace Konfus.Tools.Graph_Editor.Views.Nodes
{
    [Node("#00000001", createInputPort: false)]
    public class CommentNode : INode, IUtilityNode
    {
        private NodeView nodeView;

        public bool CreateInspectorUI()
        {
            return false;
        }

        public bool CreateNameUI()
        {
            return true;
        }

        public void Initialize(NodeController nodeController)
        {
            nodeView = nodeController.nodeView;
            // we need auto resizing, so the comment node expands to the full width of the provided text...
            nodeView.style.width = StyleKeyword.Auto;
            nodeView.AddToClassList(nameof(CommentNode));

            // comment node is a very special snowflake... :D
            // So: We completely remove all the default ui
            nodeView.ExtensionContainer.RemoveFromHierarchy();
            nodeView.InputContainer.RemoveFromHierarchy();
            nodeView.OutputContainer.RemoveFromHierarchy();
        }

        public bool ShouldColorizeBackground()
        {
            return true;
        }
    }
}