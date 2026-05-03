using System;
using DG.Tweening;
using UnityEngine;

namespace Konfus.Fx_System.Effects
{
    [Serializable]
    public class RotateToEffect : ConfigurableDurationEffect
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

            target.DOKill(false);
            _tween = null;
            _tween = target.DOLocalRotate(to.localEulerAngles, Duration, RotateMode.Fast);
            _tween.SetRelative(false);
            _tween.SetEase(easing);
            _tween.SetAutoKill(false);
            if (!Application.isPlaying) _tween.SetUpdate(UpdateType.Manual);
            _tween.Restart();
        }

        public override void Pause()
        {
            if (!HasActiveTween()) return;
            _tween.Pause();
        }

        public override void Resume()
        {
            if (!HasActiveTween()) return;
            _tween.Play();
        }

        public override void Reset()
        {
            if (!HasActiveTween()) return;
            _tween.Rewind();
            _tween.Kill();
            _tween = null;
        }

        private bool HasActiveTween()
        {
            if (_tween == null)
            {
                return false;
            }

            if (_tween.IsActive())
            {
                return true;
            }

            _tween = null;
            return false;
        }
    }
}
