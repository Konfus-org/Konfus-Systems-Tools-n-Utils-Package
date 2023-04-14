using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Konfus.Systems.Node_Graph;
using Konfus.Tools.NodeGraphEditor.Utils;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static Konfus.Systems.Node_Graph.NodeUtils;
using Node = Konfus.Systems.Node_Graph.Node;

namespace Konfus.Tools.NodeGraphEditor
{
    public static partial class NodeProvider
    {
        public struct PortDescription
        {
            public Type nodeType;
            public Type portType;
            public bool isInput;
            public string portFieldName;
            public string portIdentifier;
            public string portDisplayName;
        }

        private static readonly Dictionary<Type, MonoScript> nodeViewScripts = new();
        private static readonly Dictionary<Type, MonoScript> nodeScripts = new();
        private static readonly Dictionary<Type, Type> nodeViewPerType = new();

        public class NodeDescriptions
        {
            public Dictionary<string, Type> nodePerMenuTitle = new();
            public List<Type> slotTypes = new();
            public List<PortDescription> nodeCreatePortDescription = new();
        }

        public struct NodeSpecificToGraph
        {
            public Type nodeType;
            public List<MethodInfo> isCompatibleWithGraph;
            public Type compatibleWithGraphType;
        }

        private static readonly Dictionary<Graph, NodeDescriptions> specificNodeDescriptions = new();
        private static readonly List<NodeSpecificToGraph> specificNodes = new();

        private static readonly NodeDescriptions genericNodes = new();

        static NodeProvider()
        {
            BuildScriptCache();
            BuildGenericNodeCache();
        }

        public static void LoadGraph(Graph graph)
        {
            // Clear old graph data in case there was some
            specificNodeDescriptions.Remove(graph);
            var descriptions = new NodeDescriptions();
            specificNodeDescriptions.Add(graph, descriptions);

            Type graphType = graph.GetType();
            foreach (NodeSpecificToGraph nodeInfo in specificNodes)
            {
                bool compatible = nodeInfo.compatibleWithGraphType == null ||
                                  nodeInfo.compatibleWithGraphType == graphType;

                if (nodeInfo.isCompatibleWithGraph != null)
                    foreach (MethodInfo method in nodeInfo.isCompatibleWithGraph)
                        compatible &= (bool) method?.Invoke(null, new object[] {graph});

                if (compatible)
                    BuildCacheForNode(nodeInfo.nodeType, descriptions, graph);
            }
        }

        public static void UnloadGraph(Graph graph)
        {
            specificNodeDescriptions.Remove(graph);
        }

        private static void BuildGenericNodeCache()
        {
            foreach (Type nodeType in TypeCache.GetTypesDerivedFrom<Node>())
            {
                if (!IsNodeAccessibleFromMenu(nodeType))
                    continue;

                if (IsNodeSpecificToGraph(nodeType))
                    continue;

                BuildCacheForNode(nodeType, genericNodes);
            }
        }

        private static void BuildCacheForNode(Type nodeType, NodeDescriptions targetDescription, Graph graph = null)
        {
            var attrs = nodeType.GetCustomAttributes(typeof(NodeMenuItemAttribute), false) as NodeMenuItemAttribute[];

            if (attrs != null && attrs.Length > 0)
                foreach (NodeMenuItemAttribute attr in attrs)
                    targetDescription.nodePerMenuTitle[attr.menuTitle] = nodeType;

            foreach (MemberInfo field in nodeType
                         .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                         .Cast<MemberInfo>()
                         .Concat(nodeType.GetProperties(BindingFlags.Instance | BindingFlags.Public |
                                                        BindingFlags.NonPublic))
                    )
                if (field.GetCustomAttribute<HideInInspector>() == null && field.GetCustomAttributes()
                        .Any(c => c is InputAttribute || c is OutputAttribute))
                    targetDescription.slotTypes.Add(field.GetUnderlyingType());

            ProvideNodePortCreationDescription(nodeType, targetDescription, graph);
        }

        private static bool IsNodeAccessibleFromMenu(Type nodeType)
        {
            if (nodeType.IsAbstract)
                return false;

            return nodeType.GetCustomAttributes<NodeMenuItemAttribute>().Count() > 0;
        }

        // Check if node has anything that depends on the graph type or settings
        private static bool IsNodeSpecificToGraph(Type nodeType)
        {
            IEnumerable<MethodInfo> isCompatibleWithGraphMethods = nodeType
                .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic |
                            BindingFlags.FlattenHierarchy)
                .Where(m => m.GetCustomAttribute<IsCompatibleWithGraph>() != null);
            IEnumerable<NodeMenuItemAttribute> nodeMenuAttributes =
                nodeType.GetCustomAttributes<NodeMenuItemAttribute>();

            List<Type> compatibleGraphTypes = nodeMenuAttributes.Where(n => n.onlyCompatibleWithGraph != null)
                .Select(a => a.onlyCompatibleWithGraph).ToList();

            var compatibleMethods = new List<MethodInfo>();
            foreach (MethodInfo method in isCompatibleWithGraphMethods)
            {
                // Check if the method is static and have the correct prototype
                ParameterInfo[] p = method.GetParameters();
                if (method.ReturnType != typeof(bool) || p.Count() != 1 || p[0].ParameterType != typeof(Graph))
                    Debug.LogError(
                        $"The function '{method.Name}' marked with the IsCompatibleWithGraph attribute either doesn't return a boolean or doesn't take one parameter of BaseGraph type.");
                else
                    compatibleMethods.Add(method);
            }

            if (compatibleMethods.Count > 0 || compatibleGraphTypes.Count > 0)
            {
                // We still need to add the element in specificNode even without specific graph
                if (compatibleGraphTypes.Count == 0)
                    compatibleGraphTypes.Add(null);

                foreach (Type graphType in compatibleGraphTypes)
                    specificNodes.Add(new NodeSpecificToGraph
                    {
                        nodeType = nodeType,
                        isCompatibleWithGraph = compatibleMethods,
                        compatibleWithGraphType = graphType
                    });
                return true;
            }

            return false;
        }

        private static void BuildScriptCache()
        {
            foreach (Type nodeType in TypeCache.GetTypesDerivedFrom<Node>())
            {
                if (!IsNodeAccessibleFromMenu(nodeType))
                    continue;

                AddNodeScriptAsset(nodeType);
            }

            foreach (Type nodeViewType in TypeCache.GetTypesDerivedFrom<NodeView>())
                if (!nodeViewType.IsAbstract)
                    AddNodeViewScriptAsset(nodeViewType);
        }

        private static FieldInfo SetGraph =>
            typeof(Node).GetField("graph", BindingFlags.NonPublic | BindingFlags.Instance);

        private static void ProvideNodePortCreationDescription(Type nodeType, NodeDescriptions targetDescription,
            Graph graph = null)
        {
            var node = Activator.CreateInstance(nodeType) as Node;
            try
            {
                SetGraph.SetValue(node, graph);
                node.InitializePorts();
                node.UpdateAllPorts();
            }
            catch (Exception)
            {
            }

            foreach (NodePort p in node.inputPorts)
                AddPort(p, true);
            foreach (NodePort p in node.outputPorts)
                AddPort(p, false);

            void AddPort(NodePort p, bool input)
            {
                targetDescription.nodeCreatePortDescription.Add(new PortDescription
                {
                    nodeType = nodeType,
                    portType = p.portData.DisplayType ?? p.fieldInfo.GetUnderlyingType(),
                    isInput = input,
                    portFieldName = p.fieldName,
                    portDisplayName = p.portData.displayName ?? p.fieldName,
                    portIdentifier = p.portData.Identifier
                });
            }
        }

        private static void AddNodeScriptAsset(Type type)
        {
            MonoScript nodeScriptAsset = FindScriptFromClassName(type.Name);

            // Try find the class name with Node name at the end
            if (nodeScriptAsset == null)
                nodeScriptAsset = FindScriptFromClassName(type.Name + "Node");
            if (nodeScriptAsset != null)
                nodeScripts[type] = nodeScriptAsset;
        }

        private static void AddNodeViewScriptAsset(Type type)
        {
            var attrs = type.GetCustomAttributes(typeof(NodeCustomEditor), false) as NodeCustomEditor[];

            if (attrs != null && attrs.Length > 0)
            {
                Type nodeType = attrs.First().nodeType;
                nodeViewPerType[nodeType] = type;

                MonoScript nodeViewScriptAsset = FindScriptFromClassName(type.Name);
                if (nodeViewScriptAsset == null)
                    nodeViewScriptAsset = FindScriptFromClassName(type.Name + "View");
                if (nodeViewScriptAsset == null)
                    nodeViewScriptAsset = FindScriptFromClassName(type.Name + "NodeView");

                if (nodeViewScriptAsset != null)
                    nodeViewScripts[type] = nodeViewScriptAsset;
            }
        }

        private static MonoScript FindScriptFromClassName(string className)
        {
            string[] scriptGUIDs = AssetDatabase.FindAssets($"t:script {className}");

            if (scriptGUIDs.Length == 0)
                return null;

            foreach (string scriptGUID in scriptGUIDs)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(scriptGUID);
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);

                if (script != null && string.Equals(className, Path.GetFileNameWithoutExtension(assetPath),
                        StringComparison.OrdinalIgnoreCase))
                    return script;
            }

            return null;
        }

        public static Type GetNodeViewTypeFromType(Type nodeType)
        {
            Type view;

            if (nodeViewPerType.TryGetValue(nodeType, out view))
                return view;

            Type baseType = null;

            // Allow for inheritance in node views: multiple C# node using the same view
            foreach (KeyValuePair<Type, Type> type in nodeViewPerType)
                // Find a view (not first fitted view) of nodeType
                if (nodeType.IsSubclassOf(type.Key) && (baseType == null || type.Value.IsSubclassOf(baseType)))
                    baseType = type.Value;

            if (baseType != null)
                return baseType;

            return view;
        }

        public static IEnumerable<(Type type, CustomClassMenuItem attribute)> GetBasicCustomClassMenuItemEntries()
        {
            foreach (Type type in TypeCache.GetTypesWithAttribute<CustomClassMenuItem>())
            {
                var attribute = type.GetCustomAttribute<CustomClassMenuItem>(false);
                if (!attribute.IsBasic) continue;
                yield return (type, attribute);
            }
        }

        public static IEnumerable<NodeMenuEntry> GetNodeMenuEntries(Graph graph = null)
        {
            NodeCreationMethod creationMethod = Node.CreateFromType;
            foreach (KeyValuePair<string, Type> node in genericNodes.nodePerMenuTitle)
                yield return new NodeMenuEntry(node.Key, node.Value, creationMethod, null);

            foreach ((Type type, CustomClassMenuItem attribute) in GetBasicCustomClassMenuItemEntries())
                yield return new NodeMenuEntry(attribute.menuTitle, attribute.nodeType ?? typeof(Node), creationMethod,
                    attribute.args);

            if (graph != null && specificNodeDescriptions.TryGetValue(graph, out NodeDescriptions specificNodes))
                foreach (KeyValuePair<string, Type> node in specificNodes.nodePerMenuTitle)
                    yield return new NodeMenuEntry(node.Key, node.Value, creationMethod, null);
        }

        public static IEnumerable<NodeMenuEntryMethod> CustomMenuItemMethods()
        {
            foreach (MethodInfo methodInfo in TypeCache.GetMethodsWithAttribute<CustomMenuItem>()
                         .Where(x => IsValidCustomNodeMenuItem(x)))
                yield return new NodeMenuEntryMethod(methodInfo, methodInfo.GetCustomAttribute<CustomMenuItem>(false),
                    methodInfo.GetCustomAttributes<CustomMenuItemFilter>(false).ToArray());
        }

        public static IEnumerable<NodeMenuEntryMethod> GetCustomClassMenuItemMethods()
        {
            foreach (Type type in TypeCache.GetTypesWithAttribute<CustomClassMenuItem>())
            {
                var attribute = type.GetCustomAttribute<CustomClassMenuItem>(false);
                if (attribute.IsBasic) continue;
                MethodInfo methodInfo = attribute.methodParentClass.GetMethod(attribute.methodName,
                    BindingFlags.Static | BindingFlags.Public);
                if (!IsValidCustomNodeMenuItem(methodInfo)) continue;
                yield return new NodeMenuEntryMethod(methodInfo, attribute,
                    type.GetCustomAttributes<CustomMenuItemFilter>(false).ToArray());
            }
        }

        public static IEnumerable<NodeMenuEntry> GetCustomNodeMenuEntries(
            Graph graph = null,
            IEnumerable<NodeMenuEntryMethod> customMenuItems = null
        )
        {
            foreach (NodeMenuEntryMethod nodeMenuEntryMethod in customMenuItems ??
                                                                CustomMenuItemMethods()
                                                                    .Concat(GetCustomClassMenuItemMethods()))
            {
                MethodInfo methodInfo = nodeMenuEntryMethod.MethodInfo;
                CustomMenuItem attribute = nodeMenuEntryMethod.Context;

                var method =
                    Delegate.CreateDelegate(typeof(NodeCreationMethod), methodInfo) as NodeCreationMethod;
                yield return new NodeMenuEntry(attribute.menuTitle, methodInfo.ReturnType, method, attribute.args);
            }
        }

        public static IEnumerable<SubGraph> GetMacros()
        {
            foreach (SubGraph subGraph in ScriptableObjectUtils.GetAllInstances<SubGraph>())
                if (subGraph.IsMacro)
                    yield return subGraph;
        }

        public static IEnumerable<NodeMenuEntry> GetMacroNodeMenuEntries()
        {
            foreach (SubGraph macro in GetMacros())
                yield return new NodeMenuEntry(macro.MacroOptions.MenuLocation, typeof(MacroNode),
                    MacroNode.InstantiateMacro, new object[] {macro});
        }

        public static IEnumerable<NodeMenuEntry> GetFilteredCustomNodeMenuEntries(
            Type checkForType,
            PortDescription port,
            IEnumerable<NodeMenuEntryMethod> customMenuItems = null
        )
        {
            foreach (NodeMenuEntryMethod nodeMenuEntryMethod in customMenuItems ??
                                                                CustomMenuItemMethods()
                                                                    .Concat(GetCustomClassMenuItemMethods()))
            {
                MethodInfo methodInfo = nodeMenuEntryMethod.MethodInfo;
                CustomMenuItem attribute = nodeMenuEntryMethod.Context;
                CustomMenuItemFilter[] methodFilters = nodeMenuEntryMethod.Filters;

                Dictionary<string, Type> filters = new();
                foreach (CustomMenuItemFilter filter in methodFilters.Where(
                             x => x.key == null || x.key == attribute.key))
                    filters.Add(filter.portFieldName, filter.type);

                string portFieldName = port.portFieldName;
                Type portNodeType = port.nodeType;
                if (methodInfo.ReturnType == portNodeType &&
                    (!filters.ContainsKey(portFieldName) || filters[portFieldName] == checkForType))
                {
                    var method =
                        Delegate.CreateDelegate(typeof(NodeCreationMethod), methodInfo) as NodeCreationMethod;
                    yield return new NodeMenuEntry(attribute.menuTitle, methodInfo.ReturnType, method, attribute.args);
                }
            }
        }

        public static bool IsValidCustomNodeMenuItem(MethodInfo method)
        {
            bool isValid = true;
            if (!typeof(Node).IsAssignableFrom(method.ReturnParameter.ParameterType))
            {
                Debug.LogError("CustomMenuItem: " + method.Name + " is not of return type BaseNode!");
                isValid = false;
            }

            if (method.GetParameters().Length != 3)
            {
                Debug.LogError("CustomMenuItem: " + method.Name + " params should only be Type, Vector2 and object[]!");
                isValid = false;
            }
            else
            {
                if (method.GetParameters()[0].ParameterType != typeof(Type))
                {
                    Debug.LogError("CustomMenuItem: " + method.Name + " first param should be of type Type!");
                    isValid = false;
                }

                if (method.GetParameters()[1].ParameterType != typeof(Vector2))
                {
                    Debug.LogError("CustomMenuItem: " + method.Name + " second param should be of type Vector2!");
                    isValid = false;
                }

                if (method.GetParameters()[2].ParameterType != typeof(object[]))
                {
                    Debug.LogError(
                        "CustomMenuItem: " + method.Name + " second param should be of type params object[]!");
                    isValid = false;
                }
            }

            return isValid;
        }

        public static MonoScript GetNodeViewScript(Type type)
        {
            nodeViewScripts.TryGetValue(type, out MonoScript script);

            return script;
        }

        public static MonoScript GetNodeScript(Type type)
        {
            nodeScripts.TryGetValue(type, out MonoScript script);

            return script;
        }

        public static IEnumerable<Type> GetSlotTypes(Graph graph = null)
        {
            foreach (Type type in genericNodes.slotTypes)
                yield return type;

            if (graph != null && specificNodeDescriptions.TryGetValue(graph, out NodeDescriptions specificNodes))
                foreach (Type type in specificNodes.slotTypes)
                    yield return type;
        }

        public static IEnumerable<PortDescription> GetMacroPortDescriptions()
        {
            foreach (SubGraph macro in GetMacros())
            {
                foreach (PortData ingressPort in macro.IngressPortData)
                    yield return new PortDescription
                    {
                        nodeType = typeof(MacroNode),
                        portType = ingressPort.DisplayType,
                        isInput = true,
                        portFieldName = SubGraphNode.IngressPortsField,
                        portDisplayName = ingressPort.displayName,
                        portIdentifier = ingressPort.Identifier
                    };

                foreach (PortData egressPort in macro.EgressPortData)
                    yield return new PortDescription
                    {
                        nodeType = typeof(MacroNode),
                        portType = egressPort.DisplayType,
                        isInput = false,
                        portFieldName = SubGraphNode.EgressPortsField,
                        portDisplayName = egressPort.displayName,
                        portIdentifier = egressPort.Identifier
                    };
            }
        }

        public static IEnumerable<PortDescription> GetEdgeCreationNodeMenuEntry(PortView portView, Graph graph = null)
        {
            foreach (PortDescription description in genericNodes.nodeCreatePortDescription)
            {
                if (!IsPortCompatible(description))
                    continue;

                yield return description;
            }

            foreach (PortDescription description in GetMacroPortDescriptions())
            {
                if (!IsPortCompatible(description))
                    continue;

                yield return description;
            }

            if (graph != null && specificNodeDescriptions.TryGetValue(graph, out NodeDescriptions specificNodes))
                foreach (PortDescription description in specificNodes.nodeCreatePortDescription)
                {
                    if (!IsPortCompatible(description))
                        continue;
                    yield return description;
                }

            bool IsPortCompatible(PortDescription description)
            {
                if ((portView.direction == Direction.Input && description.isInput) ||
                    (portView.direction == Direction.Output && !description.isInput))
                    return false;

                if (portView.direction == Direction.Input)
                {
                    if (!Graph.TypesAreConnectable(description.portType, portView.portType))
                        return false;
                }
                else
                {
                    if (!Graph.TypesAreConnectable(portView.portType, description.portType))
                        return false;
                }


                return true;
            }
        }
    }
}