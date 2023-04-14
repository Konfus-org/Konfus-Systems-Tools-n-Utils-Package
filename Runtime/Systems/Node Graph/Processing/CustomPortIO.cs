using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace Konfus.Systems.Node_Graph
{
    public delegate void CustomPortIODelegate(Node node, List<SerializableEdge> edges, NodePort outputPort = null);

    public static class CustomPortIO
    {
        private class PortIOPerField : Dictionary<string, CustomPortIODelegate>
        {
        }

        private class PortIOPerNode : Dictionary<Type, PortIOPerField>
        {
        }

        private static readonly Dictionary<Type, List<Type>> assignableTypes = new();
        private static readonly PortIOPerNode customIOPortMethods = new();

        static CustomPortIO()
        {
            LoadCustomPortMethods();
        }

        private static void LoadCustomPortMethods()
        {
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            foreach (Type type in AppDomain.CurrentDomain.GetAllTypes())
            {
                if (type.IsAbstract || type.ContainsGenericParameters)
                    continue;
                if (!type.IsSubclassOf(typeof(Node)))
                    continue;

                MethodInfo[] methods = type.GetMethods(bindingFlags);

                foreach (MethodInfo method in methods)
                {
                    var portInputAttr = method.GetCustomAttribute<CustomPortInputAttribute>();
                    var portOutputAttr = method.GetCustomAttribute<CustomPortOutputAttribute>();

                    if (portInputAttr == null && portOutputAttr == null)
                        continue;

                    ParameterInfo[] p = method.GetParameters();
                    bool nodePortSignature = false;

                    // Check if the function can take a NodePort in optional param
                    if (p.Length == 2 && p[1].ParameterType == typeof(NodePort))
                        nodePortSignature = true;

                    CustomPortIODelegate deleg;
#if ENABLE_IL2CPP
					// IL2CPP doesn't support expression builders
					if (nodePortSignature)
					{
						deleg = new CustomPortIODelegate((node, edges, port) => {
							Debug.Log(port);
							method.Invoke(node, new object[]{ edges, port});
						});
					}
					else
					{
						deleg = new CustomPortIODelegate((node, edges, port) => {
							method.Invoke(node, new object[]{ edges });
						});
					}
#else
                    ParameterExpression p1 = Expression.Parameter(typeof(Node), "node");
                    ParameterExpression p2 = Expression.Parameter(typeof(List<SerializableEdge>), "edges");
                    ParameterExpression p3 = Expression.Parameter(typeof(NodePort), "port");

                    MethodCallExpression ex;
                    if (nodePortSignature)
                        ex = Expression.Call(Expression.Convert(p1, type), method, p2, p3);
                    else
                        ex = Expression.Call(Expression.Convert(p1, type), method, p2);

                    deleg = Expression.Lambda<CustomPortIODelegate>(ex, p1, p2, p3).Compile();
#endif

                    string fieldName = portInputAttr == null ? portOutputAttr.fieldName : portInputAttr.fieldName;
                    Type customType = portInputAttr == null ? portOutputAttr.outputType : portInputAttr.inputType;
                    FieldInfo field = type.GetField(fieldName, bindingFlags);
                    if (field == null)
                    {
                        Debug.LogWarning("Can't use custom IO port function '" + method.Name + "' of class '" +
                                         type.Name + "': No field named " + fieldName + " found");
                        continue;
                    }

                    Type fieldType = field.FieldType;

                    AddCustomIOMethod(type, fieldName, deleg);

                    AddAssignableTypes(customType, fieldType);
                    AddAssignableTypes(fieldType, customType);
                }
            }
        }

        public static CustomPortIODelegate GetCustomPortMethod(Type nodeType, string fieldName)
        {
            PortIOPerField portIOPerField;
            CustomPortIODelegate deleg;

            customIOPortMethods.TryGetValue(nodeType, out portIOPerField);

            if (portIOPerField == null)
                return null;

            portIOPerField.TryGetValue(fieldName, out deleg);

            return deleg;
        }

        private static void AddCustomIOMethod(Type nodeType, string fieldName, CustomPortIODelegate deleg)
        {
            if (!customIOPortMethods.ContainsKey(nodeType))
                customIOPortMethods[nodeType] = new PortIOPerField();

            customIOPortMethods[nodeType][fieldName] = deleg;
        }

        private static void AddAssignableTypes(Type fromType, Type toType)
        {
            if (!assignableTypes.ContainsKey(fromType))
                assignableTypes[fromType] = new List<Type>();

            assignableTypes[fromType].Add(toType);
        }

        public static bool IsAssignable(Type input, Type output)
        {
            if (assignableTypes.ContainsKey(input))
                return assignableTypes[input].Contains(output);
            return false;
        }
    }
}