using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Gamelogic.Grids;
using SRPGDemo.Extensions;
using Random = UnityEngine.Random;

namespace SRPGDemo.Battle.Map
{
    [AddComponentMenu("SRPG Demo/Battle/Map Generators/Unit Spawner")]
    public class UnitSpawner : MapGenerator
    {
        public MapUnit unitPrefab = null;
        public MapUnitRecipe unitRecipe = null;
        public UnitTeam team;
        public int count = 1;

        protected override void InnerGenerate()
        {
            if ((unitPrefab != null) && (unitRecipe != null))
            {
                Debug.Log("Spawning " + count + " " + unitPrefab.name);
                for (int idx = 0; idx < count; idx++)
                {
                    MapUnit newUnit = Instantiate(unitPrefab);
                    newUnit.facing = Facing.e.CW((uint)Random.Range(0, 5));
                    newUnit.team = team;
                    newUnit.LoadRecipe(unitRecipe);

                    PointyHexPoint spawnLoc = map.mapGrid.ToPointList()
                        .Where(x => unitPrefab.CanStayIn(map.CellAt(x)))
                        .RandomPick();

                    map.PlaceUnit(newUnit, spawnLoc);
                }
            }
        }
    }
}
