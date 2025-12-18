using System;
using DG.Tweening;
using Konfus.Systems.Fx_System.Effects;
using UnityEngine;

namespace Armored_Felines.Effects
{
    [Serializable]
    public class MoveObjToEffect: ConfigurableDurationEffect
    {
        [SerializeField] private GameObject objToMove;
        [SerializeField] private Vector3 position;
        [SerializeField] private Ease easing = Ease.Linear;
        
        private Tween _tween;

        public override void Play()
        {
            var transform = objToMove.transform;
            _tween = transform.DOLocalMove(position, Duration);
            _tween.SetEase(easing);
            _tween.Play();
        }

        public override void Stop()
        {
            _tween.Kill();
        }
    }
}