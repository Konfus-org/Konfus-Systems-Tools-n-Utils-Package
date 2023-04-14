using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Konfus.Systems.Node_Graph
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ConverterNodeAttribute : Attribute
    {
        public Type from, to;

        public ConverterNodeAttribute(Type from, Type to)
        {
            this.from = from;
            this.to = to;
        }
    }

    public interface IConversionNode
    {
        public string GetConversionInput();
        public string GetConversionOutput();
    }

    public static class ConversionNodeAdapter
    {
        private static bool conversionsLoaded;

        private static readonly Dictionary<(Type from, Type to), Type> adapters = new();

        private static void LoadAllAdapters()
        {
            foreach (Type currType in AppDomain.CurrentDomain.GetAllTypes())
            {
                var conversionAttrib = currType.GetCustomAttribute<ConverterNodeAttribute>();
                if (conversionAttrib != null)
                {
                    Debug.Assert(typeof(IConversionNode).IsAssignableFrom(currType),
                        "Class marked with ConverterNode attribute must implement the IConversionNode interface");
                    Debug.Assert(typeof(Node).IsAssignableFrom(currType),
                        "Class marked with ConverterNode attribute must inherit from BaseNode");

                    adapters.Add((conversionAttrib.from, conversionAttrib.to), currType);
                }
            }

            conversionsLoaded = true;
        }

        public static bool AreAssignable(Type from, Type to)
        {
            if (!conversionsLoaded)
                LoadAllAdapters();

            return adapters.ContainsKey((from, to));
        }

        public static Type GetConversionNode(Type from, Type to)
        {
            if (!conversionsLoaded)
                LoadAllAdapters();

            return adapters.TryGetValue((from, to), out Type nodeType) ? nodeType : null;
        }
    }
}