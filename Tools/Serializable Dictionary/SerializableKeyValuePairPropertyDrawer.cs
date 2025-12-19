using Konfus.Utility.Custom_Types;
using UnityEditor;
using UnityEngine;

namespace Konfus.Tools.SerializableDictionary
{
    [CustomPropertyDrawer(typeof(SerializableKeyValuePair<,>), true)]
    public class SerializableKeyValuePairPropertyDrawer : PropertyDrawer
    {
        private const string _keyFieldName = "key";
        private const string _valueFieldName = "value";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var keyProperty = property.FindPropertyRelative(_keyFieldName);
            var valueProperty = property.FindPropertyRelative(_valueFieldName);

            DrawKeyValuePairHelper.DrawKeyValueLine(keyProperty, valueProperty, position, 0);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var keyProperty = property.FindPropertyRelative(_keyFieldName);
            var valueProperty = property.FindPropertyRelative(_valueFieldName);

            float keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
            float valuePropertyHeight = valueProperty != null ? EditorGUI.GetPropertyHeight(valueProperty) : 0f;

            float lineHeight;
            if (DrawKeyValuePairHelper.CanPropertyBeExpanded(valueProperty))
            {
                lineHeight = keyPropertyHeight + valuePropertyHeight;
            }
            else
            {
                lineHeight = Mathf.Max(keyPropertyHeight, valuePropertyHeight);
            }
            return lineHeight;
        }
    }

    public static class DrawKeyValuePairHelper
    {
        private const float _indentWidth = 15f;
        private static readonly GUIContent Content = new GUIContent();

        public static float DrawKeyValueLine(SerializedProperty keyProperty, SerializedProperty valueProperty, Rect linePosition, int index)
        {
            bool keyCanBeExpanded = CanPropertyBeExpanded(keyProperty);

            if (valueProperty != null)
            {
                bool valueCanBeExpanded = CanPropertyBeExpanded(valueProperty);

                if (!keyCanBeExpanded && valueCanBeExpanded)
                {
                    return DrawKeyValueLineExpand(keyProperty, valueProperty, linePosition);
                }
                else
                {
                    var keyLabel = keyCanBeExpanded ? ("Key " + index.ToString()) : "";
                    var valueLabel = valueCanBeExpanded ? ("Value " + index.ToString()) : "";
                    return DrawKeyValueLineSimple(keyProperty, valueProperty, keyLabel, valueLabel, linePosition);
                }
            }
            else
            {
                if (!keyCanBeExpanded)
                {
                    return DrawKeyLine(keyProperty, linePosition, null);
                }
                else
                {
                    var keyLabel = string.Format("{0} {1}", ObjectNames.NicifyVariableName(keyProperty.type), index);
                    return DrawKeyLine(keyProperty, linePosition, keyLabel);
                }
            }
        }

        public static bool CanPropertyBeExpanded(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Generic:
                case SerializedPropertyType.Vector4:
                case SerializedPropertyType.Quaternion:
                    return true;
                default:
                    return false;
            }
        }

        private static float DrawKeyValueLineSimple(SerializedProperty keyProperty, SerializedProperty valueProperty, string keyLabel, string valueLabel, Rect linePosition)
        {
            float labelWidth = EditorGUIUtility.labelWidth;
            float labelWidthRelative = labelWidth / linePosition.width;
            float padding = 20;
            
            float keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
            var keyPosition = linePosition;
            keyPosition.height = keyPropertyHeight;
            keyPosition.width = labelWidth - _indentWidth - padding;
            EditorGUIUtility.labelWidth = keyPosition.width * labelWidthRelative;
            EditorGUI.PropertyField(keyPosition, keyProperty, TempContent(keyLabel), true);

            float valuePropertyHeight = EditorGUI.GetPropertyHeight(valueProperty);
            var valuePosition = linePosition;
            valuePosition.height = valuePropertyHeight;
            valuePosition.xMin += labelWidth;
            EditorGUIUtility.labelWidth = valuePosition.width * labelWidthRelative;
            EditorGUI.indentLevel--;
            EditorGUI.PropertyField(valuePosition, valueProperty, TempContent(valueLabel), true);
            EditorGUI.indentLevel++;

            EditorGUIUtility.labelWidth = labelWidth;

            return Mathf.Max(keyPropertyHeight, valuePropertyHeight);
        }

        private static float DrawKeyValueLineExpand(SerializedProperty keyProperty, SerializedProperty valueProperty, Rect linePosition)
        {
            float labelWidth = EditorGUIUtility.labelWidth;

            float keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
            var keyPosition = linePosition;
            keyPosition.height = keyPropertyHeight;
            keyPosition.width = labelWidth - _indentWidth;
            EditorGUI.PropertyField(keyPosition, keyProperty, GUIContent.none, true);

            float valuePropertyHeight = EditorGUI.GetPropertyHeight(valueProperty);
            var valuePosition = linePosition;
            valuePosition.height = valuePropertyHeight;
            valuePosition.yMin += keyPropertyHeight;
            valuePosition.yMin += EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(valuePosition, valueProperty, GUIContent.none, true);

            EditorGUIUtility.labelWidth = labelWidth;

            return Mathf.Max(keyPropertyHeight, valuePropertyHeight);
        }

        private static float DrawKeyLine(SerializedProperty keyProperty, Rect linePosition, string keyLabel)
        {
            float keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
            var keyPosition = linePosition;
            keyPosition.height = keyPropertyHeight;
            keyPosition.width = linePosition.width;

            var keyLabelContent = keyLabel != null ? TempContent(keyLabel) : GUIContent.none;
            EditorGUI.PropertyField(keyPosition, keyProperty, keyLabelContent, true);

            return keyPropertyHeight;
        }

        private static GUIContent TempContent(string text)
        {
            Content.text = text;
            return Content;
        }
    }
}