using System;
using System.Linq;
using UnityEngine;

namespace Konfus.Systems.Fx_System.Effects
{
    [Serializable]
    public class MultiEffect : Effect
    {
        [SerializeField]
        private FxSystem fxSystem;

        public override float Duration => fxSystem?.Items?.Sum(item => item?.Effect?.Duration) ?? 0;
        public FxSystem FxSystem => fxSystem;

        public override void Play()
        {
            fxSystem.PlayEffects();
        }

        public override void Stop()
        {
            fxSystem.StopEffects();
        }
    }
}