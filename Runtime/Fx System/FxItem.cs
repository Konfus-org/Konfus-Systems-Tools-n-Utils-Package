using System;
using Konfus.Fx_System.Effects;
using Sirenix.Serialization;

namespace Konfus.Fx_System
{
    [Serializable]
    public class FxItem
    {
        [OdinSerialize]
        private Effect? effect;

        public Effect? Effect => effect;
    }
}