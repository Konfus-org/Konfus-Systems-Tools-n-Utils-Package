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
        private IEffect effect;
        public IEffect Effect => effect;
    }
}