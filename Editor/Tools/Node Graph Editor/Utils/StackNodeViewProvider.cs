using System;
using System.Collections.Generic;
using System.Linq;
using Konfus.Systems.Node_Graph;
using UnityEditor;

namespace Konfus.Tools.NodeGraphEditor
{
    public static class StackNodeViewProvider
    {
        private static readonly Dictionary<Type, Type> stackNodeViewPerType = new();

        static StackNodeViewProvider()
        {
            foreach (Type t in TypeCache.GetTypesWithAttribute<CustomStackNodeView>())
            {
                CustomStackNodeView attr = t.GetCustomAttributes(false).Select(a => a as CustomStackNodeView)
                    .FirstOrDefault();

                stackNodeViewPerType.Add(attr.stackNodeType, t);
                // Debug.Log("Add " + attr.stackNodeType);
            }
        }

        public static Type GetStackNodeCustomViewType(Type stackNodeType)
        {
            // Debug.Log(stackNodeType);
            foreach (KeyValuePair<Type, Type> t in stackNodeViewPerType)
            {
                // Debug.Log(t.Key + " -> " + t.Value);
            }

            stackNodeViewPerType.TryGetValue(stackNodeType, out Type view);
            return view;
        }
    }
}