﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SRPGDemo;
using SRPGDemo.Map;
using Gamelogic.Grids;

namespace SRPGDemo.Map
{
    [AddComponentMenu("SRPG/Map Generators/Perlin Map Generator")]
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
                    mapCell.walkable = true;
                else
                    mapCell.walkable = false;
            }
        }
    }
}
