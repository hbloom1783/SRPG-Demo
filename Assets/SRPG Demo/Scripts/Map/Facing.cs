using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gamelogic.Grids;

namespace SRPGDemo.Map
{
    public enum Facing
    {
        ne,
        e,
        se,
        sw,
        w,
        nw,
    }

    public static class FacingMethods
    {
        public static Facing CW(this Facing input)
        {
            if (input == Facing.nw) return Facing.ne;
            else return input + 1;
        }

        public static Facing CCW(this Facing input)
        {
            if (input == Facing.ne) return Facing.nw;
            else return input - 1;
        }

        public static Facing CW(this Facing input, uint steps)
        {
            Facing output = input;

            for (int idx = 0; idx < steps; idx++)
            {
                output = output.CW();
            }

            return output;
        }

        public static Facing CCW(this Facing input, uint steps)
        {
            Facing output = input;

            for (int idx = 0; idx < steps; idx++)
            {
                output = output.CCW();
            }

            return output;
        }

        public static PointyHexPoint Offset(this Facing input)
        {
            switch (input)
            {
                case Facing.ne: return PointyHexPoint.NorthEast;
                case Facing.e: return PointyHexPoint.East;
                case Facing.se: return PointyHexPoint.SouthEast;
                case Facing.sw: return PointyHexPoint.SouthWest;
                case Facing.w: return PointyHexPoint.West;
                case Facing.nw: return PointyHexPoint.NorthWest;
                default: return PointyHexPoint.Zero;
            }
        }
    }

    public class FacingArray<T>
    {
        public T northEast;
        public T east;
        public T southEast;
        public T southWest;
        public T west;
        public T northWest;

        public T this[Facing index]
        {
            get
            {
                switch (index)
                {
                    case Facing.ne: return northEast;
                    case Facing.e: return east;
                    case Facing.se: return southEast;
                    case Facing.sw: return southWest;
                    case Facing.w: return west;
                    case Facing.nw: return northWest;
                    default: return default(T);
                }
            }

            set
            {
                switch (index)
                {
                    case Facing.ne: northEast = value; break;
                    case Facing.e: east = value; break;
                    case Facing.se: southEast = value; break;
                    case Facing.sw: southWest = value; break;
                    case Facing.w: west = value; break;
                    case Facing.nw: northWest = value; break;
                }
            }
        }
    }
}
