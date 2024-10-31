using Konfus.Systems.Fx_System.Effects;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.Fx_System
{
    [CustomPropertyDrawer(typeof(Effect), useForChildren: true)]
    public class FxEffectPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Draw fields - pass GUIContent.none to each so they are drawn without labels
            var effectHeight = EditorGUI.GetPropertyHeight(property, includeChildren: true);
            EditorGUI.PropertyField(
                position: new Rect(position.x, position.y, position.width, effectHeight), 
                property: property, 
                label: GUIContent.none, 
                includeChildren: true);
            
            // Draw duration for non configurable durations
            var effect = (Effect)property.managedReferenceValue;
            var effectDuration = effect?.Duration ?? 0;
            if (property.managedReferenceValue is not ConfigurableDurationEffect && 
                property.isExpanded && effectDuration <= 60)
            {
                bool wasEnabled = GUI.enabled;
                GUI.enabled = false;

                var durationYPos = position.y + effectHeight + EditorGUIUtility.standardVerticalSpacing;
                
                // Draw duration label
                EditorGUI.LabelField(
                    position: new Rect(position.x + 30, durationYPos, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight), 
                    label: new GUIContent(
                        text: "Duration", 
                        tooltip: "The time to play this effect in seconds before playing the next effect. This duration is not editable because it is calculated by the effect."));

                // Draw duration slider
                var labelWidth = EditorGUIUtility.labelWidth + 18;
                EditorGUI.Slider(
                    position: new Rect(position.x + labelWidth, durationYPos, position.width - labelWidth, EditorGUIUtility.singleLineHeight),
                    value: effectDuration, 
                    leftValue: 0,
                    rightValue: 60);

                GUI.enabled = wasEnabled;
            }
            
            // Simple cyclic dependency check
            if (property.managedReferenceValue is MultiEffect multiEffect && 
                property.serializedObject.targetObject == multiEffect.FxSystem.gameObject)
            {
                // If cyclic dependency found, set it to null
                property.serializedObject.FindProperty("FxSystem").objectReferenceValue = null;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded)
            {
                if (property.managedReferenceValue is not ConfigurableDurationEffect)
                {
                    return ((property.CountInProperty() + 1) * 
                            (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing));
                }
                
                return (property.CountInProperty() *
                        (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing));
            }

            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}
