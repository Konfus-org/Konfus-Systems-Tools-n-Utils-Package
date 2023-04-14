using System;
using Konfus.Systems.Node_Graph;
using Konfus.Systems.Node_Graph.Schema;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Konfus.Tools.NodeGraphEditor
{
    public class SubGraphSchemaGUIUtility
    {
        private SerializedObject _subGraphSerialized;
        private SerializedProperty _ingressPortDataSerialized;
        private SerializedProperty _egressPortDataSerialized;

        public SubGraphSchemaGUIUtility(SubGraphPortSchema schema)
        {
            Schema = schema;
        }

        public SubGraphPortSchema Schema { get; }

        public SerializedObject SchemaObject =>
            PropertyUtils.LazyLoad(
                ref _subGraphSerialized,
                () => new SerializedObject(Schema)
            );

        public SerializedProperty IngressPortData =>
            PropertyUtils.LazyLoad(
                ref _ingressPortDataSerialized,
                () => SchemaObject.FindProperty(SubGraphPortSchema.IngressPortDataFieldName)
            );

        public SerializedProperty EgressPortData =>
            PropertyUtils.LazyLoad(
                ref _egressPortDataSerialized,
                () => SchemaObject.FindProperty(SubGraphPortSchema.EgressPortDataFieldName)
            );

        public VisualElement DrawFullSchemaGUI()
        {
            var schemaControlsContainer = new VisualElement();

            schemaControlsContainer.Add(DrawSchemaPortControlGUI(false));
            schemaControlsContainer.Add(DrawSchemaUpdaterButtonGUI());

            schemaControlsContainer.Bind(SchemaObject);

            return schemaControlsContainer;
        }

        public VisualElement DrawSchemaPortControlGUI(bool bind = true)
        {
            VisualElement schemaPortControlContainer = new();

            schemaPortControlContainer.Add(DrawIngressPortSelectorGUI(bind));
            schemaPortControlContainer.Add(DrawEgressPortSelectorGUI(bind));

            return schemaPortControlContainer;
        }

        public PropertyField DrawIngressPortSelectorGUI(bool bind = true)
        {
            var ingressDataField = new PropertyField(IngressPortData) {label = "Ingress Port Data - Schema"};
            if (bind) ingressDataField.Bind(SchemaObject);
            return ingressDataField;
        }

        public PropertyField DrawEgressPortSelectorGUI(bool bind = true)
        {
            var egressDataField = new PropertyField(EgressPortData) {label = "Egress Port Data - Schema"};
            if (bind) egressDataField.Bind(SchemaObject);
            return egressDataField;
        }

        public Action SchemaUpdateButtonAction => () => Schema.NotifyPortsChanged();

        public Button DrawSchemaUpdaterButtonGUI()
        {
            var updatePortsButton = new Button(SchemaUpdateButtonAction) {text = "UPDATE SCHEMA"};
            return updatePortsButton;
        }

        public static Button DrawSchemaUpdaterButtonGUI(Action action)
        {
            var updatePortsButton = new Button(action) {text = "UPDATE SCHEMA"};
            return updatePortsButton;
        }
    }
}