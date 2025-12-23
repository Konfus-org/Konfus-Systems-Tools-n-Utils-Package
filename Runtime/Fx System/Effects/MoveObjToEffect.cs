using System;
using DG.Tweening;
using UnityEngine;

namespace Konfus.Fx_System.Effects
{
    [Serializable]
    public class MoveObjToEffect : ConfigurableDurationEffect
    {
        [SerializeField]
        private GameObject? objToMove;

        [SerializeField]
        private Vector3 position;

        [SerializeField]
        private Ease easing = Ease.Linear;

        private Tween? _tween;

        public override void Play()
        {
            if (!objToMove)
            {
                Debug.LogWarning($"{nameof(MoveObjToEffect)} requires a {nameof(GameObject)}");
                return;
            }

            Transform transform = objToMove.transform;
            _tween = transform.DOLocalMove(position, Duration);
            _tween.SetEase(easing);
            _tween.Play();
        }

        public override void Stop()
        {
            _tween?.Kill();
        }
    }
}