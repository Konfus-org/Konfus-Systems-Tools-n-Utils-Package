using System.Collections.Generic;
using System.Linq;
using Konfus.Systems.Node_Graph.Schema;
using UnityEditor;
using UnityEngine.UIElements;
using static Konfus.Systems.Node_Graph.Schema.EdgeProcessing;


namespace Konfus.Tools.NodeGraphEditor
{
    [CustomPropertyDrawer(typeof(EdgeProcessOrderKey))]
    public class EdgeProcessOrderKeyPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty edgeProcessOrderKeyProperty)
        {
            SerializedProperty keyValueProperty =
                edgeProcessOrderKeyProperty.FindPropertyRelative(EdgeProcessOrderKey.ValueFieldName);

            string displayName = edgeProcessOrderKeyProperty.displayName;
            List<string> choices = EdgeProcessOrderBehaviorKeyValues.ToList();
            string currentValue = choices.Contains(keyValueProperty.stringValue)
                ? keyValueProperty.stringValue
                : EdgeProcessOrder.DefaultEdgeProcessOrder;

            var edgeProcessOrderField = new DropdownField(displayName, choices, currentValue);
            edgeProcessOrderField.RegisterValueChangedCallback(e =>
            {
                keyValueProperty.stringValue = e.newValue;
                edgeProcessOrderKeyProperty.serializedObject.ApplyModifiedProperties();
            });

            return edgeProcessOrderField;
        }
    }
}