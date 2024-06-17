using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Konfus.Utility.Custom_Types.Editor
{
    [CustomPropertyDrawer(typeof(SerializableHashSet<>), true)]
    public class SerializableHashSetPropertyDrawer : PropertyDrawer
    {
        private const string _keysFieldName = "keys";
        private const string _valuesFieldName = "values";
        private const float _indentWidth = 15f;

        private static readonly GUIContent IconPlus = IconContent("Toolbar Plus", "Add entry");
        private static readonly GUIContent IconMinus = IconContent("Toolbar Minus", "Remove entry");
        private static readonly GUIContent WarningIconConflict = IconContent("console.warnicon.sml", "Conflicting key, this entry will be lost");
        private static readonly GUIContent WarningIconOther = IconContent("console.infoicon.sml", "Conflicting key");
        private static readonly GUIContent WarningIconNull = IconContent("console.warnicon.sml", "Null key, this entry will be lost");
        private static readonly GUIStyle ButtonStyle = GUIStyle.none;
        private static readonly GUIContent Content = new GUIContent();
        
        private static readonly Dictionary<PropertyIdentity, ConflictState> ConflictStateDict = new Dictionary<PropertyIdentity, ConflictState>();
        private static readonly Dictionary<SerializedPropertyType, PropertyInfo> SerializedPropertyValueAccessorsDict;

        static SerializableHashSetPropertyDrawer()
        {
            Dictionary<SerializedPropertyType, string> serializedPropertyValueAccessorsNameDict = new Dictionary<SerializedPropertyType, string>() {
                { SerializedPropertyType.Integer, "intValue" },
                { SerializedPropertyType.Boolean, "boolValue" },
                { SerializedPropertyType.Float, "floatValue" },
                { SerializedPropertyType.String, "stringValue" },
                { SerializedPropertyType.Color, "colorValue" },
                { SerializedPropertyType.ObjectReference, "objectReferenceValue" },
                { SerializedPropertyType.LayerMask, "intValue" },
                { SerializedPropertyType.Enum, "intValue" },
                { SerializedPropertyType.Vector2, "vector2Value" },
                { SerializedPropertyType.Vector3, "vector3Value" },
                { SerializedPropertyType.Vector4, "vector4Value" },
                { SerializedPropertyType.Rect, "rectValue" },
                { SerializedPropertyType.ArraySize, "intValue" },
                { SerializedPropertyType.Character, "intValue" },
                { SerializedPropertyType.AnimationCurve, "animationCurveValue" },
                { SerializedPropertyType.Bounds, "boundsValue" },
                { SerializedPropertyType.Quaternion, "quaternionValue" },
            };
            Type serializedPropertyType = typeof(SerializedProperty);

            SerializedPropertyValueAccessorsDict = new Dictionary<SerializedPropertyType, PropertyInfo>();
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;

            foreach (var kvp in serializedPropertyValueAccessorsNameDict)
            {
                PropertyInfo propertyInfo = serializedPropertyType.GetProperty(kvp.Value, flags);
                SerializedPropertyValueAccessorsDict.Add(kvp.Key, propertyInfo);
            }
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);

            Action buttonAction = Action.None;
            int buttonActionIndex = 0;

            var keyArrayProperty = property.FindPropertyRelative(_keysFieldName);
            var valueArrayProperty = property.FindPropertyRelative(_valuesFieldName);

            ConflictState conflictState = GetConflictState(property);

            if (conflictState.ConflictIndex != -1)
            {
                keyArrayProperty.InsertArrayElementAtIndex(conflictState.ConflictIndex);
                var keyProperty = keyArrayProperty.GetArrayElementAtIndex(conflictState.ConflictIndex);
                SetPropertyValue(keyProperty, conflictState.ConflictKey);
                keyProperty.isExpanded = conflictState.ConflictKeyPropertyExpanded;

                if (valueArrayProperty != null)
                {
                    valueArrayProperty.InsertArrayElementAtIndex(conflictState.ConflictIndex);
                    var valueProperty = valueArrayProperty.GetArrayElementAtIndex(conflictState.ConflictIndex);
                    SetPropertyValue(valueProperty, conflictState.ConflictValue);
                    valueProperty.isExpanded = conflictState.ConflictValuePropertyExpanded;
                }
            }

            var buttonWidth = ButtonStyle.CalcSize(IconPlus).x;

            var labelPosition = position;
            labelPosition.height = EditorGUIUtility.singleLineHeight;
            if (property.isExpanded)
                labelPosition.xMax -= ButtonStyle.CalcSize(IconPlus).x;

            EditorGUI.PropertyField(labelPosition, property, label, false);
            // property.isExpanded = EditorGUI.Foldout(labelPosition, property.isExpanded, label);
            if (property.isExpanded)
            {
                var buttonPosition = position;
                buttonPosition.xMin = buttonPosition.xMax - buttonWidth;
                buttonPosition.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.BeginDisabledGroup(conflictState.ConflictIndex != -1);
                if (GUI.Button(buttonPosition, IconPlus, ButtonStyle))
                {
                    buttonAction = Action.Add;
                    buttonActionIndex = keyArrayProperty.arraySize;
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.indentLevel++;
                var linePosition = position;
                linePosition.y += EditorGUIUtility.singleLineHeight;
                linePosition.xMax -= buttonWidth;

                foreach (var entry in EnumerateEntries(keyArrayProperty, valueArrayProperty))
                {
                    var keyProperty = entry.KeyProperty;
                    var valueProperty = entry.ValueProperty;
                    int i = entry.Index;

                    float lineHeight = DrawKeyValueLine(keyProperty, valueProperty, linePosition, i);

                    buttonPosition = linePosition;
                    buttonPosition.x = linePosition.xMax;
                    buttonPosition.height = EditorGUIUtility.singleLineHeight;
                    if (GUI.Button(buttonPosition, IconMinus, ButtonStyle))
                    {
                        buttonAction = Action.Remove;
                        buttonActionIndex = i;
                    }

                    if (i == conflictState.ConflictIndex && conflictState.ConflictOtherIndex == -1)
                    {
                        var iconPosition = linePosition;
                        iconPosition.size = ButtonStyle.CalcSize(WarningIconNull);
                        GUI.Label(iconPosition, WarningIconNull);
                    }
                    else if (i == conflictState.ConflictIndex)
                    {
                        var iconPosition = linePosition;
                        iconPosition.size = ButtonStyle.CalcSize(WarningIconConflict);
                        GUI.Label(iconPosition, WarningIconConflict);
                    }
                    else if (i == conflictState.ConflictOtherIndex)
                    {
                        var iconPosition = linePosition;
                        iconPosition.size = ButtonStyle.CalcSize(WarningIconOther);
                        GUI.Label(iconPosition, WarningIconOther);
                    }


                    linePosition.y += lineHeight;
                }

                EditorGUI.indentLevel--;
            }

            if (buttonAction == Action.Add)
            {
                keyArrayProperty.InsertArrayElementAtIndex(buttonActionIndex);
                if (valueArrayProperty != null)
                    valueArrayProperty.InsertArrayElementAtIndex(buttonActionIndex);
            }
            else if (buttonAction == Action.Remove)
            {
                DeleteArrayElementAtIndex(keyArrayProperty, buttonActionIndex);
                if (valueArrayProperty != null)
                    DeleteArrayElementAtIndex(valueArrayProperty, buttonActionIndex);
            }

            conflictState.ConflictKey = null;
            conflictState.ConflictValue = null;
            conflictState.ConflictIndex = -1;
            conflictState.ConflictOtherIndex = -1;
            conflictState.ConflictLineHeight = 0f;
            conflictState.ConflictKeyPropertyExpanded = false;
            conflictState.ConflictValuePropertyExpanded = false;

            foreach (var entry1 in EnumerateEntries(keyArrayProperty, valueArrayProperty))
            {
                var keyProperty1 = entry1.KeyProperty;
                int i = entry1.Index;
                object keyProperty1Value = GetPropertyValue(keyProperty1);

                if (keyProperty1Value == null)
                {
                    var valueProperty1 = entry1.ValueProperty;
                    SaveProperty(keyProperty1, valueProperty1, i, -1, conflictState);
                    DeleteArrayElementAtIndex(keyArrayProperty, i);
                    if (valueArrayProperty != null)
                        DeleteArrayElementAtIndex(valueArrayProperty, i);

                    break;
                }


                foreach (var entry2 in EnumerateEntries(keyArrayProperty, valueArrayProperty, i + 1))
                {
                    var keyProperty2 = entry2.KeyProperty;
                    int j = entry2.Index;
                    object keyProperty2Value = GetPropertyValue(keyProperty2);

                    if (ComparePropertyValues(keyProperty1Value, keyProperty2Value))
                    {
                        var valueProperty2 = entry2.ValueProperty;
                        SaveProperty(keyProperty2, valueProperty2, j, i, conflictState);
                        DeleteArrayElementAtIndex(keyArrayProperty, j);
                        if (valueArrayProperty != null)
                            DeleteArrayElementAtIndex(valueArrayProperty, j);

                        goto breakLoops;
                    }
                }
            }
            breakLoops:

            EditorGUI.EndProperty();
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float propertyHeight = EditorGUIUtility.singleLineHeight;

            if (property.isExpanded)
            {
                var keysProperty = property.FindPropertyRelative(_keysFieldName);
                var valuesProperty = property.FindPropertyRelative(_valuesFieldName);

                foreach (var entry in EnumerateEntries(keysProperty, valuesProperty))
                {
                    var keyProperty = entry.KeyProperty;
                    var valueProperty = entry.ValueProperty;
                    float keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
                    float valuePropertyHeight = valueProperty != null ? EditorGUI.GetPropertyHeight(valueProperty) : 0f;
                    float lineHeight = Mathf.Max(keyPropertyHeight, valuePropertyHeight);
                    propertyHeight += lineHeight;
                }

                ConflictState conflictState = GetConflictState(property);

                if (conflictState.ConflictIndex != -1)
                {
                    propertyHeight += conflictState.ConflictLineHeight;
                }
            }

            return propertyHeight;
        }
        
        public static object GetPropertyValue(SerializedProperty p)
        {
            if (SerializedPropertyValueAccessorsDict.TryGetValue(p.propertyType, out var propertyInfo))
            {
                return propertyInfo.GetValue(p, null);
            }
            else
            {
                if (p.isArray)
                    return GetPropertyValueArray(p);
                else
                    return GetPropertyValueGeneric(p);
            }
        }
        
        private static float DrawKeyValueLine(SerializedProperty keyProperty, SerializedProperty valueProperty, Rect linePosition, int index)
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

        private static float DrawKeyValueLineSimple(SerializedProperty keyProperty, SerializedProperty valueProperty, string keyLabel, string valueLabel, Rect linePosition)
        {
            float labelWidth = EditorGUIUtility.labelWidth;
            float labelWidthRelative = labelWidth / linePosition.width;

            float keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
            var keyPosition = linePosition;
            keyPosition.height = keyPropertyHeight;
            keyPosition.width = labelWidth - _indentWidth;
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

        private static bool CanPropertyBeExpanded(SerializedProperty property)
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

        private static void SaveProperty(SerializedProperty keyProperty, SerializedProperty valueProperty, int index, int otherIndex, ConflictState conflictState)
        {
            conflictState.ConflictKey = GetPropertyValue(keyProperty);
            conflictState.ConflictValue = valueProperty != null ? GetPropertyValue(valueProperty) : null;
            float keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
            float valuePropertyHeight = valueProperty != null ? EditorGUI.GetPropertyHeight(valueProperty) : 0f;
            float lineHeight = Mathf.Max(keyPropertyHeight, valuePropertyHeight);
            conflictState.ConflictLineHeight = lineHeight;
            conflictState.ConflictIndex = index;
            conflictState.ConflictOtherIndex = otherIndex;
            conflictState.ConflictKeyPropertyExpanded = keyProperty.isExpanded;
            conflictState.ConflictValuePropertyExpanded = valueProperty != null ? valueProperty.isExpanded : false;
        }

        private static ConflictState GetConflictState(SerializedProperty property)
        {
            PropertyIdentity propId = new PropertyIdentity(property);
            if (!ConflictStateDict.TryGetValue(propId, out var conflictState))
            {
                conflictState = new ConflictState();
                ConflictStateDict.Add(propId, conflictState);
            }
            return conflictState;
        }

        private static GUIContent IconContent(string name, string tooltip)
        {
            var builtinIcon = EditorGUIUtility.IconContent(name);
            return new GUIContent(builtinIcon.image, tooltip);
        }

        private static GUIContent TempContent(string text)
        {
            Content.text = text;
            return Content;
        }

        private static void DeleteArrayElementAtIndex(SerializedProperty arrayProperty, int index)
        {
            var property = arrayProperty.GetArrayElementAtIndex(index);
            // if(arrayProperty.arrayElementType.StartsWith("PPtr<$"))
            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                property.objectReferenceValue = null;
            }

            arrayProperty.DeleteArrayElementAtIndex(index);
        }

        private static void SetPropertyValue(SerializedProperty p, object v)
        {
            PropertyInfo propertyInfo;
            if (SerializedPropertyValueAccessorsDict.TryGetValue(p.propertyType, out propertyInfo))
            {
                propertyInfo.SetValue(p, v, null);
            }
            else
            {
                if (p.isArray)
                    SetPropertyValueArray(p, v);
                else
                    SetPropertyValueGeneric(p, v);
            }
        }

        private static object GetPropertyValueArray(SerializedProperty property)
        {
            object[] array = new object[property.arraySize];
            for (int i = 0; i < property.arraySize; i++)
            {
                SerializedProperty item = property.GetArrayElementAtIndex(i);
                array[i] = GetPropertyValue(item);
            }
            return array;
        }

        private static object GetPropertyValueGeneric(SerializedProperty property)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            var iterator = property.Copy();
            if (iterator.Next(true))
            {
                var end = property.GetEndProperty();
                do
                {
                    string name = iterator.name;
                    object value = GetPropertyValue(iterator);
                    dict.Add(name, value);
                } while (iterator.Next(false) && iterator.propertyPath != end.propertyPath);
            }
            return dict;
        }

        private static void SetPropertyValueArray(SerializedProperty property, object v)
        {
            object[] array = (object[])v;
            property.arraySize = array.Length;
            for (int i = 0; i < property.arraySize; i++)
            {
                SerializedProperty item = property.GetArrayElementAtIndex(i);
                SetPropertyValue(item, array[i]);
            }
        }

        private static void SetPropertyValueGeneric(SerializedProperty property, object v)
        {
            Dictionary<string, object> dict = (Dictionary<string, object>)v;
            var iterator = property.Copy();
            if (iterator.Next(true))
            {
                var end = property.GetEndProperty();
                do
                {
                    string name = iterator.name;
                    SetPropertyValue(iterator, dict[name]);
                } while (iterator.Next(false) && iterator.propertyPath != end.propertyPath);
            }
        }

        private static bool ComparePropertyValues(object value1, object value2)
        {
            if (value1 is Dictionary<string, object> && value2 is Dictionary<string, object>)
            {
                var dict1 = (Dictionary<string, object>)value1;
                var dict2 = (Dictionary<string, object>)value2;
                return CompareDictionaries(dict1, dict2);
            }
            else
            {
                return object.Equals(value1, value2);
            }
        }

        private static bool CompareDictionaries(Dictionary<string, object> dict1, Dictionary<string, object> dict2)
        {
            if (dict1.Count != dict2.Count)
                return false;

            foreach (var kvp1 in dict1)
            {
                var key1 = kvp1.Key;
                object value1 = kvp1.Value;

                object value2;
                if (!dict2.TryGetValue(key1, out value2))
                    return false;

                if (!ComparePropertyValues(value1, value2))
                    return false;
            }

            return true;
        }

        private static IEnumerable<EnumerationEntry> EnumerateEntries(SerializedProperty keyArrayProperty, SerializedProperty valueArrayProperty, int startIndex = 0)
        {
            if (keyArrayProperty.arraySize > startIndex)
            {
                int index = startIndex;
                var keyProperty = keyArrayProperty.GetArrayElementAtIndex(startIndex);
                var valueProperty = valueArrayProperty != null ? valueArrayProperty.GetArrayElementAtIndex(startIndex) : null;
                var endProperty = keyArrayProperty.GetEndProperty();

                do
                {
                    yield return new EnumerationEntry(keyProperty, valueProperty, index);
                    index++;
                } while (keyProperty.Next(false)
                    && (valueProperty != null ? valueProperty.Next(false) : true)
                    && !SerializedProperty.EqualContents(keyProperty, endProperty));
            }
        }
        private enum Action
        {
            None,
            Add,
            Remove
        }

        private readonly struct EnumerationEntry
        {
            public readonly SerializedProperty KeyProperty;
            public readonly SerializedProperty ValueProperty;
            public readonly int Index;

            public EnumerationEntry(SerializedProperty keyProperty, SerializedProperty valueProperty, int index)
            {
                KeyProperty = keyProperty;
                ValueProperty = valueProperty;
                Index = index;
            }
        }

        private class ConflictState
        {
            public object ConflictKey = null;
            public object ConflictValue = null;
            public int ConflictIndex = -1;
            public int ConflictOtherIndex = -1;
            public bool ConflictKeyPropertyExpanded = false;
            public bool ConflictValuePropertyExpanded = false;
            public float ConflictLineHeight = 0f;
        }

        private struct PropertyIdentity
        {
            public PropertyIdentity(SerializedProperty property)
            {
                this.Instance = property.serializedObject.targetObject;
                this.PropertyPath = property.propertyPath;
            }

            public UnityEngine.Object Instance;
            public string PropertyPath;
        }
    }
}