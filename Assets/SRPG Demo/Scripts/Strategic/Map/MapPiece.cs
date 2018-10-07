using System;
using System.Linq;
using UnityEngine;
using Gamelogic.Grids;
using SRPGDemo.Extensions;

namespace SRPGDemo.Strategic.Map
{
    static class VectorExt
    {
        public static Vector3 WithX(this Vector3 orig, float newX)
        {
            return new Vector3(newX, orig.y, orig.z);
        }

        public static Vector3 WithY(this Vector3 orig, float newY)
        {
            return new Vector3(orig.x, newY, orig.z);
        }

        public static Vector3 WithZ(this Vector3 orig, float newZ)
        {
            return new Vector3(orig.x, orig.y, newZ);
        }
    }

    [AddComponentMenu("SRPG Demo/Strategic/Map Controller")]
    [RequireComponent(typeof(FlatHexTileGridBuilder))]
    public class MapPiece : GridBehaviour<FlatHexPoint>
    {
        private MapController map { get { return Controllers.map; } }

        private IGrid<MapCell, FlatHexPoint> _pieceGrid = null;
        public IGrid<MapCell, FlatHexPoint> pieceGrid
        {
            get
            {
                if (_pieceGrid == null) _pieceGrid = Grid.CastValues<MapCell, FlatHexPoint>();

                return _pieceGrid;
            }
        }

        public FlatHexPoint WhereIs(MapCell cell)
        {
            PointList<FlatHexPoint> pointList = pieceGrid.WhereCell(x => x == cell).ToPointList();

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
                return pieceGrid[loc];
            else
                return null;
        }

        public bool InBounds(FlatHexPoint loc)
        {
            return Grid.Contains(loc);
        }

        void Update()
        {
            transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition)
                .WithZ(transform.position.z);

            /*pieceGrid
                .Select(CellAt)
                .Select(MapCoords)
                .Where(map.InBounds)
                .Select(map.CellAt)
                .Where*/
        }

        public override void InitGrid()
        {
            if (Application.isPlaying)
            {
                pieceGrid
                    .Select(CellAt)
                    .ForEach(x =>
                {
                    x.spriteRenderer.sortingLayerName = "Effects";
                    x.state = CellState.valid;
                });
            }
        }
    }
}
