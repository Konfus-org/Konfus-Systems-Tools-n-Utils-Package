using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Konfus.Utility.Attributes;

namespace Konfus.Editor.TagSelector
{
     [CustomPropertyDrawer(typeof(TagSelectorAttribute))]
     public class TagSelectorPropertyDrawer : PropertyDrawer
     {
         public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
         {
             if (property.propertyType == SerializedPropertyType.String)
             {
                 EditorGUI.BeginProperty(position, label, property);
                 
                 // Generate the taglist + custom tags
                 var tagList = new List<string> {"Nothing"};
                 tagList.AddRange(UnityEditorInternal.InternalEditorUtility.tags);
                 string propertyString = property.stringValue;
                 int index = -1;

                 // The tag is empty
                 if (propertyString == "")
                 {
                     //first index is the special Nothing entry
                     index = 0;
                 }
                 // Check if there is an entry that matches the entry and get the index
                 else
                 {
                     // Skip 0th position since that is our special 'Nothing' case...
                     for (int i = 1; i < tagList.Count; i++)
                     {
                         if (tagList[i] != propertyString) continue;
                         index = i;
                         break;
                     }
                 }

                 // Draw the popup box with the current selected index
                 index = EditorGUI.Popup(position, label.text, index, tagList.ToArray());
                 property.stringValue = index switch
                 {
                     // Adjust the actual string value of the property based on the selection
                     0 => "",
                     >= 1 => tagList[index],
                     _ => ""
                 };

                 EditorGUI.EndProperty();
             }
             else
             {
                 EditorGUI.PropertyField(position, property, label);
             }
         }
     }
}