using System;
using DG.Tweening;
using Konfus.Systems.Fx_System.Effects;
using UnityEngine;

namespace Armored_Felines.Effects
{
    [Serializable]
    public class RotateToEffect: ConfigurableDurationEffect
    {
        [SerializeField] private Vector3 to;
        [SerializeField] private Transform target;
        [SerializeField] private Ease easing = Ease.Linear;
        
        private Tween _tween;

        public override void Play()
        {
            _tween = target.DOLocalRotate(to, Duration);
            _tween.SetRelative(false);
            _tween.SetEase(easing);
            _tween.Restart();
        }

        public override void Stop()
        {
            _tween.Kill();
        }
    }
}