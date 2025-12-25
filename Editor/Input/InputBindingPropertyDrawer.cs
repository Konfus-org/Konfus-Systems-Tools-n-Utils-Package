using System;
using System.Collections.Generic;
using System.Reflection;
using Konfus.Input;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace Konfus.Editor.Input
{
    [CustomPropertyDrawer(typeof(InputTarget))]
    public sealed class InputTargetDrawer : PropertyDrawer
    {
        private static readonly Dictionary<Type, List<MethodOption>> _cache = new();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Two rows: target + method
            return EditorGUIUtility.singleLineHeight * 2f + EditorGUIUtility.standardVerticalSpacing;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                SerializedProperty? targetProp = property.FindPropertyRelative("target");
                SerializedProperty? methodProp = property.FindPropertyRelative("method");

                float lineH = EditorGUIUtility.singleLineHeight;
                float space = EditorGUIUtility.standardVerticalSpacing;

                var r0 = new Rect(position.x, position.y, position.width, lineH);
                var r1 = new Rect(position.x, position.y + lineH + space, position.width, lineH);

                // Target field
                EditorGUI.PropertyField(r0, targetProp, new GUIContent(label.text));

                // Method dropdown (disabled until target exists)
                Object targetObj = targetProp.objectReferenceValue;
                using (new EditorGUI.DisabledScope(targetObj == null))
                {
                    DrawMethodPopup(r1, targetObj as MonoBehaviour, methodProp, "Method");
                }
            }
        }

        private static void DrawMethodPopup(Rect rect, MonoBehaviour? target, SerializedProperty methodProp,
            string label)
        {
            string currentName = methodProp.stringValue ?? string.Empty;

            if (!target)
            {
                EditorGUI.Popup(rect, label, 0, new[] { "<assign target>" });
                return;
            }

            List<MethodOption> options = GetMethodOptions(target.GetType());
            if (options.Count == 0)
            {
                EditorGUI.Popup(rect, label, 0, new[] { "<no valid methods>" });
                methodProp.stringValue = string.Empty;
                return;
            }

            // Build display list and find current index
            var index = 0;
            var display = new string[options.Count + 1];

            display[0] = "<none>";
            for (var i = 0; i < options.Count; i++)
            {
                display[i + 1] = options[i].Display;
                if (!string.IsNullOrEmpty(currentName) && options[i].Name == currentName)
                    index = i + 1;
            }

            // If current method name is set but not found, show Missing at top
            bool missing = !string.IsNullOrEmpty(currentName) && index == 0;
            if (missing)
                display[0] = $"(Missing) {currentName}";

            int newIndex = EditorGUI.Popup(rect, label, index, display);

            if (newIndex != index)
            {
                if (newIndex <= 0)
                    methodProp.stringValue = string.Empty;
                else
                    methodProp.stringValue = options[newIndex - 1].Name;
            }
        }

        private static List<MethodOption> GetMethodOptions(Type type)
        {
            if (_cache.TryGetValue(type, out List<MethodOption>? cached))
                return cached;

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            MethodInfo[] methods = type.GetMethods(flags);

            var list = new List<MethodOption>(64);

            foreach (MethodInfo m in methods)
            {
                // Skip property accessors/operators/etc.
                if (m.IsSpecialName)
                    continue;

                // Filter: only 0 or 1 param overloads
                ParameterInfo[] ps = m.GetParameters();
                if (ps.Length > 1)
                    continue;

                // If you want, restrict 1-param to "ctx or value" signatures:
                // allow: CallbackContext OR any non-byref parameter
                if (ps.Length == 1)
                {
                    Type pt = ps[0].ParameterType;
                    if (pt.IsByRef) // no ref/out
                        continue;

                    // (Optional) If you want to ONLY allow ctx + common value types:
                    // if (pt != typeof(InputAction.CallbackContext) && pt != typeof(Vector2) && pt != typeof(float) && pt != typeof(bool))
                    //     continue;
                }

                list.Add(new MethodOption
                {
                    Name = m.Name, // NOTE: stores only name; if you have overloads with same name this is ambiguous.
                    Display = BuildDisplay(m)
                });
            }

            // Sort nicely
            list.Sort((a, b) => string.CompareOrdinal(a.Display, b.Display));

            _cache[type] = list;
            return list;
        }

        private static string BuildDisplay(MethodInfo m)
        {
            ParameterInfo[] ps = m.GetParameters();
            if (ps.Length == 0)
                return $"{m.Name}()";

            return $"{m.Name}({FriendlyTypeName(ps[0].ParameterType)})";
        }

        private static string FriendlyTypeName(Type t)
        {
            if (t == typeof(InputAction.CallbackContext)) return "CallbackContext";
            if (t == typeof(Vector2)) return "Vector2";
            if (t == typeof(Vector3)) return "Vector3";
            if (t == typeof(float)) return "float";
            if (t == typeof(int)) return "int";
            if (t == typeof(bool)) return "bool";
            return t.Name;
        }

        private struct MethodOption
        {
            public string Name; // stored value
            public string Display; // shown in popup
        }
    }
}