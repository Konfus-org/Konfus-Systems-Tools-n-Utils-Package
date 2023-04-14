using System;
using System.Collections.Generic;
using System.Linq;

namespace Konfus.Systems.Node_Graph
{
    public static class GraphUtils
    {
        private enum State
        {
            White,
            Grey,
            Black
        }

        private class TarversalNode
        {
            public readonly Node node;
            public List<TarversalNode> inputs = new();
            public List<TarversalNode> outputs = new();
            public State state = State.White;

            public TarversalNode(Node node)
            {
                this.node = node;
            }
        }

        // A structure made for easy graph traversal
        private class TraversalGraph
        {
            public readonly List<TarversalNode> nodes = new();
            public readonly List<TarversalNode> outputs = new();
        }

        private static TraversalGraph ConvertGraphToTraversalGraph(Graph graph)
        {
            var g = new TraversalGraph();
            var nodeMap = new Dictionary<Node, TarversalNode>();

            foreach (Node node in graph.nodes)
            {
                var tn = new TarversalNode(node);
                g.nodes.Add(tn);
                nodeMap[node] = tn;

                if (graph.graphOutputs.Contains(node))
                    g.outputs.Add(tn);
            }

            foreach (TarversalNode tn in g.nodes)
            {
                tn.inputs = tn.node.GetInputNodes().Where(n => nodeMap.ContainsKey(n)).Select(n => nodeMap[n]).ToList();
                tn.outputs = tn.node.GetOutputNodes().Where(n => nodeMap.ContainsKey(n)).Select(n => nodeMap[n])
                    .ToList();
            }

            return g;
        }

        public static List<Node> DepthFirstSort(Graph g)
        {
            TraversalGraph graph = ConvertGraphToTraversalGraph(g);
            var depthFirstNodes = new List<Node>();

            foreach (TarversalNode n in graph.nodes)
                DFS(n);

            void DFS(TarversalNode n)
            {
                if (n.state == State.Black)
                    return;

                n.state = State.Grey;

                if (n.node is ParameterNode parameterNode && parameterNode.accessor == ParameterAccessor.Get)
                {
                    foreach (TarversalNode setter in graph.nodes.Where(x =>
                                 x.node is ParameterNode p &&
                                 p.parameterGUID == parameterNode.parameterGUID &&
                                 p.accessor == ParameterAccessor.Set))
                        if (setter.state == State.White)
                            DFS(setter);
                }
                else
                {
                    foreach (TarversalNode input in n.inputs)
                        if (input.state == State.White)
                            DFS(input);
                }

                n.state = State.Black;

                // Only add the node when his children are completely visited
                depthFirstNodes.Add(n.node);
            }

            return depthFirstNodes;
        }

        public static void FindCyclesInGraph(Graph g, Action<Node> cyclicNode)
        {
            TraversalGraph graph = ConvertGraphToTraversalGraph(g);
            var cyclicNodes = new List<TarversalNode>();

            foreach (TarversalNode n in graph.nodes)
                DFS(n);

            void DFS(TarversalNode n)
            {
                if (n.state == State.Black)
                    return;

                n.state = State.Grey;

                foreach (TarversalNode input in n.inputs)
                    if (input.state == State.White)
                        DFS(input);
                    else if (input.state == State.Grey)
                        cyclicNodes.Add(n);
                n.state = State.Black;
            }

            cyclicNodes.ForEach(tn => cyclicNode?.Invoke(tn.node));
        }

        public static T FindNodeInGraphOfType<T>(Graph graph) where T : Node
        {
            return graph.nodes.Find(x => x is T) as T;
        }
    }
}