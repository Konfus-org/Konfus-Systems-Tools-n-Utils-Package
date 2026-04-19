using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Konfus.Fx_System.Effects
{
    [Serializable]
    public class ImageFillEffect : ConfigurableDurationEffect
    {
        [SerializeField]
        [Range(0, 1)]
        private float startingFillAmount;

        [SerializeField]
        [Range(0, 1)]
        private float targetFillAmount = 1;

        [SerializeField]
        private Ease easing = Ease.Linear;

        [SerializeField]
        private Origin origin;

        [SerializeField]
        private Color color;

        [SerializeField]
        private Image? image;

        private Tween? _tween;

        public override void Initialize(GameObject parentGo)
        {
            if (!image)
            {
                Debug.LogWarning($"{nameof(ImageFillEffect)} requires an image component.");
                return;
            }

            image.color = color;
            image.fillOrigin = (int)origin;
            image.fillMethod = Image.FillMethod.Horizontal;
            image.fillAmount = startingFillAmount;
            image.enabled = true;
        }

        public override void Play()
        {
            _tween?.Kill();
            _tween = DOTween.To(
                () => image ? image.fillAmount : targetFillAmount,
                newFillAmount =>
                {
                    if (image) image.fillAmount = newFillAmount;
                },
                targetFillAmount,
                Duration);
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

        public override void OnValidate()
        {
            if (image == null) return;
            image.color = color;
            image.fillOrigin = (int)origin;
            image.fillMethod = Image.FillMethod.Horizontal;
            image.fillAmount = startingFillAmount;
        }

        private enum Origin
        {
            Left = 0,
            Right = 1
        }
    }
}
