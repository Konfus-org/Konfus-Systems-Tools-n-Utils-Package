using System;
using System.Collections.Generic;
using UnityEngine;

namespace Konfus.Systems.Node_Graph
{
    [Serializable]
    public class ParameterNode : Node
    {
        [Input] public object input;

        [Output] public object output;

        // We serialize the GUID of the exposed parameter in the graph so we can retrieve the true ExposedParameter from the graph
        [SerializeField] [HideInInspector] public string parameterGUID;

        public ParameterAccessor accessor;

        public event Action onParameterChanged;

        public override string name => "Parameter";

        public ExposedParameter parameter { get; private set; }

        [CustomPortBehavior(nameof(input))]
        protected virtual IEnumerable<PortData> GetInputPort(List<SerializableEdge> edges)
        {
            if (accessor == ParameterAccessor.Set)
                yield return new PortData
                {
                    identifier = "input",
                    displayName = "Value",
                    DisplayType = parameter == null ? typeof(object) : parameter.GetValueType()
                };
        }

        [CustomPortBehavior(nameof(output))]
        protected virtual IEnumerable<PortData> GetOutputPort(List<SerializableEdge> edges)
        {
            if (accessor == ParameterAccessor.Get)
                yield return new PortData
                {
                    identifier = "output",
                    displayName = "Value",
                    DisplayType = parameter == null ? typeof(object) : parameter.GetValueType(),
                    acceptMultipleEdges = true
                };
        }

        protected override void Enable()
        {
            // load the parameter
            LoadExposedParameter();

            graph.onExposedParameterModified += OnParamChanged;
            if (onParameterChanged != null)
                onParameterChanged?.Invoke();
        }

        protected override void Process()
        {
#if UNITY_EDITOR // In the editor, an undo/redo can change the parameter instance in the graph, in this case the field in this class will point to the wrong parameter
            parameter = graph.GetExposedParameterFromGUID(parameterGUID);
#endif

            ClearMessages();
            if (parameter == null)
            {
                AddMessage($"Parameter not found: {parameterGUID}", NodeMessageType.Error);
                return;
            }

            if (accessor == ParameterAccessor.Get)
                output = parameter.value;
            else
                graph.UpdateExposedParameter(parameter.guid, input);
        }

        private void OnParamChanged(ExposedParameter modifiedParam)
        {
            if (parameter == modifiedParam) onParameterChanged?.Invoke();
        }

        private void LoadExposedParameter()
        {
            parameter = graph.GetExposedParameterFromGUID(parameterGUID);

            if (parameter == null)
            {
                Debug.Log("Property \"" + parameterGUID + "\" Can't be found !");

                // Delete this node as the property can't be found
                graph.RemoveNode(this);
                return;
            }

            output = parameter.value;
        }
    }

    public enum ParameterAccessor
    {
        Get,
        Set
    }
}