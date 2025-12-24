using UnityEngine;

namespace Konfus.Editor.Sensor_Toolkit
{
    internal static class SensorColors
    {
        internal static readonly Color NoHitColor = new(Color.red.r, Color.red.g, Color.red.b, 0.5f);
        internal static readonly Color HitColor = new(Color.green.r, Color.green.g, Color.green.b, 0.5f);
    }
}