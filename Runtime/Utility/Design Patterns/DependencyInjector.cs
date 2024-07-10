using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Konfus.Utility.Design_Patterns
{
    public class DependencyInjector : Singleton<DependencyInjector>
    {
        private const BindingFlags BINDING_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        private readonly Dictionary<Type, object> _registry = new();

        protected override void OnAwake()
        {
            base.OnAwake();

            // Register dependency providers
            MonoBehaviour[] monoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
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
            Type type = obj.GetType();

            FieldInfo[] fields = type.GetFields(BINDING_FLAGS);
            foreach (var field in fields)
            {
                if (!Attribute.IsDefined(field, typeof(InjectAttribute))) continue;
                
                Type fieldType = field.FieldType;
                object resolvedInstance = Resolve(fieldType);
                if (resolvedInstance == null)
                {
                    throw new Exception($"Could not resolve dependency for {fieldType.Name} on {type.Name}");
                }
                
                field.SetValue(obj, resolvedInstance);
            }
            
            PropertyInfo[] properties = type.GetProperties(BINDING_FLAGS);
            foreach (var property in properties)
            {
                if (!Attribute.IsDefined(property, typeof(InjectAttribute))) continue;
                
                Type propertyType = property.PropertyType;
                object resolvedInstance = Resolve(propertyType);
                if (resolvedInstance == null)
                {
                    throw new Exception($"Could not resolve dependency for {propertyType.Name} on {type.Name}");
                }
                
                property.SetValue(obj, resolvedInstance);
            }
            
            MethodInfo[] methods = type.GetMethods(BINDING_FLAGS);
            foreach (var method in methods)
            {
                if (!Attribute.IsDefined(method, typeof(InjectAttribute))) continue;
                
                Type paramType = method.GetParameters().First().ParameterType;
                object resolvedInstance = Resolve(paramType);
                if (resolvedInstance == null)
                {
                    throw new Exception($"Could not resolve dependency for {method.Name}({paramType.Name} x) on {type.Name}");
                }

                method.Invoke(obj, new[] { resolvedInstance });
            }
        }

        private void RegisterProvider(MonoBehaviour provider)
        {
            FieldInfo[] fields = provider.GetType().GetFields(BINDING_FLAGS);
            foreach (var field in fields)
            {
                if (!Attribute.IsDefined(field, typeof(ProvideAttribute))) continue;
                Register(provider, field.FieldType, field.GetValue(provider));
            }
            
            PropertyInfo[] properties = provider.GetType().GetProperties(BINDING_FLAGS);;
            foreach (var property in properties)
            {
                if (!Attribute.IsDefined(property, typeof(ProvideAttribute))) continue;
                Register(provider, property.PropertyType, property.GetValue(provider));
            }
            
            MethodInfo[] methods = provider.GetType().GetMethods(BINDING_FLAGS);
            foreach (var method in methods)
            {
                if (!Attribute.IsDefined(method, typeof(ProvideAttribute))) continue;
                Register(provider, method.ReturnType, method.Invoke(provider, null));
            }
        }
        
        private void Register(MonoBehaviour provider, Type type, object instance)
        {
            if (instance != null) _registry.Add(type, instance);
            else throw new Exception($"Provider {provider.GetType().Name} returned null for {type.Name}");
        }

        private object Resolve(Type type)
        {
            _registry.TryGetValue(type, out var instance);
            return instance;
        }
    }
}