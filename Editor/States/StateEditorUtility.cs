using System;
using System.Collections.Generic;
using System.Linq;
using Konfus.States;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.States
{
    internal static class StateEditorUtility
    {
        public static StateList? GetStateList(SerializedProperty property)
        {
            var stateListProperty = property.serializedObject.FindProperty("masterStateList");
            if (stateListProperty?.objectReferenceValue is StateList referencedStateList)
            {
                return referencedStateList;
            }

            var stateControllerProperty = property.serializedObject.FindProperty("stateController");
            if (stateControllerProperty?.objectReferenceValue is StateController referencedController)
            {
                return referencedController.MasterStateList;
            }

            if (property.serializedObject.targetObject is StateList stateList)
            {
                return stateList;
            }

            if (property.serializedObject.targetObject is StateController controller)
            {
                return controller.MasterStateList;
            }

            return null;
        }

        public static string[] GetStateOptions(StateList? stateList)
        {
            if (stateList == null)
            {
                return Array.Empty<string>();
            }

            return stateList.AvailableStateNames.ToArray();
        }

        public static bool ContainsState(StateList? stateList, string stateName)
        {
            return stateList != null && stateList.ContainsState(stateName);
        }

        public static List<string> ValidateExpressionProperty(SerializedProperty property, StateList? stateList)
        {
            var expressionText = property.FindPropertyRelative("expressionText")?.stringValue ?? string.Empty;
            if (!StateExpression.TryParse(expressionText, out var expression, out var parseError))
            {
                return new List<string> { parseError };
            }

            if (stateList == null)
            {
                return new List<string> { "Assign a State List on the owning object to validate this expression." };
            }

            return expression.Validate(stateList.AvailableStateNames);
        }

        public static float GetHelpBoxHeight(string message, float width)
        {
            return EditorStyles.helpBox.CalcHeight(new GUIContent(message), width);
        }

        public static string BuildAsciiStateMachine(StateExpression expression)
        {
            var lines = new List<string>();

            foreach (var rule in expression.Rules)
            {
                var fromState = rule.WhenState.StateName;
                var toState = rule.TransitionTo.StateName;

                if (string.IsNullOrWhiteSpace(fromState) || string.IsNullOrWhiteSpace(toState))
                {
                    continue;
                }

                lines.Add($"[{fromState}] -> [{toState}]");
            }

            if (expression.HasElseRule && expression.ElseState.HasValue)
            {
                lines.Add($"[else] -> [{expression.ElseState.StateName}]");
            }

            return lines.Count > 0
                ? string.Join("\n", lines)
                : "[empty]";
        }

        public static string GetExpressionPreview(SerializedProperty property)
        {
            return property.FindPropertyRelative("expressionText")?.stringValue ?? string.Empty;
        }

        public static string GetStateReferenceName(SerializedProperty? property)
        {
            return property?.FindPropertyRelative("stateName")?.stringValue ?? string.Empty;
        }

        public static List<string> ValidateCompiledExpressionProperty(SerializedProperty property, StateList? stateList)
        {
            var errors = new List<string>();
            var rulesProperty = property.FindPropertyRelative("rules");
            var hasElseRuleProperty = property.FindPropertyRelative("hasElseRule");
            var elseStateProperty = property.FindPropertyRelative("elseState");
            var seenConditions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if ((rulesProperty == null || rulesProperty.arraySize == 0) &&
                (hasElseRuleProperty == null || !hasElseRuleProperty.boolValue))
            {
                errors.Add("Expression has no rules.");
            }

            if (rulesProperty != null)
            {
                for (var i = 0; i < rulesProperty.arraySize; i++)
                {
                    var ruleProperty = rulesProperty.GetArrayElementAtIndex(i);
                    var whenState = GetStateReferenceName(ruleProperty.FindPropertyRelative("whenState"));
                    var transitionTo = GetStateReferenceName(ruleProperty.FindPropertyRelative("transitionTo"));
                    var label = $"Rule {i + 1}";

                    if (string.IsNullOrWhiteSpace(whenState))
                    {
                        errors.Add($"{label}: missing condition state.");
                    }
                    else
                    {
                        if (!ContainsState(stateList, whenState))
                        {
                            errors.Add($"{label}: condition state '{whenState}' is not defined in the state list.");
                        }

                        if (!seenConditions.Add(whenState))
                        {
                            errors.Add($"{label}: duplicate condition for state '{whenState}'.");
                        }
                    }

                    if (string.IsNullOrWhiteSpace(transitionTo))
                    {
                        errors.Add($"{label}: missing target state.");
                    }
                    else if (!ContainsState(stateList, transitionTo))
                    {
                        errors.Add($"{label}: target state '{transitionTo}' is not defined in the state list.");
                    }
                }
            }

            if (hasElseRuleProperty != null && hasElseRuleProperty.boolValue)
            {
                var elseState = GetStateReferenceName(elseStateProperty);
                if (string.IsNullOrWhiteSpace(elseState))
                {
                    errors.Add("Else rule is enabled but has no target state.");
                }
                else if (!ContainsState(stateList, elseState))
                {
                    errors.Add($"Else rule target state '{elseState}' is not defined in the state list.");
                }
            }

            return errors;
        }
    }
}
