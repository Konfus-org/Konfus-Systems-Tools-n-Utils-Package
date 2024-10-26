using System;
using UnityEngine;

namespace Konfus.Systems.Fx_System.Effects
{
    [Serializable]
    public abstract class ConfigurableDurationEffect : Effect
    {
        [SerializeField, Range(0f, 60f), Tooltip("The time to play the effect in seconds before playing the next effect.")]
        private float duration;
        public override float Duration => duration;
    }
}