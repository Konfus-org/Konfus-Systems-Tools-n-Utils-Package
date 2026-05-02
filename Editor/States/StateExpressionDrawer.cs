using System.Collections.Generic;
using Konfus.States;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.States
{
    [CustomPropertyDrawer(typeof(StateExpression))]
    internal class StateExpressionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var hasVisibleLabel = label != null && label != GUIContent.none && !string.IsNullOrWhiteSpace(label.text);
            var y = position.y;

            if (hasVisibleLabel)
            {
                var foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);
                y = foldoutRect.yMax + EditorGUIUtility.standardVerticalSpacing;

                if (!property.isExpanded)
                {
                    EditorGUI.EndProperty();
                    return;
                }
            }
            else
            {
                property.isExpanded = true;
            }
            var expressionTextProperty = property.FindPropertyRelative("expressionText");

            if (expressionTextProperty != null)
            {
                var textHeight = Mathf.Max(
                    EditorGUIUtility.singleLineHeight * 4f,
                    EditorGUI.GetPropertyHeight(expressionTextProperty, true));
                var textRect = new Rect(position.x, y, position.width, textHeight);
                expressionTextProperty.stringValue = EditorGUI.TextArea(textRect, expressionTextProperty.stringValue);
                y = textRect.yMax + EditorGUIUtility.standardVerticalSpacing;
            }

            var stateList = StateEditorUtility.GetStateList(property);
            var errors = StateEditorUtility.ValidateExpressionProperty(property, stateList);

            if (errors.Count > 0)
            {
                var message = string.Join("\n", errors);
                var helpHeight = StateEditorUtility.GetHelpBoxHeight(message, position.width);
                var helpRect = new Rect(position.x, y, position.width, helpHeight);
                EditorGUI.HelpBox(helpRect, message, MessageType.Error);
                y = helpRect.yMax + EditorGUIUtility.standardVerticalSpacing;
            }
            else
            {
                var expressionText = expressionTextProperty?.stringValue ?? string.Empty;
                if (StateExpression.TryParse(expressionText, out var parsedExpression, out _))
                {
                    var successMessage = "Valid expression";
                    var successHeight = StateEditorUtility.GetHelpBoxHeight(successMessage, position.width);
                    var successRect = new Rect(position.x, y, position.width, successHeight);
                    DrawSuccessBox(successRect, successMessage);
                    y = successRect.yMax + EditorGUIUtility.standardVerticalSpacing;

                    var asciiDiagram = StateEditorUtility.BuildAsciiStateMachine(parsedExpression);
                    var asciiMessage = $"State machine\n{asciiDiagram}";
                    var asciiHeight = EditorStyles.helpBox.CalcHeight(new GUIContent(asciiMessage), position.width);
                    var asciiRect = new Rect(position.x, y, position.width, asciiHeight);
                    EditorGUI.HelpBox(asciiRect, asciiMessage, MessageType.None);
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var hasVisibleLabel = label != null && label != GUIContent.none && !string.IsNullOrWhiteSpace(label.text);
            var height = hasVisibleLabel ? EditorGUIUtility.singleLineHeight : 0f;

            if (hasVisibleLabel && !property.isExpanded)
            {
                return height;
            }

            if (hasVisibleLabel)
            {
                height += EditorGUIUtility.standardVerticalSpacing;
            }

            var expressionTextProperty = property.FindPropertyRelative("expressionText");
            if (expressionTextProperty != null)
            {
                height += Mathf.Max(
                    EditorGUIUtility.singleLineHeight * 4f,
                    EditorGUI.GetPropertyHeight(expressionTextProperty, true));
                height += EditorGUIUtility.standardVerticalSpacing;
            }

            var errors = StateEditorUtility.ValidateExpressionProperty(property, StateEditorUtility.GetStateList(property));

            if (errors.Count > 0)
            {
                var message = string.Join("\n", errors);
                height += StateEditorUtility.GetHelpBoxHeight(message, EditorGUIUtility.currentViewWidth - 48f);
            }
            else
            {
                var expressionText = expressionTextProperty?.stringValue ?? string.Empty;
                if (StateExpression.TryParse(expressionText, out var parsedExpression, out _))
                {
                    height += StateEditorUtility.GetHelpBoxHeight("Valid expression", EditorGUIUtility.currentViewWidth - 48f);
                    height += EditorGUIUtility.standardVerticalSpacing;
                    height += StateEditorUtility.GetHelpBoxHeight(
                        $"State machine\n{StateEditorUtility.BuildAsciiStateMachine(parsedExpression)}",
                        EditorGUIUtility.currentViewWidth - 48f);
                }
            }

            return height;
        }

        private static void DrawSuccessBox(Rect rect, string message)
        {
            var background = new Color(0.84f, 0.95f, 0.84f, 1f);
            EditorGUI.DrawRect(rect, background);
            GUI.Box(rect, GUIContent.none, EditorStyles.helpBox);

            var iconRect = new Rect(rect.x + 8f, rect.y + 4f, 20f, rect.height - 8f);
            var textRect = new Rect(iconRect.xMax + 4f, rect.y + 2f, rect.width - 36f, rect.height - 4f);
            var icon = EditorGUIUtility.IconContent("TestPassed");
            if (icon.image != null)
            {
                GUI.Label(iconRect, icon);
            }

            var labelStyle = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = new Color(0.08f, 0.24f, 0.08f, 1f) },
                hover = { textColor = new Color(0.08f, 0.24f, 0.08f, 1f) },
                focused = { textColor = new Color(0.08f, 0.24f, 0.08f, 1f) },
                active = { textColor = new Color(0.08f, 0.24f, 0.08f, 1f) }
            };

            EditorGUI.LabelField(textRect, message, labelStyle);
        }
    }
}
