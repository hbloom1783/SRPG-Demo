using Gamelogic.Grids;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SRPGDemo.Extensions
{
    public static class FlatHexPointExt
    {
        public static IEnumerable<FlatHexPoint> GetNeighbors(this FlatHexPoint point)
        {
            yield return point + FlatHexPoint.North;
            yield return point + FlatHexPoint.NorthEast;
            yield return point + FlatHexPoint.NorthWest;

            yield return point + FlatHexPoint.South;
            yield return point + FlatHexPoint.SouthEast;
            yield return point + FlatHexPoint.SouthWest;
        }
    }
}
