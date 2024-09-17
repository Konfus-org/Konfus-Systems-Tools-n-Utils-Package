using System.Collections;
using System.Collections.Generic;
using Konfus.Utility.Attributes;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.ShowIf
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute), true)]
    public class ShowIfPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIfAttribute = (ShowIfAttribute)attribute;
            bool show = GetConditionalSourceField(property, showIfAttribute);

            // if is enable draw the label, else hide it
            if (show) EditorGUI.PropertyField(position, property, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIfAttribute = (ShowIfAttribute)attribute;
            bool show = GetConditionalSourceField(property, showIfAttribute);

            // if is enable draw the label
            if (show)
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }
            
            // else hide it
            return -EditorGUIUtility.standardVerticalSpacing;
        }
        
        public override bool CanCacheInspectorGUI(SerializedProperty property)
        {
            return true;
        }

        /// <summary>
        /// Get if the conditional what expected is true.
        /// </summary>
        /// <param name="property"> is used for get the value of the property and check if return enable true or false </param>
        /// <param name="showIfAttribute"> is the attribute what contains the values what we need </param>
        /// <returns> only if the field y is same to the value expected return true</returns>
        private bool GetConditionalSourceField(SerializedProperty property, ShowIfAttribute showIfAttribute)
        {
            bool show = false;
            SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(showIfAttribute.ConditionalSourceField);

            if (sourcePropertyValue != null)
            {
                show = sourcePropertyValue.boolValue;
                show = show == showIfAttribute.ExpectedValue;
            }
            else
            {
                string warning = $"[{nameof(ShowIfAttribute)}] Unable to find conditional source field: " +
                                 $"{showIfAttribute.ConditionalSourceField} on object: {property.serializedObject.targetObject}";
                Debug.LogWarning(warning);
            }

            return show;
        }
        
        private static object GetParentObject(SerializedProperty property)
        {
            string[] path = property.propertyPath.Split('.');
     
            object propertyObject = property.serializedObject.targetObject;
            object propertyParent = null;
            for (int i = 0; i < path.Length; ++i)
            {
                if (path[i] == "Array")
                {
                    int index = path[i + 1][path[i + 1].Length - 2] - '0';
                    propertyObject = ((IList)propertyObject)[index];
                    ++i;
                }
                else
                {
                    propertyParent = propertyObject;
                    propertyObject = propertyObject.GetType().GetField(path[i]).GetValue(propertyObject);
                }
            }
     
            return propertyParent;
        }
    }
}