using UnityEngine;
using GridLib.Hex;

namespace SRPGDemo.Battle.Map
{
    [AddComponentMenu("SRPG Demo/Battle/Map Generators/Generate Big Hex")]
    public class GenerateBigHex : HexGridInitializer<MapCell>
    {
        public MapCell cellPrefab = null;
        public uint radius = 3;

        public override void InitGrid(HexGridManager<MapCell> grid, bool isPlaying)
        {
            MapController map = (MapController)grid;

            foreach (HexCoords loc in HexCoords.O.CompoundRing(0, radius))
            {
                map.InitCell(loc, Instantiate(cellPrefab));
            }
        }
    }
}
