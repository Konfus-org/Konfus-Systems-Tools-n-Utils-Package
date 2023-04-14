using System;
using System.Collections.Generic;
using System.Linq;
using Konfus.Systems.Node_Graph;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Node = Konfus.Systems.Node_Graph.Node;

namespace Konfus.Tools.NodeGraphEditor
{
    // TODO: replace this by the new UnityEditor.Searcher package
    internal class CreateNodeMenuWindow : ScriptableObject, ISearchWindowProvider
    {
        private EdgeView edgeFilter;
        private GraphView graphView;
        private Texture2D icon;
        private PortView inputPortView;
        private PortView outputPortView;
        private EditorWindow window;

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node"))
            };

            if (edgeFilter == null)
                CreateStandardNodeMenu(tree);
            else
                CreateEdgeNodeMenu(tree);

            return tree;
        }

        // Node creation when validate a choice
        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            // window to graph position
            VisualElement windowRoot = window.rootVisualElement;
            Vector2 windowMousePosition = windowRoot.ChangeCoordinatesTo(windowRoot.parent,
                context.screenMousePosition - window.position.position);
            Vector2 graphMousePosition = graphView.contentViewContainer.WorldToLocal(windowMousePosition);

            if (searchTreeEntry.userData is NodeProvider.NodeMenuEntry)
            {
                var userData = searchTreeEntry.userData as NodeProvider.NodeMenuEntry;
                Type nodeType = userData.NodeType;
                NodeUtils.NodeCreationMethod method = userData.CreationMethod;
                object[] methodArgs = userData.CreationMethodArgs;

                graphView.RegisterCompleteObjectUndo("Added " + nodeType);
                graphView.AddNode(method.Invoke(nodeType, graphMousePosition, methodArgs));
            }
            else if (searchTreeEntry.userData is Tuple<NodeProvider.PortDescription, NodeProvider.NodeMenuEntry>)
            {
                var userData =
                    searchTreeEntry.userData as Tuple<NodeProvider.PortDescription, NodeProvider.NodeMenuEntry>;
                Type nodeType = userData.Item1.nodeType;
                NodeUtils.NodeCreationMethod method = userData.Item2.CreationMethod;
                object[] methodArgs = userData.Item2.CreationMethodArgs;

                graphView.RegisterCompleteObjectUndo("Added " + nodeType);
                NodeView view = graphView.AddNode(method.Invoke(nodeType, graphMousePosition, methodArgs));

                PortView targetPort =
                    view.GetPortViewFromFieldName(userData.Item1.portFieldName, userData.Item1.portIdentifier);
                if (inputPortView == null)
                    graphView.Connect(targetPort, outputPortView);
                else
                    graphView.Connect(inputPortView, targetPort);
            }
            else
            {
                var userData = (NodeProvider.PortDescription) searchTreeEntry.userData;

                graphView.RegisterCompleteObjectUndo("Added " + userData.nodeType);
                NodeView nodeView = graphView.AddNode(Node.CreateFromType(userData.nodeType, graphMousePosition));
                PortView targetPort =
                    nodeView.GetPortViewFromFieldName(userData.portFieldName, userData.portIdentifier);

                if (inputPortView == null)
                    graphView.Connect(targetPort, outputPortView);
                else
                    graphView.Connect(inputPortView, targetPort);
            }

            return true;
        }

        public void Initialize(GraphView graphView, EditorWindow window, EdgeView edgeFilter = null)
        {
            this.graphView = graphView;
            this.window = window;
            this.edgeFilter = edgeFilter;
            inputPortView = edgeFilter?.input as PortView;
            outputPortView = edgeFilter?.output as PortView;

            // Transparent icon to trick search window into indenting items
            if (icon == null)
                icon = new Texture2D(1, 1);
            icon.SetPixel(0, 0, new Color(0, 0, 0, 0));
            icon.Apply();
        }

        private void OnDestroy()
        {
            if (icon != null)
            {
                DestroyImmediate(icon);
                icon = null;
            }
        }

        private void CreateEdgeNodeMenu(List<SearchTreeEntry> tree)
        {
            IEnumerable<NodeProvider.NodeMenuEntryMethod> cachedCustomMenuItemMethods =
                NodeProvider.CustomMenuItemMethods().Concat(NodeProvider.GetCustomClassMenuItemMethods());
            IEnumerable<NodeProvider.PortDescription> entries =
                NodeProvider.GetEdgeCreationNodeMenuEntry((edgeFilter.input ?? edgeFilter.output) as PortView,
                    graphView.graph);

            var titlePaths = new HashSet<string>();

            IEnumerable<NodeProvider.NodeMenuEntry> macroMenuEntries = NodeProvider.GetMacroNodeMenuEntries();
            IEnumerable<NodeProvider.NodeMenuEntry> customMenuEntries =
                NodeProvider.GetCustomNodeMenuEntries(graphView.graph, cachedCustomMenuItemMethods);
            IEnumerable<NodeProvider.NodeMenuEntry> menuEntries = NodeProvider.GetNodeMenuEntries(graphView.graph)
                .Concat(customMenuEntries).Concat(macroMenuEntries);

            tree.Add(new SearchTreeEntry(new GUIContent("Relay", icon))
            {
                level = 1,
                userData = new NodeProvider.PortDescription
                {
                    nodeType = typeof(RelayNode),
                    portType = typeof(object),
                    isInput = inputPortView != null,
                    portFieldName = inputPortView != null ? nameof(RelayNode.output) : nameof(RelayNode.input),
                    portIdentifier = "0",
                    portDisplayName = inputPortView != null ? "Out" : "In"
                }
            });

            IOrderedEnumerable<(NodeProvider.PortDescription port, string Path)> sortedMenuItems =
                entries.Select(port => (port, menuEntries.FirstOrDefault(kp => kp.NodeType == port.nodeType).Path))
                    .OrderBy(e => e.Path);

            // Sort menu by alphabetical order and submenus
            foreach ((NodeProvider.PortDescription port, string Path) menuEntry in sortedMenuItems)
            {
                string portFieldName = menuEntry.port.portFieldName;
                NodeProvider.PortDescription port = menuEntry.port;
                Type portNodeType = port.nodeType;
                Type portType = port.portType;
                IEnumerable<NodeProvider.NodeMenuEntry> filteredCustomNodePaths =
                    NodeProvider.GetFilteredCustomNodeMenuEntries((edgeFilter.input ?? edgeFilter.output).portType,
                        port, cachedCustomMenuItemMethods);
                foreach (NodeProvider.NodeMenuEntry node in menuEntries.Where(kp => kp.NodeType == portNodeType))
                {
                    string nodePath = node.Path;

                    // Ignore the node if it's not in the create menu
                    if (string.IsNullOrEmpty(nodePath))
                        continue;

                    // Ignore the node if it has filters and it doesn't meet the requirements.
                    if (customMenuEntries.Contains(node) && !filteredCustomNodePaths.Contains(node))
                        continue;

                    string nodeName = nodePath;
                    int level = 0;
                    string[] parts = nodePath.Split('/');

                    if (parts.Length > 1)
                    {
                        level++;
                        nodeName = parts[^1];
                        string fullTitleAsPath = "";

                        for (int i = 0; i < parts.Length - 1; i++)
                        {
                            string title = parts[i];
                            fullTitleAsPath += title;
                            level = i + 1;

                            // Add section title if the node is in subcategory
                            if (!titlePaths.Contains(fullTitleAsPath))
                            {
                                tree.Add(new SearchTreeGroupEntry(new GUIContent(title))
                                {
                                    level = level
                                });
                                titlePaths.Add(fullTitleAsPath);
                            }
                        }
                    }

                    tree.Add(new SearchTreeEntry(new GUIContent($"{nodeName}:  {port.portDisplayName}", icon))
                    {
                        level = level + 1,
                        userData = new Tuple<NodeProvider.PortDescription, NodeProvider.NodeMenuEntry>(port, node)
                    });
                }
            }
        }

        private void CreateStandardNodeMenu(List<SearchTreeEntry> tree)
        {
            // Sort menu by alphabetical order and submenus
            IOrderedEnumerable<NodeProvider.NodeMenuEntry> nodeEntries = graphView.FilterCreateNodeMenuEntries()
                .Concat(graphView.FilterCreateCustomNodeMenuEntries())
                .Concat(graphView.FilterMacroMenuEntries())
                .OrderBy(k => k.Path);

            var titlePaths = new HashSet<string>();

            foreach (NodeProvider.NodeMenuEntry nodeEntry in nodeEntries)
            {
                string nodePath = nodeEntry.Path;
                string nodeName = nodePath;
                int level = 0;
                string[] parts = nodePath.Split('/');

                if (parts.Length > 1)
                {
                    level++;
                    nodeName = parts[^1];
                    string fullTitleAsPath = "";

                    for (int i = 0; i < parts.Length - 1; i++)
                    {
                        string title = parts[i];
                        fullTitleAsPath += title;
                        level = i + 1;

                        // Add section title if the node is in subcategory
                        if (!titlePaths.Contains(fullTitleAsPath))
                        {
                            tree.Add(new SearchTreeGroupEntry(new GUIContent(title))
                            {
                                level = level
                            });
                            titlePaths.Add(fullTitleAsPath);
                        }
                    }
                }

                tree.Add(new SearchTreeEntry(new GUIContent(nodeName, icon))
                {
                    level = level + 1,
                    userData = nodeEntry
                });
            }
        }
    }
}