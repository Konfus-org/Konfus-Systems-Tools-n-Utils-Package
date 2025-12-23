using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Konfus.Utility.Attributes;
using UnityEngine;

namespace Konfus.Utility.Design_Patterns
{
    public class DependencyInjector : MonoBehaviour
    {
        private const BindingFlags BindingFlags = System.Reflection.BindingFlags.Public |
                                                  System.Reflection.BindingFlags.NonPublic |
                                                  System.Reflection.BindingFlags.Instance;

        private readonly Dictionary<Type, object> _registry = new();

        private void Awake()
        {
            // Register dependency providers
            var monoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (var provider in monoBehaviours)
            {
                RegisterProvider(provider);
            }

            // Inject dependencies
            foreach (var behaviour in monoBehaviours)
            {
                Inject(behaviour);
            }
        }

        private void Inject(MonoBehaviour obj)
        {
            var type = obj.GetType();

            var fields = type.GetFields(BindingFlags);
            foreach (var field in fields)
            {
                if (!Attribute.IsDefined(field, typeof(InjectAttribute))) continue;

                var fieldType = field.FieldType;
                var resolvedInstance = Resolve(fieldType);
                if (resolvedInstance == null)
                    throw new Exception($"Could not resolve dependency for {fieldType.Name} on {type.Name}");

                field.SetValue(obj, resolvedInstance);
            }

            var properties = type.GetProperties(BindingFlags);
            foreach (var property in properties)
            {
                if (!Attribute.IsDefined(property, typeof(InjectAttribute))) continue;

                var propertyType = property.PropertyType;
                var resolvedInstance = Resolve(propertyType);
                if (resolvedInstance == null)
                    throw new Exception($"Could not resolve dependency for {propertyType.Name} on {type.Name}");

                property.SetValue(obj, resolvedInstance);
            }

            var methods = type.GetMethods(BindingFlags);
            foreach (var method in methods)
            {
                if (!Attribute.IsDefined(method, typeof(InjectAttribute))) continue;

                var paramType = method.GetParameters().FirstOrDefault()?.ParameterType;
                var resolvedInstance = Resolve(paramType);
                if (resolvedInstance == null)
                    throw new Exception(
                        $"Could not resolve dependency for {method.Name}({paramType?.Name} x) on {type.Name}");

                method.Invoke(obj, new[] { resolvedInstance });
            }
        }

        private void RegisterProvider(MonoBehaviour provider)
        {
            var type = provider.GetType();

            if (Attribute.IsDefined(type, typeof(ProvideAttribute))) Register(provider, type, provider);

            var fields = type.GetFields(BindingFlags);
            foreach (var field in fields)
            {
                if (!Attribute.IsDefined(field, typeof(ProvideAttribute))) continue;
                Register(provider, field.FieldType, field.GetValue(provider));
            }

            var properties = type.GetProperties(BindingFlags);
            ;
            foreach (var property in properties)
            {
                if (!Attribute.IsDefined(property, typeof(ProvideAttribute))) continue;
                Register(provider, property.PropertyType, property.GetValue(provider));
            }

            var methods = type.GetMethods(BindingFlags);
            foreach (var method in methods)
            {
                if (!Attribute.IsDefined(method, typeof(ProvideAttribute))) continue;
                Register(provider, method.ReturnType, method.Invoke(provider, null));
            }
        }

        private void Register(MonoBehaviour provider, Type type, object? instance)
        {
            if (instance != null) _registry[type] = instance;
            else throw new Exception($"Provider {provider.GetType().Name} returned null for {type.Name}");
        }

        private object? Resolve(Type? type)
        {
            if (type == null) return null;
            _registry.TryGetValue(type, out var instance);
            return instance;
        }
    }
}