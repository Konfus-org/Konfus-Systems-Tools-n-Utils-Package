using Konfus.Systems.Node_Graph;
using Konfus.Systems.Node_Graph.Schema;
using Konfus.Tools.NodeGraphEditor.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Konfus.Tools.NodeGraphEditor
{
    public class SubGraphGUIUtility
    {
        private SerializedObject _subGraphSerialized;
        private SerializedProperty _ingressPortDataSerialized;
        private SerializedProperty _egressPortDataSerialized;
        private SerializedProperty _schemaSerialized;
        private SerializedProperty _isMacro;
        private SerializedProperty _macroOptions;

        public SubGraphGUIUtility(SubGraph subGraph)
        {
            SubGraph = subGraph;
        }

        public SubGraph SubGraph { get; }

        public SubGraphOptionsGUIUtility OptionsGUIUtil => new(SubGraph);
        private SubGraphSchemaGUIUtility _schemaGUIUtil;

        public SubGraphSchemaGUIUtility SchemaGUIUtil =>
            PropertyUtils.LazyLoad(
                ref _schemaGUIUtil,
                () => SubGraph.Schema ? new SubGraphSchemaGUIUtility(SubGraph.Schema) : null,
                x => x == null || SubGraph.Schema != x.Schema
            );

        public SerializedObject SubGraphObject =>
            PropertyUtils.LazyLoad(
                ref _subGraphSerialized,
                () => new SerializedObject(SubGraph)
            );

        public SerializedProperty IngressPortData =>
            PropertyUtils.LazyLoad(
                ref _ingressPortDataSerialized,
                () => SubGraphObject.FindProperty(SubGraph.IngressPortDataFieldName)
            );

        public SerializedProperty EgressPortData =>
            PropertyUtils.LazyLoad(
                ref _egressPortDataSerialized,
                () => SubGraphObject.FindProperty(SubGraph.EgressPortDataFieldName)
            );


        public SerializedProperty Schema =>
            PropertyUtils.LazyLoad(
                ref _schemaSerialized,
                () => SubGraphObject.FindProperty(SubGraph.SchemaFieldName)
            );

        public SerializedProperty IsMacro =>
            PropertyUtils.LazyLoad(
                ref _isMacro,
                () => SubGraphObject.FindProperty(SubGraph.IsMacroFieldName)
            );

        public SerializedProperty MacroOptions =>
            PropertyUtils.LazyLoad(
                ref _macroOptions,
                () => SubGraphObject.FindProperty(SubGraph.MacroOptionsFieldName)
            );

        public VisualElement DrawSubGraphPortControlGUI()
        {
            var portSelectionFoldout = new Foldout
            {
                text = "SubGraph Port Control"
            };

            portSelectionFoldout.Add(DrawIngressPortSelectorGUI(false));
            portSelectionFoldout.Add(DrawEgressPortSelectorGUI(false));
            portSelectionFoldout.Add(DrawPortUpdaterButtonGUI());

            portSelectionFoldout.Bind(SubGraphObject);

            return portSelectionFoldout;
        }

        public PropertyField DrawSchemaFieldGUI(bool bind = true)
        {
            var schemaField = new PropertyField(Schema);
            if (bind) schemaField.Bind(SubGraphObject);
            return schemaField;
        }

        public PropertyField DrawSchemaFieldWithCallback(EventCallback<SerializedPropertyChangeEvent> onChangeCallback,
            bool visible = true, bool bind = true)
        {
            PropertyField schemaField = DrawSchemaFieldGUI(bind);
            schemaField.RegisterValueChangeCallback(onChangeCallback);
            if (!visible)
            {
                schemaField.visible = false;
                schemaField.style.height = 0;
            }

            return schemaField;
        }

        public PropertyField DrawIngressPortSelectorGUI(bool bind = true)
        {
            var ingressDataField = new PropertyField(IngressPortData) {label = "Ingress Port Data - SubGraph"};
            if (bind) ingressDataField.Bind(SubGraphObject);
            return ingressDataField;
        }

        public PropertyField DrawEgressPortSelectorGUI(bool bind = true)
        {
            var egressDataField = new PropertyField(EgressPortData) {label = "Egress Port Data - SubGraph"};
            if (bind) egressDataField.Bind(SubGraphObject);
            return egressDataField;
        }

        public Button DrawPortUpdaterButtonGUI()
        {
            var updateSchemaButton = new Button(() => SubGraph.NotifyPortsChanged()) {text = "UPDATE PORTS"};
            return updateSchemaButton;
        }

        public VisualElement DrawOptionsGUI()
        {
            return OptionsGUIUtil.DrawGUI();
        }

        public VisualElement DrawMacroOptionsGUI()
        {
            VisualElement root = new();
            VisualElement macroOptionsContainer = new();

            var isMacroField = new PropertyField(IsMacro);
            isMacroField.RegisterValueChangeCallback(prop =>
            {
                if (macroOptionsContainer.IsShowing() && !SubGraph.IsMacro)
                    macroOptionsContainer.Hide();
                else if (!macroOptionsContainer.IsShowing() && SubGraph.IsMacro) macroOptionsContainer.Show();
            });

            var macroOptionsField = new PropertyField(MacroOptions);
            macroOptionsField.RegisterCallback<ChangeEvent<MacroOptions>>(prop => { Debug.Log("Changed"); });
            isMacroField.Bind(SubGraphObject);
            macroOptionsField.Bind(SubGraphObject);

            macroOptionsContainer.Add(macroOptionsField);

            root.Add(isMacroField);
            root.Add(macroOptionsContainer);

            return root;
        }
    }
}