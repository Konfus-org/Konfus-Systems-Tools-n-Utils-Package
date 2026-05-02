using System;
using Konfus.States;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.States
{
    [CustomPropertyDrawer(typeof(StateReference))]
    internal class StateReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var stateNameProperty = property.FindPropertyRelative("stateName");
            if (stateNameProperty == null)
            {
                EditorGUI.LabelField(position, label.text, "State reference is invalid.");
                return;
            }

            var stateList = StateEditorUtility.GetStateList(property);
            var options = StateEditorUtility.GetStateOptions(stateList);
            var currentValue = stateNameProperty.stringValue;
            var popupOptions = BuildPopupOptions(options, currentValue, out var selectedIndex);

            EditorGUI.BeginProperty(position, label, property);
            var nextIndex = EditorGUI.Popup(position, label.text, selectedIndex, popupOptions);
            EditorGUI.EndProperty();

            stateNameProperty.stringValue = nextIndex switch
            {
                0 => string.Empty,
                _ when options.Length == 0 => currentValue,
                _ when currentValue.Length > 0 && FindOptionIndex(options, currentValue) < 0 && nextIndex == popupOptions.Length - 1 => currentValue,
                _ => options[Mathf.Clamp(nextIndex - 1, 0, options.Length - 1)]
            };
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        private static string[] BuildPopupOptions(string[] options, string currentValue, out int selectedIndex)
        {
            var currentIndex = FindOptionIndex(options, currentValue);
            var hasMissingValue = !string.IsNullOrWhiteSpace(currentValue) && currentIndex < 0;
            var popupOptions = new string[options.Length + (hasMissingValue ? 2 : 1)];
            popupOptions[0] = "<None>";

            for (var i = 0; i < options.Length; i++)
            {
                popupOptions[i + 1] = options[i];
            }

            if (hasMissingValue)
            {
                popupOptions[^1] = $"<Missing> {currentValue}";
                selectedIndex = popupOptions.Length - 1;
                return popupOptions;
            }

            selectedIndex = string.IsNullOrWhiteSpace(currentValue)
                ? 0
                : Mathf.Max(0, currentIndex + 1);

            return popupOptions;
        }

        private static int FindOptionIndex(string[] options, string currentValue)
        {
            for (var i = 0; i < options.Length; i++)
            {
                if (string.Equals(options[i], currentValue, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
