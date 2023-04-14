using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Konfus.Systems.Node_Graph;
using Konfus.Systems.Node_Graph.Schema;
using Konfus.Tools.NodeGraphEditor.Utils;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Node = UnityEditor.Experimental.GraphView.Node;
using NodeView = UnityEditor.Experimental.GraphView.Node;
using Object = UnityEngine.Object;

namespace Konfus.Tools.NodeGraphEditor
{
    [NodeCustomEditor(typeof(Systems.Node_Graph.Node))]
    public class NodeView : Node
    {
        public Systems.Node_Graph.Node nodeTarget;

        public List<PortView> inputPortViews = new();
        public List<PortView> outputPortViews = new();

        public GraphView owner { private set; get; }

        protected Dictionary<MemberInfo, List<PortView>> portsByMemberInfo = new();

        public VisualElement controlsContainer;
        protected VisualElement debugContainer;
        protected VisualElement rightTitleContainer;
        protected VisualElement topPortContainer;
        protected VisualElement bottomPortContainer;
        private VisualElement inputContainerElement;

        private VisualElement settings;
        private NodeSettingsView settingsContainer;
        private Button settingButton;
        private TextField titleTextField;
        private Image renameIcon;

        private readonly Label computeOrderLabel = new();

        public event Action<PortView> onPortConnected;
        public event Action<PortView> onPortDisconnected;

        protected virtual bool hasSettings { get; set; }

        public bool initializing; //Used for applying SetPosition on locked node at init.

        private readonly string baseNodeStyle = "GraphProcessorStyles/BaseNodeView";

        private bool settingsExpanded;

        [NonSerialized] private readonly List<IconBadge> badges = new();

        private List<Node> selectedNodes = new();
        private float selectedNodesFarLeft;
        private float selectedNodesNearLeft;
        private float selectedNodesFarRight;
        private float selectedNodesNearRight;
        private float selectedNodesFarTop;
        private float selectedNodesNearTop;
        private float selectedNodesFarBottom;
        private float selectedNodesNearBottom;
        private float selectedNodesAvgHorizontal;
        private float selectedNodesAvgVertical;

        private float _noPortOpacity = -1;

        protected virtual float NoPortOpacity =>
            PropertyUtils.LazyLoad(ref _noPortOpacity, () =>
                {
                    Type nodeType = nodeTarget.GetType();
                    if (nodeType.HasCustomAttribute<NodeOpacityIfNoPorts>())
                        return nodeType.GetCustomAttribute<NodeOpacityIfNoPorts>().Opacity;

                    return 1;
                },
                value => value == -1);

        protected bool HasPorts => inputPortViews.Count + outputPortViews.Count > 0;

        protected NodeRenamePolicy RenamePolicy => nodeTarget.RenamePolicy;

        #region Initialization

        public void Initialize(GraphView owner, Systems.Node_Graph.Node node)
        {
            nodeTarget = node;
            this.owner = owner;

            if (!node.deletable)
                capabilities &= ~Capabilities.Deletable;
            // Note that the Renamable capability is useless right now as it isn't implemented in GraphView.
            // We implement our own in SetupRenamableTitle
            if (!RenamePolicy.Is(NodeRenamePolicy.DISABLED))
                capabilities |= Capabilities.Renamable;

            owner.computeOrderUpdated += ComputeOrderUpdatedCallback;
            node.onMessageAdded += AddMessageView;
            node.onMessageRemoved += RemoveMessageView;
            node.onPortsUpdated += a => schedule.Execute(_ => UpdatePortsForField(a)).ExecuteLater(0);

            styleSheets.Add(Resources.Load<StyleSheet>(baseNodeStyle));

            if (!string.IsNullOrEmpty(node.layoutStyle))
                styleSheets.Add(Resources.Load<StyleSheet>(node.layoutStyle));

            InitializeView();
            InitializePorts();
            InitializeDebug();

            // If the standard Enable method is still overwritten, we call it
            if (GetType().GetMethod(nameof(Enable), new Type[] { }).DeclaringType != typeof(NodeView))
                ExceptionToLog.Call(() => Enable());
            else
                ExceptionToLog.Call(() => Enable(false));

            InitializeSettings();

            RefreshExpandedState();

            RefreshPorts();
            SetOpacity(HasPorts ? 1 : NoPortOpacity);

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            RegisterCallback<DetachFromPanelEvent>(e => ExceptionToLog.Call(Disable));
            RegisterCallback<PointerDownEvent>(e =>
            {
                if (e.clickCount == 2 && e.button == (int) MouseButton.LeftMouse)
                {
                    if (RenamePolicy.Is(NodeRenamePolicy.DISABLED)) return;
                    if (RenamePolicy.IsAny(NodeRenamePolicy.DOUBLE_CLICK, NodeRenamePolicy.BOTH))
                        if (titleContainer.ContainsPoint(e.localPosition))
                            return;
                    if (RenamePolicy.IsAny(NodeRenamePolicy.ICON, NodeRenamePolicy.BOTH))
                        if (renameIcon.ContainsPoint(e.localPosition))
                            return;

                    OnDoubleClicked();
                }
            });
            OnGeometryChanged(null);

            InitializeNodeToViewInterface();
        }

        private void InitializeNodeToViewInterface()
        {
            nodeTarget.View = new ViewDelegates(nodeTarget, GetPosition, SetPosition, UpdateTitle);
        }

        private void InitializePorts()
        {
            BaseEdgeConnectorListener listener = owner.connectorListener;

            foreach (NodePort inputPort in nodeTarget.inputPorts)
                AddPort(inputPort.fieldInfo, Direction.Input, listener, inputPort.portData);

            foreach (NodePort outputPort in nodeTarget.outputPorts)
                AddPort(outputPort.fieldInfo, Direction.Output, listener, outputPort.portData);
        }

        private void InitializeView()
        {
            controlsContainer = new VisualElement {name = "controls"};
            controlsContainer.AddToClassList("NodeControls");
            if (!nodeTarget.HideNodeInspectorBlock)
                mainContainer.Add(controlsContainer);

            rightTitleContainer = new VisualElement {name = "RightTitleContainer"};
            titleContainer.Add(rightTitleContainer);

            topPortContainer = new VisualElement {name = "TopPortContainer"};
            Insert(0, topPortContainer);

            bottomPortContainer = new VisualElement {name = "BottomPortContainer"};
            Add(bottomPortContainer);

            if (nodeTarget.showControlsOnHover)
            {
                bool mouseOverControls = false;
                controlsContainer.style.display = DisplayStyle.None;
                RegisterCallback<MouseOverEvent>(e =>
                {
                    controlsContainer.style.display = DisplayStyle.Flex;
                    mouseOverControls = true;
                });
                RegisterCallback<MouseOutEvent>(e =>
                {
                    Rect rect = GetPosition();
                    Vector2 graphMousePosition = owner.contentViewContainer.WorldToLocal(e.mousePosition);
                    if (rect.Contains(graphMousePosition) || !nodeTarget.showControlsOnHover)
                        return;
                    mouseOverControls = false;
                    schedule.Execute(_ =>
                    {
                        if (!mouseOverControls)
                            controlsContainer.style.display = DisplayStyle.None;
                    }).ExecuteLater(500);
                });
            }

            Undo.undoRedoPerformed += UpdateFieldValues;

            debugContainer = new VisualElement {name = "debug"};
            if (nodeTarget.debug)
                mainContainer.Add(debugContainer);

            initializing = true;

            UpdateTitle();
            SetPosition(nodeTarget.position.position != Vector2.zero
                ? nodeTarget.position
                : nodeTarget.initialPosition);
            SetNodeColor(nodeTarget.color);

            AddInputContainer();

            // // Add renaming capability
            // if ((capabilities & Capabilities.Renamable) != 0)
            SetupRenamableTitle();
        }

        private void SetupRenamableTitle()
        {
            var titleLabel = this.Q("title-label") as Label;

            titleTextField = new TextField {isDelayed = true};
            titleTextField.Hide();
            titleLabel.parent.Insert(0, titleTextField);

            renameIcon = new Image {image = EditorGUIUtility.IconContent("d_InputField Icon").image};
            renameIcon.SetPosition(Position.Absolute).SetSize(16, 16).SetOffset(10, 0, -9, 0).SetOpacity(0.4f);

            bool isPointerOverImage = false;
            renameIcon.RegisterCallback<PointerOverEvent>(e =>
            {
                renameIcon.SetOpacity(1);
                isPointerOverImage = true;
            });
            renameIcon.RegisterCallback<PointerOutEvent>(e =>
            {
                renameIcon.SetOpacity(0.4f);
                isPointerOverImage = false;
            });
            renameIcon.RegisterCallback<MouseDownEvent>(ImageMouseDownCallback);
            Add(renameIcon);

            if (!RenamePolicy.IsAny(NodeRenamePolicy.ICON, NodeRenamePolicy.BOTH))
                renameIcon.Hide();

            titleLabel.RegisterCallback<MouseDownEvent>(e =>
            {
                if (!RenamePolicy.IsAny(NodeRenamePolicy.DOUBLE_CLICK, NodeRenamePolicy.BOTH)) return;

                if (e.clickCount == 2 && e.button == (int) MouseButton.LeftMouse)
                    OpenTitleEditor();
            });

            titleTextField.RegisterValueChangedCallback(e => CloseAndSaveTitleEditor(e.newValue));

            titleTextField.RegisterCallback<MouseDownEvent>(e =>
            {
                if (e.clickCount == 2 && e.button == (int) MouseButton.LeftMouse)
                    CloseAndSaveTitleEditor(titleTextField.value);
            });

            void ImageMouseDownCallback(MouseDownEvent e)
            {
                if (!titleTextField.IsShowing())
                    OpenTitleEditor();
                else
                    CloseAndSaveTitleEditor(titleTextField.value);

                e.StopPropagation();
            }

            void TitleTextFieldFocusOut(FocusOutEvent e)
            {
                if (isPointerOverImage && Event.current?.button == 0) return;

                CloseAndSaveTitleEditor(titleTextField.value);
            }

            titleTextField.RegisterCallback<FocusOutEvent>(TitleTextFieldFocusOut, TrickleDown.TrickleDown);


            void OpenTitleEditor()
            {
                // show title textbox
                titleTextField.Show();
                titleLabel.Hide();
                titleTextField.focusable = true;
                // titleTextField.RegisterCallback<FocusOutEvent>(TitleTextFieldFocusOut);

                titleTextField.SetValueWithoutNotify(title);
                titleTextField.Focus();
                titleTextField.SelectAll();
            }

            void CloseAndSaveTitleEditor(string newTitle)
            {
                owner.RegisterCompleteObjectUndo("Renamed node " + newTitle);
                nodeTarget.SetCustomName(newTitle);

                // hide title TextBox
                titleTextField.Hide();
                titleLabel.Show();
                titleTextField.focusable = false;
                // titleTextField.UnregisterCallback<FocusOutEvent>(TitleTextFieldFocusOut);

                UpdateTitle();
            }
        }

        private void UpdateTitle()
        {
            title = nodeTarget.GetCustomName() ?? nodeTarget.GetType().Name;

            if (RenamePolicy.IsAny(NodeRenamePolicy.ICON, NodeRenamePolicy.BOTH))
                renameIcon?.Show();
            else
                renameIcon?.Hide();
        }

        private void InitializeSettings()
        {
            // Initialize settings button:
            if (hasSettings)
            {
                CreateSettingButton();
                settingsContainer = new NodeSettingsView();
                settingsContainer.visible = false;
                settings = new VisualElement();
                // Add Node type specific settings
                settings.Add(CreateSettingsView());
                settingsContainer.Add(settings);
                Add(settingsContainer);

                FieldInfo[] fields = nodeTarget.GetType()
                    .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                foreach (FieldInfo field in fields)
                    if (field.HasCustomAttribute<SettingAttribute>())
                        AddSettingField(field);
            }
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (settingButton != null)
            {
                Rect settingsButtonLayout =
                    settingButton.ChangeCoordinatesTo(settingsContainer.parent, settingButton.layout);
                settingsContainer.style.top = settingsButtonLayout.yMax - 18f;
                settingsContainer.style.left = settingsButtonLayout.xMin - layout.width + 20f;
            }
        }

        // Workaround for bug in GraphView that makes the node selection border way too big
        private VisualElement selectionBorder, nodeBorder;

        internal void EnableSyncSelectionBorderHeight()
        {
            if (selectionBorder == null || nodeBorder == null)
            {
                selectionBorder = this.Q("selection-border");
                nodeBorder = this.Q("node-border");

                schedule.Execute(() => { selectionBorder.style.height = nodeBorder.localBound.height; }).Every(17);
            }
        }

        private void CreateSettingButton()
        {
            settingButton = new Button(ToggleSettings) {name = "settings-button"};
            settingButton.Add(new Image {name = "icon", scaleMode = ScaleMode.ScaleToFit});

            titleContainer.Add(settingButton);
        }

        private void ToggleSettings()
        {
            settingsExpanded = !settingsExpanded;
            if (settingsExpanded)
                OpenSettings();
            else
                CloseSettings();
        }

        public void OpenSettings()
        {
            if (settingsContainer != null)
            {
                owner.ClearSelection();
                owner.AddToSelection(this);

                settingButton.AddToClassList("clicked");
                settingsContainer.visible = true;
                settingsExpanded = true;
            }
        }

        public void CloseSettings()
        {
            if (settingsContainer != null)
            {
                settingButton.RemoveFromClassList("clicked");
                settingsContainer.visible = false;
                settingsExpanded = false;
            }
        }

        private void InitializeDebug()
        {
            ComputeOrderUpdatedCallback();
            debugContainer.Add(computeOrderLabel);
        }

        #endregion

        #region API

        public List<PortView> GetPortViewsFromFieldName(string fieldName)
        {
            MemberInfo info = portsByMemberInfo.Keys.First(info => info.Name == fieldName);

            if (info == null) return null;

            portsByMemberInfo.TryGetValue(info, out List<PortView> ret);

            return ret;
        }

        public PortView GetFirstPortViewFromFieldName(string fieldName)
        {
            return GetPortViewsFromFieldName(fieldName)?.First();
        }

        public PortView GetPortViewFromFieldName(string fieldName, string identifier)
        {
            return GetPortViewsFromFieldName(fieldName)?.FirstOrDefault(pv =>
            {
                return pv.portData.Identifier == identifier || (string.IsNullOrEmpty(pv.portData.Identifier) &&
                                                                string.IsNullOrEmpty(identifier));
            });
        }


        public PortView AddPort(MemberInfo fieldInfo, Direction direction, BaseEdgeConnectorListener listener,
            PortData portData)
        {
            PortView p = CreatePortView(direction, fieldInfo, portData, listener);

            if (p.direction == Direction.Input)
            {
                inputPortViews.Add(p);

                if (portData.vertical)
                    topPortContainer.Add(p);
                else
                    inputContainer.Add(p);
            }
            else
            {
                outputPortViews.Add(p);

                if (portData.vertical)
                    bottomPortContainer.Add(p);
                else
                    outputContainer.Add(p);
            }

            p.Initialize(this, portData?.displayName);

            List<PortView> ports;
            portsByMemberInfo.TryGetValue(p.MemberInfo, out ports);
            if (ports == null)
            {
                ports = new List<PortView>();
                portsByMemberInfo[p.MemberInfo] = ports;
            }

            ports.Add(p);

            return p;
        }

        protected virtual PortView CreatePortView(Direction direction, MemberInfo fieldInfo, PortData portData,
            BaseEdgeConnectorListener listener)
        {
            return PortView.CreatePortView(direction, fieldInfo, portData, listener);
        }

        public void InsertPort(PortView portView, int index)
        {
            if (portView.direction == Direction.Input)
            {
                if (portView.portData.vertical)
                {
                    int position = topPortContainer.childCount < index ? topPortContainer.childCount : index;
                    topPortContainer.Insert(position, portView);
                }
                else
                {
                    int position = inputContainer.childCount < index ? inputContainer.childCount : index;
                    inputContainer.Insert(position, portView);
                }
            }
            else
            {
                if (portView.portData.vertical)
                {
                    int position = bottomPortContainer.childCount < index ? bottomPortContainer.childCount : index;
                    bottomPortContainer.Insert(position, portView);
                }
                else
                {
                    int position = outputContainer.childCount < index ? outputContainer.childCount : index;
                    outputContainer.Insert(position, portView);
                }
            }
        }

        public void RemovePort(PortView p)
        {
            // Remove all connected edges:
            List<EdgeView> edgesCopy = p.GetEdges().ToList();
            foreach (EdgeView e in edgesCopy)
                owner.Disconnect(e, false);

            if (p.direction == Direction.Input)
            {
                if (inputPortViews.Remove(p))
                    p.RemoveFromHierarchy();
            }
            else
            {
                if (outputPortViews.Remove(p))
                    p.RemoveFromHierarchy();
            }

            portsByMemberInfo.TryGetValue(p.MemberInfo, out List<PortView> ports);
            ports.Remove(p);
        }

        private void SetValuesForSelectedNodes()
        {
            selectedNodes = new List<Node>();
            owner.nodes.ForEach(node =>
            {
                if (node.selected) selectedNodes.Add(node);
            });

            if (selectedNodes.Count < 2) return; //	No need for any of the calculations below

            selectedNodesFarLeft = int.MinValue;
            selectedNodesFarRight = int.MinValue;
            selectedNodesFarTop = int.MinValue;
            selectedNodesFarBottom = int.MinValue;

            selectedNodesNearLeft = int.MaxValue;
            selectedNodesNearRight = int.MaxValue;
            selectedNodesNearTop = int.MaxValue;
            selectedNodesNearBottom = int.MaxValue;

            foreach (Node selectedNode in selectedNodes)
            {
                IStyle nodeStyle = selectedNode.style;
                float nodeWidth = selectedNode.localBound.size.x;
                float nodeHeight = selectedNode.localBound.size.y;

                if (nodeStyle.left.value.value > selectedNodesFarLeft)
                    selectedNodesFarLeft = nodeStyle.left.value.value;
                if (nodeStyle.left.value.value + nodeWidth > selectedNodesFarRight)
                    selectedNodesFarRight = nodeStyle.left.value.value + nodeWidth;
                if (nodeStyle.top.value.value > selectedNodesFarTop) selectedNodesFarTop = nodeStyle.top.value.value;
                if (nodeStyle.top.value.value + nodeHeight > selectedNodesFarBottom)
                    selectedNodesFarBottom = nodeStyle.top.value.value + nodeHeight;

                if (nodeStyle.left.value.value < selectedNodesNearLeft)
                    selectedNodesNearLeft = nodeStyle.left.value.value;
                if (nodeStyle.left.value.value + nodeWidth < selectedNodesNearRight)
                    selectedNodesNearRight = nodeStyle.left.value.value + nodeWidth;
                if (nodeStyle.top.value.value < selectedNodesNearTop) selectedNodesNearTop = nodeStyle.top.value.value;
                if (nodeStyle.top.value.value + nodeHeight < selectedNodesNearBottom)
                    selectedNodesNearBottom = nodeStyle.top.value.value + nodeHeight;
            }

            selectedNodesAvgHorizontal = (selectedNodesNearLeft + selectedNodesFarRight) / 2f;
            selectedNodesAvgVertical = (selectedNodesNearTop + selectedNodesFarBottom) / 2f;
        }

        public static Rect GetNodeRect(Node node, float left = int.MaxValue, float top = int.MaxValue)
        {
            return new Rect(
                new Vector2(left != int.MaxValue ? left : node.style.left.value.value,
                    top != int.MaxValue ? top : node.style.top.value.value),
                new Vector2(node.style.width.value.value, node.style.height.value.value)
            );
        }

        public void AlignToLeft()
        {
            SetValuesForSelectedNodes();
            if (selectedNodes.Count < 2) return;

            foreach (Node selectedNode in selectedNodes)
                selectedNode.SetPosition(GetNodeRect(selectedNode, selectedNodesNearLeft));
        }

        public void AlignToCenter()
        {
            SetValuesForSelectedNodes();
            if (selectedNodes.Count < 2) return;

            foreach (Node selectedNode in selectedNodes)
                selectedNode.SetPosition(GetNodeRect(selectedNode,
                    selectedNodesAvgHorizontal - selectedNode.localBound.size.x / 2f));
        }

        public void AlignToRight()
        {
            SetValuesForSelectedNodes();
            if (selectedNodes.Count < 2) return;

            foreach (Node selectedNode in selectedNodes)
                selectedNode.SetPosition(GetNodeRect(selectedNode,
                    selectedNodesFarRight - selectedNode.localBound.size.x));
        }

        public void AlignToTop()
        {
            SetValuesForSelectedNodes();
            if (selectedNodes.Count < 2) return;

            foreach (Node selectedNode in selectedNodes)
                selectedNode.SetPosition(GetNodeRect(selectedNode, top: selectedNodesNearTop));
        }

        public void AlignToMiddle()
        {
            SetValuesForSelectedNodes();
            if (selectedNodes.Count < 2) return;

            foreach (Node selectedNode in selectedNodes)
                selectedNode.SetPosition(GetNodeRect(selectedNode,
                    top: selectedNodesAvgVertical - selectedNode.localBound.size.y / 2f));
        }

        public void AlignToBottom()
        {
            SetValuesForSelectedNodes();
            if (selectedNodes.Count < 2) return;

            foreach (Node selectedNode in selectedNodes)
                selectedNode.SetPosition(GetNodeRect(selectedNode,
                    top: selectedNodesFarBottom - selectedNode.localBound.size.y));
        }

        public void OpenNodeViewScript()
        {
            MonoScript script = NodeProvider.GetNodeViewScript(GetType());

            if (script != null)
                AssetDatabase.OpenAsset(script.GetInstanceID(), 0, 0);
        }

        public void OpenNodeScript()
        {
            MonoScript script = NodeProvider.GetNodeScript(nodeTarget.GetType());

            if (script != null)
                AssetDatabase.OpenAsset(script.GetInstanceID(), 0, 0);
        }

        public void ToggleDebug()
        {
            nodeTarget.debug = !nodeTarget.debug;
            UpdateDebugView();
        }

        public void UpdateDebugView()
        {
            if (nodeTarget.debug)
                mainContainer.Add(debugContainer);
            else
                mainContainer.Remove(debugContainer);
        }

        public void AddMessageView(string message, Texture icon, Color color)
        {
            AddBadge(new NodeBadgeView(message, icon, color));
        }

        public void AddMessageView(string message, NodeMessageType messageType)
        {
            IconBadge badge = null;
            switch (messageType)
            {
                case NodeMessageType.Warning:
                    badge = new NodeBadgeView(message, EditorGUIUtility.IconContent("Collab.Warning").image,
                        Color.yellow);
                    break;
                case NodeMessageType.Error:
                    badge = IconBadge.CreateError(message);
                    break;
                case NodeMessageType.Info:
                    badge = IconBadge.CreateComment(message);
                    break;
                default:
                case NodeMessageType.None:
                    badge = new NodeBadgeView(message, null, Color.grey);
                    break;
            }

            AddBadge(badge);
        }

        private void AddBadge(IconBadge badge)
        {
            Add(badge);
            badges.Add(badge);
            badge.AttachTo(topContainer, SpriteAlignment.TopRight);
        }

        private void RemoveBadge(Func<IconBadge, bool> callback)
        {
            badges.RemoveAll(b =>
            {
                if (callback(b))
                {
                    b.Detach();
                    b.RemoveFromHierarchy();
                    return true;
                }

                return false;
            });
        }

        public void RemoveMessageViewContains(string message)
        {
            RemoveBadge(b => b.badgeText.Contains(message));
        }

        public void RemoveMessageView(string message)
        {
            RemoveBadge(b => b.badgeText == message);
        }

        public void Highlight()
        {
            AddToClassList("Highlight");
        }

        public void UnHighlight()
        {
            RemoveFromClassList("Highlight");
        }

        #endregion

        #region Callbacks & Overrides

        private void ComputeOrderUpdatedCallback()
        {
            //Update debug compute order
            computeOrderLabel.text = "Compute order: " + nodeTarget.computeOrder;
        }

        public virtual void Enable(bool fromInspector = false)
        {
            DrawDefaultInspector(fromInspector);
        }

        public virtual void Enable()
        {
            DrawDefaultInspector();
        }

        public virtual void Disable()
        {
        }

        private readonly Dictionary<string, List<(object value, VisualElement target)>> visibleConditions = new();
        private readonly Dictionary<string, VisualElement> hideElementIfConnected = new();
        private readonly Dictionary<UnityPath, List<VisualElement>> fieldControlsMap = new();

        protected void AddInputContainer()
        {
            inputContainerElement = new VisualElement {name = "input-container"};
            mainContainer.parent.Add(inputContainerElement);
            inputContainerElement.SendToBack();
            inputContainerElement.pickingMode = PickingMode.Ignore;
        }

        protected virtual void DrawDefaultInspector(bool fromInspector = false)
        {
            if (!fromInspector)
            {
                inputContainerElement.Clear();
                controlsContainer.Clear();
            }

            nodeTarget.DrawControlsContainer(controlsContainer);

            DrawFields(FindNodeMembers(), fromInspector);
        }

        private List<MemberInfo> FindNodeMembers()
        {
            List<MemberInfo> nodeMembers = nodeTarget.GetType()
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Cast<MemberInfo>()
                .Concat(nodeTarget.GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                // Filter fields from the BaseNode type since we are only interested in user-defined fields
                // (better than BindingFlags.DeclaredOnly because we keep any inherited user-defined fields) 
                .Where(f => f.DeclaringType != typeof(Systems.Node_Graph.Node)).ToList();

            nodeMembers.AddRange(FindAllNestedPortMembers(nodeMembers));

            nodeMembers = nodeTarget.OverrideFieldOrder(nodeMembers).Reverse().ToList();

            return nodeMembers;

            List<MemberInfo> FindAllNestedPortMembers(in IEnumerable<MemberInfo> membersToSearch)
            {
                List<MemberInfo> nestedPorts = new();
                foreach (MemberInfo field in new List<MemberInfo>(membersToSearch.Where(f =>
                             f.HasCustomAttribute<NestedPortsAttribute>())))
                {
                    IEnumerable<MemberInfo> nestedFields = field.GetUnderlyingType()
                        .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        .Cast<MemberInfo>()
                        .Concat(field.GetUnderlyingType()
                            .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));

                    List<MemberInfo> foundNestedPorts =
                        nestedFields.Where(f => portsByMemberInfo.ContainsKey(f)).ToList();

                    nestedPorts.AddRange(foundNestedPorts);
                    nestedPorts.AddRange(FindAllNestedPortMembers(nestedFields));
                }

                return nestedPorts;
            }
        }

        protected virtual void DrawFields(List<MemberInfo> fields, bool fromInspector = false)
        {
            for (int i = 0; i < fields.Count; i++)
            {
                MemberInfo field = fields[i];

                if (!portsByMemberInfo.ContainsKey(field))
                {
                    if (field.HasCustomAttribute<CustomBehaviourOnly>()) continue;
                    DrawField(field, new UnityPath(field), fromInspector);
                    continue;
                }

                foreach (PortView portView in portsByMemberInfo[field])
                {
                    UnityPath fieldPath = portView.portData.IsProxied
                        ? portView.portData.proxiedFieldPath
                        : new UnityPath(portView.fieldName);

                    DrawField(field, fieldPath, fromInspector, portView);
                }
            }
        }

        protected virtual void DrawField(MemberInfo origin, UnityPath memberPath, bool fromInspector,
            PortView portView = null)
        {
            bool hasPortView = portView != null;
            MemberInfo memberToDraw = memberPath.GetPathAsMemberInfoList(nodeTarget)?.Last() ?? origin;
            PortData portData = portView?.portData;
            bool isProxied = hasPortView && portData.IsProxied;

            if (!memberToDraw.IsField()) return;

            var field = memberToDraw as FieldInfo;

            //skip if the field is a node setting
            if (field.HasCustomAttribute<SettingAttribute>())
            {
                hasSettings = true;
                return;
            }

            //skip if the field is not serializable
            bool serializeField = field.HasCustomAttribute<SerializeField>();
            bool serializeReference = field.HasCustomAttribute<SerializeReference>();
            if ((!field.IsPublic && !serializeField && !serializeReference) || field.IsNotSerialized)
            {
                AddEmptyField(field, fromInspector);
                return;
            }

            //skip if the field is an input/output and not marked as SerializedField
            var inputAttribute = field.GetCustomAttribute<InputAttribute>();
            bool hasInputAttribute = inputAttribute != null;
            bool isInput = (!hasPortView && hasInputAttribute) ||
                           (hasPortView && portView.direction == Direction.Input);
            bool showAsDrawer = !fromInspector && isInput &&
                                (inputAttribute.showAsDrawer || field.HasCustomAttribute<ShowAsDrawer>());
            showAsDrawer |= !fromInspector && isInput && hasPortView && portData.showAsDrawer;
            if (((!serializeField && !serializeReference) || isProxied) && (hasPortView || hasInputAttribute) &&
                !showAsDrawer)
            {
                AddEmptyField(field, fromInspector);
                return;
            }

            //skip if marked with NonSerialized or HideInInspector
            if (field.HasCustomAttribute<NonSerializedAttribute>() || field.HasCustomAttribute<HideInInspector>())
            {
                // Debug.Log("3 " + fieldPath);
                AddEmptyField(field, fromInspector);
                return;
            }

            // Hide the field if we want to display in in the inspector
            var showInInspector = field.GetCustomAttribute<ShowInInspector>();
            if (!serializeField && !serializeReference && showInInspector != null && !showInInspector.showInNode &&
                !fromInspector)
            {
                // Debug.Log("4 " + fieldPath);
                AddEmptyField(field, fromInspector);
                return;
            }


            bool showInputDrawer = hasInputAttribute && (serializeField || serializeReference);
            showInputDrawer |= showAsDrawer;
            showInputDrawer &= !fromInspector; // We can't show a drawer in the inspector
            showInputDrawer &= !typeof(IList).IsAssignableFrom(field.GetUnderlyingType());

            string displayName = ObjectNames.NicifyVariableName(field.Name);

            var inspectorNameAttribute = field.GetCustomAttribute<InspectorNameAttribute>();
            if (inspectorNameAttribute != null)
                displayName = inspectorNameAttribute.displayName;

            VisualElement elem = AddControlField(memberPath, displayName, showInputDrawer);
            if (hasInputAttribute || (hasPortView && portView.direction == Direction.Input))
            {
                hideElementIfConnected[memberPath] = elem;

                // Hide the field right away if there is already a connection:
                if (portsByMemberInfo.TryGetValue(memberToDraw, out List<PortView> pvs))
                    if (pvs.Any(pv => pv.GetEdges().Count > 0))
                        elem.style.display = DisplayStyle.None;
            }
        }

        protected virtual void SetNodeColor(Color color)
        {
            titleContainer.style.borderBottomColor = new StyleColor(color);
            titleContainer.style.borderBottomWidth = new StyleFloat(color.a > 0 ? 5f : 0f);
        }

        private void AddEmptyField(MemberInfo field, bool fromInspector)
        {
            if (!field.HasCustomAttribute<InputAttribute>() || fromInspector)
                return;

            if (field.HasCustomAttribute<VerticalAttribute>())
                return;

            var box = new VisualElement {name = field.Name};
            box.AddToClassList("port-input-element");
            box.AddToClassList("empty");
            inputContainerElement.Add(box);
        }

        private void UpdateFieldVisibility(string fieldName, object newValue)
        {
            if (newValue == null)
                return;
            if (visibleConditions.TryGetValue(fieldName, out List<(object value, VisualElement target)> list))
                foreach ((object value, VisualElement target) elem in list)
                    if (newValue.Equals(elem.value))
                        elem.target.style.display = DisplayStyle.Flex;
                    else
                        elem.target.style.display = DisplayStyle.None;
        }

        private void UpdateOtherFieldValueSpecific<T>(UnityPath field, object newValue)
        {
            foreach (VisualElement inputField in fieldControlsMap[field])
            {
                var notify = inputField as INotifyValueChanged<T>;
                if (notify != null)
                    notify.SetValueWithoutNotify((T) newValue);
            }
        }

        private static readonly MethodInfo specificUpdateOtherFieldValue =
            typeof(NodeView).GetMethod(nameof(UpdateOtherFieldValueSpecific),
                BindingFlags.NonPublic | BindingFlags.Instance);

        private void UpdateOtherFieldValue(UnityPath info, object newValue)
        {
            // Warning: Keep in sync with FieldFactory CreateField
            MemberInfo member = info.GetPathAsMemberInfoList(nodeTarget).Last();
            Type fieldType = member.GetType().IsSubclassOf(typeof(Object))
                ? typeof(Object)
                : member.GetUnderlyingType();
            MethodInfo genericUpdate = specificUpdateOtherFieldValue.MakeGenericMethod(fieldType);

            genericUpdate.Invoke(this, new[] {info, newValue});
        }

        private object GetInputFieldValueSpecific<T>(UnityPath field)
        {
            if (fieldControlsMap.TryGetValue(field, out List<VisualElement> list))
                foreach (VisualElement inputField in list)
                    if (inputField is INotifyValueChanged<T> notify)
                        return notify.value;
            return null;
        }

        private static readonly MethodInfo specificGetValue =
            typeof(NodeView).GetMethod(nameof(GetInputFieldValueSpecific),
                BindingFlags.NonPublic | BindingFlags.Instance);

        private object GetInputFieldValue(UnityPath info)
        {
            // Warning: Keep in sync with FieldFactory CreateField
            MemberInfo member = info.GetPathAsMemberInfoList(nodeTarget).Last();
            Type fieldType = member.GetUnderlyingType().IsSubclassOf(typeof(Object))
                ? typeof(Object)
                : member.GetUnderlyingType();
            MethodInfo genericUpdate = specificGetValue.MakeGenericMethod(fieldType);

            return genericUpdate.Invoke(this, new object[] {info});
        }

        private readonly Regex s_ReplaceNodeIndexPropertyPath = new(@"(^nodes.Array.data\[)(\d+)(\])");

        internal void SyncSerializedPropertyPathes()
        {
            int nodeIndex = owner.graph.nodes.FindIndex(n => n == nodeTarget);

            // If the node is not found, then it means that it has been deleted from serialized data.
            if (nodeIndex == -1)
                return;

            string nodeIndexString = nodeIndex.ToString();
            foreach (PropertyField propertyField in this.Query<PropertyField>().ToList())
            {
                if (propertyField.bindingPath == null)
                    continue;

                propertyField.Unbind();
                // The property path look like this: nodes.Array.data[x].fieldName
                // And we want to update the value of x with the new node index:
                propertyField.bindingPath = s_ReplaceNodeIndexPropertyPath.Replace(propertyField.bindingPath,
                    m => m.Groups[1].Value + nodeIndexString + m.Groups[3].Value);
                propertyField.Bind(owner.serializedGraph);
            }
        }

        protected SerializedProperty FindSerializedProperty(UnityPath fieldName)
        {
            return FindSerializedProperty(fieldName, out _);
        }

        protected SerializedProperty FindSerializedProperty(UnityPath fieldPath, out SerializedObject parent)
        {
            int i = owner.graph.nodes.FindIndex(n => n == nodeTarget);

            SerializedObject parentObject = owner.serializedGraph;
            SerializedProperty property = parentObject.FindProperty("nodes").GetArrayElementAtIndex(i);
            for (int x = 0; x < fieldPath.PathArray.Length; x++)
                if (property.propertyType == SerializedPropertyType.ObjectReference)
                {
                    parentObject = new SerializedObject(property.objectReferenceValue);
                    property = parentObject.FindProperty(fieldPath.PathArray[x]);
                }
                else
                {
                    property = property.FindPropertyRelative(fieldPath.PathArray[x]);
                }

            parent = parentObject;
            return property;
        }

        protected VisualElement AddControlField(UnityPath unityPath, string label = null, bool showInputDrawer = false,
            Action valueChangedCallback = null)
        {
            MemberInfo memberInfo = unityPath?.GetPathAsMemberInfoList(nodeTarget)?.Last();
            string path = unityPath.Path;

            if (memberInfo == null)
                return null;

            var element = new PropertyField(FindSerializedProperty(unityPath, out SerializedObject fieldParentObject),
                showInputDrawer ? "" : label);
            element.Bind(fieldParentObject);

#if UNITY_2020_3 // In Unity 2020.3 the empty label on property field doesn't hide it, so we do it manually
			if ((showInputDrawer || String.IsNullOrEmpty(label)) && element != null)
				element.AddToClassList("DrawerField_2020_3");
#endif

            if (typeof(IList).IsAssignableFrom(memberInfo.GetUnderlyingType()))
            {
                EnableSyncSelectionBorderHeight();

                // Prevent node stealing focus from ListView 
                void ListViewSelectionFixCallback(GeometryChangedEvent e)
                {
                    // Wait until propertyfield has generated its children
                    if (element.childCount == 0) return;

                    //#unity-content-container is the list view content
                    element.Q("unity-content-container").RegisterCallback<MouseDownEvent>(_ =>
                    {
                        // avoid handing the event over to the SelectionDragger to prevent sorting issues
                        _.StopImmediatePropagation();
                    }, TrickleDown.TrickleDown);

                    // Unregister this callback as we don't need it anymore
                    element.UnregisterCallback<GeometryChangedEvent>(ListViewSelectionFixCallback);
                }

                element.RegisterCallback<GeometryChangedEvent>(ListViewSelectionFixCallback);
            }

            element.RegisterValueChangeCallback(e =>
            {
                UpdateFieldVisibility(memberInfo.Name, unityPath.GetValueOfMemberAtPath(nodeTarget));
                valueChangedCallback?.Invoke();
                NotifyNodeChanged();
            });

            // Disallow picking scene objects when the graph is not linked to a scene
            if (element != null && !owner.graph.IsLinkedToScene())
            {
                var objectField = element.Q<ObjectField>();
                if (objectField != null)
                    objectField.allowSceneObjects = false;
            }

            if (!fieldControlsMap.TryGetValue(unityPath, out List<VisualElement> inputFieldList))
                inputFieldList = fieldControlsMap[unityPath] = new List<VisualElement>();
            inputFieldList.Add(element);

            if (element != null)
            {
                if (showInputDrawer)
                {
                    var box = new VisualElement {name = memberInfo.Name};
                    box.AddToClassList("port-input-element");
                    box.Add(element);
                    inputContainerElement.Add(box);
                }
                else
                {
                    controlsContainer.Add(element);
                }

                element.name = memberInfo.Name;
            }
            else
            {
                // Make sure we create an empty placeholder if FieldFactory can not provide a drawer
                if (showInputDrawer) AddEmptyField(memberInfo, false);
            }

            var visibleCondition = memberInfo.GetCustomAttribute<VisibleIf>();
            if (visibleCondition != null)
            {
                // Check if target field exists:
                FieldInfo conditionField = nodeTarget.GetType().GetField(visibleCondition.fieldName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (conditionField == null)
                {
                    Debug.LogError(
                        $"[VisibleIf] Field {visibleCondition.fieldName} does not exists in node {nodeTarget.GetType()}");
                }
                else
                {
                    visibleConditions.TryGetValue(visibleCondition.fieldName,
                        out List<(object value, VisualElement target)> list);
                    if (list == null)
                        list = visibleConditions[visibleCondition.fieldName] =
                            new List<(object value, VisualElement target)>();
                    list.Add((visibleCondition.value, element));
                    UpdateFieldVisibility(visibleCondition.fieldName, conditionField.GetValue(nodeTarget));
                }
            }

            return element;
        }

        private void UpdateFieldValues()
        {
            foreach (KeyValuePair<UnityPath, List<VisualElement>> kp in fieldControlsMap)
                UpdateOtherFieldValue(kp.Key, kp.Key.GetValueOfMemberAtPath(nodeTarget));
        }

        protected void AddSettingField(FieldInfo field)
        {
            if (field == null)
                return;

            string label = field.GetCustomAttribute<SettingAttribute>().name;

            var element = new PropertyField(FindSerializedProperty(new UnityPath(field)));
            element.Bind(owner.serializedGraph);

            if (element != null)
            {
                settingsContainer.Add(element);
                element.name = field.Name;
            }
        }

        internal void OnPortConnected(PortView port)
        {
            string fieldName = port.portData.IsProxied ? port.portData.proxiedFieldPath : port.fieldName;

            if (port.direction == Direction.Input && inputContainerElement?.Q(fieldName) != null)
                inputContainerElement.Q(fieldName).AddToClassList("empty");

            if (hideElementIfConnected.TryGetValue(fieldName, out VisualElement elem))
                elem.style.display = DisplayStyle.None;

            onPortConnected?.Invoke(port);
        }

        internal void OnPortDisconnected(PortView port) //
        {
            bool isProxied = port.portData.IsProxied;
            UnityPath fieldName = isProxied ? port.portData.proxiedFieldPath : new UnityPath(port.fieldName);

            if (port.direction == Direction.Input && inputContainerElement?.Q(fieldName) != null)
            {
                inputContainerElement.Q(fieldName).RemoveFromClassList("empty");

                object valueBeforeConnection = GetInputFieldValue(fieldName);

                if (valueBeforeConnection != null) fieldName.SetValueOfMemberAtPath(nodeTarget, valueBeforeConnection);
            }

            if (hideElementIfConnected.TryGetValue(fieldName, out VisualElement elem))
                elem.style.display = DisplayStyle.Flex;

            onPortDisconnected?.Invoke(port);
        }

        // TODO: a function to force to reload the custom behavior ports (if we want to do a button to add ports for example)

        public virtual void OnRemoved()
        {
        }

        public virtual void OnCreated()
        {
        }

        public override void SetPosition(Rect newPos)
        {
            if (!initializing && nodeTarget.isLocked) return;

            base.SetPosition(newPos);

            if (!initializing)
                owner.RegisterCompleteObjectUndo("Moved graph node");

            nodeTarget.position = newPos;
            initializing = false;
        }

        public override bool expanded
        {
            get => base.expanded;
            set
            {
                base.expanded = value;
                nodeTarget.expanded = value;
            }
        }

        public void ChangeLockStatus()
        {
            nodeTarget.nodeLock ^= true;
        }

        public virtual void OnDoubleClicked()
        {
        }

        public override void OnSelected()
        {
            base.OnSelected();
            SetOpacity(1);
        }

        public override void OnUnselected()
        {
            base.OnUnselected();
            SetOpacity(HasPorts ? 1 : NoPortOpacity);
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            BuildAlignMenu(evt);
            evt.menu.AppendAction("Open Node Script", e => OpenNodeScript(), OpenNodeScriptStatus);
            evt.menu.AppendAction("Open Node View Script", e => OpenNodeViewScript(), OpenNodeViewScriptStatus);
            evt.menu.AppendAction("Debug", e => ToggleDebug(), DebugStatus);
            if (nodeTarget.unlockable)
                evt.menu.AppendAction(nodeTarget.isLocked ? "Unlock" : "Lock", e => ChangeLockStatus(), LockStatus);
        }

        protected void BuildAlignMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Align/To Left", e => AlignToLeft());
            evt.menu.AppendAction("Align/To Center", e => AlignToCenter());
            evt.menu.AppendAction("Align/To Right", e => AlignToRight());
            evt.menu.AppendSeparator("Align/");
            evt.menu.AppendAction("Align/To Top", e => AlignToTop());
            evt.menu.AppendAction("Align/To Middle", e => AlignToMiddle());
            evt.menu.AppendAction("Align/To Bottom", e => AlignToBottom());
            evt.menu.AppendSeparator();
        }

        private DropdownMenuAction.Status LockStatus(DropdownMenuAction action)
        {
            return DropdownMenuAction.Status.Normal;
        }

        private DropdownMenuAction.Status DebugStatus(DropdownMenuAction action)
        {
            if (nodeTarget.debug)
                return DropdownMenuAction.Status.Checked;
            return DropdownMenuAction.Status.Normal;
        }

        private DropdownMenuAction.Status OpenNodeScriptStatus(DropdownMenuAction action)
        {
            if (NodeProvider.GetNodeScript(nodeTarget.GetType()) != null)
                return DropdownMenuAction.Status.Normal;
            return DropdownMenuAction.Status.Disabled;
        }

        private DropdownMenuAction.Status OpenNodeViewScriptStatus(DropdownMenuAction action)
        {
            if (NodeProvider.GetNodeViewScript(GetType()) != null)
                return DropdownMenuAction.Status.Normal;
            return DropdownMenuAction.Status.Disabled;
        }

        private IEnumerable<PortView> SyncPortCounts(IEnumerable<NodePort> ports, IEnumerable<PortView> portViews)
        {
            BaseEdgeConnectorListener listener = owner.connectorListener;
            List<PortView> portViewList = portViews.ToList();

            // Maybe not good to remove ports as edges are still connected :/
            foreach (PortView pv in portViews.ToList())
                // If the port have disappeared from the node data, we remove the view:
                // We can use the identifier here because this function will only be called when there is a custom port behavior
                if (!ports.Any(p => p.portData.Identifier == pv.portData.Identifier))
                {
                    RemovePort(pv);
                    portViewList.Remove(pv);
                }

            foreach (NodePort p in ports)
                // Add missing port views
                if (!portViews.Any(pv => p.portData.Identifier == pv.portData.Identifier))
                {
                    Direction portDirection = nodeTarget.IsFieldInput(p.fieldName) ? Direction.Input : Direction.Output;
                    PortView pv = AddPort(p.fieldInfo, portDirection, listener, p.portData);
                    portViewList.Add(pv);
                }

            return portViewList;
        }

        private void SyncPortOrder(IEnumerable<NodePort> ports, IEnumerable<PortView> portViews)
        {
            List<PortView> portViewList = portViews.ToList();
            List<NodePort> portsList = ports.ToList();

            // Re-order the port views to match the ports order in case a custom behavior re-ordered the ports
            for (int i = 0; i < portsList.Count; i++)
            {
                string id = portsList[i].portData.Identifier;

                PortView pv = portViewList.FirstOrDefault(p => p.portData.Identifier == id);
                if (pv != null)
                    InsertPort(pv, i);
            }
        }

        public void RedrawControlDrawers()
        {
            // IEnumerable<string> uniqueFields = this.nodeTarget.GetAllPorts().Select(p => p.fieldName).Distinct();
            // var fields = nodeTarget.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            //     .Cast<MemberInfo>()
            //     .Concat(nodeTarget.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            //     // Filter fields from the BaseNode type since we are only interested in user-defined fields
            //     // (better than BindingFlags.DeclaredOnly because we keep any inherited user-defined fields) 
            //     .Where(f => f.DeclaringType != typeof(BaseNode)).ToList()
            //     .Where(f => uniqueFields.Contains(f.Name));

            // fields = nodeTarget.OverrideFieldOrder(fields).Reverse().ToList();

            // DrawFields(fields.ToList());
            DrawDefaultInspector();
        }

        public new virtual bool RefreshPorts()
        {
            // If a port behavior was attached to one port, then
            // the port count might have been updated by the node
            // so we have to refresh the list of port views.
            UpdatePortViewWithPorts(nodeTarget.inputPorts, inputPortViews);
            UpdatePortViewWithPorts(nodeTarget.outputPorts, outputPortViews);

            void UpdatePortViewWithPorts(NodePortContainer ports, List<PortView> portViews)
            {
                if (ports.Count == 0 && portViews.Count == 0) // Nothing to update
                    return;

                // When there is no current portviews, we can't zip the list so we just add all
                if (portViews.Count == 0)
                {
                    SyncPortCounts(ports, new PortView[] { });
                }
                else if (ports.Count == 0) // Same when there is no ports
                {
                    SyncPortCounts(new NodePort[] { }, portViews);
                }
                else if (portViews.Count != ports.Count)
                {
                    SyncPortCounts(ports, portViews);
                }
                else
                {
                    IEnumerable<IGrouping<string, NodePort>> p = ports.GroupBy(n => n.fieldName);
                    IEnumerable<IGrouping<string, PortView>> pv = portViews.GroupBy(v => v.fieldName);
                    p.Zip(pv, (portPerFieldName, portViewPerFieldName) =>
                    {
                        IEnumerable<PortView> portViewsList = portViewPerFieldName;
                        if (portPerFieldName.Count() != portViewPerFieldName.Count())
                            portViewsList = SyncPortCounts(portPerFieldName, portViewPerFieldName);
                        SyncPortOrder(portPerFieldName, portViewsList);
                        // We don't care about the result, we just iterate over port and portView
                        return "";
                    }).ToList();
                }

                // Here we're sure that we have the same amount of port and portView
                // so we can update the view with the new port data (if the name of a port have been changed for example)

                for (int i = 0; i < portViews.Count; i++)
                    portViews[i].UpdatePortView(ports[i].portData);
            }

            return base.RefreshPorts();
        }

        public void ForceUpdatePorts()
        {
            nodeTarget.UpdateAllPorts();

            RefreshPorts();

            RedrawControlDrawers();

            SetOpacity(HasPorts ? 1 : NoPortOpacity);
        }

        private void UpdatePortsForField(string fieldName)
        {
            // TODO: actual code
            RefreshPorts();
            RedrawControlDrawers();
            SetOpacity(HasPorts ? 1 : NoPortOpacity);
        }

        protected void SetNoPortOpacity(float opacity)
        {
            _noPortOpacity = opacity;
        }

        protected void SetOpacity(float opacity)
        {
            style.opacity = opacity;
        }

        protected virtual VisualElement CreateSettingsView()
        {
            return new Label("Settings") {name = "header"};
        }

        /// <summary>
        ///     Send an event to the graph telling that the content of this node have changed
        /// </summary>
        public void NotifyNodeChanged()
        {
            owner.graph.NotifyNodeChanged(nodeTarget);
        }

        #endregion
    }
}