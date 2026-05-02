using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Konfus.States
{
    [Serializable]
    public class StateExpression
    {
        [Serializable]
        public class Rule
        {
            [SerializeField] private StateReference whenState = new();
            [SerializeField] private StateReference transitionTo = new();

            public StateReference WhenState => whenState;
            public StateReference TransitionTo => transitionTo;
        }

        [SerializeField, TextArea(3, 8)] private string expressionText = string.Empty;
        [SerializeField, HideInInspector] private List<Rule> rules = new();
        [SerializeField, HideInInspector] private bool hasElseRule;
        [SerializeField, HideInInspector] private StateReference elseState = new();

        public IReadOnlyList<Rule> Rules => rules;
        public bool HasElseRule => hasElseRule;
        public StateReference ElseState => elseState;
        public string ExpressionText => expressionText;

        public bool TryEvaluate(string currentState, out string nextState)
        {
            if (TryGetEffectiveExpression(out var effectiveExpression, out _))
            {
                return effectiveExpression.TryEvaluateCompiled(currentState, out nextState);
            }

            nextState = string.Empty;
            return false;
        }

        public List<string> Validate(IEnumerable<string> validStates)
        {
            if (!TryGetEffectiveExpression(out var effectiveExpression, out var parseError))
            {
                return new List<string> { parseError };
            }

            return effectiveExpression.ValidateCompiled(validStates);
        }

        public void SetExpressionText(string value)
        {
            expressionText = value ?? string.Empty;
        }

        public override string ToString()
        {
            return !string.IsNullOrWhiteSpace(expressionText)
                ? expressionText
                : BuildCompiledExpressionText();
        }

        private bool TryEvaluateCompiled(string currentState, out string nextState)
        {
            foreach (var rule in rules)
            {
                if (!rule.WhenState.HasValue || !rule.TransitionTo.HasValue)
                {
                    continue;
                }

                if (string.Equals(rule.WhenState.StateName, currentState, StringComparison.OrdinalIgnoreCase))
                {
                    nextState = rule.TransitionTo.StateName;
                    return true;
                }
            }

            if (hasElseRule && elseState.HasValue)
            {
                nextState = elseState.StateName;
                return true;
            }

            nextState = string.Empty;
            return false;
        }

        private List<string> ValidateCompiled(IEnumerable<string> validStates)
        {
            var stateSet = new HashSet<string>(
                validStates.Where(static state => !string.IsNullOrWhiteSpace(state)),
                StringComparer.OrdinalIgnoreCase);

            var errors = new List<string>();
            var seenConditions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (rules.Count == 0 && !hasElseRule)
            {
                errors.Add("Expression has no rules.");
            }

            for (var i = 0; i < rules.Count; i++)
            {
                var rule = rules[i];
                var whenState = rule.WhenState.StateName;
                var transitionTo = rule.TransitionTo.StateName;
                var label = $"Rule {i + 1}";

                if (string.IsNullOrWhiteSpace(whenState))
                {
                    errors.Add($"{label}: missing condition state.");
                }
                else
                {
                    if (!stateSet.Contains(whenState))
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
                else if (!stateSet.Contains(transitionTo))
                {
                    errors.Add($"{label}: target state '{transitionTo}' is not defined in the state list.");
                }
            }

            if (hasElseRule)
            {
                if (!elseState.HasValue)
                {
                    errors.Add("Else rule is enabled but has no target state.");
                }
                else if (!stateSet.Contains(elseState.StateName))
                {
                    errors.Add($"Else rule target state '{elseState.StateName}' is not defined in the state list.");
                }
            }

            return errors;
        }

        private bool TryGetEffectiveExpression(out StateExpression effectiveExpression, out string error)
        {
            if (!string.IsNullOrWhiteSpace(expressionText))
            {
                return TryParse(expressionText, out effectiveExpression, out error);
            }

            effectiveExpression = this;
            error = string.Empty;
            return true;
        }

        private string BuildCompiledExpressionText()
        {
            var builder = new StringBuilder();

            if (hasElseRule)
            {
                for (var i = 0; i < rules.Count; i++)
                {
                    var rule = rules[i];
                    if (builder.Length > 0)
                    {
                        builder.AppendLine();
                    }

                    builder.Append(i == 0 ? "if " : "elif ");
                    builder.Append(rule.WhenState.StateName);
                    builder.Append(": ");
                    builder.Append(rule.TransitionTo.StateName);
                }

                if (builder.Length > 0)
                {
                    builder.AppendLine();
                }

                builder.Append("else: ");
                builder.Append(elseState.StateName);
            }
            else
            {
                for (var i = 0; i < rules.Count; i++)
                {
                    var rule = rules[i];
                    if (builder.Length > 0)
                    {
                        builder.AppendLine();
                    }

                    builder.Append(i == 0 ? "if " : "elif ");
                    builder.Append(rule.WhenState.StateName);
                    builder.Append(": ");
                    builder.Append(rule.TransitionTo.StateName);
                }
            }

            return builder.ToString();
        }

        public static bool TryParse(string expressionText, out StateExpression expression, out string error)
        {
            expression = new StateExpression();

            if (string.IsNullOrWhiteSpace(expressionText))
            {
                error = "Expression is empty.";
                return false;
            }

            var remaining = expressionText.Trim();
            var parsedRules = new List<Rule>();
            StateReference parsedElseState = new();
            var hasElseRule = false;

            while (!string.IsNullOrEmpty(remaining))
            {
                if (TryConsumeKeyword(ref remaining, "if") || TryConsumeKeyword(ref remaining, "elif"))
                {
                    var separatorIndex = remaining.IndexOf(':');
                    if (separatorIndex < 0)
                    {
                        error = "Each if/elif rule must contain ':'.";
                        return false;
                    }

                    var condition = remaining[..separatorIndex].Trim();
                    remaining = remaining[(separatorIndex + 1)..].TrimStart();

                    if (string.IsNullOrWhiteSpace(condition))
                    {
                        error = "A rule is missing its condition state.";
                        return false;
                    }

                    var nextKeywordIndex = FindNextKeywordIndex(remaining);
                    string targetState;

                    if (nextKeywordIndex >= 0)
                    {
                        targetState = remaining[..nextKeywordIndex].Trim();
                        remaining = remaining[nextKeywordIndex..].TrimStart();
                    }
                    else
                    {
                        targetState = remaining.Trim();
                        remaining = string.Empty;
                    }

                    if (string.IsNullOrWhiteSpace(targetState))
                    {
                        error = $"Rule for '{condition}' is missing its target state.";
                        return false;
                    }

                    parsedRules.Add(new Rule());
                    parsedRules[^1].WhenState.Set(condition);
                    parsedRules[^1].TransitionTo.Set(targetState);
                    continue;
                }

                if (TryConsumeKeyword(ref remaining, "else"))
                {
                    if (!remaining.StartsWith(":", StringComparison.Ordinal))
                    {
                        error = "Else rule must use the format 'else: TargetState'.";
                        return false;
                    }

                    remaining = remaining[1..].TrimStart();

                    if (string.IsNullOrWhiteSpace(remaining))
                    {
                        error = "Else rule is missing its target state.";
                        return false;
                    }

                    parsedElseState.Set(remaining.Trim());
                    hasElseRule = true;
                    remaining = string.Empty;
                    continue;
                }

                error = $"Could not parse expression near '{remaining}'.";
                return false;
            }

            expression.rules.Clear();
            expression.rules.AddRange(parsedRules);
            expression.hasElseRule = hasElseRule;
            expression.elseState = parsedElseState;
            expression.expressionText = expressionText.Trim();

            error = string.Empty;
            return true;
        }

        private static bool TryConsumeKeyword(ref string value, string keyword)
        {
            if (!value.StartsWith(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (value.Length > keyword.Length && !char.IsWhiteSpace(value[keyword.Length]) && value[keyword.Length] != ':')
            {
                return false;
            }

            value = value[keyword.Length..].TrimStart();
            return true;
        }

        private static int FindNextKeywordIndex(string value)
        {
            var elifIndex = IndexOfKeyword(value, "elif");
            var elseIndex = IndexOfElseKeyword(value);

            if (elifIndex < 0)
            {
                return elseIndex;
            }

            if (elseIndex < 0)
            {
                return elifIndex;
            }

            return Mathf.Min(elifIndex, elseIndex);
        }

        private static int IndexOfKeyword(string value, string keyword)
        {
            for (var i = 0; i <= value.Length - keyword.Length; i++)
            {
                if (!value.AsSpan(i, keyword.Length).Equals(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var startsAtBoundary = i == 0 || char.IsWhiteSpace(value[i - 1]);
                var endsAtBoundary = i + keyword.Length >= value.Length || char.IsWhiteSpace(value[i + keyword.Length]);
                if (startsAtBoundary && endsAtBoundary)
                {
                    return i;
                }
            }

            return -1;
        }

        private static int IndexOfElseKeyword(string value)
        {
            for (var i = 0; i <= value.Length - 4; i++)
            {
                if (!value.AsSpan(i, 4).Equals("else", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var startsAtBoundary = i == 0 || char.IsWhiteSpace(value[i - 1]);
                var colonIndex = i + 4;
                if (startsAtBoundary && colonIndex < value.Length && value[colonIndex] == ':')
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
