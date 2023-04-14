using Konfus.Systems.Node_Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Konfus.Tools.NodeGraphEditor
{
    public class ExposedParameterFieldView : BlackboardField
    {
        protected GraphView graphView;

        public ExposedParameter parameter { get; }

        public ExposedParameterFieldView(GraphView graphView, ExposedParameter param) : base(null, param.name,
            param.shortType)
        {
            this.graphView = graphView;
            parameter = param;
            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
            this.Q("icon").AddToClassList("parameter-" + param.shortType);
            this.Q("icon").visible = true;

            (this.Q("textField") as TextField).RegisterValueChangedCallback(e =>
            {
                param.name = e.newValue;
                text = e.newValue;
                graphView.graph.UpdateExposedParameterName(param, e.newValue);
            });
        }

        private void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Rename", a => OpenTextEditor(), DropdownMenuAction.AlwaysEnabled);
            evt.menu.AppendAction("Delete", a => graphView.graph.RemoveExposedParameter(parameter),
                DropdownMenuAction.AlwaysEnabled);

            evt.StopPropagation();
        }
    }
}