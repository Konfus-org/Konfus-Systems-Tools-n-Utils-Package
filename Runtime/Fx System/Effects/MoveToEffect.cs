using System;
using DG.Tweening;
using UnityEngine;

namespace Konfus.Fx_System.Effects
{
    [Serializable]
    public class MoveToEffect : ConfigurableDurationEffect
    {
        [SerializeField]
        private Vector3 position;

        [SerializeField]
        private Ease easing = Ease.Linear;

        private Transform? _transform;
        private Tween? _tween;

        public override void Initialize(GameObject parentGo)
        {
            _transform = parentGo.transform;
            base.Initialize(parentGo);
        }

        public override void Play()
        {
            if (!_transform)
            {
                Debug.LogWarning($"{nameof(MoveToEffect)} requires an transform component.");
                return;
            }

            _tween?.Kill();
            _tween = _transform.DOLocalMove(position, Duration);
            _tween.SetEase(easing);
            _tween.SetAutoKill(false);
            if (!Application.isPlaying) _tween.SetUpdate(UpdateType.Manual);
            _tween.Play();
        }

        public override void Pause()
        {
            if (_tween == null) return;
            _tween.Pause();
        }

        public override void Resume()
        {
            if (_tween == null) return;
            _tween.Play();
        }

        public override void Reset()
        {
            if (_tween == null) return;
            _tween.Rewind();
            _tween.Kill();
            _tween = null;
        }
    }
}
