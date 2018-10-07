using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using Gamelogic.Grids;
using Gamelogic.Extensions;
using SRPGDemo.Extensions;

namespace SRPGDemo.Battle.Map
{
    public static class MapHighlight
    {
        public static void ClearMap()
        {
            foreach (PointyHexPoint point in Controllers.map.cache.mapGrid)
            {
                Controllers.map.cache.CellAt(point).ClearTint();
            }
        }

        public static void Shimmer(PointyHexPoint loc)
        {
            Controllers.map.cache.CellAt(loc).SetTint(
                Color.white,
                ColorExt.Grayscale(0.5f),
                0.25f);
        }

        public static void Shimmer(PointyHexPoint loc, Color color)
        {
            Controllers.map.cache.CellAt(loc).SetTint(
                color,
                color.Mix(Color.black),
                0.25f);
        }

        public static void TintRange(IEnumerable<PointyHexPoint> locs, Color color)
        {
            locs.Select(Controllers.map.cache.CellAt).ForEach(x => x.SetTint(color));
        }

        public static void ShimmerRange(IEnumerable<PointyHexPoint> locs)
        {
            locs.ForEach(x => Shimmer(x));
        }

        public static void ShimmerRange(IEnumerable<PointyHexPoint> locs, Color color)
        {
            locs.ForEach(x => Shimmer(x, color));
        }
    }
    
    public enum TerrainType
    {
        open,
        notWalkable,
        blocking,
    }

    [AddComponentMenu("SRPG Demo/Battle/Map Cell")]
    [RequireComponent(typeof(SpriteRenderer))]
    public class MapCell : TileCell
    {
        // Universal

        public MapUnit unitPresent = null;

        #region Shorthands

        private MapController map { get { return Controllers.map; } }
        public PointyHexPoint loc
        {
            get
            {
                Profiler.BeginSample("MapCell.loc");

                PointyHexPoint result = map.WhereIs(this);

                Profiler.EndSample();

                return result;
            }
        }

        public IEnumerable<MapCell> GetNeighbors()
        {
            Profiler.BeginSample("MapCell.GetNeighbors()");

            IEnumerable<MapCell> result = map.WhereIs(this).GetNeighbors()
                .Where(map.InBounds)
                .Select(map.CellAt);

            Profiler.EndSample();

            return result;
        }

        #endregion

        #region TileCell implementation

        public override void __UpdatePresentation(bool forceUpdate = false)
        {
            if (forceUpdate) UpdatePresentation();
        }

        public override Color Color
        {
            get
            {
                return spriteRenderer.color;
            }

            set
            {
                spriteRenderer.color = value;
            }
        }

        public override Vector2 Dimensions
        {
            get
            {
                return spriteRenderer.bounds.size;
            }
        }

        public override void SetAngle(float angle)
        {
            spriteRenderer.transform.SetLocalRotationZ(angle);
        }

        public override void AddAngle(float angle)
        {
            spriteRenderer.transform.RotateAroundZ(angle);
        }

        #endregion

        #region Monobehaviour

        private void Start()
        {
            UpdatePresentation();
        }

        #endregion

        #region Sprite passthrough

        private SpriteRenderer _spriteRenderer = null;
        public SpriteRenderer spriteRenderer
        {
            get
            {
                if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
                return _spriteRenderer;
            }
        }

        #endregion

        #region Tint controls

        private IEnumerator TintShifter(Color startColor, Color endColor, float cycleTime)
        {
            Color a = startColor;
            Color b = endColor;

            while (true)
            {
                for (float timeElapsed = 0.0f; timeElapsed < cycleTime; timeElapsed += Time.deltaTime)
                {
                    spriteRenderer.color = Color.Lerp(a, b, timeElapsed / cycleTime);
                    yield return null;
                }

                Color swap = a;
                a = b;
                b = swap;
            }
        }

        private Coroutine tintShifter = null;

        public void SetTint(Color color)
        {
            ClearTint();

            spriteRenderer.color = color;
        }

        public void SetTint(Color startColor, Color endColor, float cycleTime = 1.0f)
        {
            ClearTint();

            tintShifter = StartCoroutine(TintShifter(startColor, endColor, cycleTime));
        }

        public void ClearTint()
        {
            if (tintShifter != null)
            {
                StopCoroutine(tintShifter);
                tintShifter = null;
            }

            spriteRenderer.color = Color.white;
        }

        #endregion
       
        // Implementation-specific
        
        #region Pathfinding information

        public List<MapUnit> threatList = new List<MapUnit>();
        public List<MapUnit> neighborUnitList = new List<MapUnit>();

        private List<MapCell> blockingNeighborList = new List<MapCell>();
        public bool hasBlockingNeighbors
        {
            get { return blockingNeighborList.Any(); }
        }

        #endregion

        #region Presentation Properties

        public Sprite walkableSprite = null;
        public Sprite nonWalkableSprite = null;

        private TerrainType _terrainType = TerrainType.open;
        public TerrainType terrainType
        {
            get { return _terrainType; }
            set
            {
                bool wasBlocking = isBlocking;
                _terrainType = value;
                if (wasBlocking != isBlocking)
                {
                    if (isBlocking)
                        Controllers.map.cache.WhereIs(this).GetNeighbors()
                            .Where(Controllers.map.cache.InBounds)
                            .Select(Controllers.map.cache.CellAt)
                            .ForEach(x => x.blockingNeighborList.Add(this));
                    else
                        Controllers.map.cache.WhereIs(this).GetNeighbors()
                            .Where(Controllers.map.cache.InBounds)
                            .Select(Controllers.map.cache.CellAt)
                            .ForEach(x => x.blockingNeighborList.Remove(this));
                }
                UpdatePresentation();
            }
        }
        
        public bool isWalkable
        {
            get
            {
                switch (terrainType)
                {
                    default:
                    case TerrainType.open:
                        return true;

                    case TerrainType.notWalkable:
                    case TerrainType.blocking:
                        return false;
                }
            }
        }

        public bool isBlocking    
        {
            get
            {
                switch (terrainType)
                {
                    default:
                    case TerrainType.open:
                    case TerrainType.notWalkable:
                        return false;

                    case TerrainType.blocking:
                        return true;
                }
            }
        }

        public void UpdatePresentation()
        {
            switch (isWalkable)
            {
                case true:
                    spriteRenderer.sprite = walkableSprite;
                    break;
                case false:
                    spriteRenderer.sprite = nonWalkableSprite;
                    break;
            }
        }

        #endregion
    }
}
