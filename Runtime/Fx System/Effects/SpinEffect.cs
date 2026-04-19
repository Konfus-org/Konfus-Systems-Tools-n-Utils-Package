using System;
using DG.Tweening;
using UnityEngine;

namespace Konfus.Fx_System.Effects
{
    [Serializable]
    public class SpinEffect : ConfigurableDurationEffect
    {
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
                Debug.LogWarning($"{nameof(SetGameObjectActiveEffect)} requires a transform.");
                return;
            }

            _tween?.Kill();
            _tween = _transform.DOLocalRotate(new Vector3(0, 360, 0), Duration, RotateMode.FastBeyond360);
            _tween.SetRelative(true);
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
