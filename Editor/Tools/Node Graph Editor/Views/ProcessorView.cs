using Konfus.Systems.Node_Graph;
using UnityEngine.UIElements;

namespace Konfus.Tools.NodeGraphEditor
{
    public class ProcessorView : PinnedElementView
    {
        private GraphProcessor processor;

        public ProcessorView()
        {
            title = "Process panel";
        }

        protected override void Initialize(GraphView graphView)
        {
            processor = new ProcessGraphProcessor(graphView.graph);

            graphView.computeOrderUpdated += processor.UpdateComputeOrder;

            var b = new Button(OnPlay) {name = "ActionButton", text = "Play !"};

            content.Add(b);
        }

        private void OnPlay()
        {
            processor.Run();
        }
    }
}