using System;

namespace Konfus.Systems.FX
{
    [Serializable]
    public class NoEffect : Effect
    {
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