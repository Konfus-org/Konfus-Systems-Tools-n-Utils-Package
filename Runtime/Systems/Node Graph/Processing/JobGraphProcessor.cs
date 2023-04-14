using System.Collections.Generic;
using System.Linq;
using Unity.Jobs;

// using Unity.Entities;

namespace Konfus.Systems.Node_Graph
{
	/// <summary>
	///     Graph processor
	/// </summary>
	public class JobGraphProcessor : GraphProcessor
    {
        private GraphScheduleList[] scheduleList;

        internal class GraphScheduleList
        {
            public Node node;
            public Node[] dependencies;

            public GraphScheduleList(Node node)
            {
                this.node = node;
            }
        }

        /// <summary>
        ///     Manage graph scheduling and processing
        /// </summary>
        /// <param name="graph">Graph to be processed</param>
        public JobGraphProcessor(Graph graph) : base(graph)
        {
        }

        public override void UpdateComputeOrder()
        {
            scheduleList = graph.nodes.OrderBy(n => n.computeOrder).Select(n =>
            {
                var gsl = new GraphScheduleList(n);
                gsl.dependencies = n.GetInputNodes().ToArray();
                return gsl;
            }).ToArray();
        }

        /// <summary>
        ///     Schedule the graph into the job system
        /// </summary>
        public override void Run()
        {
            int count = scheduleList.Length;
            var scheduledHandles = new Dictionary<Node, JobHandle>();

            for (int i = 0; i < count; i++)
            {
                var dep = default(JobHandle);
                GraphScheduleList schedule = scheduleList[i];
                int dependenciesCount = schedule.dependencies.Length;

                for (int j = 0; j < dependenciesCount; j++)
                    dep = JobHandle.CombineDependencies(dep, scheduledHandles[schedule.dependencies[j]]);

                // TODO: call the onSchedule on the current node
                // JobHandle currentJob = schedule.node.OnSchedule(dep);
                // scheduledHandles[schedule.node] = currentJob;
            }

            JobHandle.ScheduleBatchedJobs();
        }
    }
}