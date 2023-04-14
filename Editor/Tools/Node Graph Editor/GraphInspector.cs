using Konfus.Systems.Node_Graph;
using UnityEditor;
using UnityEngine.UIElements;

namespace Konfus.Tools.NodeGraphEditor
{
    public class GraphInspector : Editor
    {
        protected ExposedParameterFieldFactory exposedParameterFactory;
        protected Graph graph;
        protected VisualElement root;

        private VisualElement parameterContainer;

        public sealed override VisualElement CreateInspectorGUI()
        {
            root = new VisualElement();
            graph.CreateInspectorGUI(root);
            CreateInspector();
            return root;
        }

        // Don't use ImGUI
        public sealed override void OnInspectorGUI()
        {
        }

        protected virtual void CreateInspector()
        {
            parameterContainer = new VisualElement
            {
                name = "ExposedParameters"
            };
            FillExposedParameters(parameterContainer);

            root.Add(parameterContainer);
        }

        protected virtual void OnDisable()
        {
            graph.onExposedParameterListChanged -= UpdateExposedParameters;
            graph.onExposedParameterModified -= UpdateExposedParameters;
            exposedParameterFactory?.Dispose(); //  Graphs that created in GraphBehaviour sometimes gives null ref.
            exposedParameterFactory = null;
        }

        protected virtual void OnEnable()
        {
            graph = target as Graph;
            graph.onExposedParameterListChanged += UpdateExposedParameters;
            graph.onExposedParameterModified += UpdateExposedParameters;
            if (exposedParameterFactory == null)
                exposedParameterFactory = new ExposedParameterFieldFactory(graph);
        }

        protected void FillExposedParameters(VisualElement parameterContainer)
        {
            if (graph.exposedParameters.Count != 0)
                parameterContainer.Add(new Label("Exposed Parameters:"));

            foreach (ExposedParameter param in graph.exposedParameters)
            {
                if (param.settings.isHidden)
                    continue;

                VisualElement field = exposedParameterFactory.GetParameterValueField(param, newValue =>
                {
                    param.value = newValue;
                    serializedObject.ApplyModifiedProperties();
                    graph.NotifyExposedParameterValueChanged(param);
                });
                parameterContainer.Add(field);
            }
        }

        private void UpdateExposedParameters(ExposedParameter param)
        {
            UpdateExposedParameters();
        }

        private void UpdateExposedParameters()
        {
            parameterContainer.Clear();
            FillExposedParameters(parameterContainer);
        }
    }
}