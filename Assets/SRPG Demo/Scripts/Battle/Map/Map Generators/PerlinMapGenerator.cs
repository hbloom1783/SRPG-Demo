using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamelogic.Grids;
using SRPGDemo;
using SRPGDemo.Battle.Map;

namespace SRPGDemo.Battle.Map
{
    [AddComponentMenu("SRPG Demo/Battle/Map Generators/Perlin Map Generator")]
    public class PerlinMapGenerator : MapGenerator
    {
        public float walkable = 0.6f;
        public float perlinScale = 10;

        protected override void InnerGenerate()
        {
            float xOffset = Random.Range(0.0f, 10000.0f);
            float yOffset = Random.Range(0.0f, 10000.0f);

            foreach (PointyHexPoint point in map.mapGrid)
            {
                float perlinX = (Map[point].x * perlinScale) + xOffset;
                float perlinY = (Map[point].y * perlinScale) + yOffset;

                float value = Mathf.PerlinNoise(perlinX, perlinY);

                MapCell mapCell = map.mapGrid[point];

                if (value <= walkable)
                    mapCell.terrainType = TerrainType.open;
                else
                    mapCell.terrainType = TerrainType.notWalkable;
            }
        }
    }
}
