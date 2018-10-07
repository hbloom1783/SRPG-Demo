using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using Gamelogic.Grids;
using Gamelogic.Extensions;
using SRPGDemo.Extensions;

namespace SRPGDemo.Strategic.Map
{
    public enum CellState
    {
        // In either case:
        empty,

        // On the map itself:
        filled,
        proposed,
        conflicted,

        // On the floating map piece:
        valid,
        invalid,
    }

    [AddComponentMenu("SRPG Demo/Strategic/Map Cell")]
    [RequireComponent(typeof(SpriteRenderer))]
    public class MapCell : TileCell
    {
        #region Shorthands

        private MapController map { get { return Controllers.map; } }
        public FlatHexPoint loc
        {
            get
            {
                Profiler.BeginSample("MapCell.loc");

                FlatHexPoint result = map.WhereIs(this);

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

        #region Presentation

        private CellState _state = CellState.empty;
        public CellState state
        {
            get { return _state; }
            set
            {
                _state = value;
                UpdatePresentation();
            }
        }

        public void UpdatePresentation()
        {
            switch (state)
            {
                case CellState.empty:
                    Color = Color.clear;
                    break;

                case CellState.filled:
                    Color = Color.white;
                    break;
                case CellState.proposed:
                    Color = Color.clear;
                    //Color = Color.green.Mix(Color.clear);
                    break;
                case CellState.conflicted:
                    Color = Color.clear;
                    //Color = Color.red.Mix(Color.clear);
                    break;

                case CellState.valid:
                    Color = Color.green.Mix(Color.clear);
                    break;

                case CellState.invalid:
                    Color = Color.red.Mix(Color.clear);
                    break;
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
