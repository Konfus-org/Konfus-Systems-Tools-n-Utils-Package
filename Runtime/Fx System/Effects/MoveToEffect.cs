using System;
using DG.Tweening;
using UnityEngine;

namespace Konfus.Fx_System.Effects
{
    [Serializable]
    public class MoveObjToEffect : ConfigurableDurationEffect
    {
        [SerializeField]
        private Transform? target;

        [SerializeField]
        private Transform? to;

        [SerializeField]
        private Ease easing = Ease.Linear;

        private Tween? _tween;

        public override void Play()
        {
            if (!target || !to)
            {
                Debug.LogWarning($"{nameof(RotateToEffect)} requires a target GameObject and to GameObject!");
                return;
            }

            Transform transform = target;
            _tween?.Kill();
            _tween = transform.DOLocalMove(to.localPosition, Duration);
            _tween.SetEase(easing);
            _tween.SetAutoKill(false);
            _tween.SetRelative(false);
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
