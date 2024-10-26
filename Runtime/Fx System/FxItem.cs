using System;
using Konfus.Systems.Fx_System.Effects;
using UnityEngine;

namespace Konfus.Systems.Fx_System
{
    [Serializable]
    public class FxItem
    {
        [SerializeField]
        private string effectType;
        [SerializeReference]
        private Effect effect;
        public Effect Effect => effect;
    }
}