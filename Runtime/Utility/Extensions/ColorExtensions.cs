using UnityEngine;

namespace Konfus.Utility.Extensions
{
    public static class ColorExtensions
    {
        public static Color Invert(this Color colorToInvert)
        {
            const int RGB_MAX = 1;
            return new Color(RGB_MAX - colorToInvert.r, RGB_MAX - colorToInvert.g, RGB_MAX - colorToInvert.b, colorToInvert.a);
        }
    }
}