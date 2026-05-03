using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Konfus.Input
{
    [Serializable]
    public sealed class InputTarget
    {
        [SerializeField]
        private MonoBehaviour? target;
        [SerializeField]
        private string? method;
        [NonSerialized]
        private MethodInfo? _cachedMethod;
        [NonSerialized]
        private ParameterInfo[]? _cachedParams;

        public void InvalidateCache()
        {
            _cachedMethod = null;
            _cachedParams = null;
        }

        public void Invoke(InputAction.CallbackContext ctx)
        {
            InvokeInternal(ctx, allowContextParam: true);
        }

        public void Invoke()
        {
            InvokeInternal(null, allowContextParam: false);
        }

        public void Invoke(object? value)
        {
            InvokeInternal(value, allowContextParam: false);
        }

        public void Invoke(InputConditionType trigger, object? value)
        {
            if (TryInvokePhaseFallback(trigger, value))
            {
                return;
            }

            Invoke(value);
        }

        private bool CacheMethod()
        {
            if (target == null)
                return false;

            if (string.IsNullOrWhiteSpace(method))
                return false;

            if (_cachedMethod != null && _cachedParams != null)
                return true;

            Type type = target.GetType();

            const BindingFlags flags =
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic;

            // Find method by name with 0 or 1 parameters (prefer 1 param if multiple overloads exist)
            MethodInfo? found0 = null;
            MethodInfo? found1 = null;

            foreach (MethodInfo m in type.GetMethods(flags))
            {
                if (!string.Equals(m.Name, method, StringComparison.Ordinal))
                    continue;

                ParameterInfo[] ps = m.GetParameters();
                if (ps.Length == 0)
                    found0 ??= m;
                else if (ps.Length == 1)
                    found1 ??= m;
            }

            _cachedMethod = found1 ?? found0;

            if (_cachedMethod == null)
            {
                Debug.LogWarning(
                    $"Method '{method}' not found on '{type.Name}' (only 0 or 1 parameter overloads are supported).",
                    target
                );
                _cachedParams = null;
                return false;
            }

            _cachedParams = _cachedMethod.GetParameters();
            return true;
        }

        private void InvokeInternal(object? value, bool allowContextParam)
        {
            if (!CacheMethod())
                return;

            int paramCount = _cachedParams!.Length;
            if (paramCount == 0)
            {
                _cachedMethod!.Invoke(target, null);
                return;
            }

            Type paramType = _cachedParams[0].ParameterType;
            if (allowContextParam && value is InputAction.CallbackContext ctx && paramType == typeof(InputAction.CallbackContext))
            {
                _cachedMethod!.Invoke(target, new object?[] { ctx });
                return;
            }

            object? boxedValue = value;
            if (allowContextParam && value is InputAction.CallbackContext callbackContext)
            {
                boxedValue = callbackContext.ReadValueAsObject();
            }

            if (boxedValue == null)
            {
                if (paramType.IsValueType)
                {
                    boxedValue = Activator.CreateInstance(paramType);
                }

                _cachedMethod!.Invoke(target, new[] { boxedValue });
                return;
            }

            Type valueType = boxedValue.GetType();
            if (paramType.IsAssignableFrom(valueType))
            {
                _cachedMethod!.Invoke(target, new[] { boxedValue });
                return;
            }

            try
            {
                object converted = Convert.ChangeType(boxedValue, paramType);
                _cachedMethod!.Invoke(target, new[] { converted });
            }
            catch
            {
                Debug.LogWarning(
                    $"Type mismatch invoking '{method}' on '{target}'. " +
                    $"Expected '{paramType.Name}', got '{valueType.Name}'.",
                    target
                );
            }
        }

        private bool TryInvokePhaseFallback(InputConditionType trigger, object? value)
        {
            if (target == null || string.IsNullOrWhiteSpace(method))
            {
                return false;
            }

            string? fallbackMethodName = GetPhaseFallbackMethodName(trigger);
            if (string.IsNullOrWhiteSpace(fallbackMethodName))
            {
                return false;
            }

            if (!TryFindMethod(target.GetType(), fallbackMethodName, out MethodInfo? fallbackMethod, out ParameterInfo[]? fallbackParams))
            {
                return false;
            }

            if (fallbackMethod == null || fallbackParams == null)
            {
                return false;
            }

            InvokeMethod(fallbackMethod, fallbackParams, value, allowContextParam: false);
            return true;
        }

        private string? GetPhaseFallbackMethodName(InputConditionType trigger)
        {
            string? methodName = method;
            if (string.IsNullOrWhiteSpace(methodName))
            {
                return null;
            }

            if (string.Equals(methodName, "HandleInteract", StringComparison.Ordinal))
            {
                return trigger == InputConditionType.Cancelled ? "Interact" : null;
            }

            if (string.Equals(methodName, "HandleUnpossess", StringComparison.Ordinal))
            {
                return trigger == InputConditionType.Performed ? "Unpossess" : null;
            }

            const string prefix = "Handle";
            if (!methodName.StartsWith(prefix, StringComparison.Ordinal) || methodName.Length <= prefix.Length)
            {
                return null;
            }

            string actionName = methodName.Substring(prefix.Length);
            return trigger switch
            {
                InputConditionType.Performed => $"Start{actionName}",
                InputConditionType.Cancelled => $"Stop{actionName}",
                _ => null
            };
        }

        private static bool TryFindMethod(
            Type type,
            string methodName,
            out MethodInfo? methodInfo,
            out ParameterInfo[]? parameters)
        {
            const BindingFlags flags =
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic;

            MethodInfo? found0 = null;
            MethodInfo? found1 = null;

            foreach (MethodInfo m in type.GetMethods(flags))
            {
                if (!string.Equals(m.Name, methodName, StringComparison.Ordinal))
                    continue;

                ParameterInfo[] ps = m.GetParameters();
                if (ps.Length == 0)
                    found0 ??= m;
                else if (ps.Length == 1)
                    found1 ??= m;
            }

            methodInfo = found1 ?? found0;
            parameters = methodInfo?.GetParameters();
            return methodInfo != null && parameters != null;
        }

        private void InvokeMethod(
            MethodInfo methodInfo,
            ParameterInfo[] parameters,
            object? value,
            bool allowContextParam)
        {
            int paramCount = parameters.Length;
            if (paramCount == 0)
            {
                methodInfo.Invoke(target, null);
                return;
            }

            Type paramType = parameters[0].ParameterType;
            if (allowContextParam && value is InputAction.CallbackContext ctx && paramType == typeof(InputAction.CallbackContext))
            {
                methodInfo.Invoke(target, new object?[] { ctx });
                return;
            }

            object? boxedValue = value;
            if (allowContextParam && value is InputAction.CallbackContext callbackContext)
            {
                boxedValue = callbackContext.ReadValueAsObject();
            }

            if (boxedValue == null)
            {
                if (paramType.IsValueType)
                {
                    boxedValue = Activator.CreateInstance(paramType);
                }

                methodInfo.Invoke(target, new[] { boxedValue });
                return;
            }

            Type valueType = boxedValue.GetType();
            if (paramType.IsAssignableFrom(valueType))
            {
                methodInfo.Invoke(target, new[] { boxedValue });
                return;
            }

            try
            {
                object converted = Convert.ChangeType(boxedValue, paramType);
                methodInfo.Invoke(target, new[] { converted });
            }
            catch
            {
                Debug.LogWarning(
                    $"Type mismatch invoking '{method}' on '{target}'. " +
                    $"Expected '{paramType.Name}', got '{valueType.Name}'.",
                    target
                );
            }
        }
    }
}
