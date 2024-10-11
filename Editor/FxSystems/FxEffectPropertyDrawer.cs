using Konfus.Systems.FX;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.FxSystems
{
    [CustomPropertyDrawer(typeof(Effect), useForChildren: true)]
    public class FxEffectPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Calculate rects
            var effectRect = new Rect(position.x, position.y, position.width, EditorGUI.GetPropertyHeight(property, includeChildren: true));
            
            // Draw fields - pass GUIContent.none to each so they are drawn without labels
            EditorGUI.PropertyField(effectRect, property, GUIContent.none, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded)
            {
                return property.CountInProperty() * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            }

            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}
