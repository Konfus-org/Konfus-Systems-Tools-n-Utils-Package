using System.Collections.Generic;

namespace Konfus.Systems.Node_Graph
{
    public abstract class SubGraphProcessor
    {
        protected SubGraph graph;

        /// <summary>
        ///     Manage graph scheduling and processing
        /// </summary>
        /// <param name="graph">Graph to be processed</param>
        public SubGraphProcessor(SubGraph graph)
        {
            this.graph = graph;

            UpdateComputeOrder();
        }

        public abstract void UpdateComputeOrder();

        /// <summary>
        ///     Schedule the graph into the job system
        /// </summary>
        public abstract void Run(Dictionary<PortData, object> ingress);
    }
}