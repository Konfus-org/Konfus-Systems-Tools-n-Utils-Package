

// using Unity.Entities;

namespace Konfus.Systems.Node_Graph
{
	/// <summary>
	///     Graph processor
	/// </summary>
	public abstract class GraphProcessor
    {
        protected Graph graph;

        /// <summary>
        ///     Manage graph scheduling and processing
        /// </summary>
        /// <param name="graph">Graph to be processed</param>
        public GraphProcessor(Graph graph)
        {
            this.graph = graph;

            UpdateComputeOrder();
        }

        public abstract void UpdateComputeOrder();

        /// <summary>
        ///     Schedule the graph into the job system
        /// </summary>
        public abstract void Run();
    }
}