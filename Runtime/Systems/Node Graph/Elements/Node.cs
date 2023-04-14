using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Konfus.Systems.Node_Graph.Schema;
using UnityEngine;
using UnityEngine.UIElements;

namespace Konfus.Systems.Node_Graph
{
    [Serializable]
    public abstract class Node
    {
        /// <summary>
        ///     Container of input ports
        /// </summary>
        [NonSerialized] public readonly NodeInputPortContainer inputPorts;

        /// <summary>
        ///     Container of output ports
        /// </summary>
        [NonSerialized] public readonly NodeOutputPortContainer outputPorts;

        [NonSerialized]
        internal Dictionary<Type, NodeDelegates.CustomPortTypeBehaviorDelegateInfo> customPortTypeBehaviorMap = new();

        [NonSerialized] internal Dictionary<string, NodeFieldInformation> nodeFields = new();

        [NonSerialized] protected Graph graph;

        /// <summary>
        ///     Can the node be renamed in the UI. By default a node can be renamed by double clicking it's name.
        /// </summary>
        private NodeRenamePolicy? __renamePolicy;

        //id
        [SerializeField] private PropertyName _guid;

        [NonSerialized] private bool _needsInspector = false;

        // Used in port update algorithm
        private Stack<PortUpdate> fieldsToUpdate = new();

        [NonSerialized] private List<string> messages = new();

        private HashSet<Node> portUpdateHashSet = new();
        private HashSet<PortUpdate> updatedFields = new();

        [SerializeField] internal string nodeCustomName; // The name of the node in case it was renamed by a user

        public int computeOrder = -1;

        //Node view datas
        public Rect position;
        public Rect initialPosition;

        /// <summary>
        ///     Is the node expanded
        /// </summary>
        public bool expanded;

        /// <summary>
        ///     Is debug visible
        /// </summary>
        public bool debug;

        /// <summary>
        ///     Node locked state
        /// </summary>
        public bool nodeLock;

        protected Node()
        {
            inputPorts = new NodeInputPortContainer(this);
            outputPorts = new NodeOutputPortContainer(this);

            InitializeInOutDatas();
        }

        public delegate void ProcessDelegate();

        /// <summary>
        ///     Triggered after an edge was connected on the node
        /// </summary>
        public event Action<SerializableEdge> onAfterEdgeConnected;

        /// <summary>
        ///     Triggered after an edge was disconnected on the node
        /// </summary>
        public event Action<SerializableEdge> onAfterEdgeDisconnected;

        public event Action<string, NodeMessageType> onMessageAdded;
        public event Action<string> onMessageRemoved;

        /// <summary>
        ///     Triggered after a single/list of port(s) is updated, the parameter is the field name
        /// </summary>
        public event Action<string> onPortsUpdated;

        /// <summary>
        ///     Triggered when the node is processes
        /// </summary>
        public event ProcessDelegate onProcessed;

        /// <summary>
        ///     Tell wether or not the node can be processed. Do not check anything from inputs because this step happens
        ///     before inputs are sent to the node
        /// </summary>
        public virtual bool canProcess => true;

        /// <summary>
        ///     The accent color of the node
        /// </summary>
        public virtual Color color => Color.clear;

        /// <summary>
        ///     Is the node created from a duplicate operation (either ctrl-D or copy/paste).
        /// </summary>
        public bool createdFromDuplication { get; internal set; } = false;

        /// <summary>
        ///     True only when the node was created from a duplicate operation and is inside a group that was also duplicated at
        ///     the same time.
        /// </summary>
        public bool createdWithinGroup { get; internal set; } = false;

        /// <summary>True if the node can be deleted, false otherwise</summary>
        public virtual bool deletable => true;

        public PropertyName GUID
        {
            get
            {
                if (PropertyName.IsNullOrEmpty(_guid))
                    _guid = Guid.NewGuid().ToString();

                return _guid;
            }
        }

        public virtual bool HideNodeInspectorBlock => false;

        /// <summary>
        ///     Is the node is locked (if locked it can't be moved)
        /// </summary>
        public virtual bool isLocked => nodeLock;

        /// <summary>
        ///     Set a custom uss file for the node. We use a Resources.Load to get the stylesheet so be sure to put the correct
        ///     resources path
        ///     https://docs.unity3d.com/ScriptReference/Resources.Load.html
        /// </summary>
        public virtual string layoutStyle => string.Empty;

        /// <summary>
        ///     Name of the node, it will be displayed in the title section
        /// </summary>
        /// <returns></returns>
        public virtual string name => GetType().Name;

        /// <summary>
        ///     Does the node needs to be visible in the inspector (when selected).
        /// </summary>
        public virtual bool needsInspector => _needsInspector;

        public NodeRenamePolicy RenamePolicy => __renamePolicy ?? DefaultRenamePolicy;

        /// <summary>Show the node controlContainer only when the mouse is over the node</summary>
        public virtual bool showControlsOnHover => false;

        /// <summary>
        ///     If the node can be locked or not
        /// </summary>
        public virtual bool unlockable => true;

        public ViewDelegates View { get; set; }
        protected virtual NodeRenamePolicy DefaultRenamePolicy => NodeRenamePolicy.DISABLED;

        /// <summary>
        ///     Creates a node of type T at a certain position
        /// </summary>
        /// <param name="position">position in the graph in pixels</param>
        /// <typeparam name="T">type of the node</typeparam>
        /// <returns>the node instance</returns>
        public static T CreateFromType<T>(Vector2 position, params object[] args) where T : Node
        {
            return CreateFromType(typeof(T), position) as T;
        }

        /// <summary>
        ///     Creates a node of type nodeType at a certain position
        /// </summary>
        /// <param name="position">position in the graph in pixels</param>
        /// <typeparam name="nodeType">type of the node</typeparam>
        /// <returns>the node instance</returns>
        public static Node CreateFromType(Type nodeType, Vector2 position, params object[] args)
        {
            if (!nodeType.IsSubclassOf(typeof(Node)))
                return null;

            var node = Activator.CreateInstance(nodeType) as Node;

            node.initialPosition = new Rect(position, new Vector2(100, 100));

            node.View = new ViewDelegates(node);

            ExceptionToLog.Call(() => node.OnNodeCreated());

            return node;
        }

        public virtual void DrawControlsContainer(VisualElement root)
        {
        }

        public virtual FieldInfo[] GetNodeFields()
        {
            return GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public virtual PropertyInfo[] GetNodeProperties()
        {
            return GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        /// <summary>
        ///     Use this function to initialize anything related to ports generation in your node
        ///     This will allow the node creation menu to correctly recognize ports that can be connected between nodes
        /// </summary>
        public virtual void InitializePorts()
        {
            InitializeCustomPortTypeMethods();

            foreach (MemberInfo key in OverrideFieldOrder(nodeFields.Values.Select(k => k.info)))
            {
                NodeFieldInformation nodeField = nodeFields[key.Name];

                if (HasCustomBehavior(nodeField))
                    UpdatePortsForField(nodeField.fieldName, false);
                else
                    // If we don't have a custom behavior on the node, we just have to create a simple port
                    AddPort(
                        nodeField.input,
                        nodeField.info,
                        new PortData
                        {
                            acceptMultipleEdges = nodeField.isMultiple,
                            displayName = nodeField.name,
                            displayType = nodeField.displayType,
                            edgeProcessOrder = nodeField.processOrder ?? EdgeProcessOrder.DefaultEdgeProcessOrder,
                            proxiedFieldPath = nodeField.proxiedFieldPath,
                            tooltip = nodeField.tooltip,
                            vertical = nodeField.vertical,
                            showAsDrawer = nodeField.showAsDrawer
                        }
                    );
            }
        }

        /// <summary>
        ///     Called only when the node is created, not when instantiated
        /// </summary>
        public virtual void OnNodeCreated()
        {
            _guid = Guid.NewGuid().ToString();
        }

        /// <summary>
        ///     Override the field order inside the node. It allows to re-order all the ports and field in the UI.
        /// </summary>
        /// <param name="fields">List of fields to sort</param>
        /// <returns>Sorted list of fields</returns>
        public virtual IEnumerable<MemberInfo> OverrideFieldOrder(IEnumerable<MemberInfo> fields)
        {
            long GetFieldInheritanceLevel(MemberInfo f)
            {
                int level = 0;
                Type t = f.DeclaringType;
                while (t != null)
                {
                    t = t.BaseType;
                    level++;
                }

                return level;
            }

            // Order by MetadataToken and inheritance level to sync the order with the port order (make sure FieldDrawers are next to the correct port)
            return fields.OrderByDescending(f => (GetFieldInheritanceLevel(f) << 32) | (uint) f.MetadataToken);
        }

        /// <summary>
        ///     Add a message on the node
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageType"></param>
        public void AddMessage(string message, NodeMessageType messageType)
        {
            if (messages.Contains(message))
                return;

            onMessageAdded?.Invoke(message, messageType);
            messages.Add(message);
        }

        /// <summary>
        ///     Add a port
        /// </summary>
        /// <param name="input">is input port</param>
        /// <param name="fieldName">C# field name</param>
        /// <param name="portData">Data of the port</param>
        public void AddPort(bool input, string fieldName, PortData portData)
        {
            // Fixup port data info if needed:
            if (portData.DisplayType == null)
            {
                Type displayType = nodeFields[fieldName].info.GetUnderlyingType();
                if (input && portData.acceptMultipleEdges)
                {
                    if (displayType.IsArray) displayType = displayType.GetElementType();
                    else if (displayType.IsGenericType) displayType = displayType.GenericTypeArguments[0];
                }

                portData.DisplayType = displayType;
            }

            if (input)
                inputPorts.Add(new NodePort(this, fieldName, portData));
            else
                outputPorts.Add(new NodePort(this, fieldName, portData));
        }

        /// <summary>
        ///     Add a port
        /// </summary>
        /// <param name="input">is input port</param>
        /// <param name="memberInfo">MemberInfo to attach this port to</param>
        /// <param name="portData">Data of the port</param>
        public void AddPort(bool input, MemberInfo memberInfo, PortData portData)
        {
            // Fixup port data info if needed:
            if (portData.DisplayType == null)
            {
                Type displayType = nodeFields[memberInfo.Name].info.GetUnderlyingType();
                if (input && portData.acceptMultipleEdges)
                {
                    if (displayType.IsArray) displayType = displayType.GetElementType();
                    else if (displayType.IsGenericType) displayType = displayType.GenericTypeArguments[0];
                }

                portData.DisplayType = displayType;
            }

            if (input)
                inputPorts.Add(new NodePort(this, nodeFields[memberInfo.Name], portData));
            else
                outputPorts.Add(new NodePort(this, nodeFields[memberInfo.Name], portData));
        }

        /// <summary>
        ///     Remove all messages on the node
        /// </summary>
        public void ClearMessages()
        {
            foreach (string message in messages)
                onMessageRemoved?.Invoke(message);
            messages.Clear();
        }

        /// <summary>
        ///     Return a node matching the condition in the dependencies of the node
        /// </summary>
        /// <param name="condition">Condition to choose the node</param>
        /// <returns>Matched node or null</returns>
        public Node FindInDependencies(Func<Node, bool> condition)
        {
            var dependencies = new Stack<Node>();

            dependencies.Push(this);

            int depth = 0;
            while (dependencies.Count > 0)
            {
                Node node = dependencies.Pop();

                // Guard for infinite loop (faster than a HashSet based solution)
                depth++;
                if (depth > 2000)
                    break;

                if (condition(node))
                    return node;

                foreach (Node dep in node.GetInputNodes())
                    dependencies.Push(dep);
            }

            return null;
        }

        /// <summary>
        ///     Return all the connected edges of the node
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SerializableEdge> GetAllEdges()
        {
            foreach (NodePort port in GetAllPorts())
            foreach (SerializableEdge edge in port.GetEdges())
                yield return edge;
        }

        /// <summary>
        ///     Return all the ports of the node
        /// </summary>
        /// <returns></returns>
        public IEnumerable<NodePort> GetAllPorts()
        {
            foreach (NodePort port in inputPorts)
                yield return port;
            foreach (NodePort port in outputPorts)
                yield return port;
        }

        /// <summary>
        ///     Get the name of the node. If the node have a custom name (set using the UI by double clicking on the node title)
        ///     then it will return this name first, otherwise it returns the value of the name field.
        /// </summary>
        /// <returns>The name of the node as written in the title</returns>
        public string GetCustomName()
        {
            return string.IsNullOrEmpty(nodeCustomName) ? name : nodeCustomName;
        }

        /// <summary>
        ///     Get all the nodes connected to the input ports of this node
        /// </summary>
        /// <returns>an enumerable of node</returns>
        public IEnumerable<Node> GetInputNodes()
        {
            foreach (NodePort port in inputPorts)
            foreach (SerializableEdge edge in port.GetEdges())
                yield return edge.outputNode;
        }

        /// <summary>
        ///     Get all the nodes connected to the output ports of this node
        /// </summary>
        /// <returns>an enumerable of node</returns>
        public IEnumerable<Node> GetOutputNodes()
        {
            foreach (NodePort port in outputPorts)
            foreach (SerializableEdge edge in port.GetEdges())
                yield return edge.inputNode;
        }

        /// <summary>
        ///     Get the port from field name and identifier
        /// </summary>
        /// <param name="fieldName">C# field name</param>
        /// <param name="identifier">Unique port identifier</param>
        /// <returns></returns>
        public NodePort GetPort(string fieldName, string identifier)
        {
            return inputPorts.Concat(outputPorts).FirstOrDefault(p =>
            {
                bool bothNull = string.IsNullOrEmpty(identifier) && string.IsNullOrEmpty(p.portData.Identifier);
                return p.fieldName == fieldName && (bothNull || identifier == p.portData.Identifier);
            });
        }

        // called by the BaseGraph when the node is added to the graph
        public void Initialize(Graph graph)
        {
            this.graph = graph;

            ExceptionToLog.Call(() => Enable());

            InitializePorts();
        }

        public void InvokeOnProcessed()
        {
            onProcessed?.Invoke();
        }

        /// <summary>
        ///     Is the port an input
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public bool IsFieldInput(string fieldName)
        {
            return nodeFields[fieldName].input;
        }

        public void OnEdgeConnected(SerializableEdge edge)
        {
            bool input = edge.inputNode == this;
            NodePortContainer portCollection = input ? inputPorts : outputPorts;

            portCollection.Add(edge);

            UpdateAllPorts();

            onAfterEdgeConnected?.Invoke(edge);
        }

        public void OnEdgeDisconnected(SerializableEdge edge)
        {
            if (edge == null)
                return;

            bool input = edge.inputNode == this;
            NodePortContainer portCollection = input ? inputPorts : outputPorts;

            portCollection.Remove(edge);

            // Reset default values of input port:
            bool haveConnectedEdges = edge.inputNode.inputPorts.Where(p => p.fieldName == edge.inputFieldName)
                .Any(p => p.GetEdges().Count != 0);
            if (edge.inputNode == this && !haveConnectedEdges && CanResetPort(edge.inputPort))
                edge.inputPort?.ResetToDefault();

            UpdateAllPorts();

            onAfterEdgeDisconnected?.Invoke(edge);
        }

        public void OnProcess()
        {
            ExceptionToLog.Call(() => PreProcess());

            inputPorts.PullDatas();

            ExceptionToLog.Call(() => Process());

            InvokeOnProcessed();

            outputPorts.PushDatas();

            ExceptionToLog.Call(() => PostProcess());
        }

        /// <summary>
        ///     Remove a message on the node
        /// </summary>
        /// <param name="message"></param>
        public void RemoveMessage(string message)
        {
            onMessageRemoved?.Invoke(message);
            messages.Remove(message);
        }

        /// <summary>
        ///     Remove a message that contains
        /// </summary>
        /// <param name="subMessage"></param>
        public void RemoveMessageContains(string subMessage)
        {
            string toRemove = messages.Find(m => m.Contains(subMessage));
            messages.Remove(toRemove);
            onMessageRemoved?.Invoke(toRemove);
        }

        /// <summary>
        ///     Remove a port
        /// </summary>
        /// <param name="input">is input port</param>
        /// <param name="port">the port to delete</param>
        public void RemovePort(bool input, NodePort port)
        {
            if (input)
                inputPorts.Remove(port);
            else
                outputPorts.Remove(port);
        }

        /// <summary>
        ///     Remove port(s) from field name
        /// </summary>
        /// <param name="input">is input</param>
        /// <param name="fieldName">C# field name</param>
        public void RemovePort(bool input, string fieldName)
        {
            if (input)
                inputPorts.RemoveAll(p => p.fieldName == fieldName);
            else
                outputPorts.RemoveAll(p => p.fieldName == fieldName);
        }

        public void RepaintTitle()
        {
            View?.UpdateTitle();
        }

        /// <summary>
        ///     Set the custom name of the node. This is intended to be used by renamable nodes.
        ///     This custom name will be serialized inside the node.
        /// </summary>
        /// <param name="customNodeName">New name of the node.</param>
        public void SetCustomName(string customName)
        {
            nodeCustomName = customName;
            RepaintTitle();
        }

        public void SetRenameMethod(NodeRenamePolicy renameMethod)
        {
            __renamePolicy = renameMethod;
            RepaintTitle();
        }

        /// <summary>
        ///     Update all ports of the node
        /// </summary>
        public bool UpdateAllPorts()
        {
            bool changed = false;

            foreach (MemberInfo key in OverrideFieldOrder(nodeFields.Values.Select(k => k.info)))
            {
                NodeFieldInformation field = nodeFields[key.Name];
                changed |= UpdatePortsForField(field.fieldName);
            }

            return changed;
        }

        /// <summary>
        ///     Update all ports of the node without updating the connected ports. Only use this method when you need to update all
        ///     the nodes ports in your graph.
        /// </summary>
        public bool UpdateAllPortsLocal()
        {
            bool changed = false;

            foreach (MemberInfo key in OverrideFieldOrder(nodeFields.Values.Select(k => k.info)))
            {
                NodeFieldInformation field = nodeFields[key.Name];
                changed |= UpdatePortsForFieldLocal(field.fieldName);
            }

            return changed;
        }

        /// <summary>
        ///     Update the ports related to one C# property field and all connected nodes in the graph
        /// </summary>
        /// <param name="fieldName"></param>
        public bool UpdatePortsForField(string fieldName, bool sendPortUpdatedEvent = true)
        {
            bool changed = false;

            fieldsToUpdate.Clear();
            updatedFields.Clear();

            fieldsToUpdate.Push(new PortUpdate {fieldNames = new List<string> {fieldName}, node = this});

            // Iterate through all the ports that needs to be updated, following graph connection when the 
            // port is updated. This is required ton have type propagation multiple nodes that changes port types
            // are connected to each other (i.e. the relay node)
            while (fieldsToUpdate.Count != 0)
            {
                (List<string> fields, Node node) = fieldsToUpdate.Pop();

                // Avoid updating twice a port
                if (updatedFields.Any(t => t.node == node && fields.SequenceEqual(t.fieldNames))) continue;
                updatedFields.Add(new PortUpdate {fieldNames = fields, node = node});

                foreach (string field in fields)
                {
                    if (!node.UpdatePortsForFieldLocal(field, sendPortUpdatedEvent)) continue;
                    
                    NodePortContainer ports = node.IsFieldInput(field) ? node.inputPorts : node.outputPorts;
                    foreach (NodePort port in ports)
                    {
                        if (port.fieldName != field) continue;

                        foreach (SerializableEdge edge in port.GetEdges())
                        {
                            Node edgeNode = node.IsFieldInput(field) ? edge.outputNode : edge.inputNode;
                            List<string> fieldsWithBehavior = edgeNode.nodeFields.Values
                                .Where(f => HasCustomBehavior(f)).Select(f => f.fieldName).ToList();
                            fieldsToUpdate.Push(new PortUpdate {fieldNames = fieldsWithBehavior, node = edgeNode});
                        }
                    }

                    changed = true;

                }
            }

            return changed;
        }


        /// <summary>
        ///     Update the ports related to one C# property field (only for this node)
        /// </summary>
        /// <param name="fieldName"></param>
        public bool UpdatePortsForFieldLocal(string fieldName, bool sendPortUpdatedEvent = true)
        {
            bool changed = false;

            if (!nodeFields.ContainsKey(fieldName))
                return false;

            NodeFieldInformation fieldInfo = nodeFields[fieldName];

            if (!HasCustomBehavior(fieldInfo))
                return false;

            var finalPorts = new List<string>();

            NodePortContainer portCollection = fieldInfo.input ? inputPorts : outputPorts;

            // Gather all ports for this field (before to modify them)
            IEnumerable<NodePort> nodePorts = portCollection.Where(p => p.fieldName == fieldName);
            // Gather all edges connected to these ports:
            List<SerializableEdge> edges = nodePorts.SelectMany(n => n.GetEdges()).ToList();

            if (fieldInfo.behavior != null)
            {
                foreach (PortData portData in fieldInfo.behavior.Delegate(edges))
                    if (portData != null)
                        AddPortData(!fieldInfo.behavior.CloneResults ? portData : portData.Clone() as PortData);
            }
            else
            {
                NodeDelegates.CustomPortTypeBehaviorDelegateInfo customPortTypeBehavior =
                    customPortTypeBehaviorMap[fieldInfo.info.GetUnderlyingType()];

                foreach (PortData portData in customPortTypeBehavior.Delegate(fieldName, fieldInfo.name,
                             fieldInfo.info.GetValue(this)))
                    AddPortData(!customPortTypeBehavior.CloneResults ? portData : portData.Clone() as PortData);
            }

            void AddPortData(PortData portData)
            {
                NodePort port = nodePorts.FirstOrDefault(n => n.portData.Identifier == portData.Identifier);
                // Guard using the port identifier so we don't duplicate identifiers
                if (port == null)
                {
                    AddPort(fieldInfo.input, fieldInfo.info, portData);
                    changed = true;
                }
                else
                {
                    // in case the port type have changed for an incompatible type, we disconnect all the edges attached to this port
                    if (!Graph.TypesAreConnectable(port.portData.DisplayType, portData.DisplayType))
                        foreach (SerializableEdge edge in port.GetEdges().ToList())
                            graph.Disconnect(edge.GUID);

                    // patch the port data
                    if (!port.portData.Equals(portData))
                    {
                        port.portData.CopyFrom(portData);
                        changed = true;
                    }
                }

                finalPorts.Add(portData.Identifier);
            }

            // TODO
            // Remove only the ports that are no more in the list
            if (nodePorts != null)
            {
                List<NodePort> currentPortsCopy = nodePorts.ToList();
                foreach (NodePort currentPort in currentPortsCopy)
                    // If the current port does not appear in the list of final ports, we remove it
                    if (!finalPorts.Any(id => id == currentPort.portData.Identifier))
                    {
                        RemovePort(fieldInfo.input, currentPort);
                        changed = true;
                    }
            }

            // Make sure the port order is correct:
            portCollection.Sort((p1, p2) =>
            {
                int p1Index = finalPorts.FindIndex(id => p1.portData.Identifier == id);
                int p2Index = finalPorts.FindIndex(id => p2.portData.Identifier == id);

                if (p1Index == -1 || p2Index == -1)
                    return 0;

                return p1Index.CompareTo(p2Index);
            });

            if (sendPortUpdatedEvent && changed)
                onPortsUpdated?.Invoke(fieldName);

            return changed;
        }

        internal void DestroyInternal()
        {
            ExceptionToLog.Call(() => Destroy());
        }

        internal void DisableInternal()
        {
            // port containers are initialized in the OnEnable
            inputPorts.Clear();
            outputPorts.Clear();

            ExceptionToLog.Call(() => Disable());
        }

        protected virtual bool CanResetPort(NodePort port)
        {
            return true;
        }

        /// <summary>
        ///     Called when the node is removed
        /// </summary>
        protected virtual void Destroy()
        {
        }

        /// <summary>
        ///     Called when the node is disabled
        /// </summary>
        protected virtual void Disable()
        {
        }

        /// <summary>
        ///     Called when the node is enabled
        /// </summary>
        protected virtual void Enable()
        {
        }

        /// <summary>
        ///     Called after outputs are pushed
        /// </summary>
        protected virtual void PostProcess()
        {
        }

        /// <summary>
        ///     Prepare node before inputs are pulled
        /// </summary>
        protected virtual void PreProcess()
        {
        }

        /// <summary>
        ///     Override this method to implement custom processing
        /// </summary>
        protected virtual void Process()
        {
        }

        private bool HasCustomBehavior(NodeFieldInformation info)
        {
            if (info.behavior != null)
                return true;

            if (customPortTypeBehaviorMap.ContainsKey(info.info.GetUnderlyingType()))
                return true;

            return false;
        }

        private void InitializeCustomPortTypeMethods()
        {
            var methods = new MethodInfo[0];
            Type baseType = GetType();
            while (true)
            {
                methods = baseType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (MethodInfo method in methods)
                {
                    CustomPortTypeBehavior[] typeBehaviors =
                        method.GetCustomAttributes<CustomPortTypeBehavior>().ToArray();

                    if (typeBehaviors.Length == 0)
                        continue;

                    NodeDelegates.CustomPortTypeBehaviorDelegate deleg = null;
                    try
                    {
                        deleg = Delegate.CreateDelegate(typeof(NodeDelegates.CustomPortTypeBehaviorDelegate), this,
                            method) as NodeDelegates.CustomPortTypeBehaviorDelegate;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                        Debug.LogError(
                            $"Cannot convert method {method} to a delegate of type {typeof(NodeDelegates.CustomPortTypeBehaviorDelegate)}");
                    }

                    foreach (CustomPortTypeBehavior typeBehavior in typeBehaviors)
                        customPortTypeBehaviorMap[typeBehavior.type] =
                            new NodeDelegates.CustomPortTypeBehaviorDelegateInfo(deleg, typeBehavior.cloneResults);
                }

                // Try to also find private methods in the base class
                baseType = baseType.BaseType;
                if (baseType == null)
                    break;
            }
        }

        private void InitializeInOutDatas()
        {
            foreach (NodeFieldInformation nodeFieldInformation in PortGeneration.GetAllPortInformation(this))
                nodeFields[nodeFieldInformation.fieldName] = nodeFieldInformation;
        }

        private struct PortUpdate
        {
            public List<string> fieldNames;
            public Node node;

            public void Deconstruct(out List<string> fieldNames, out Node node)
            {
                fieldNames = this.fieldNames;
                node = this.node;
            }
        }


        public class NodeFieldInformation
        {
            public string name;
            public string fieldName;
            public UnityPath proxiedFieldPath;
            public object memberOwner;
            public MemberInfo info;
            public bool input;
            public bool isMultiple;
            public EdgeProcessOrderKey processOrder;
            public Type displayType;
            public string tooltip;
            public bool showAsDrawer;
            public NodeDelegates.CustomPortBehaviorDelegateInfo behavior;
            public bool vertical;

            public NodeFieldInformation(object memberOwner, MemberInfo info,
                NodeDelegates.CustomPortBehaviorDelegateInfo behavior, UnityPath proxiedFieldPath = null)
            {
                var inputAttribute = info.GetCustomAttribute<InputAttribute>();
                var outputAttribute = info.GetCustomAttribute<OutputAttribute>();
                var tooltipAttribute = info.GetCustomAttribute<TooltipAttribute>();

                string name = info.Name;
                if (!string.IsNullOrEmpty(inputAttribute?.name))
                    name = inputAttribute.name;
                if (!string.IsNullOrEmpty(outputAttribute?.name))
                    name = outputAttribute.name;

                this.memberOwner = memberOwner;
                input = inputAttribute != null;
                isMultiple = inputAttribute != null
                    ? inputAttribute.AcceptsMultipleEdges
                    : outputAttribute.allowMultiple;
                this.info = info;
                this.name = name;
                fieldName = info.Name;
                this.proxiedFieldPath = proxiedFieldPath;
                displayType = inputAttribute?.displayType;
                processOrder = (inputAttribute as MultiEdgeInputAttribute)?.processOrder ??
                               EdgeProcessOrder.DefaultEdgeProcessOrder;
                this.behavior = behavior;
                tooltip = tooltipAttribute?.tooltip;
                showAsDrawer = input && inputAttribute.showAsDrawer;
                vertical = info.HasCustomAttribute<VerticalAttribute>();
            }
        }
    }
}