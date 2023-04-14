using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Konfus.Utility.Serialization.Editor
{
    [CustomPropertyDrawer(typeof(SerializableDict<,>), true)]
    public class SerializableDictionaryPropertyDrawer : PropertyDrawer
    {
        private const string _pairsFieldName = "pairs";
        private const string _keyFieldName = "key";

        static readonly GUIContent WarningIconConflict = IconContent("console.warnicon.sml", "Conflicting key, this entry will be lost");
        static readonly GUIContent WarningIconOther = IconContent("console.infoicon.sml", "Conflicting key");
        static readonly GUIContent WarningIconNull = IconContent("console.warnicon.sml", "Null key, this entry will be lost");

        static readonly Dictionary<PropertyIdentity, ConflictState> ConflictStateDict = new Dictionary<PropertyIdentity, ConflictState>();
        static readonly Dictionary<SerializedPropertyType, PropertyInfo> SerializedPropertyValueAccessorsDict;

        private ReorderableList _reorderableList = null;
        private ConflictState _conflictState = null;


        static SerializableDictionaryPropertyDrawer()
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

            var pairsProperty = property.FindPropertyRelative(_pairsFieldName);

            _conflictState = GetConflictState(property);

            if (_conflictState.ConflictIndex != -1 && _conflictState.ConflictIndex <= pairsProperty.arraySize)
            {
                pairsProperty.InsertArrayElementAtIndex(_conflictState.ConflictIndex);
                var pairProperty = pairsProperty.GetArrayElementAtIndex(_conflictState.ConflictIndex);
                SetPropertyValue(pairProperty, _conflictState.ConflictPair);
                pairProperty.isExpanded = _conflictState.ConflictPairPropertyExpanded;
            }

            var labelPosition = position;
            labelPosition.height = EditorGUIUtility.singleLineHeight;

            EditorGUI.PropertyField(labelPosition, property, label, false);
            if (property.isExpanded)
            {
                ReorderableList reorderableList = GetList(pairsProperty, label);

                var listRect = position;
                listRect.y += EditorGUIUtility.singleLineHeight;
                reorderableList.DoList(listRect);
            }

            _conflictState.ConflictPair = null;
            _conflictState.ConflictIndex = -1;
            _conflictState.ConflictOtherIndex = -1;
            _conflictState.ConflictLineHeight = 0f;
            _conflictState.ConflictPairPropertyExpanded = false;

            for (int i = 0; i < pairsProperty.arraySize; i++)
            {
                var pairProperty1 = pairsProperty.GetArrayElementAtIndex(i);
                var keyProperty1 = pairProperty1.FindPropertyRelative(_keyFieldName);
                object keyProperty1Value = GetPropertyValue(keyProperty1);

                if (keyProperty1Value == null)
                {
                    SaveProperty(pairProperty1, i, -1, _conflictState);
                    pairsProperty.DeleteArrayElementAtIndex(i);

                    break;
                }

                for (int j = i + 1; j < pairsProperty.arraySize; j++)
                {
                    var pairProperty2 = pairsProperty.GetArrayElementAtIndex(j);
                    var keyProperty2 = pairProperty2.FindPropertyRelative(_keyFieldName);

                    if (SerializedProperty.DataEquals(keyProperty1, keyProperty2))
                    {
                        SaveProperty(pairProperty2, j, i, _conflictState);
                        pairsProperty.DeleteArrayElementAtIndex(j);

                        goto breakLoops;
                    }
                }
            }
            breakLoops:

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var ret = EditorGUI.GetPropertyHeight(property, false);
            if (property.isExpanded)
            {
                var pairsProperty = property.FindPropertyRelative(_pairsFieldName);

                if (_conflictState != null && _conflictState.ConflictIndex != -1 && _conflictState.ConflictIndex <= pairsProperty.arraySize)
                {
                    pairsProperty.InsertArrayElementAtIndex(_conflictState.ConflictIndex);
                    var pairProperty = pairsProperty.GetArrayElementAtIndex(_conflictState.ConflictIndex);
                    SetPropertyValue(pairProperty, _conflictState.ConflictPair);
                    pairProperty.isExpanded = _conflictState.ConflictPairPropertyExpanded;

                    ret += EditorGUI.GetPropertyHeight(pairProperty);

                    pairsProperty.DeleteArrayElementAtIndex(_conflictState.ConflictIndex);
                }

                ret += this.GetList(pairsProperty, label).GetHeight();
            }

            return ret;
        }
        
        private static object GetPropertyValue(SerializedProperty p)
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

        private static GUIContent IconContent(string name, string tooltip)
        {
            var builtinIcon = EditorGUIUtility.IconContent(name);
            return new GUIContent(builtinIcon.image, tooltip);
        }

        private static ConflictState GetConflictState(SerializedProperty property)
        {
            ConflictState conflictState;
            PropertyIdentity propId = new PropertyIdentity(property);
            if (!ConflictStateDict.TryGetValue(propId, out conflictState))
            {
                conflictState = new ConflictState();
                ConflictStateDict.Add(propId, conflictState);
            }
            return conflictState;
        }

        private static void SaveProperty(SerializedProperty pairProperty, int index, int otherIndex, ConflictState conflictState)
        {
            conflictState.ConflictPair = GetPropertyValue(pairProperty);
            float pairPropertyHeight = EditorGUI.GetPropertyHeight(pairProperty);
            conflictState.ConflictLineHeight = pairPropertyHeight;
            conflictState.ConflictIndex = index;
            conflictState.ConflictOtherIndex = otherIndex;
            conflictState.ConflictPairPropertyExpanded = pairProperty.isExpanded;
        }

        private ReorderableList GetList(SerializedProperty pairsProperty, GUIContent label)
        {
            bool shouldNewList = true;
            try
            {
                shouldNewList = _reorderableList == null || !SerializedProperty.DataEquals(_reorderableList.serializedProperty, pairsProperty);
            }
            //SerializedObject of _reorderableList has been Disposed.
            catch (NullReferenceException e)
            {
                Debug.Log(e.Message);
            }

            if (shouldNewList)
            {
                _reorderableList = new ReorderableList(pairsProperty.serializedObject, pairsProperty, true, false, true, true)
                {
                    drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                    {
                        if (index == _conflictState.ConflictIndex && _conflictState.ConflictOtherIndex == -1)
                        {
                            var iconPosition = rect;
                            iconPosition.size = GUIStyle.none.CalcSize(WarningIconNull);
                            GUI.Label(iconPosition, WarningIconNull);

                            rect.xMin += iconPosition.size.x;
                        }
                        else if (index == _conflictState.ConflictIndex)
                        {
                            var iconPosition = rect;
                            iconPosition.size = GUIStyle.none.CalcSize(WarningIconConflict);
                            GUI.Label(iconPosition, WarningIconConflict);

                            rect.xMin += iconPosition.size.x;
                        }
                        else if (index == _conflictState.ConflictOtherIndex)
                        {
                            var iconPosition = rect;
                            iconPosition.size = GUIStyle.none.CalcSize(WarningIconOther);
                            GUI.Label(iconPosition, WarningIconOther);

                            rect.xMin += iconPosition.size.x;
                        }

                        var pairProperty = pairsProperty.GetArrayElementAtIndex(index);

                        EditorGUI.PropertyField(rect, pairProperty, label, false);
                    },
                    elementHeightCallback = (int index) =>
                    {
                        var pairProperty = pairsProperty.GetArrayElementAtIndex(index);
                        return EditorGUI.GetPropertyHeight(pairProperty);
                    }
                };
            }

            return _reorderableList;
        }

        private class ConflictState
        {
            public object ConflictPair = null;
            public int ConflictIndex = -1;
            public int ConflictOtherIndex = -1;
            public bool ConflictPairPropertyExpanded = false;
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