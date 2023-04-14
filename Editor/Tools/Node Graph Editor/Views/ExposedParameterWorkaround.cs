using System;
using System.Collections.Generic;
using Konfus.Systems.Node_Graph;
using UnityEngine;

namespace Konfus.Tools.NodeGraphEditor
{
    [Serializable]
    public class ExposedParameterWorkaround : ScriptableObject
    {
        [SerializeReference] public List<ExposedParameter> parameters = new();

        public Graph graph;
    }
}