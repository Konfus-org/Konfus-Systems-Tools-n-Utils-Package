using Konfus.Systems.Graph.Attributes;
using Konfus.Systems.Graph;
using Konfus.Tools.Graph_Editor.Editor.Controllers;
using UnityEngine;
using UnityEngine.UIElements;

namespace Konfus.Tools.Graph_Editor.Views.Nodes
{
    [Node("#7a7a7a", createInputPort = false)]
    public class RedirectNode : INode
    {
        [PortBase(name = ""), SerializeReference]
        public INode from;
        [Port(name = ""), SerializeReference]
        public INode to;

        public void Initialize(NodeController nodeController)
        {
            NodeView nodeView = nodeController.nodeView;
            
            // we need auto resizing, so the comment node expands to the full width of the provided text...
            nodeView.style.width = StyleKeyword.Auto;
            nodeView.AddToClassList(nameof(RedirectNode));
            
            // comment node is a very special snowflake... :D
            // So: We completely remove all the default ui
            nodeView.TitleContainer.RemoveFromHierarchy();
            nodeView.ExtensionContainer.RemoveFromHierarchy();
        }
    }
}