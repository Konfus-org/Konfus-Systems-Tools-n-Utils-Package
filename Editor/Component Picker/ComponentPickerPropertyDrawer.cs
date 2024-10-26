using System;
using System.Collections.Generic;
using System.Linq;
using Konfus.Utility.Attributes;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.Component_Picker
{
    [CustomPropertyDrawer(typeof(ComponentPickerAttribute), true)]
    public class ComponentPickerPropertyDrawer : PropertyDrawer
    {
        private static string[] _choices = null;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Get gameobject this property is on
            var targetGo = ((MonoBehaviour)property.serializedObject.targetObject).gameObject;
            
            // Get filter
            var filter = ((ComponentPickerAttribute)attribute).TypeFilter ?? new [] { typeof(Component) };
            
            // Get available components
            _choices = GetAvailableComponents(targetGo, filter).ToArray();

            // Deserialize scene choice
            var selectedComponentTypeName = property.boxedValue?.GetType().Name;
            var selectionIndex = string.IsNullOrEmpty(selectedComponentTypeName) ? 0 : Array.IndexOf(_choices, selectedComponentTypeName);

            // If we can't find the component, draw property as red to signify an error
            var originalGuiColor = GUI.color;
            label.tooltip = $"The component of type {selectedComponentTypeName} is selected.";

            // Draw component choice dropdown
            EditorGUI.BeginChangeCheck();
            {
                if (selectionIndex == -1)
                {
                    selectionIndex = 0;
                }

                var labelWidth = EditorGUIUtility.labelWidth - (EditorGUI.indentLevel * 14);
                selectionIndex = EditorGUI.Popup(
                    position: new Rect()
                    {
                        position = position.position + new Vector2(labelWidth, 0), 
                        height = EditorGUIUtility.singleLineHeight, 
                        width = position.width - labelWidth
                    },
                    selectedIndex: selectionIndex,
                    displayedOptions: _choices);
            }

            // Set the new scene if we chose one
            if (EditorGUI.EndChangeCheck())
            {
                var newSelectedComponentTypeName = selectionIndex >= 0 ? _choices[selectionIndex] : null;
                var newSelectedComponent = targetGo.GetComponent(newSelectedComponentTypeName);
                property.objectReferenceValue = newSelectedComponent;
            }

            // Draw label
            EditorGUI.LabelField(position, label);

            // Set GUI color back to OG color
            GUI.color = originalGuiColor;
        }

        public override bool CanCacheInspectorGUI(SerializedProperty property)
        {
            return true;
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        private string[] GetAvailableComponents(GameObject go, Type[] filter)
        {
            var components = go.GetComponents<Component>();
            var options = new List<string>() { "None" };
            options.AddRange(components
                .Where(c => filter.Any(type => type.IsAssignableFrom(c.GetType())))
                .Select(component => component.GetType().Name));
            return options.ToArray();
        }
    }
}