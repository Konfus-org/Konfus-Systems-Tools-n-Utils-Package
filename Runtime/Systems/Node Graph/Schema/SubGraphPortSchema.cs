using System.Collections.Generic;
using UnityEngine;

namespace Konfus.Systems.Node_Graph.Schema
{
    [CreateAssetMenu(fileName = "SubGraphPortSchema", menuName = "NGP/Schema/SubGraphPortSchema", order = 0)]
    public class SubGraphPortSchema : ScriptableObject
    {
        public const string EgressPortDataFieldName = nameof(egressPortData);
        public const string IngressPortDataFieldName = nameof(ingressPortData);

        [SerializeField] public List<PortData> ingressPortData;

        [SerializeField] public List<PortData> egressPortData;

        public event Notify OnPortsUpdated;

        public void AddUpdatePortsListener(Notify listener)
        {
            OnPortsUpdated += listener;
        }

        public void NotifyPortsChanged()
        {
            OnPortsUpdated?.Invoke();
        }

        public void RemoveUpdatePortsListener(Notify listener)
        {
            OnPortsUpdated -= listener;
        }
    }
}