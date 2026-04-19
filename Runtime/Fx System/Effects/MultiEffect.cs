using System;
using UnityEngine;

namespace Konfus.Fx_System.Effects
{
    [Serializable]
    public class MultiEffect : Effect
    {
        [SerializeField]
        private FxSystem? fxSystem;

        public override float Duration
        {
            get
            {
                if (fxSystem?.Items == null) return 0f;

                float sequentialCursor = 0f;
                float maxEnd = 0f;

                foreach (FxItem item in fxSystem.Items)
                {
                    Effect? nestedEffect = item?.Effect;
                    if (nestedEffect == null) continue;

                    float start = sequentialCursor;
                    float end = start + Mathf.Max(0f, nestedEffect.Duration);
                    maxEnd = Mathf.Max(maxEnd, end);

                    if (!nestedEffect.ShouldPlayAsync)
                        sequentialCursor = end;
                }

                return maxEnd;
            }
        }

        public FxSystem? FxSystem => fxSystem;

        public override void Play()
        {
            fxSystem?.PlayEffects();
        }

        public override void Pause()
        {
            fxSystem?.PauseEffects();
        }

        public override void Resume()
        {
            fxSystem?.PlayEffects();
        }

        public override void Reset()
        {
            fxSystem?.ResetEffects();
        }
    }
}
