using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Konfus.Utility.Attributes;

namespace Konfus.Tools.Utility
{
    public static class SerializedPropertyExtensions
    {
        private const BindingFlags ALL_BINDING_FLAGS = (BindingFlags)(-1);
        
        private static readonly Regex ElementIdentifier = new Regex(@"^[_a-zA-Z][_a-zA-Z0-9]*(\[[0-9]*\])+$");

        private static readonly Regex ElementIndex = new Regex(@"^(\[[0-9]*\])+$");

        private static readonly Regex MemberIdentifier = new Regex(@"^[_a-zA-Z][_a-zA-Z0-9]*$");

        public static bool IsReadonly(this SerializedProperty property)
        {
            var attribute = property.GetPropertyAttribute<IsReadOnlyAttribute>();
            return attribute != null;
        }
        
        public static bool? IsVisible(this SerializedProperty property)
        {
            SerializedProperty GetConditionProperty(SerializedProperty property, ShowIfAttribute showIf)
            {
                // Try finding on serialized obj
                var conditionProperty = property.serializedObject.FindProperty(showIf.ConditionalSourceField);
                if (conditionProperty != null) return conditionProperty;
            
                // Try finding via parent if going through serialized obj failed...
                var parent = property.FindParentProperty();
                conditionProperty = parent.FindPropertyRelative(showIf.ConditionalSourceField);
                return conditionProperty;
            }

            bool GetConditionValue(SerializedProperty conditionProperty)
            {
                // Check the type of the condition property and get its value
                switch (conditionProperty.propertyType)
                {
                    case SerializedPropertyType.Boolean:
                        return conditionProperty.boolValue;
                    case SerializedPropertyType.Enum:
                        return conditionProperty.enumValueIndex == 1; // Assume true if enum index is 1
                    default:
                        Debug.LogError($"Unsupported condition property type '{conditionProperty.propertyType}' in ShowIf attribute.");
                        return false;
                }
            }
            
            var showIfAttrib = property.GetPropertyAttribute<ShowIfAttribute>();
            if (showIfAttrib == null) return true; // No show if, return true
            
            var conditionProperty = GetConditionProperty(property, showIfAttrib);
            if (conditionProperty == null) return null; // Error state
            
            return GetConditionValue(conditionProperty) == showIfAttrib.ExpectedValue; // Show if value, return true/false depending on evaluated condition
        }

        public static Type GetPropertyType(this SerializedProperty property)
        {
            Type type = property.serializedObject.targetObject.GetType();
            FieldInfo field = type.GetField(property.propertyPath, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            return field?.FieldType;
        }
        /// <summary>
        /// Returns attributes of type <typeparamref name="TAttribute"/> on <paramref name="serializedProperty"/>.
        /// </summary>
        public static TAttribute GetPropertyAttribute<TAttribute>(this SerializedProperty serializedProperty, bool inherit = true) where TAttribute : Attribute
        {
            if (serializedProperty == null)
            {
                throw new ArgumentNullException(nameof(serializedProperty));
            }

            var targetObjectType = serializedProperty.serializedObject.targetObject.GetType();

            if (targetObjectType == null)
            {
                throw new ArgumentException($"Could not find the {nameof(targetObjectType)} of {nameof(serializedProperty)}");
            }

            TAttribute attribute = null;
            foreach (var pathSegment in serializedProperty.propertyPath.Split('.'))
            {
                var fieldInfo = targetObjectType.GetField(pathSegment, ALL_BINDING_FLAGS);
                if (fieldInfo != null)
                {
                    targetObjectType = fieldInfo.FieldType;
                    attribute = fieldInfo.GetCustomAttributes<TAttribute>(inherit).FirstOrDefault();
                }
                else
                {
                    var propertyInfo = targetObjectType.GetProperty(pathSegment, ALL_BINDING_FLAGS);
                    if (propertyInfo == null) throw new ArgumentException($"Could not find the field or property of {nameof(serializedProperty)}");
                    targetObjectType = propertyInfo.PropertyType;
                    attribute = propertyInfo.GetCustomAttributes<TAttribute>(inherit).FirstOrDefault();
                }
            }

            return attribute;
        }
        
        public static IEnumerable<SerializedProperty> EnumerateChildProperties(this SerializedObject serializedObject)
        {
            var iterator = serializedObject.GetIterator();
            if (iterator.NextVisible(true))
                // yield return property; // skip "m_Script"
            {
                while (iterator.NextVisible(false))
                {
                    yield return iterator;
                }
            }
        }

        public static IEnumerable<SerializedProperty> EnumerateChildProperties(this SerializedProperty property)
        {
            var iterator = property.Copy();
            var end = iterator.GetEndProperty();
            if (iterator.NextVisible(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(iterator, end))
                        yield break;

                    yield return iterator;
                } while (iterator.NextVisible(false));
            }
        }

        public static SerializedProperty FindParentProperty(this SerializedProperty property)
        {
            var serializedObject = property.serializedObject;
            string propertyPath = property.propertyPath;
            object[] propertyKeys = ParsePropertyPath(propertyPath: propertyPath).ToArray();
            int propertyKeyCount = propertyKeys.Length;
            if (propertyKeyCount == 1) return null; // parent is serialized object
            object lastPropertyKey = propertyKeys[propertyKeyCount - 1];
            if (lastPropertyKey is int)
            {
                // parent is an array, drop [Array,data,N] from path
                IEnumerable<object> parentKeys = propertyKeys.Take(propertyKeyCount - 3);
                string parentPath = JoinPropertyPath(keys: parentKeys);
                return serializedObject.FindProperty(propertyPath: parentPath);
            }
            else
            {
                // parent is a structure, drop [name] from path
                Debug.Assert(lastPropertyKey is string);
                IEnumerable<object> parentKeys = propertyKeys.Take(propertyKeyCount - 1);
                string parentPath = JoinPropertyPath(keys: parentKeys);
                return serializedObject.FindProperty(propertyPath: parentPath);
            }
        }

        public static object FindObject(this object obj, IEnumerable<object> path)
        {
            foreach (object key in path)
            {
                if (key is string)
                {
                    var objType = obj.GetType();
                    string fieldName = (string)key;
                    var fieldInfo = objType.FindFieldInfo(fieldName: fieldName);
                    if (fieldInfo == null)
                        throw FieldNotFoundException(objType, fieldName);
                    obj = fieldInfo.GetValue(obj: obj);
                    continue;
                }

                if (key is int)
                {
                    int elementIndex = (int)key;
                    var array = (IList)obj;
                    obj = array[index: elementIndex];
                }
            }

            return obj;
        }

        public static object GetObject(this SerializedProperty property)
        {
            var obj = property.serializedObject.targetObject;
            IEnumerable<object> path = ParseValuePath(property: property);
            return FindObject(obj, path);
        }

        public static bool IsArrayOrList(this SerializedProperty property)
        {
            return property.propertyType == SerializedPropertyType.Generic &&
                   property.isArray;
        }

        public static bool IsStructure(this SerializedProperty property)
        {
            return property.propertyType == SerializedPropertyType.Generic &&
                   property.isArray == false &&
                   property.hasChildren;
        }

        public static string JoinPropertyPath(IEnumerable<object> keys)
        {
            return JoinValuePath(keys: keys);
        }

        public static string JoinValuePath(IEnumerable<object> keys)
        {
            var builder = new StringBuilder();
            foreach (object key in keys)
            {
                if (key is string)
                {
                    if (builder.Length > 0) builder.Append('.');
                    builder.Append((string)key);
                    continue;
                }

                if (key is int)
                {
                    builder.Append('[');
                    builder.Append((int)key);
                    builder.Append(']');
                    continue;
                }

                throw new Exception(string.Format(
                    "invalid key: {0}", key
                ));
            }

            return builder.ToString();
        }

        private static Exception FieldNotFoundException(Type type, string fieldName)
        {
            return new KeyNotFoundException(string.Format("{0}.{1} not found", type, fieldName));
        }

        private static FieldInfo FindFieldInfo(this Type type, string fieldName)
        {
            const BindingFlags bindingFlags =
                BindingFlags.FlattenHierarchy |
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic;
            var fieldInfo = type.GetField(fieldName, bindingFlags);
            if (fieldInfo != null)
                return fieldInfo;

            var baseType = type.BaseType;
            if (baseType == null)
                return null;

            return FindFieldInfo(baseType, fieldName);
        }

        private static IEnumerable<object> ParsePropertyPath(SerializedProperty property)
        {
            return ParsePropertyPath(propertyPath: property.propertyPath);
        }

        private static IEnumerable<object> ParsePropertyPath(string propertyPath)
        {
            return ParseValuePath(fieldPath: propertyPath);
        }

        private static string GetValuePath(SerializedProperty property)
        {
            return property.propertyPath.Replace(".Array.data[", "[");
        }

        private static IEnumerable<object> ParseValuePath(SerializedProperty property)
        {
            return ParseValuePath(GetValuePath(property: property));
        }

        private static IEnumerable<object> ParseValuePath(string fieldPath)
        {
            string[] keys = fieldPath.Split('.');
            foreach (string key in keys)
            {
                if (key.IsElementIdentifier())
                {
                    string[] subkeys = key.Split('[', ']');
                    yield return subkeys[0];
                    foreach (string subkey in subkeys.Skip(1))
                    {
                        if (string.IsNullOrEmpty(value: subkey)) continue;
                        int index = int.Parse(s: subkey);
                        yield return index;
                    }

                    continue;
                }

                if (key.IsElementIndex())
                {
                    string[] subkeys = key.Split('[', ']');
                    foreach (string subkey in subkeys)
                    {
                        if (string.IsNullOrEmpty(value: subkey)) continue;
                        int index = int.Parse(s: subkey);
                        yield return index;
                    }

                    continue;
                }

                if (key.IsMemberIdentifier())
                {
                    yield return key;
                    continue;
                }

                throw new Exception(string.Format(
                    "invalid path: {0}", fieldPath
                ));
            }
        }

        private static bool IsElementIdentifier(this string s)
        {
            return ElementIdentifier.IsMatch(input: s);
        }

        private static bool IsElementIndex(this string s)
        {
            return ElementIndex.IsMatch(input: s);
        }

        private static bool IsMemberIdentifier(this string s)
        {
            return MemberIdentifier.IsMatch(input: s);
        }

        private static Exception UnsupportedValue(SerializedProperty property, object value)
        {
            var serializedObject = property.serializedObject;
            var targetObject = serializedObject.targetObject;
            var targetType = targetObject.GetType();
            string targetTypeName = targetType.Name;
            string propertyPath = property.propertyPath;
            return new Exception(string.Format(
                "unsupported value {0} for {1}.{2}",
                value, targetTypeName, propertyPath
            ));
        }

        private static Exception UnsupportedValue(SerializedProperty property, object value, string expected)
        {
            var serializedObject = property.serializedObject;
            var targetObject = serializedObject.targetObject;
            var targetType = targetObject.GetType();
            string targetTypeName = targetType.Name;
            string propertyPath = property.propertyPath;
            if (value == null)
                value = "null";
            else
                value = string.Format("'{0}'", value);
            return new Exception(string.Format(
                "unsupported value {0} for {1}.{2}, expected {3}",
                value, targetTypeName, propertyPath, expected
            ));
        }

        private static Exception UnsupportedValueType(SerializedProperty property)
        {
            var serializedObject = property.serializedObject;
            var targetObject = serializedObject.targetObject;
            var targetType = targetObject.GetType();
            string targetTypeName = targetType.Name;
            string valueTypeName = property.propertyType.ToString();
            string propertyPath = property.propertyPath;
            return new Exception(string.Format(
                "unsupported value type {0} for {1}.{2}",
                valueTypeName, targetTypeName, propertyPath
            ));
        }
    }
}