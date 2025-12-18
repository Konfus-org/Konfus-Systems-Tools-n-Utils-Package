using System;
using DG.Tweening;
using Konfus.Systems.Fx_System.Effects;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Armored_Felines.Effects
{
    [Serializable]
    public class ImageFillEffect: ConfigurableDurationEffect
    {
        [SerializeField, Range(0, 1)] private float startingFillAmount = 0;
        [SerializeField, Range(0, 1)] private float targetFillAmount = 1;
        [SerializeField] private Ease easing = Ease.Linear;
        [SerializeField] private Origin origin;
        [SerializeField] private Color color;
        [SerializeField] private Image image;
        
        private Tween _tween;

        public override void Initialize(GameObject parentGo)
        {
            image.color = color;
            image.fillOrigin = (int)origin;
            image.fillMethod = Image.FillMethod.Horizontal;
            image.fillAmount = startingFillAmount;
            image.enabled = true;
        }

        public override void Play()
        {
            _tween = DOTween.To(
                getter: () => image ? image.fillAmount : targetFillAmount,
                setter: newFillAmount =>
                {
                    if (image) image.fillAmount = newFillAmount;
                },
                endValue: targetFillAmount,
                duration: Duration);
            _tween.SetEase(easing);
            _tween.Play();
        }

        public override void Stop()
        {
            if (_tween == null) return;
            _tween.timeScale = 20;
            _tween.SmoothRewind();
            _tween.timeScale = 1;
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
            Right = 1,
        }
    }
}