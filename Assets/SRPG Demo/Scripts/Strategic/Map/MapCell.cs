using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using GridLib.Hex;
using UnityEngine.UI;

namespace SRPGDemo.Strategic.Map
{
    public static class EnumerableExt
    {
        private static MapController map { get { return MapController.instance; } }

        public static IEnumerable<HexCoords> Frontier(this IEnumerable<HexCoords> region)
        {
            HashSet<HexCoords> done = new HashSet<HexCoords>(region);

            foreach (HexCoords target in region)
            {
                foreach (HexCoords neighbor in target.neighbors)
                {
                    if (!done.Contains(neighbor))
                    {
                        yield return neighbor;
                        done.Add(neighbor);
                    }
                }
            }
        }

        public static IEnumerable<MapCell> Frontier(this IEnumerable<MapCell> region)
        {
            return region.Select(x => x.loc).Frontier().Select(map.CellAt);
        }
    }

    public enum CellType
    {
        empty,

        // Moon
        moonPlain,
        moonCrashSite,

        // Big Green
        greenPlain,
        greenCity,
    }

    public enum CellHighlight
    {
        neutral,
        valid,
        invalid,
    }
    
    public enum CellFogOfWar
    {
        unexplored,
        clear,
        returned,
    }

    [AddComponentMenu("SRPG Demo/Strategic/Map Cell")]
    [RequireComponent(typeof(SpriteRenderer))]
    public class MapCell : HexGridCell
    {
        #region Shorthands

        private MapController map { get { return MapController.instance; } }

        public IEnumerable<MapCell> neighbors
        {
            get
            {
                return loc.neighbors
                    .Where(map.InBounds)
                    .Select(map.CellAt);
            }
        }

        public MapCell OffsetBy(HexCoords offset)
        {
            if (map.InBounds(loc + offset)) return map.CellAt(loc + offset);
            else return null;
        }

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

        #region MapUnit

        public MapUnit unitPresent = null;

        #endregion

        #region Presentation

        public Sprite emptySprite = null;
        public Sprite greenSprite = null;
        public Sprite moonSprite = null;

        private CellType _type = CellType.empty;
        public CellType type
        {
            get { return _type; }
            set
            {
                _type = value;
                UpdatePresentation();
            }
        }

        private CellType _overlayType = CellType.empty;
        public CellType overlayType
        {
            get { return _overlayType; }
            set
            {
                _overlayType = value;
                UpdatePresentation();
            }
        }

        public CellType displayType
        {
            get
            {
                return (overlayType == CellType.empty) ? type : overlayType;
            }
        }

        private CellHighlight _highlight = CellHighlight.neutral;
        public CellHighlight highlight
        {
            get { return _highlight; }
            set
            {
                _highlight = value;
                UpdatePresentation();
            }
        }

        private CellFogOfWar _fog = CellFogOfWar.unexplored;
        public CellFogOfWar fog
        {
            get { return _fog; }
            set
            {
                _fog = value;
                UpdatePresentation();
            }
        }
        
        public bool isFilled { get { return _type != CellType.empty; } }

        private Sprite SwitchSprite()
        {
            switch (displayType)
            {
                default:
                case CellType.empty:
                    return emptySprite;

                case CellType.moonPlain:
                case CellType.moonCrashSite:
                    return moonSprite;

                case CellType.greenPlain:
                case CellType.greenCity:
                    return greenSprite;
            }
        }

        private Color SwitchColor()
        {
            switch (highlight)
            {
                default:
                case CellHighlight.neutral:
                    return Color.white;

                case CellHighlight.valid:
                    return Color.green;

                case CellHighlight.invalid:
                    return Color.red;
            }
        }

        public void UpdatePresentation()
        {
            spriteRenderer.sprite = SwitchSprite();
            spriteRenderer.color = SwitchColor();

            if (debugText != null)
            {
                debugText.text  = type.ToString() + "\n";
                debugText.text += fog.ToString();
            }
        }

        #endregion

        #region Monobehaviour

        void Start()
        {
            UpdatePresentation();
        }

        #endregion
    }
}
