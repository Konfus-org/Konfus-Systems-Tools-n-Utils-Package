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
            if (!CacheMethod())
                return;

            int paramCount = _cachedParams!.Length;

            // Support: void Method()
            if (paramCount == 0)
            {
                _cachedMethod!.Invoke(target, null);
                return;
            }

            // We only allow 0 or 1 parameters in EnsureMethod(), so param_count must be 1 here.
            Type paramType = _cachedParams[0].ParameterType;

            // Support: void Method(InputAction.CallbackContext ctx)
            if (paramType == typeof(InputAction.CallbackContext))
            {
                _cachedMethod!.Invoke(target, new object?[] { ctx });
                return;
            }

            // Support: void Method(T value) for arbitrary T
            // ReadValueAsObject returns boxed value of the action's current value type.
            object? value = ctx.ReadValueAsObject();

            if (value == null)
            {
                if (paramType.IsValueType)
                {
                    // For value types, pass default(T)
                    value = Activator.CreateInstance(paramType);
                }

                _cachedMethod!.Invoke(target, new[] { value });
                return;
            }

            // If the boxed type matches (or derives), we're good.
            Type valueType = value.GetType();
            if (paramType.IsAssignableFrom(valueType))
            {
                _cachedMethod!.Invoke(target, new[] { value });
                return;
            }

            // Common case: action is float but handler wants double/int/etc (or vice versa)
            // Convert.ChangeType only works for IConvertible primitives.
            try
            {
                object converted = Convert.ChangeType(value, paramType);
                _cachedMethod!.Invoke(target, new[] { converted });
            }
            catch
            {
                Debug.LogWarning(
                    $"Type mismatch invoking '{method}' on '{target}'. " +
                    $"Expected '{paramType.Name}', got '{valueType.Name}'. " +
                    $"Consider using signature '{method}(InputAction.CallbackContext ctx)' to read the type explicitly.",
                    target
                );
            }
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
    }
}