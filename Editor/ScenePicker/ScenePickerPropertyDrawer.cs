using System.Linq;
using Konfus.Utility.Attributes;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.ScenePicker
{
    [CustomPropertyDrawer(typeof(ScenePickerAttribute), true)]
    public class ScenePickerPropertyDrawer : PropertyDrawer
    {
        private static string[] _choices = null;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Get available scenes if we haven't already
            //CacheChoicesIfNotAlreadyCached(); // For now disabling caching, as causes issues if scene are renamed or moved....
            _choices = GetAvailableScenePaths();
            
            // Deserialize scene choice
            var effectTypeIndex = string.IsNullOrEmpty(property.stringValue) 
                ? 0
                : _choices.ToList().IndexOf(GetDisplayPath(property.stringValue));

            // If we can't find the scene, draw property as red to signify an error
            var originalGuiColor = GUI.color;
            if (effectTypeIndex == -1)
            {
                var errorColor = Color.red;
                GUI.color = errorColor;
                label.tooltip = $"ERROR: Scene picker could not find the scene {property.name}, ensure its been added to the scenes list.";
            }
            else
            {
                label.tooltip = $"Scene at index {effectTypeIndex} of with the path {property.stringValue} is selected.";
            }

            // Draw scene choice dropdown
            EditorGUI.BeginChangeCheck();
            {
                if (effectTypeIndex == -1)
                {
                    effectTypeIndex = 0;
                }
                effectTypeIndex = EditorGUI.Popup(
                    position: new Rect(){ position = position.position + new Vector2(EditorGUIUtility.labelWidth + 1, 0), height = EditorGUIUtility.singleLineHeight, width = EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth },
                    selectedIndex: effectTypeIndex,
                    displayedOptions: _choices);
            }

            // Set the new scene if we chose one
            if (EditorGUI.EndChangeCheck())
            {
                property.stringValue = effectTypeIndex >= 0 ? $"Assets/{_choices[effectTypeIndex]}.unity" : null;
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

        private void CacheChoicesIfNotAlreadyCached()
        {
            if (_choices == null || EditorBuildSettings.scenes.Length != _choices.Length)
            {
                _choices = GetAvailableScenePaths();
            }
        }

        private string[] GetAvailableScenePaths()
        {
            var scenePaths = EditorBuildSettings.scenes.Select(scene => scene.path).ToArray();
            return scenePaths.Select(GetDisplayPath).ToArray();
        }

        private string GetDisplayPath(string fullPath)
        {
            return fullPath.Replace("Assets/", string.Empty).Replace(".unity", string.Empty);
        }
    }
}