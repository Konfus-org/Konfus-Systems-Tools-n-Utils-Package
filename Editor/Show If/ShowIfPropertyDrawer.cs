using Konfus.Editor.Utility;
using Konfus.Utility.Attributes;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.ShowIf
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute), true)]
    public class ShowIfPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIf = (ShowIfAttribute)attribute;

            // Get the condition field
            var conditionProperty = GetConditionProperty(property, showIf);
            if (conditionProperty == null)
            {
                Debug.LogError($"Condition field '{showIf.ConditionalSourceField}' not found in {property.serializedObject.targetObject.GetType()}.");
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            // Determine if the property should be displayed
            bool conditionMet = GetConditionValue(conditionProperty) == showIf.ExpectedValue;

            if (conditionMet)
            {
                // Show the property if the condition is met
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIf = (ShowIfAttribute)attribute;
            var conditionProperty = GetConditionProperty(property, showIf);

            // Couldn't find condition property...
            if (conditionProperty == null) return EditorGUI.GetPropertyHeight(property, label, true);
            
            // Determine if the property should be displayed
            bool conditionMet = GetConditionValue(conditionProperty) == showIf.ExpectedValue;

            // Only reserve height if the condition is met
            return conditionMet ? EditorGUI.GetPropertyHeight(property, label, true) : 0;
        }
        
        public override bool CanCacheInspectorGUI(SerializedProperty property)
        {
            return true;
        }
        
        private static SerializedProperty GetConditionProperty(SerializedProperty property, ShowIfAttribute showIf)
        {
            // Try finding on serialized obj
            var conditionProperty = property.serializedObject.FindProperty(showIf.ConditionalSourceField);
            if (conditionProperty != null) return conditionProperty;
            
            // Try finding via parent if going through serialized obj failed...
            var parent = property.FindParentProperty();
            conditionProperty = parent.FindPropertyRelative(showIf.ConditionalSourceField);
            return conditionProperty;
        }

        private bool GetConditionValue(SerializedProperty conditionProperty)
        {
            // Check the type of the condition property and get its value
            switch (conditionProperty.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    return conditionProperty.boolValue;
                case SerializedPropertyType.Enum:
                    return conditionProperty.enumValueIndex == 1; // Assume true if enum index is 1
                default:
                    Debug.LogError($"Unsupported condition property type '{conditionProperty.propertyType}' in ShowIf attribute.");
                    return false;
            }
        }
    }
}