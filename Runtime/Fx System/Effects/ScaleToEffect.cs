using System;
using DG.Tweening;
using UnityEngine;

namespace Konfus.Fx_System.Effects
{
    [Serializable]
    public class ScaleToEffect : ConfigurableDurationEffect
    {
        [SerializeField]
        private Vector3 to;

        [SerializeField]
        private Transform? target;

        [SerializeField]
        private Ease easing = Ease.Linear;

        private Tween? _tween;

        public override void Play()
        {
            if (!target)
            {
                Debug.LogWarning($"{nameof(ScaleToEffect)} requires a target GameObject");
                return;
            }

            _tween?.Kill();
            _tween = target.DOScale(to, Duration);
            _tween.SetRelative(false);
            _tween.SetEase(easing);
            _tween.SetAutoKill(false);
            if (!Application.isPlaying) _tween.SetUpdate(UpdateType.Manual);
            _tween.Restart();
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
