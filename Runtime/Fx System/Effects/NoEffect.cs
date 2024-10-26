using System;

namespace Konfus.Systems.Fx_System.Effects
{
    [Serializable]
    public class NoEffect : Effect
    {
        public override float Duration => 0;

        public override void Play()
        {
            // Do nothing
        }

        public override void Stop()
        {
            // do nothing...
        }
    }
}