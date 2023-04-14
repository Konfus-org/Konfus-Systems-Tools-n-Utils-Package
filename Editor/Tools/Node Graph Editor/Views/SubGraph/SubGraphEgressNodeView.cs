using Konfus.Systems.Node_Graph;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Konfus.Tools.NodeGraphEditor.View
{
    [NodeCustomEditor(typeof(SubGraphEgressNode))]
    public class SubgraphEgressNodeView : NodeView
    {
        private SubGraphEgressNode Target => nodeTarget as SubGraphEgressNode;
        private SubGraph SubGraph => Target.SubGraph;

        private SubGraphGUIUtility _subGraphSerializer;

        private SubGraphGUIUtility SubGraphSerializer =>
            PropertyUtils.LazyLoad(ref _subGraphSerializer, () => new SubGraphGUIUtility(SubGraph));


        protected override void DrawDefaultInspector(bool fromInspector = false)
        {
            controlsContainer.Add(DrawSubGraphControlsGUI());
            controlsContainer.Add(DrawSchemaControlsGUI());
        }

        public VisualElement DrawSubGraphControlsGUI()
        {
            var subgraphPortFoldout = new Foldout
            {
                text = "SubGraph Port Selection"
            };

            subgraphPortFoldout.Add(SubGraphSerializer.DrawEgressPortSelectorGUI());
            subgraphPortFoldout.Add(SubGraphSerializer.DrawPortUpdaterButtonGUI());

            return subgraphPortFoldout;
        }

        public VisualElement DrawSchemaControlsGUI()
        {
            var schemaFoldout = new Foldout
            {
                text = "Schema Port Selection"
            };
            VisualElement schemaControls = new();

            PropertyField schemaField = SubGraphSerializer.DrawSchemaFieldWithCallback(prop =>
            {
                if (schemaFoldout.visible && SubGraph.Schema == null)
                {
                    schemaFoldout.visible = false;
                    schemaControls.Clear();
                }
                else if (!schemaFoldout.visible && SubGraph.Schema != null)
                {
                    schemaControls.Add(SubGraphSerializer.SchemaGUIUtil.DrawEgressPortSelectorGUI());
                    schemaControls.Add(SubGraphSerializer.SchemaGUIUtil.DrawSchemaUpdaterButtonGUI());
                    schemaFoldout.visible = true;
                }
            }, false);
            schemaControls.Add(SubGraphSerializer.SchemaGUIUtil?.DrawEgressPortSelectorGUI());
            schemaControls.Add(SubGraphSerializer.SchemaGUIUtil?.DrawSchemaUpdaterButtonGUI());

            schemaFoldout.Add(schemaField);
            schemaFoldout.Add(schemaControls);

            return schemaFoldout;
        }
    }
}