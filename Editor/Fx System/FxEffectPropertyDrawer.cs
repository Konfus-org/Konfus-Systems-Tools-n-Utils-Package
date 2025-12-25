using Konfus.Fx_System.Effects;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.Fx_System
{
    [CustomPropertyDrawer(typeof(Effect), true)]
    internal class FxEffectPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Simple cyclic dependency check
            if (property.managedReferenceValue is MultiEffect multiEffect)
            {
                // If cyclic dependency found, set it to null
                if (property.serializedObject.targetObject == multiEffect.FxSystem)
                {
                    property.managedReferenceValue = new MultiEffect();
                    Debug.LogError(
                        $"Cyclic dependency detected, reseting multi-effect on the game object {property.serializedObject.targetObject.name}'s fx system!");
                }
            }

            // Draw fields - pass GUIContent.none to each so they are drawn without labels
            float effectHeight = EditorGUI.GetPropertyHeight(property, true);
            EditorGUI.PropertyField(
                new Rect(position.x, position.y, position.width, effectHeight),
                property,
                GUIContent.none,
                true);

            // Draw duration for non configurable durations
            var effect = (Effect)property.managedReferenceValue;
            float effectDuration = effect?.Duration ?? 0;
            if (property.managedReferenceValue is not ConfigurableDurationEffect &&
                property.isExpanded && effectDuration <= 60)
            {
                EditorGUI.indentLevel++;
                bool wasEnabled = GUI.enabled;
                GUI.enabled = false;

                float durationYPos = position.y + effectHeight + EditorGUIUtility.standardVerticalSpacing;

                // Draw duration label
                EditorGUI.LabelField(
                    new Rect(position.x, durationYPos, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight),
                    new GUIContent(
                        "Duration",
                        "The time to play this effect in seconds before playing the next effect. This duration is not editable because it is calculated by the effect."));

                // Draw duration slider
                EditorGUI.Slider(
                    new Rect(position.x + EditorGUIUtility.labelWidth - 12f, durationYPos,
                        position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight),
                    effectDuration,
                    0,
                    60);

                GUI.enabled = wasEnabled;
                EditorGUI.indentLevel--;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded)
            {
                if (property.managedReferenceValue is not ConfigurableDurationEffect)
                {
                    return EditorGUI.GetPropertyHeight(property, label, true) + EditorGUIUtility.singleLineHeight +
                           EditorGUIUtility.standardVerticalSpacing * 2;
                }

                return EditorGUI.GetPropertyHeight(property, label, true) +
                       EditorGUIUtility.standardVerticalSpacing * 2;
            }

            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}