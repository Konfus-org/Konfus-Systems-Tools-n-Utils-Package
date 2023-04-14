using Konfus.Systems.Node_Graph;
using Konfus.Systems.Node_Graph.Schema;
using UnityEditor;
using UnityEngine.UIElements;

namespace Konfus.Tools.NodeGraphEditor
{
    [CustomEditor(typeof(SubGraphPortSchema))]
    public class SubGraphPortSchema_Inspector : Editor
    {
        private SubGraphSchemaGUIUtility _schemaSerializer;
        private SubGraphPortSchema Schema => target as SubGraphPortSchema;

        private SubGraphSchemaGUIUtility SchemaSerializer =>
            PropertyUtils.LazyLoad(ref _schemaSerializer, () => new SubGraphSchemaGUIUtility(Schema));


        public override VisualElement CreateInspectorGUI()
        {
            var schema = target as SubGraphPortSchema;

            // Create a new VisualElement to be the root of our inspector UI
            VisualElement root = new();

            // Add a simple label
            root.Add(SchemaSerializer.DrawFullSchemaGUI());

            // Return the finished inspector UI
            return root;
        }
    }
}