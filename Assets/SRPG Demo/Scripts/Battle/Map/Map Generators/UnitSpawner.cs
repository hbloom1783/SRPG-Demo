using System.Linq;
using UnityEngine;
using GridLib.Hex;
using SRPGDemo.Extensions;
using SRPGDemo.Battle.Map.Pathing;

namespace SRPGDemo.Battle.Map
{
    [AddComponentMenu("SRPG Demo/Battle/Map Generators/Unit Spawner")]
    public class UnitSpawner : HexGridInitializer<MapCell>
    {
        public MapUnit unitPrefab = null;
        public MapUnitRecipe unitRecipe = null;
        public UnitTeam team;
        public int count = 1;

        public override void InitGrid(HexGridManager<MapCell> grid, bool isPlaying)
        {
            MapController map = (MapController)grid;
            
            if (isPlaying && (unitPrefab != null) && (unitRecipe != null))
            {
                Debug.Log("Spawning " + count + " " + unitPrefab.name + " (" + unitRecipe.name + ")");
                for (int idx = 0; idx < count; idx++)
                {
                    MapUnit newUnit = Instantiate(unitPrefab);
                    newUnit.facing = HexFacingMethods.random;
                    newUnit.team = team;
                    newUnit.LoadRecipe(unitRecipe);

                    HexCoords spawnLoc = map.coords
                        .Where(unitPrefab.CanEnter)
                        .Where(unitPrefab.CanStay)
                        .RandomPick();
                    
                    map.PlaceUnit(newUnit, spawnLoc);
                    foreach (MapCell cell in newUnit.GetThreatArea().Select(map.CellAt))
                        cell.AddThreat(newUnit);
                }
            }
        }
    }
}
