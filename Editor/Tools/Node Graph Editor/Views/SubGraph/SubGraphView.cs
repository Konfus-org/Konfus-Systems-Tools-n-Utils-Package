using Konfus.Systems.Node_Graph;
using Konfus.Systems.Node_Graph.Schema;
using Konfus.Tools.NodeGraphEditor.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Konfus.Tools.NodeGraphEditor
{
    [CustomEditor(typeof(SubGraph), true)]
    public class SubGraphView : GraphInspector
    {
        private SubGraphGUIUtility _subGraphSerializer;
        protected SubGraph SubGraph => target as SubGraph;
        protected SubGraphPortSchema Schema => SubGraph.Schema;

        private SubGraphGUIUtility SubGraphSerializer =>
            PropertyUtils.LazyLoad(ref _subGraphSerializer, () => new SubGraphGUIUtility(SubGraph));

        protected override void CreateInspector()
        {
            base.CreateInspector();

            VisualElement optionsGUI = SubGraphSerializer.DrawOptionsGUI();
            optionsGUI.Add(SubGraphSerializer.DrawMacroOptionsGUI());
            root.Add(optionsGUI);
            root.Add(SubGraphSerializer.DrawSubGraphPortControlGUI());
            root.Add(DrawSchemaControlGUI());
        }

        private VisualElement DrawSchemaControlGUI()
        {
            VisualElement schemaControlsFoldout = new Foldout
            {
                text = "Schema Controls"
            };

            VisualElement schemaControls = new();
            schemaControls.Add(SubGraphSerializer.SchemaGUIUtil?.DrawSchemaPortControlGUI());

            PropertyField schemaField = SubGraphSerializer.DrawSchemaFieldGUI();
            schemaField.RegisterCallback<ChangeEvent<Object>>(e =>
            {
                var prevSchemaValue = e.previousValue as SubGraphPortSchema;
                var newSchemaValue = e.newValue as SubGraphPortSchema;

                if (prevSchemaValue == newSchemaValue) return;
                if (prevSchemaValue) prevSchemaValue.OnPortsUpdated -= SubGraph.NotifyPortsChanged;

                schemaControls.Clear();

                if (!newSchemaValue)
                {
                    schemaControls.Hide();
                }
                else
                {
                    newSchemaValue.OnPortsUpdated += SubGraph.NotifyPortsChanged;
                    schemaControls.Add(SubGraphSerializer.SchemaGUIUtil.DrawSchemaPortControlGUI());
                    schemaControls.Show();
                }
            });

            schemaControlsFoldout.Add(schemaField);
            schemaControlsFoldout.Add(schemaControls);
            schemaControlsFoldout.Add(SubGraphSchemaGUIUtility.DrawSchemaUpdaterButtonGUI(() =>
            {
                if (SubGraph.Schema == null) SubGraph.NotifyPortsChanged();
                else SubGraphSerializer.SchemaGUIUtil.SchemaUpdateButtonAction();
            }));

            return schemaControlsFoldout;
        }
    }
}