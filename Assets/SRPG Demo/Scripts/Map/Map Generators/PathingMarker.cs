using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamelogic.Grids;
using System;
using SRPGDemo.Extensions;

namespace SRPGDemo.Map
{
    [AddComponentMenu("SRPG/Map Generators/Pathing Marker")]
    public class PathingMarker : MapGenerator
    {
        private MapMobile playerMobile = null;

        private void ResetMap()
        {
            foreach (PointyHexPoint point in Controllers.map.mapGrid)
            {
                map.CellAt(point).ClearTint();
            }

            playerMobile.PathFlood().ForEach(node =>
            {
                switch (node.Value.passage)
                {
                    case PassageType.open:
                        map.mapGrid[node.Key].SetTint(
                            Color.white,
                            ColorExt.Grayscale(0.5f),
                            0.25f);
                        break;
                    case PassageType.canPass:
                        map.mapGrid[node.Key].SetTint(Color.green);
                        break;
                    case PassageType.inThreat:
                        map.mapGrid[node.Key].SetTint(Color.yellow);
                        break;
                    case PassageType.blocked:
                        map.mapGrid[node.Key].SetTint(Color.red);
                        break;
                }
            });
        }

        protected override void InnerGenerate()
        {
            playerMobile = map.Mobiles().Where(x => x.team == MobileTeam.player).RandomPick();
            Debug.Log(playerMobile);
            
            ResetMap();

            Controllers.game.hexTouchedEvent += mouseLoc =>
            {
                ResetMap();

                var pathFlood = playerMobile.PathFlood();

                if (pathFlood.ContainsKey(mouseLoc))
                {
                    map[mouseLoc].SetTint(Color.blue);

                    pathFlood[mouseLoc].PathBack().ForEach(x =>
                    {
                        map[x.loc].SetTint(Color.blue);
                    });
                }
            };
        }

        PointyHexPoint mouseLoc;

        /*void Update()
        {
            if (map.MousePosition != mouseLoc)
            {
                mouseLoc = map.MousePosition;
                
                ResetMap();

                var pathFlood = playerMobile.PathFlood();

                if (pathFlood.ContainsKey(mouseLoc))
                {
                    map[mouseLoc].SetTint(Color.blue);

                    pathFlood[mouseLoc].PathBack().ForEach(x =>
                    {
                        map[x.loc].SetTint(Color.blue);
                    });
                }
            }
        }*/
    }
}
