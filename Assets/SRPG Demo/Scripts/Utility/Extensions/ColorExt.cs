using UnityEngine;

namespace SRPGDemo.Extensions
{
    static class ColorExt
    {
        public static Color Grayscale(float tint)
        {
            return Color.Lerp(Color.black, Color.white, tint);
        }

        public static Color Mix(this Color color, Color other, float ratio = 0.5f)
        {
            return Color.Lerp(color, other, ratio);
        }

        public static Color Alpha(this Color color, float a)
        {
            return new Color(color.r, color.g, color.b, a);
        }
    }
}
