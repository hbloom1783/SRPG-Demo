using System;

namespace GridLib
{
    public static class NumUtil
    {
        public static ulong WrapPos(long value, ulong divisor)
        {
            long result = value;

            if (result < 0)
            {
                result *= (long)(divisor - 1);
                result *= -1;
            }

            return ((ulong)result % divisor);
        }

        public static uint WrapPos(int value, uint divisor)
        {
            return (uint)WrapPos((long)value, (ulong)divisor);
        }

        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        public static int Lerp(int a, int b, float t)
        {
            return (int)Math.Round(Lerp((float)a, (float)b, t));
        }
    }
}
