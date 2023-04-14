using Konfus.Systems.Node_Graph;
using Konfus.Tools.NodeGraphEditor.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Konfus.Tools.NodeGraphEditor.View
{
    [NodeCustomEditor(typeof(SubGraphNode))]
    public class SubGraphNodeView : NodeView
    {
        private SubGraphNode Target => nodeTarget as SubGraphNode;
        private SubGraph SubGraph => Target.SubGraph;

        private SubGraphGUIUtility SubGraphSerializer => SubGraph ? new SubGraphGUIUtility(SubGraph) : null;

        public override void OnDoubleClicked()
        {
            if (SubGraph == null) return;

            EditorWindow.GetWindow<SubGraphWindow>().InitializeGraph(SubGraph);
        }

        protected override void DrawDefaultInspector(bool fromInspector = false)
        {
            VisualElement subGraphGUIContainer = new();
            if (SubGraph != null)
                subGraphGUIContainer.Add(DrawSubGraphGUI());


            controlsContainer.Add(subGraphGUIContainer);
            controlsContainer.Add(DrawSubGraphField(subGraphGUIContainer));
        }

        protected VisualElement DrawSubGraphGUI()
        {
            VisualElement subGraphGUIContainer = new();

            subGraphGUIContainer.Add(SubGraphSerializer?.DrawSubGraphPortControlGUI());
            subGraphGUIContainer.Add(DrawSchemaControls());

            return subGraphGUIContainer;
        }

        protected VisualElement DrawSchemaControls()
        {
            Foldout schemaControlFoldout = new() {text = "Schema Port Control"};
            PropertyField schemaField = SubGraphSerializer.DrawSchemaFieldWithCallback(prop =>
            {
                // We check visibility due to this callback being called twice
                if (schemaControlFoldout.visible && SubGraph.Schema == null)
                {
                    schemaControlFoldout.visible = false;
                    schemaControlFoldout.Clear();
                }
                else if (!schemaControlFoldout.visible && SubGraph.Schema != null)
                {
                    schemaControlFoldout.Add(SubGraphSerializer.SchemaGUIUtil.DrawFullSchemaGUI());
                    schemaControlFoldout.visible = true;
                }
            }, false);
            schemaControlFoldout.Add(schemaField);
            schemaControlFoldout.Add(SubGraphSerializer.SchemaGUIUtil?.DrawFullSchemaGUI());

            return schemaControlFoldout;
        }

        protected VisualElement DrawSubGraphField(VisualElement subGraphGUIContainer)
        {
            ObjectField subGraphField = new("SubGraph")
            {
                objectType = typeof(SubGraph),
                value = Target.SubGraph
            };
            subGraphField.RegisterValueChangedCallback(prop =>
            {
                if (prop.previousValue == prop.newValue) return;

                Target.SetPrivateFieldValue(SubGraphNode.SubGraphField, prop.newValue as SubGraph);
                Target.UpdateAllPortsLocal();
                Target.RepaintTitle();

                subGraphGUIContainer.Clear();
                if (prop.newValue == null)
                {
                    subGraphGUIContainer.Hide();
                }
                else
                {
                    subGraphGUIContainer.Add(DrawSubGraphGUI());
                    subGraphGUIContainer.Show();
                }
            });
            return subGraphField;
        }
    }
}