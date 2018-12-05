using GridLib.Hex;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SRPGDemo.Battle.Map
{
    [AddComponentMenu("SRPG Demo/Battle/Map Generators/Perlin Map Generator")]
    public class PerlinMapGenerator : HexGridInitializer<MapCell>
    {
        public float walkable = 0.6f;
        public float perlinScale = 10;

        public override void InitGrid(HexGridManager<MapCell> grid, bool isPlaying)
        {
            MapController map = (MapController)grid;
            if (isPlaying)
            {
                float xOffset = Random.Range(0.0f, 10000.0f);
                float yOffset = Random.Range(0.0f, 10000.0f);

                foreach (HexCoords point in map.coords)
                {
                    float perlinX = (map.GridToWorld(point).x * perlinScale) + xOffset;
                    float perlinY = (map.GridToWorld(point).y * perlinScale) + yOffset;

                    float value = Mathf.PerlinNoise(perlinX, perlinY);

                    MapCell mapCell = map[point];

                    if (value <= walkable)
                        mapCell.terrainType = TerrainType.open;
                    else
                        mapCell.terrainType = TerrainType.notWalkable;
                }
            }
        }
    }
}
