using UnityEngine;

namespace SRPGDemo.Extensions
{
    static class ColorExt
    {
        public static Color Grayscale(float tint)
        {
            return Color.Lerp(Color.black, Color.white, tint);
        }
    }
}
