using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using GridLib.Hex;
using SRPGDemo.Extensions;
using UnityEngine.UI;

namespace SRPGDemo.Battle.Map
{
    public enum TerrainType
    {
        open,
        notWalkable,
        blocking,
    }

    [AddComponentMenu("SRPG Demo/Battle/Map Cell")]
    [RequireComponent(typeof(SpriteRenderer))]
    public class MapCell : HexGridCell
    {
        // Universal

        public MapUnit unitPresent = null;

        #region Shorthands

        private MapController map { get { return MapController.instance; } }

        public IEnumerable<MapCell> neighbors { get { return loc.neighbors.Where(map.InBounds).Select(map.CellAt); } }


        private SpriteRenderer _spriteRenderer = null;
        public SpriteRenderer spriteRenderer
        {
            get
            {
                if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
                return _spriteRenderer;
            }
        }
        
        public Text debugText = null;

        #endregion

        #region Monobehaviour

        private void Start()
        {
            UpdatePresentation();
        }

        #endregion
        
        #region Tint controls

        private Color _tint = Color.white;
        public Color tint
        {
            get { return _tint; }
            set
            {
                _tint = value;
                UpdatePresentation();
            }
        }

        private Color tintDark { get { return Color.Lerp(tint, Color.black, 0.5f); } }

        private float _tintLerp = 0.0f;
        public float tintLerp
        {
            get { return _tintLerp; }
            set
            {
                _tintLerp = value;
                UpdatePresentation();
            }
        }

        #endregion

        // Implementation-specific

        #region Pathfinding information

        public HashSet<MapUnit> threatSet = new HashSet<MapUnit>();

        public void AddThreat(MapUnit threat)
        {
            threatSet.Add(threat);
            UpdatePresentation();
        }

        public void RemoveThreat(MapUnit threat)
        {
            threatSet.Remove(threat);
            UpdatePresentation();
        }

        #endregion

        #region Presentation properties

        public Sprite walkableSprite = null;
        public Sprite nonWalkableSprite = null;

        private TerrainType _terrainType = TerrainType.open;
        public TerrainType terrainType
        {
            get { return _terrainType; }
            set
            {
                _terrainType = value;
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

            if (debugText != null)
            {
                debugText.text = loc.ToString();
                
                foreach (MapUnit unit in threatSet)
                    debugText.text += "\nT: " + unit.name;
            }

            spriteRenderer.color = Color.Lerp(tint, tintDark, tintLerp);
        }

        #endregion
    }
}
