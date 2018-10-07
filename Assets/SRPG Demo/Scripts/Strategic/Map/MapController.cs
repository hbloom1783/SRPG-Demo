using System;
using System.Linq;
using UnityEngine;
using Gamelogic.Grids;
using SRPGDemo.Extensions;

namespace SRPGDemo.Strategic.Map
{
    [AddComponentMenu("SRPG Demo/Strategic/Map Controller")]
    [RequireComponent(typeof(FlatHexTileGridBuilder))]
    public class MapController : GridBehaviour<FlatHexPoint>
    {
        #region Map reference

        private IGrid<MapCell, FlatHexPoint> _mapGrid = null;
        public IGrid<MapCell, FlatHexPoint> mapGrid
        {
            get
            {
                if (_mapGrid == null) _mapGrid = Grid.CastValues<MapCell, FlatHexPoint>();

                return _mapGrid;
            }
        }

        public FlatHexPoint WhereIs(MapCell cell)
        {
            PointList<FlatHexPoint> pointList = mapGrid.WhereCell(x => x == cell).ToPointList();

            if (pointList.Count == 1)
                return pointList[0];
            else if (pointList.Count > 1)
                throw new ArgumentOutOfRangeException("cell", "Count > 1");
            else
                throw new ArgumentOutOfRangeException("cell", "Count <= 0");
        }

        public MapCell CellAt(FlatHexPoint loc)
        {
            if (InBounds(loc))
                return mapGrid[loc];
            else
                return null;
        }

        public bool InBounds(FlatHexPoint loc)
        {
            return Grid.Contains(loc);
        }

        #endregion

        public FlatHexPoint crashSite { get { return new FlatHexPoint(10, -1); } }

        public override void InitGrid()
        {
            _mapGrid = null;

            if (Application.isPlaying)
            {
                FlatHexPoint crashSite = mapGrid.RandomPick();

                mapGrid[crashSite].state = CellState.filled;
                crashSite.GetNeighbors()
                    .Where(InBounds)
                    .Select(x => mapGrid[x])
                    .ForEach(x => x.state = CellState.filled);
            }
        }
    }
}
