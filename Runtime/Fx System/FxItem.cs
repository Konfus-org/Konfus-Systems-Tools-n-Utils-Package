using System;
using Konfus.Fx_System.Effects;
using Sirenix.Serialization;
using UnityEngine;

namespace Konfus.Fx_System
{
    [Serializable]
    public class FxItem
    {
        [SerializeField]
        private string effectType = "";

        [OdinSerialize]
        private Effect? effect;

        public Effect? Effect => effect;
    }
}