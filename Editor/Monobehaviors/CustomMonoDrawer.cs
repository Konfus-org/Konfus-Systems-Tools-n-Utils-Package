using Konfus.Editor.Utility;
using UnityEditor;
using UnityEngine;
namespace Konfus.Editor.Monobehaviors
{
    [CustomPropertyDrawer(typeof(MonoBehaviour), useForChildren: true)]
    public class CustomMonoDrawer : PropertyDrawer 
    {
        public override void OnGUI(Rect _, SerializedProperty property, GUIContent __) 
        {
            var childDepth = property.depth + 1;
            EditorGUILayout.PropertyField(property, includeChildren: false);
            if (!property.isExpanded) 
            {
                return;
            }
            EditorGUI.indentLevel++;
            foreach (SerializedProperty child in property) 
            {
                if (child.depth == childDepth)
                {
                    var childReadonly = child.IsReadonly();
                    if (childReadonly) GUI.enabled = false;
                    EditorGUILayout.PropertyField(child);
                    if (childReadonly) GUI.enabled = true;
                }
            }
            EditorGUI.indentLevel--;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) 
        {
            return EditorGUI.GetPropertyHeight(property, label, includeChildren: false) - 20;
        }
    }
}