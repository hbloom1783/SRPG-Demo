using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gamelogic.Grids;
using SRPGDemo.Map;

namespace SRPGDemo.Extensions
{
    public static class Map3DExt
    {
        public static List<PointyHexPoint> GetArc(
            this IMap3D<PointyHexPoint> map,
            PointyHexPoint origin,
            Facing startAngle,
            Facing endAngle,
            int minRadius,
            int maxRadius)
        {
            List<PointyHexPoint> result = new List<PointyHexPoint>();

            for (int radius = minRadius; radius <= maxRadius; radius++)
            {
                PointyHexPoint cursor = origin + (startAngle.Offset() * radius);
                PointyHexPoint finish = origin + (endAngle.Offset() * radius);
                Facing facing = startAngle.CW(2);

                while (cursor != finish)
                {
                    for (int step = 0; step < radius; step++)
                    {
                        result.Add(cursor);
                        cursor += facing.Offset();
                    }

                    facing = facing.CW();
                }

                result.Add(cursor);
            }

            return result;
        }

        public static List<PointyHexPoint> GetCircle(
            this IMap3D<PointyHexPoint> map,
            PointyHexPoint origin,
            int minRadius,
            int maxRadius)
        {
            List<PointyHexPoint> result = new List<PointyHexPoint>();

            for (int radius = minRadius; radius <= maxRadius; radius++)
            {
                if (radius == 0)
                {
                    result.Add(origin);
                }
                else
                {
                    PointyHexPoint cursor = origin + (Facing.e.Offset() * radius);
                    Facing facing = Facing.sw;

                    for (int side = 0; side < 6; side++)
                    {
                        for (int step = 0; step < radius; step++)
                        {
                            result.Add(cursor);
                            cursor += facing.Offset();
                        }

                        facing = facing.CW();
                    }
                }
            }

            return result;
        }
    }
}
