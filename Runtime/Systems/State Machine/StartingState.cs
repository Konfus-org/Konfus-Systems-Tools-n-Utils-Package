using System;
using Konfus.Systems.Graph;
using Konfus.Systems.Graph.Attributes;
using UnityEngine;

namespace Konfus.Systems.State_Machine
{
    [Serializable, Node("#008225", nodeName: "Start", createInputPort = false)]
    public class StartingState : INode
    {
        [Port("To"), SerializeReference]
        public State startAt;
    }
}