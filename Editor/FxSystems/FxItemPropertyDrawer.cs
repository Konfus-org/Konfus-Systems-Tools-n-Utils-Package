using System;
using System.Linq;
using System.Reflection;
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
            
            // Draw choice dropdown
            var effectTypeProperty = property.FindPropertyRelative("effectType");
            EditorGUI.BeginChangeCheck();
            {
                effectTypeProperty.intValue = EditorGUI.Popup(
                    position: new Rect(position){ height = EditorGUIUtility.singleLineHeight },
                    selectedIndex: effectTypeProperty.intValue,
                    displayedOptions: _choices);
            }

            // Set the effect type if we chose one
            if (EditorGUI.EndChangeCheck())
            {
                var effectProperty = property.FindPropertyRelative("effect");
                if (effectTypeProperty.intValue == 0) 
                {
                    // We chose none, set the effect property to null
                    effectProperty.managedReferenceValue = null;
                }
                else
                {
                    // Create chosen type and set the effect property to it
                    Type effectType = _availableEffectTypes[effectTypeProperty.intValue - 1];
                    object effect = Activator.CreateInstance(effectType);
                    effectProperty.managedReferenceValue = effect;
                }
            }

            if (effectTypeProperty.intValue != 0)
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
            _availableEffectTypes = Assembly.GetAssembly(typeof(Effect)).GetTypes()
                .Where(myType =>
                    myType.IsClass && !myType.IsAbstract && !myType.IsGenericType &&
                    myType.IsSubclassOf(typeof(Effect)))
                .ToArray();
            string[] choices = { "None" };
            _choices = choices.Union(_availableEffectTypes.Select(type => type.Name)).ToArray();
        }
    }
}
