using System.Linq;
using GridLib.Hex;
using UnityEngine;

namespace SRPGDemo.Strategic.Map
{
    class CrashSiteSpawner : HexGridInitializer<MapCell>
    {
        public MapUnit unitPrefab = null;

        public override void InitGrid(HexGridManager<MapCell> grid, bool isPlaying)
        {
            MapController map = (MapController)grid;

            if (isPlaying)
            {
                MapCell crashSite = map.InitCell(HexCoords.O);
                crashSite.type = CellType.moonCrashSite;
                map.PlaceUnit(Instantiate(unitPrefab), crashSite);
                
                foreach (HexCoords loc in crashSite.loc.neighbors)
                    map.InitCell(loc).type = CellType.moonPlain;

                foreach (HexCoords loc in crashSite.loc.CompoundRing(2, 10).Where(x => !map.InBounds(x)))
                    map.InitCell(loc);
            }
        }
    }
}
