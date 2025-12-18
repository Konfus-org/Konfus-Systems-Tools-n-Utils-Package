using System;
using System.Linq;
using Konfus.Systems.Fx_System.Effects;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Armored_Felines.Effects
{
    [Serializable]
    public class SetDecalsOpacity: Effect
    {
        [SerializeField] private DecalProjector[] projectors;
        [SerializeField] private float opacity;

        public override float Duration => 0;

        public override void Play()
        {
            foreach (DecalProjector projector in projectors)
            {
                projector.fadeFactor = opacity;
            }
        }

        public override void Stop()
        {
        }
    }
}