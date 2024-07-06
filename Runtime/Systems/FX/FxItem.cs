using System;
using UnityEngine;

namespace Konfus.Systems.FX
{
    [Serializable]
    public class FxItem
    {
        [SerializeField]
        private int effectType = 0;
        [SerializeReference]
        private Effect effect;
        public Effect Effect => effect;
    }
}