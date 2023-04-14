using System;
using System.Collections.Generic;
using Konfus.Systems.Graph.Attributes;
using UnityEditor;
using UnityEngine;

namespace Konfus.Systems.Graph
{
    /// <summary>
    /// Our data model for Nodes split into a minimalistic runtime part and editor specific extensions.
    /// See #if UNITY_EDITOR sections for all editor specific parts of this class.
    /// The compilation tags allow us to strip away any unwated data for a runtime scenario.
    /// Source: https://docs.unity3d.com/2022.2/Documentation/Manual/script-Serialization.html
    /// Discussion: https://forum.unity.com/threads/serialize-fields-only-in-editor.433422/
    /// </summary>
    [Serializable]
    public class Node
    {
        /// <summary>
        /// The actual node data.
        /// </summary>
        [SerializeReference] public INode nodeData;

#if UNITY_EDITOR
        // FROM HERE BE DRAGONS...

        /// <summary>
        /// node position
        /// </summary>
        [SerializeField] private float nodeX, nodeY;

        /// <summary>
        /// Node name
        /// </summary>
        [SerializeField] private string name;

        public const string nameIdentifier = nameof(name);

        /// <summary>
        /// Is this node a utility node?
        /// </summary>
        public bool isUtilityNode = false;

        /// <summary>
        /// standard .isExpanded behavior does not work for us, especially if there are two views.
        /// So we need to manage the expanded states of our "groups"/foldouts ourselves.
        /// </summary>
        [Serializable]
        public class FoldoutState
        {
            public int relativePropertyPathHash;
            [NonSerialized] public bool used = false;
            public bool isExpanded = true;
        }

        /// <summary>
        /// Serialied list of all foldout states
        /// </summary>
        [SerializeField] private List<FoldoutState> foldouts = new();

        /// <summary>
        /// Dictionary is needed as we might have many foldouts based on how deep the class structure goes
        /// </summary>
        private Dictionary<int, FoldoutState> _foldoutsLookup = null;

        private Dictionary<int, FoldoutState> FoldoutsLookup
        {
            get
            {
                if (_foldoutsLookup == null)
                {
                    _foldoutsLookup = new Dictionary<int, FoldoutState>();
                    foreach (FoldoutState foldoutState in foldouts)
                        _foldoutsLookup.Add(foldoutState.relativePropertyPathHash, foldoutState);
                }

                return _foldoutsLookup;
            }
        }

        [NonSerialized] private bool _dataIsSet = false;
        [NonSerialized] private SerializedProperty _serializedProperty;
        [NonSerialized] private SerializedProperty _nodeDataSerializedProperty;
        [NonSerialized] private SerializedProperty _nameProperty;
        [NonSerialized] private SerializedProperty _nodeXProperty;
        [NonSerialized] private SerializedProperty _nodeYProperty;
        [NonSerialized] private static Dictionary<Type, NodeAttribute> _NodeInfo = new();
        [NonSerialized] public Type nodeType;
        [NonSerialized] public NodeAttribute nodeAttribute;

        public Node(INode nodeData)
        {
            this.nodeData = nodeData;
            Initialize();
        }

        /// <summary>
        /// Get a foldout state based on a hash or create it if not present
        /// </summary>
        /// <param name="pathHash"></param>
        /// <param name="defaultState"></param>
        /// <returns></returns>
        public FoldoutState GetOrCreateFoldout(int pathHash, bool defaultState = true)
        {
            if (!FoldoutsLookup.ContainsKey(pathHash))
            {
                //serializedProperty.serializedObject.Update();

                var foldoutState = new FoldoutState() {relativePropertyPathHash = pathHash, isExpanded = defaultState};
                FoldoutsLookup.Add(pathHash, foldoutState);
                foldouts.Add(foldoutState);

                //EditorUtility.SetDirty(serializedProperty.serializedObject.targetObject);
                //serializedProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }

            return FoldoutsLookup[pathHash];
        }

        /// <summary>
        /// Remove unused states so we dont pollute this if the class structure changes.
        /// </summary>
        public void CleanupFoldoutStates()
        {
            //serializedProperty.serializedObject.Update();

            for (int i = foldouts.Count - 1; i >= 0; i--)
            {
                FoldoutState state = foldouts[i];
                if (!state.used || state.relativePropertyPathHash == default) foldouts.RemoveAt(i);
            }

            //EditorUtility.SetDirty(serializedProperty.serializedObject.targetObject);
            //serializedProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        public Vector2 GetPosition()
        {
            return new Vector2(nodeX, nodeY);
        }

        public string GetName()
        {
            return name;
        }

        public static NodeAttribute GetNodeAttribute(Type type)
        {
            if (!_NodeInfo.ContainsKey(type))
                _NodeInfo.Add(type, Attribute.GetCustomAttribute(type, typeof(NodeAttribute)) as NodeAttribute);
            return _NodeInfo[type];
        }

        public void Initialize()
        {
            nodeType = nodeData.GetType();
            nodeAttribute = GetNodeAttribute(nodeType);
        }

        public string SetName(string name)
        {
            this.name = name;
            if (_dataIsSet) _nameProperty.serializedObject.Update();
            return name;
        }

        public void SetPosition(float positionX, float positionY)
        {
            if (_dataIsSet)
            {
                if (positionX != nodeX || positionY != nodeY)
                {
                    _nodeXProperty.floatValue = positionX;
                    _nodeYProperty.floatValue = positionY;
                    _nodeXProperty.serializedObject.ApplyModifiedProperties();
                }
            }
            else
            {
                nodeX = positionX;
                nodeY = positionY;
            }
        }

        public SerializedProperty GetSerializedProperty()
        {
            return _serializedProperty;
        }

        public SerializedProperty GetSpecificSerializedProperty()
        {
            return _nodeDataSerializedProperty;
        }

        public SerializedProperty GetNameSerializedProperty()
        {
            return _nameProperty;
        }

        public void SetData(SerializedProperty serializedProperty)
        {
            this._serializedProperty = serializedProperty;
            _nodeDataSerializedProperty = serializedProperty.FindPropertyRelative(nameof(nodeData));
            _nameProperty = serializedProperty.FindPropertyRelative(nameof(name));
            _nodeXProperty = serializedProperty.FindPropertyRelative(nameof(nodeX));
            _nodeYProperty = serializedProperty.FindPropertyRelative(nameof(nodeY));
            _dataIsSet = true;
        }
#endif
    }
}