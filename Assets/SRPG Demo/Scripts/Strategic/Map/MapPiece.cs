using GridLib.Hex;
using SRPGDemo.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SRPGDemo.Strategic.Map
{
    public class MapPiece : ScriptableObject
    {
        private MapController map { get { return MapController.instance; } }

        public Dictionary<HexCoords, CellType> cells = new Dictionary<HexCoords, CellType>();

        public IEnumerable<HexCoords> TargetArea(HexCoords center, HexRotation pieceRot)
        {
            return cells.Keys
                .Select(x => x.Rotate(pieceRot))
                .Select(x => x + center);
        }

        public IEnumerable<MapCell> TargetArea(MapCell center, HexRotation pieceRot)
        {
            return TargetArea(center.loc, pieceRot).Select(map.CellAt);
        }

        private static IEnumerable<HexCoords> ContiguousPoints(int count, bool balance = true)
        {
            if (count == 0) return new List<HexCoords>();

            List<HexCoords> points = new List<HexCoords>();
            points.Add(HexCoords.O);

            for (int x = 1; x < count; x++) points.Add(points.Frontier().RandomPick());

            if (balance)
            {
                int maxX = points.Select(p => p.x).Max();
                int minX = points.Select(p => p.x).Min();

                int maxY = points.Select(p => p.y).Max();
                int minY = points.Select(p => p.y).Min();

                int maxZ = points.Select(p => p.z).Max();
                int minZ = points.Select(p => p.z).Min();

                HexCoords circumcenter = HexCoords.Round(
                    (minX + maxX) / 2,
                    (minY + maxY) / 2,
                    (minZ + maxZ) / 2);

                points = points.Select(p => p - circumcenter).ToList();
            }

            return points;
        }

        public static MapPiece Generate()
        {
            MapPiece result = CreateInstance<MapPiece>();

            List<HexCoords> points = ContiguousPoints(Random.Range(4, 6)).ToList();

            foreach(HexCoords point in ContiguousPoints(Random.Range(4, 6)))
            {
                result.cells[point] = CellType.greenPlain;
            }

            return result;
        }
    }
}
