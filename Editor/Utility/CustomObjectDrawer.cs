using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.Utility
{
    // TODO: use this to make things like readonly and showif work on arrays...
    //[CustomPropertyDrawer(typeof(object), useForChildren: true)]
    public class CustomObjectDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) 
        {
            var childDepth = property.depth + 1;
            EditorGUI.PropertyField(
                position: position,
                property: property, 
                label: label, 
                includeChildren: true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) 
        {
            return EditorGUI.GetPropertyHeight(property, label, includeChildren: true);
        }
    }
}