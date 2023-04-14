using System.Collections.Generic;
using System.Linq;

// using Unity.Entities;

namespace Konfus.Systems.Node_Graph
{
    /// <summary>
    ///     Graph processor
    /// </summary>
    public class ProcessGraphProcessor : GraphProcessor
    {
        private List<Node> processList;

        /// <summary>
        ///     Manage graph scheduling and processing
        /// </summary>
        /// <param name="graph">Graph to be processed</param>
        public ProcessGraphProcessor(Graph graph) : base(graph)
        {
        }

        public override void UpdateComputeOrder()
        {
            processList = graph.nodes.OrderBy(n => n.computeOrder).ToList();
        }

        /// <summary>
        ///     Process all the nodes following the compute order.
        /// </summary>
        public override void Run()
        {
            int count = processList.Count;

            for (int i = 0; i < count; i++)
                processList[i].OnProcess();
        }
    }
}