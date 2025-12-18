using System;
using DG.Tweening;
using UnityEngine;

namespace Konfus.Fx_System.Effects
{
    [Serializable]
    public class SpinEffect: ConfigurableDurationEffect
    {
        [SerializeField] private Ease easing = Ease.Linear;
        
        private Transform _transform;
        private Tween _tween;

        public override void Initialize(GameObject parentGo)
        {
            _transform = parentGo.transform;
            base.Initialize(parentGo);
        }

        public override void Play()
        {
            _tween = _transform.DOLocalRotate(new Vector3(0, 360, 0), Duration, RotateMode.FastBeyond360);
            _tween.SetRelative(true);
            _tween.SetEase(easing);
            _tween.Play();
        }

        public override void Stop()
        {
            _tween.Kill();
        }
    }
}