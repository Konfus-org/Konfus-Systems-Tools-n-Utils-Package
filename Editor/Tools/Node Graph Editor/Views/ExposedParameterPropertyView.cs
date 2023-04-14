using Konfus.Systems.Node_Graph;
using UnityEngine.UIElements;

namespace Konfus.Tools.NodeGraphEditor
{
    public class ExposedParameterPropertyView : VisualElement
    {
        protected GraphView graphView;

        public ExposedParameter parameter { get; }

        public Toggle hideInInspector { get; private set; }

        public ExposedParameterPropertyView(GraphView graphView, ExposedParameter param)
        {
            this.graphView = graphView;
            parameter = param;

            VisualElement field = graphView.exposedParameterFactory.GetParameterSettingsField(param,
                newValue => { param.settings = newValue as ExposedParameter.Settings; });

            Add(field);
        }
    }
}