using System;
using System.Collections.Generic;
using Konfus.Systems.Node_Graph.Schema;
using UnityEngine;

namespace Konfus.Systems.Node_Graph
{
    [Serializable]
    public class SubGraph : Graph
    {
        public const string EgressPortDataFieldName = nameof(egressPortData);
        public const string IngressPortDataFieldName = nameof(ingressPortData);
        public const string IsMacroFieldName = nameof(isMacro);
        public const string MacroOptionsFieldName = nameof(macroOptions);
        public const string OptionsFieldName = nameof(options);
        public const string SchemaFieldName = nameof(schema);


        [NonSerialized] private SubGraphEgressNode _egressNode;

        [NonSerialized] private SubGraphIngressNode _ingressNode;

        [SerializeField] private SubGraphOptions options;

        [SerializeField] private bool isMacro;

        [SerializeField] private MacroOptions macroOptions;

        [SerializeField] private List<PortData> ingressPortData = new();

        [SerializeField] private List<PortData> egressPortData = new();

        [SerializeField] private SubGraphPortSchema schema;

        public event Notify OnOptionsUpdated;

        public event Notify OnPortsUpdated;

        public SubGraphEgressNode EgressNode =>
            PropertyUtils.LazyLoad(ref _egressNode, () => GraphUtils.FindNodeInGraphOfType<SubGraphEgressNode>(this));

        public List<PortData> EgressPortData
        {
            get
            {
                var portData = new List<PortData>();
                if (schema) portData.AddRange(schema.egressPortData);
                portData.AddRange(egressPortData);
                return portData;
            }
        }

        public SubGraphIngressNode IngressNode =>
            PropertyUtils.LazyLoad(ref _ingressNode, () => GraphUtils.FindNodeInGraphOfType<SubGraphIngressNode>(this));

        public List<PortData> IngressPortData
        {
            get
            {
                var portData = new List<PortData>();
                if (schema) portData.AddRange(schema.ingressPortData);
                portData.AddRange(ingressPortData);
                return portData;
            }
        }


        public bool IsMacro => isMacro;
        public MacroOptions MacroOptions => macroOptions;
        public SubGraphOptions Options => options;
        public SubGraphPortSchema Schema => schema;

        public override void Initialize()
        {
            base.Initialize();

            if (IngressNode == null || !nodesPerGUID.ContainsKey(IngressNode.GUID))
            {
                if (IngressNode != null)
                    RemoveNode(IngressNode);

                _ingressNode = Node.CreateFromType<SubGraphIngressNode>(Vector2.zero);
                AddNode(_ingressNode);
            }

            if (EgressNode == null || !nodesPerGUID.ContainsKey(EgressNode.GUID))
            {
                if (EgressNode != null)
                    RemoveNode(EgressNode);


                _egressNode = Node.CreateFromType<SubGraphEgressNode>(Vector2.zero);
                AddNode(_egressNode);
            }
        }

        public void AddIngressPort(PortData portData)
        {
            ingressPortData.Add(portData);
        }

        public void AddOptionsListener(Notify listener)
        {
            OnOptionsUpdated += listener;
        }

        public void AddReturnPort(PortData portData)
        {
            egressPortData.Add(portData);
        }

        public void AddUpdatePortsListener(Notify listener)
        {
            OnPortsUpdated += listener;
        }

        public void NotifyOptionsChanged()
        {
            OnOptionsUpdated?.Invoke();
        }

        public void NotifyPortsChanged()
        {
            OnPortsUpdated?.Invoke();
            DestroyBrokenGraphElements();
        }

        public void RemoveOptionsListener(Notify listener)
        {
            OnOptionsUpdated -= listener;
        }

        public void RemoveUpdatePortsListener(Notify listener)
        {
            OnPortsUpdated -= listener;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            schema?.AddUpdatePortsListener(NotifyPortsChanged);
        }
    }
}