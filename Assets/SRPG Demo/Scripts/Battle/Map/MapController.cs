using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Gamelogic.Grids;
using SRPGDemo.Extensions;

namespace SRPGDemo.Battle.Map
{
    [AddComponentMenu("SRPG Demo/Battle/Map Controller")]
    [RequireComponent(typeof(PointyHexTileGridBuilder))]
    public class MapController : GridBehaviour<PointyHexPoint>
    {
        #region Map Reference

        private IGrid<MapCell, PointyHexPoint> _mapGrid = null;
        public IGrid<MapCell, PointyHexPoint> mapGrid
        {
            get
            {
                if (_mapGrid == null) _mapGrid = Grid.CastValues<MapCell, PointyHexPoint>();

                return _mapGrid;
            }
        }
        
        public PointyHexPoint WhereIs(MapCell cell)
        {
            PointList<PointyHexPoint> pointList = mapGrid.WhereCell(x => x == cell).ToPointList();

            if (pointList.Count == 1)
                return pointList[0];
            else if (pointList.Count > 1)
                throw new ArgumentOutOfRangeException("cell", "Count > 1");
            else
                throw new ArgumentOutOfRangeException("cell", "Count <= 0");
        }

        public PointyHexPoint WhereIs(MapUnit unit)
        {
            return unitIndex[unit];
        }

        public MapCell CellAt(PointyHexPoint loc)
        {
            if (InBounds(loc))
                return mapGrid[loc];
            else
                return null;
        }

        public MapUnit UnitAt(PointyHexPoint loc)
        {
            if (InBounds(loc))
                return mapGrid[loc].unitPresent;
            else
                return null;
        }

        public bool InBounds(PointyHexPoint loc)
        {
            return Grid.Contains(loc);
        }

        #endregion

        #region Map Extensions

        public List<PointyHexPoint> GetArc(
            PointyHexPoint origin,
            Facing startAngle,
            Facing endAngle,
            int minRadius,
            int maxRadius)
        {
            return Map.GetArc(origin, startAngle, endAngle, minRadius, maxRadius);
        }

        public List<PointyHexPoint> GetCircle(
           PointyHexPoint origin,
           int minRadius,
           int maxRadius)
        {
            return Map.GetCircle(origin, minRadius, maxRadius);
        }

        #endregion

        #region Unit management

        Dictionary<MapUnit, PointyHexPoint> unitIndex = new Dictionary<MapUnit, PointyHexPoint>();

        public void PlaceUnit(MapUnit unit, MapCell cell)
        {
            if (cell.unitPresent != null)
                throw new ArgumentException("Cell not empty!");

            cell.unitPresent = unit;
            unit.transform.parent = cell.transform;
            unit.transform.position = cell.transform.position;
            unit.spriteRenderer.sortingOrder = cell.spriteRenderer.sortingOrder;
            unitIndex[unit] = WhereIs(cell);

            // Establish threat
            unit.GetThreatArea()
                .Select(CellAt)
                .ForEach(x => x.threatList.Add(unit));

            // Notify neighbors
            cell.GetNeighbors()
                .ForEach(x => x.neighborUnitList.Add(unit));
        }

        public void PlaceUnit(MapUnit unit, PointyHexPoint point)
        {
            PlaceUnit(unit, mapGrid[point]);
        }

        public void UnplaceUnit(MapUnit unit)
        {
            mapGrid.Select(CellAt).ForEach(x =>
            {
                x.threatList.Remove(unit);
                x.neighborUnitList.Remove(unit);
            });

            mapGrid[WhereIs(unit)].unitPresent = null;
            unit.transform.parent = transform;
            unitIndex.Remove(unit);
        }

        public void ReseatUnit(MapUnit unit)
        {
            mapGrid.Select(CellAt).ForEach(x =>
            {
                x.threatList.Remove(unit);
            });

            // Establish threat
            unit.GetThreatArea()
                .Select(CellAt)
                .ForEach(x => x.threatList.Add(unit));
        }

        public IEnumerable<MapUnit> Units()
        {
            return unitIndex.Keys;
        }

        public IEnumerable<MapUnit> Units(UnitTeam team)
        {
            return Units().Where(x => x.team == team);
        }

        public bool HasUnit(MapUnit unit)
        {
            return unitIndex.ContainsKey(unit);
        }

        #endregion

        #region Map Creation

        // After grid instantiates
        public override void InitGrid()
        {
            // Force the mapGrid cache to refresh
            _mapGrid = null;

            // Reorder the cells for visual clarity
            foreach (PointyHexPoint point in mapGrid.ToPointList())
            {
                mapGrid[point].spriteRenderer.sortingOrder = -point.Y;
            }
        }

        void Start()
        {
            if (Application.isPlaying)
            {
                // Run all the attached generation routines
                foreach (MapGenerator generator in GetComponents<MapGenerator>())
                {
                    generator.Generate();
                }
            }
        }

        #endregion
    }
}
