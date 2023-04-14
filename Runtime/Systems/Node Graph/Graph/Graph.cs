using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Konfus.Systems.Node_Graph
{
    public class GraphChanges
    {
        public SerializableEdge removedEdge;
        public SerializableEdge addedEdge;
        public Node removedNode;
        public Node addedNode;
        public Node nodeChanged;
        public Group addedGroups;
        public Group removedGroups;
        public StackNode addedStackNode;
        public StackNode removedStackNode;
        public StickyNote addedStickyNotes;
        public StickyNote removedStickyNotes;
    }

    /// <summary>
    ///     Compute order type used to determine the compute order integer on the nodes
    /// </summary>
    public enum ComputeOrderType
    {
        DepthFirst,
        BreadthFirst
    }

    [Serializable]
    public class Graph : ScriptableObject, ISerializationCallbackReceiver
    {
        /// <summary>Invalid compute order number of a node can't process</summary>
        public static readonly int invalidComputeOrder = -1;

        /// <summary>Invalid compute order number of a node when it's inside a loop</summary>
        public static readonly int loopComputeOrder = -2;

        private static readonly int maxComputeOrderDepth = 1000;

        /// <summary>
        ///     Dictionary of edges per GUID, faster than a search in a list
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="SerializableEdge"></typeparam>
        /// <returns></returns>
        [NonSerialized] public Dictionary<PropertyName, SerializableEdge> edgesPerGUID = new();

        /// <summary>
        ///     Dictionary to access node per GUID, faster than a search in a list
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <typeparam name="BaseNode"></typeparam>
        /// <returns></returns>
        [NonSerialized] public Dictionary<PropertyName, Node> nodesPerGUID = new();

        [NonSerialized] private bool _isEnabled;

        [NonSerialized] private Dictionary<Node, int> computeOrderDictionary = new();

        private HashSet<Node> infiniteLoopTracker = new();

        [NonSerialized] private Scene linkedScene;

        /// <summary>
        ///     Json list of serialized nodes only used for copy pasting in the editor. Note that this field isn't serialized
        /// </summary>
        /// <typeparam name="JsonElement"></typeparam>
        /// <returns></returns>
        [SerializeField] [Obsolete("Use BaseGraph.nodes instead")]
        public List<JsonElement> serializedNodes = new();

        /// <summary>
        ///     List of all the nodes in the graph.
        /// </summary>
        /// <typeparam name="BaseNode"></typeparam>
        /// <returns></returns>
        [SerializeReference] public List<Node> nodes = new();

        /// <summary>
        ///     Json list of edges
        /// </summary>
        /// <typeparam name="SerializableEdge"></typeparam>
        /// <returns></returns>
        [SerializeField] public List<SerializableEdge> edges = new();

        /// <summary>
        ///     All groups in the graph
        /// </summary>
        /// <typeparam name="Group"></typeparam>
        /// <returns></returns>
        [SerializeField] [FormerlySerializedAs("commentBlocks")]
        public List<Group> groups = new();

        /// <summary>
        ///     All Stack Nodes in the graph
        /// </summary>
        /// <typeparam name="stackNodes"></typeparam>
        /// <returns></returns>
        [SerializeField] [SerializeReference] // Polymorphic serialization
        public List<StackNode> stackNodes = new();

        /// <summary>
        ///     All pinned elements in the graph
        /// </summary>
        /// <typeparam name="PinnedElement"></typeparam>
        /// <returns></returns>
        [SerializeField] public List<PinnedElement> pinnedElements = new();

        /// <summary>
        ///     All exposed parameters in the graph
        /// </summary>
        /// <typeparam name="ExposedParameter"></typeparam>
        /// <returns></returns>
        [SerializeField] [SerializeReference] public List<ExposedParameter> exposedParameters = new();

        [SerializeField] [FormerlySerializedAs("exposedParameters")] // We keep this for upgrade
        private List<ExposedParameter> serializedParameterList = new();

        [SerializeField] public List<StickyNote> stickyNotes = new();

        // Trick to keep the node inspector alive during the editor session
        [SerializeField] internal Object nodeInspectorReference;

        //graph visual properties
        public Vector3 position = Vector3.zero;
        public Vector3 scale = Vector3.one;

        /// <summary>
        ///     Triggered when the graph is enabled
        /// </summary>
        public event Action onEnabled;

        /// <summary>
        ///     Triggered when something is changed in the list of exposed parameters
        /// </summary>
        public event Action onExposedParameterListChanged;

        public event Action<ExposedParameter> onExposedParameterModified;
        public event Action<ExposedParameter> onExposedParameterValueChanged;

        /// <summary>
        ///     Triggered when the graph is changed
        /// </summary>
        public event Action<GraphChanges> onGraphChanges;

        /// <summary>
        ///     Triggered when the graph is linked to an active scene.
        /// </summary>
        public event Action<Scene> onSceneLinked;

        public HashSet<Node> graphOutputs { get; private set; } = new();

        public bool isEnabled
        {
            get => _isEnabled;
            private set => _isEnabled = value;
        }

        public void OnAfterDeserialize()
        {
        }

        public void OnBeforeSerialize()
        {
            // Cleanup broken elements
            stackNodes.RemoveAll(s => s == null);
            nodes.RemoveAll(n => n == null);
        }

        /// <summary>
        ///     Tell if two types can be connected in the context of a graph
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static bool
            TypesAreConnectable(Type from, Type to) // NOTE: Extend this later for adding conversion nodes
        {
            if (from == null || to == null)
                return false;

            if (TypeAdapter.AreIncompatible(from, to))
                return false;

            //Check if there is custom adapters for this assignation
            if (CustomPortIO.IsAssignable(from, to))
                return true;

            //Check for type assignability
            if (to.IsReallyAssignableFrom(from))
                return true;

            // User defined type conversions
            if (TypeAdapter.AreAssignable(from, to))
                return true;

            if (ConversionNodeAdapter.AreAssignable(from, to))
                return true;

            return false;
        }

#if UNITY_EDITOR
        public virtual void CreateInspectorGUI(VisualElement root)
        {
        }
#endif

        public virtual void Initialize()
        {
        }

        public virtual void OnAssetDeleted()
        {
        }

        /// <summary>
        ///     Add an exposed parameter
        /// </summary>
        /// <param name="name">parameter name</param>
        /// <param name="type">parameter type (must be a subclass of ExposedParameter)</param>
        /// <param name="value">default value</param>
        /// <returns>The unique id of the parameter</returns>
        public string AddExposedParameter(string name, Type type, object value = null)
        {
            if (!type.IsSubclassOf(typeof(ExposedParameter)))
                Debug.LogError($"Can't add parameter of type {type}, the type doesn't inherit from ExposedParameter.");

            var param = Activator.CreateInstance(type) as ExposedParameter;

            // patch value with correct type:
            if (param.GetValueType().IsValueType)
                value = Activator.CreateInstance(param.GetValueType());

            param.Initialize(name, value);
            exposedParameters.Add(param);

            onExposedParameterListChanged?.Invoke();

            return param.guid;
        }

        /// <summary>
        ///     Add an already allocated / initialized parameter to the graph
        /// </summary>
        /// <param name="parameter">The parameter to add</param>
        /// <returns>The unique id of the parameter</returns>
        public string AddExposedParameter(ExposedParameter parameter)
        {
            string guid = Guid.NewGuid().ToString(); // Generated once and unique per parameter

            parameter.guid = guid;
            exposedParameters.Add(parameter);

            onExposedParameterListChanged?.Invoke();

            return guid;
        }

        /// <summary>
        ///     Add a group
        /// </summary>
        /// <param name="block"></param>
        public void AddGroup(Group block)
        {
            groups.Add(block);
            onGraphChanges?.Invoke(new GraphChanges {addedGroups = block});
        }

        /// <summary>
        ///     Adds a node to the graph
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public Node AddNode(Node node)
        {
            nodesPerGUID[node.GUID] = node;

            nodes.Add(node);
            node.Initialize(this);

            onGraphChanges?.Invoke(new GraphChanges {addedNode = node});

            return node;
        }

        /// <summary>
        ///     Add a StackNode
        /// </summary>
        /// <param name="stackNode"></param>
        public void AddStackNode(StackNode stackNode)
        {
            stackNodes.Add(stackNode);
            onGraphChanges?.Invoke(new GraphChanges {addedStackNode = stackNode});
        }

        /// <summary>
        ///     Add a sticky note
        /// </summary>
        /// <param name="note"></param>
        public void AddStickyNote(StickyNote note)
        {
            stickyNotes.Add(note);
            onGraphChanges?.Invoke(new GraphChanges {addedStickyNotes = note});
        }

        /// <summary>
        ///     Closes a pinned element of type viewType
        /// </summary>
        /// <param name="viewType">type of the pinned element</param>
        public void ClosePinned(Type viewType)
        {
            PinnedElement pinned = pinnedElements.Find(p => p.editorType.type == viewType);

            pinned.opened = false;
        }

        /// <summary>
        ///     Connect two ports with an edge
        /// </summary>
        /// <param name="inputPort">input port</param>
        /// <param name="outputPort">output port</param>
        /// <param name="DisconnectInputs">is the edge allowed to disconnect another edge</param>
        /// <returns>the connecting edge</returns>
        public SerializableEdge Connect(NodePort inputPort, NodePort outputPort, bool autoDisconnectInputs = true)
        {
            var edge = SerializableEdge.CreateNewEdge(this, inputPort, outputPort);

            //If the input port does not support multi-connection, we remove them
            if (autoDisconnectInputs && !inputPort.portData.acceptMultipleEdges)
                foreach (SerializableEdge e in inputPort.GetEdges().ToList())
                    // TODO: do not disconnect them if the connected port is the same than the old connected
                    Disconnect(e);
            // same for the output port:
            if (autoDisconnectInputs && !outputPort.portData.acceptMultipleEdges)
                foreach (SerializableEdge e in outputPort.GetEdges().ToList())
                    // TODO: do not disconnect them if the connected port is the same than the old connected
                    Disconnect(e);

            edges.Add(edge);

            // Add the edge to the list of connected edges in the nodes
            inputPort.owner.OnEdgeConnected(edge);
            outputPort.owner.OnEdgeConnected(edge);

            onGraphChanges?.Invoke(new GraphChanges {addedEdge = edge});

            return edge;
        }

        // We can deserialize data here because it's called in a unity context
        // so we can load objects references
        public void Deserialize()
        {
            // Disable nodes correctly before removing them:
            if (nodes != null)
                foreach (Node node in nodes)
                    node.DisableInternal();

            MigrateGraphIfNeeded();

            InitializeGraphElements();
        }

        /// <summary>
        ///     Disconnect two ports
        /// </summary>
        /// <param name="inputNode">input node</param>
        /// <param name="inputFieldName">input field name</param>
        /// <param name="outputNode">output node</param>
        /// <param name="outputFieldName">output field name</param>
        public void Disconnect(Node inputNode, string inputFieldName, Node outputNode, string outputFieldName)
        {
            edges.RemoveAll(r =>
            {
                bool remove = r.inputNode == inputNode
                              && r.outputNode == outputNode
                              && r.outputFieldName == outputFieldName
                              && r.inputFieldName == inputFieldName;

                if (remove)
                {
                    r.inputNode?.OnEdgeDisconnected(r);
                    r.outputNode?.OnEdgeDisconnected(r);
                    onGraphChanges?.Invoke(new GraphChanges {removedEdge = r});
                }

                return remove;
            });
        }

        /// <summary>
        ///     Disconnect an edge
        /// </summary>
        /// <param name="edge"></param>
        public void Disconnect(SerializableEdge edge)
        {
            Disconnect(edge.GUID);
        }

        /// <summary>
        ///     Disconnect an edge
        /// </summary>
        /// <param name="edgeGUID"></param>
        public void Disconnect(PropertyName edgeGUID)
        {
            List<(Node, SerializableEdge)> disconnectEvents = new();

            edges.RemoveAll(r =>
            {
                if (r.GUID == edgeGUID)
                {
                    disconnectEvents.Add((r.inputNode, r));
                    disconnectEvents.Add((r.outputNode, r));
                    onGraphChanges?.Invoke(new GraphChanges {removedEdge = r});
                }

                return r.GUID == edgeGUID;
            });

            // Delay the edge disconnect event to avoid recursion
            foreach ((Node node, SerializableEdge edge) in disconnectEvents)
                node?.OnEdgeDisconnected(edge);
        }

        /// <summary>
        ///     Get the exposed parameter from name
        /// </summary>
        /// <param name="name">name</param>
        /// <returns>the parameter or null</returns>
        public ExposedParameter GetExposedParameter(string name)
        {
            return exposedParameters.FirstOrDefault(e => e.name == name);
        }

        /// <summary>
        ///     Get exposed parameter from GUID
        /// </summary>
        /// <param name="guid">GUID of the parameter</param>
        /// <returns>The parameter</returns>
        public ExposedParameter GetExposedParameterFromGUID(string guid)
        {
            return exposedParameters.FirstOrDefault(e => e?.guid == guid);
        }

        /// <summary>
        ///     Get the linked scene. If there is no linked scene, it returns an invalid scene
        /// </summary>
        public Scene GetLinkedScene()
        {
            return linkedScene;
        }

        /// <summary>
        ///     Get the parameter value
        /// </summary>
        /// <param name="name">parameter name</param>
        /// <returns>value</returns>
        public object GetParameterValue(string name)
        {
            return exposedParameters.FirstOrDefault(p => p.name == name)?.value;
        }

        /// <summary>
        ///     Get the parameter value template
        /// </summary>
        /// <param name="name">parameter name</param>
        /// <typeparam name="T">type of the parameter</typeparam>
        /// <returns>value</returns>
        public T GetParameterValue<T>(string name)
        {
            return (T) GetParameterValue(name);
        }

        /// <summary>
        ///     Return true when the graph is linked to a scene, false otherwise.
        /// </summary>
        public bool IsLinkedToScene()
        {
            return linkedScene.IsValid();
        }

        /// <summary>
        ///     Link the current graph to the scene in parameter, allowing the graph to pick and serialize objects from the scene.
        /// </summary>
        /// <param name="scene">Target scene to link</param>
        public void LinkToScene(Scene scene)
        {
            linkedScene = scene;
            onSceneLinked?.Invoke(scene);
        }

        public void MigrateGraphIfNeeded()
        {
#pragma warning disable CS0618
            // Migration step from JSON serialized nodes to [SerializeReference]
            if (serializedNodes.Count > 0)
            {
                nodes.Clear();
                foreach (JsonElement serializedNode in serializedNodes.ToList())
                {
                    Node node = JsonSerializer.DeserializeNode(serializedNode);
                    if (node != null)
                        nodes.Add(node);
                }

                serializedNodes.Clear();

                // we also migrate parameters here:
                List<ExposedParameter> paramsToMigrate = serializedParameterList.ToList();
                exposedParameters.Clear();
                foreach (ExposedParameter param in paramsToMigrate)
                {
                    if (param == null)
                        continue;

                    ExposedParameter newParam = param.Migrate();

                    if (newParam == null)
                    {
                        Debug.LogError(
                            $"Can't migrate parameter of type {param.type}, please create an Exposed Parameter class that implements this type.");
                        continue;
                    }

                    exposedParameters.Add(newParam);
                }
            }
#pragma warning restore CS0618
        }

        /// <summary>
        ///     Update parameter visibility
        /// </summary>
        /// <param name="parameter">The parameter</param>
        /// <param name="isHidden">is Hidden</param>
        public void NotifyExposedParameterChanged(ExposedParameter parameter)
        {
            onExposedParameterModified?.Invoke(parameter);
        }

        public void NotifyExposedParameterValueChanged(ExposedParameter parameter)
        {
            onExposedParameterValueChanged?.Invoke(parameter);
        }

        /// <summary>
        ///     Invoke the onGraphChanges event, can be used as trigger to execute the graph when the content of a node is changed
        /// </summary>
        /// <param name="node"></param>
        public void NotifyNodeChanged(Node node)
        {
            onGraphChanges?.Invoke(new GraphChanges {nodeChanged = node});
        }

        /// <summary>
        ///     Open a pinned element of type viewType
        /// </summary>
        /// <param name="viewType">type of the pinned element</param>
        /// <returns>the pinned element</returns>
        public PinnedElement OpenPinned(Type viewType)
        {
            PinnedElement pinned = pinnedElements.Find(p => p.editorType.type == viewType);

            if (pinned == null)
            {
                pinned = new PinnedElement(viewType);
                pinnedElements.Add(pinned);
            }
            else
            {
                pinned.opened = true;
            }

            return pinned;
        }

        /// <summary>
        ///     Remove an exposed parameter
        /// </summary>
        /// <param name="ep">the parameter to remove</param>
        public void RemoveExposedParameter(ExposedParameter ep)
        {
            exposedParameters.Remove(ep);

            onExposedParameterListChanged?.Invoke();
        }

        /// <summary>
        ///     Remove an exposed parameter
        /// </summary>
        /// <param name="guid">GUID of the parameter</param>
        public void RemoveExposedParameter(string guid)
        {
            if (exposedParameters.RemoveAll(e => e.guid == guid) != 0)
                onExposedParameterListChanged?.Invoke();
        }

        /// <summary>
        ///     Removes a group
        /// </summary>
        /// <param name="block"></param>
        public void RemoveGroup(Group block)
        {
            groups.Remove(block);
            onGraphChanges?.Invoke(new GraphChanges {removedGroups = block});
        }

        /// <summary>
        ///     Removes a node from the graph
        /// </summary>
        /// <param name="node"></param>
        public void RemoveNode(Node node)
        {
            node.DisableInternal();
            node.DestroyInternal();

            nodesPerGUID.Remove(node.GUID);

            nodes.Remove(node);

            onGraphChanges?.Invoke(new GraphChanges {removedNode = node});
        }

        /// <summary>
        ///     Remove a StackNode
        /// </summary>
        /// <param name="stackNode"></param>
        public void RemoveStackNode(StackNode stackNode)
        {
            stackNodes.Remove(stackNode);
            onGraphChanges?.Invoke(new GraphChanges {removedStackNode = stackNode});
        }

        /// <summary>
        ///     Removes a sticky note
        /// </summary>
        /// <param name="note"></param>
        public void RemoveStickyNote(StickyNote note)
        {
            stickyNotes.Remove(note);
            onGraphChanges?.Invoke(new GraphChanges {removedStickyNotes = note});
        }

        /// <summary>
        ///     Set parameter value from name. (Warning: the parameter name can be changed by the user)
        /// </summary>
        /// <param name="name">name of the parameter</param>
        /// <param name="value">new value</param>
        /// <returns>true if the value have been assigned</returns>
        public bool SetParameterValue(string name, object value)
        {
            ExposedParameter e = exposedParameters.FirstOrDefault(p => p.name == name);

            if (e == null)
                return false;

            e.value = value;

            return true;
        }

        /// <summary>
        ///     Update the compute order of the nodes in the graph
        /// </summary>
        /// <param name="type">Compute order type</param>
        public void UpdateComputeOrder(ComputeOrderType type = ComputeOrderType.DepthFirst)
        {
            if (nodes.Count == 0)
                return;

            // Find graph outputs (end nodes) and reset compute order
            graphOutputs.Clear();
            foreach (Node node in nodes)
            {
                if (node.GetOutputNodes().Count() == 0)
                    graphOutputs.Add(node);
                node.computeOrder = 0;
            }

            computeOrderDictionary.Clear();
            infiniteLoopTracker.Clear();

            switch (type)
            {
                default:
                case ComputeOrderType.DepthFirst:
                    UpdateComputeOrderDepthFirst();
                    break;
                case ComputeOrderType.BreadthFirst:
                    foreach (Node node in nodes)
                        UpdateComputeOrderBreadthFirst(0, node);
                    break;
            }
        }

        /// <summary>
        ///     Update an exposed parameter value
        /// </summary>
        /// <param name="guid">GUID of the parameter</param>
        /// <param name="value">new value</param>
        public void UpdateExposedParameter(string guid, object value)
        {
            ExposedParameter param = exposedParameters.Find(e => e.guid == guid);
            if (param == null)
                return;

            if (value != null && !param.GetValueType().IsAssignableFrom(value.GetType()))
                throw new Exception("Type mismatch when updating parameter " + param.name + ": from " +
                                    param.GetValueType() + " to " + value.GetType().AssemblyQualifiedName);

            param.value = value;
            onExposedParameterModified?.Invoke(param);
        }

        /// <summary>
        ///     Update the exposed parameter name
        /// </summary>
        /// <param name="parameter">The parameter</param>
        /// <param name="name">new name</param>
        public void UpdateExposedParameterName(ExposedParameter parameter, string name)
        {
            parameter.name = name;
            onExposedParameterModified?.Invoke(parameter);
        }

        internal void NotifyExposedParameterListChanged()
        {
            onExposedParameterListChanged?.Invoke();
        }

        protected virtual void OnDisable()
        {
            isEnabled = false;
            foreach (Node node in nodes)
                node.DisableInternal();
        }

        protected virtual void OnEnable()
        {
            if (isEnabled)
                OnDisable();

            MigrateGraphIfNeeded();
            InitializeGraphElements();
            DestroyBrokenGraphElements();
            UpdateComputeOrder();
            isEnabled = true;
            onEnabled?.Invoke();
        }

        protected void DestroyBrokenGraphElements()
        {
            List<SerializableEdge> brokenEdges = edges.FindAll(e => e.inputNode == null
                                                                    || e.outputNode == null
                                                                    || !e.inputNode.GetAllEdges().Contains(e)
                                                                    || !e.outputNode.GetAllEdges().Contains(e)
                                                                    || string.IsNullOrEmpty(e.outputFieldName)
                                                                    || string.IsNullOrEmpty(e.inputFieldName));

            brokenEdges.ForEach(e => Disconnect(e.GUID));
            edges.RemoveAll(e => brokenEdges.Contains(e));

            nodes.RemoveAll(n => n == null);

            UpdateComputeOrder();
        }

        private void InitializeGraphElements()
        {
            // Sanitize the element lists (it's possible that nodes are null if their full class name have changed)
            // If you rename / change the assembly of a node or parameter, please use the MovedFrom() attribute to avoid breaking the graph.
            nodes.RemoveAll(n => n == null);
            exposedParameters.RemoveAll(e => e == null);

            foreach (Node node in nodes.ToList())
            {
                nodesPerGUID[node.GUID] = node;
                node.Initialize(this);
            }

            foreach (SerializableEdge edge in edges.ToList())
            {
                edge.Deserialize();
                edgesPerGUID[edge.GUID] = edge;

                // Sanity check for the edge:
                if (edge.inputPort == null || edge.outputPort == null)
                {
                    Disconnect(edge.GUID);
                    continue;
                }

                // Add the edge to the non-serialized port data
                edge.inputPort.owner.OnEdgeConnected(edge);
                edge.outputPort.owner.OnEdgeConnected(edge);
            }
        }

        private void PropagateComputeOrder(Node node, int computeOrder)
        {
            var deps = new Stack<Node>();
            var loop = new HashSet<Node>();

            deps.Push(node);
            while (deps.Count > 0)
            {
                Node n = deps.Pop();
                n.computeOrder = computeOrder;

                if (!loop.Add(n))
                    continue;

                foreach (Node dep in n.GetOutputNodes())
                    deps.Push(dep);
            }
        }

        private int UpdateComputeOrderBreadthFirst(int depth, Node node)
        {
            int computeOrder = 0;

            if (depth > maxComputeOrderDepth)
            {
                Debug.LogError("Recursion error while updating compute order");
                return -1;
            }

            if (computeOrderDictionary.ContainsKey(node))
                return node.computeOrder;

            if (!infiniteLoopTracker.Add(node))
                return -1;

            if (!node.canProcess)
            {
                node.computeOrder = -1;
                computeOrderDictionary[node] = -1;
                return -1;
            }

            foreach (Node dep in node.GetInputNodes())
            {
                int c = UpdateComputeOrderBreadthFirst(depth + 1, dep);

                if (c == -1)
                {
                    computeOrder = -1;
                    break;
                }

                computeOrder += c;
            }

            if (computeOrder != -1)
                computeOrder++;

            node.computeOrder = computeOrder;
            computeOrderDictionary[node] = computeOrder;

            return computeOrder;
        }

        private void UpdateComputeOrderDepthFirst()
        {
            var dfs = new Stack<Node>();

            GraphUtils.FindCyclesInGraph(this, n => { PropagateComputeOrder(n, loopComputeOrder); });

            int computeOrder = 0;
            foreach (Node node in GraphUtils.DepthFirstSort(this))
            {
                if (node.computeOrder == loopComputeOrder)
                    continue;
                if (!node.canProcess)
                    node.computeOrder = -1;
                else
                    node.computeOrder = computeOrder++;
            }
        }
    }
}