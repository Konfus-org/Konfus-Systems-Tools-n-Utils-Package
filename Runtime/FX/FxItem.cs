using System;
using UnityEngine;

namespace Konfus.Systems.FX
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