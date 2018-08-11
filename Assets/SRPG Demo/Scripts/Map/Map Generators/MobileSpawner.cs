using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Gamelogic.Grids;
using SRPGDemo.Extensions;
using Random = UnityEngine.Random;

namespace SRPGDemo.Map
{
    [AddComponentMenu("SRPG/Map Generators/Mobile Spawner")]
    public class MobileSpawner : MapGenerator
    {
        public MapMobile mobilePrefab = null;
        public int count = 1;

        protected override void InnerGenerate()
        {
            if (mobilePrefab != null)
            {
                Debug.Log("Spawning " + count + " " + mobilePrefab.name);
                for (int idx = 0; idx < count; idx++)
                {
                    IGrid<PassageNode, PointyHexPoint> passageMap = map.PassageMap(mobilePrefab);
                    PointyHexPoint spawnLoc = map.mapGrid.ToPointList()
                        .Where(x => mobilePrefab.CanStayIn(passageMap[x]))
                        .RandomPick();

                    MapMobile newMobile = Instantiate(mobilePrefab);

                    if (newMobile is LargeMobile)
                        ((LargeMobile)newMobile).facing = Facing.e.CW((uint)Random.Range(0, 5));

                    map.PlaceMobile(newMobile, spawnLoc);
                }
            }
        }
    }
}
