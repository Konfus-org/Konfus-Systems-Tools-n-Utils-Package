using System;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace Konfus.Systems.FX
{
    public class FxSystem : SerializedMonoBehaviour
    {
        [OdinSerialize]
        private IEffect[] effects;

        public void PlayEffects()
        {
            foreach (IEffect effect in effects)
            {
                effect.Play();
                float playTime = effect.GetPlayTime();
                Task.Delay(TimeSpan.FromSeconds(playTime)).Wait();
            }
        }

        private void Start()
        {
            foreach (IEffect effect in effects)
            {
                effect.Initialize(gameObject);
            }
        }
    }
}