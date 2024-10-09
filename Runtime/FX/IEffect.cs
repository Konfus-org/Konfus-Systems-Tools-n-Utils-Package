using UnityEngine;

namespace Konfus.Systems.FX
{
    public interface IEffect
    {
        float GetPlayTime();
        void Initialize(GameObject parentGo);
        void Play();
    }
}