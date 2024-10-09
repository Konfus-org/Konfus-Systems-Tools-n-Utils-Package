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
            CacheChoicesIfNotAlreadyCached();
            
            // Deserialize the effect type and index
            var effectTypeIndex = DeserializeEffectType(property, out var effectTypeProperty);

            // Draw choice dropdown
            EditorGUI.BeginChangeCheck();
            {
                effectTypeIndex = EditorGUI.Popup(
                    position: new Rect(position){ height = EditorGUIUtility.singleLineHeight },
                    selectedIndex: effectTypeIndex,
                    displayedOptions: _choices);
            }

            // Set the effect type if we chose one
            var fxItemsProperty = property.serializedObject.FindProperty("fxItems");
            bool hasBeenDuped = HasBeenDuplicated(property, fxItemsProperty);
            if (EditorGUI.EndChangeCheck() || hasBeenDuped)
            {
                var effectProperty = property.FindPropertyRelative("effect");
                CreateEffect(effectTypeIndex, effectProperty, effectTypeProperty);
                if (hasBeenDuped)
                {
                    // This is a hack... for some reason we reset the original value instead of the duped value so for now, we will just swap them :,)
                    fxItemsProperty.MoveArrayElement(fxItemsProperty.arraySize - 2, fxItemsProperty.arraySize - 1);
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

        public override bool CanCacheInspectorGUI(SerializedProperty property)
        {
            return true;
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

        private void CacheChoicesIfNotAlreadyCached()
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
        
        private static void CreateEffect(int effectTypeIndex, SerializedProperty effectProperty, SerializedProperty effectTypeProperty)
        {
            if (effectTypeProperty.stringValue == "None") 
            {
                // We chose none, set the effect property to null and save choice
                effectProperty.managedReferenceValue = null;
                effectTypeProperty.stringValue = "None";
            }
            else
            {
                // Create chosen type and set the effect property to it and save choice
                var effectType = _availableEffectTypes[effectTypeIndex - 1];
                object effect = Activator.CreateInstance(effectType);
                effectProperty.managedReferenceValue = effect;
                effectTypeProperty.stringValue = effectType.Name;
            }
        }

        private static int DeserializeEffectType(SerializedProperty property, out SerializedProperty effectTypeProperty)
        {
            int effectTypeIndex = 0;
            effectTypeProperty = property.FindPropertyRelative("effectType");
            if (effectTypeProperty.stringValue != string.Empty)
            {
                var effectTypeName = effectTypeProperty.stringValue;
                Type effectType = _availableEffectTypes.First(type => type.Name == effectTypeName);
                effectTypeIndex = Array.IndexOf(_availableEffectTypes, effectType) + 1;
            }

            return effectTypeIndex;
        }
        
        private static bool HasBeenDuplicated(SerializedProperty property, SerializedProperty fxItemsProperty)
        {
            if (fxItemsProperty is not { arraySize: > 1 }) return false;
            
            var fxItemPotentialDuplicateProperty = fxItemsProperty.GetArrayElementAtIndex(fxItemsProperty.arraySize - 1);
            if (fxItemPotentialDuplicateProperty.contentHash != property.contentHash) return false;
            
            var fxItemDuplicatedProperty = fxItemsProperty.GetArrayElementAtIndex(fxItemsProperty.arraySize - 2);
            var hasBeenDuped = fxItemPotentialDuplicateProperty.contentHash == fxItemDuplicatedProperty.contentHash;
            if (hasBeenDuped)
            {
                return true;
            }

            return false;
        }
    }
}
