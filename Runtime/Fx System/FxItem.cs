using System;
using Konfus.Fx_System.Effects;
using UnityEngine;

namespace Konfus.Fx_System
{
    [Serializable]
    public class FxItem
    {
        [SerializeReference]
        private Effect? effect;

        public string effectType = string.Empty;

        public Effect? Effect => effect;
    }
}