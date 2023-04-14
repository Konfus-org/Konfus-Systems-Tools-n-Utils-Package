using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Konfus.Systems.Node_Graph.Schema
{
    public static class EdgeProcessing
    {
        private static Dictionary<EdgeProcessOrderKey, EdgeProcessOrderCallback> _edgeProcessOrderCallbackByKey;

        public delegate IList<SerializableEdge> EdgeProcessOrderCallback(IList<SerializableEdge> edges);

        public static Dictionary<EdgeProcessOrderKey, EdgeProcessOrderCallback> EdgeProcessOrderCallbackByKey =>
            PropertyUtils.LazyLoad(ref _edgeProcessOrderCallbackByKey, BuildEdgeProcessOrderBehaviorDict);

        public static EdgeProcessOrderKey[] EdgeProcessOrderBehaviorKeys =>
            EdgeProcessOrderCallbackByKey.Keys.ToArray();

        public static IEnumerable<string> EdgeProcessOrderBehaviorKeyValues =>
            EdgeProcessOrderBehaviorKeys.Select(e => e.Value);

        private static Dictionary<EdgeProcessOrderKey, EdgeProcessOrderCallback> BuildEdgeProcessOrderBehaviorDict()
        {
            Dictionary<EdgeProcessOrderKey, EdgeProcessOrderCallback> edgeProcessOrderByName = new();

            foreach (MethodInfo methodInfo in TypeCache.GetMethodsWithAttribute<EdgeOrdererAttribute>())
            {
                var attribute = methodInfo.GetCustomAttribute<EdgeOrdererAttribute>();

                if (edgeProcessOrderByName.ContainsKey(attribute.Key))
                {
                    Debug.LogError("Edge Ordering Method with Key: " + attribute.Key + " already exists. SKIPPING!");
                    continue;
                }

                edgeProcessOrderByName.Add(attribute.Key,
                    methodInfo.CreateDelegate(typeof(EdgeProcessOrderCallback)) as EdgeProcessOrderCallback);
            }

            return edgeProcessOrderByName;
        }
    }
}