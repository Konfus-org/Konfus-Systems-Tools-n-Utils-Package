using System;
using System.Linq;
using Konfus.Systems.FX;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.FxSystems
{
    [CustomPropertyDrawer(typeof(FxItem), useForChildren: false)]
    public class FxItemPropertyDrawer : PropertyDrawer
    {
        private static string[] _choices = null;
        private static Type[] _availableEffectTypes = null;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Initialize if we haven't already...
            Initialize();
            
            // Deserialize the effect type index
            Type effectType = null;
            int effectTypeIndex = 0;
            SerializedProperty effectTypeProperty = property.FindPropertyRelative("effectType");
            if (effectTypeProperty.stringValue != string.Empty)
            {
                effectType = _availableEffectTypes.First(type => type.Name == effectTypeProperty.stringValue);
                effectTypeIndex = Array.IndexOf(_availableEffectTypes, effectType) + 1;
            }
            
            // Draw choice dropdown
            EditorGUI.BeginChangeCheck();
            {
                effectTypeIndex = EditorGUI.Popup(
                    position: new Rect(position){ height = EditorGUIUtility.singleLineHeight },
                    selectedIndex: effectTypeIndex,
                    displayedOptions: _choices);
            }

            // Set the effect type if we chose one
            if (EditorGUI.EndChangeCheck())
            {
                var effectProperty = property.FindPropertyRelative("effect");
                if (effectTypeProperty.stringValue == "None") 
                {
                    // We chose none, set the effect property to null and save choice
                    effectProperty.managedReferenceValue = null;
                    effectTypeProperty.stringValue = "None";
                }
                else
                {
                    // Create chosen type and set the effect property to it and save choice
                    effectType = _availableEffectTypes[effectTypeIndex - 1];
                    object effect = Activator.CreateInstance(effectType);
                    effectProperty.managedReferenceValue = effect;
                    effectTypeProperty.stringValue = effectType.Name;
                }
            }

            if (effectTypeIndex != 0)
            {
                // Draw effect property if our choice is something other than none
                var effectPos = new Rect(position)
                {
                    width = position.width,
                    height = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("effect"))
                };
                EditorGUI.PropertyField(effectPos, property.FindPropertyRelative("effect"), GUIContent.none);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var effectProperty = property.FindPropertyRelative("effect");
            if (effectProperty.isExpanded)
            {
                return effectProperty.CountInProperty() * EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + 2f;
            }

            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        private void Initialize()
        {
            // We've already initialized return
            if (_choices != null) return;
            
            // Get available choices and types
            _availableEffectTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(type => typeof(IEffect).IsAssignableFrom(type) && !type.IsAbstract && !type.IsGenericType)
                .ToArray();
            string[] choices = { "None" };
            _choices = choices.Union(_availableEffectTypes.Select(type => type.Name)).ToArray();
        }
    }
}
