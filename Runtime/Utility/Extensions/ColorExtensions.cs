using UnityEngine;

namespace Konfus.Utility.Extensions
{
    public static class ColorExtensions
    {
        public static Color Invert(this Color colorToInvert)
        {
            const int rgbMax = 1;
            return new Color(rgbMax - colorToInvert.r, rgbMax - colorToInvert.g, rgbMax - colorToInvert.b,
                colorToInvert.a);
        }
    }
}