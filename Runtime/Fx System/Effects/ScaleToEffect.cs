using System;
using DG.Tweening;
using UnityEngine;

namespace Konfus.Fx_System.Effects
{
    [Serializable]
    public class ScaleToEffect: ConfigurableDurationEffect
    {
        [SerializeField] private Vector3 to;
        [SerializeField] private Transform target;
        [SerializeField] private Ease easing = Ease.Linear;
        
        private Tween _tween;

        public override void Play()
        {
            _tween = target.DOScale(to, Duration);
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