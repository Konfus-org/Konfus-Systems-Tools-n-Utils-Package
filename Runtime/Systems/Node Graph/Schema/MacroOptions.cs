using System;
using UnityEngine;

namespace Konfus.Systems.Node_Graph.Schema
{
    [Serializable]
    public struct MacroOptions
    {
        [SerializeField] private string menuLocation;

        public string MenuLocation => menuLocation;
    }
}