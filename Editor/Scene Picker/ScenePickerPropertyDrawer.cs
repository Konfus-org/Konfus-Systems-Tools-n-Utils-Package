using System.Collections.Generic;
using System.Linq;
using Konfus.Utility.Attributes;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.Scene_Picker
{
    [CustomPropertyDrawer(typeof(ScenePickerAttribute), true)]
    internal class ScenePickerPropertyDrawer : PropertyDrawer
    {
        private static string[]? _choices;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Get available scenes if we haven't already
            //CacheChoicesIfNotAlreadyCached(); // For now disabling caching, as causes issues if scene are renamed or moved....
            _choices = GetAvailableScenePaths();

            // Deserialize scene choice
            int selectionIndex = string.IsNullOrEmpty(property.stringValue)
                ? 0
                : _choices.ToList().IndexOf(GetDisplayPath(property.stringValue));

            // If we can't find the scene, draw property as red to signify an error
            Color originalGuiColor = GUI.color;
            if (selectionIndex == -1)
            {
                // Need to add missing choice to list then update the index to display it...
                List<string> newChoicesWithError = _choices.ToList();
                newChoicesWithError.Add(GetDisplayPath(property.stringValue));
                selectionIndex = newChoicesWithError.IndexOf(GetDisplayPath(property.stringValue));
                _choices = newChoicesWithError.ToArray();

                // Make property red to signify error and update tooltip to say whats wrong!
                Color errorColor = Color.red;
                GUI.color = errorColor;
                label.tooltip =
                    $"ERROR: Scene picker could not find the scene {property.stringValue}, ensure its been added to the scenes list.";
            }
            else
                label.tooltip = $"Scene at index {selectionIndex} of with the path {property.stringValue} is selected.";

            // Draw scene choice dropdown
            EditorGUI.BeginChangeCheck();
            {
                if (selectionIndex == -1) selectionIndex = 0;

                float labelWidth = EditorGUIUtility.labelWidth - EditorGUI.indentLevel * 14;
                selectionIndex = EditorGUI.Popup(
                    new Rect
                    {
                        position = position.position + new Vector2(labelWidth, 0),
                        height = EditorGUIUtility.singleLineHeight,
                        width = position.width - labelWidth
                    },
                    selectionIndex,
                    _choices);
            }

            // Set the new scene if we chose one
            if (EditorGUI.EndChangeCheck())
                property.stringValue = selectionIndex >= 0 ? $"Assets/{_choices[selectionIndex]}.unity" : null;

            // Draw label
            EditorGUI.LabelField(position, label);

            // Set GUI color back to OG color
            GUI.color = originalGuiColor;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        /*private void CacheChoicesIfNotAlreadyCached()
        {
            if (_choices == null || EditorBuildSettings.scenes.Length != _choices.Length)
                _choices = GetAvailableScenePaths();
        }*/

        private string[] GetAvailableScenePaths()
        {
            string[] scenePaths = EditorBuildSettings.scenes.Select(scene => scene.path).ToArray();
            return scenePaths.Select(GetDisplayPath).ToArray();
        }

        private string GetDisplayPath(string fullPath)
        {
            return fullPath.Replace("Assets/", string.Empty).Replace(".unity", string.Empty);
        }
    }
}