using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Gamelogic.Grids;
using SRPGDemo.Extensions;

namespace SRPGDemo.Map
{
    public class PassageNode
    {
        public MapCell cell = null;
        public bool blocked = false;
        public List<MapMobile> threatList = new List<MapMobile>();
        public List<MapMobile> passList = new List<MapMobile>();

        public PassageNode(MapCell cell)
        {
            this.cell = cell;
        }
    }

    public enum MobileTeam
    {
        player,
        enemy,
    }

    [RequireComponent(typeof(SpriteRenderer))]
    public abstract class MapMobile : MonoBehaviour
    {
        public MobileTeam team = MobileTeam.player;
        public int moveSpeed = 8;

        public bool engaged = false;

        public MapController map
        {
            get
            {
                return Controllers.map;
            }
        }

        #region Presentation

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

        #region Game-specific

        public int actionsLeft = 1;

        public abstract IEnumerable<PointyHexPoint> ThreatArea();

        #endregion

        public void Start()
        {
        }

        public void Update()
        {
        }

        public void OnDestroy()
        {
            if (Controllers.map != null)
                Controllers.map.UnplaceMobile(this);
        }
    }

    [AddComponentMenu("SRPG/Map Controller")]
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

        public MapCell this[PointyHexPoint loc]
        {
            get
            {
                return (MapCell)Grid[loc];
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

        public PointyHexPoint WhereIs(MapMobile mobile)
        {
            PointList<PointyHexPoint> pointList = mapGrid.WhereCell(x => x.MobilesPresent().Contains(mobile)).ToPointList();

            if (pointList.Count == 1)
                return pointList[0];
            else if (pointList.Count > 1)
                throw new ArgumentOutOfRangeException("mobile", "Count > 1");
            else
                throw new ArgumentOutOfRangeException("mobile", "Count <= 0");
        }

        public MapCell CellAt(PointyHexPoint loc)
        {
            if (InBounds(loc))
                return (MapCell)Grid[loc];
            else
                return null;
        }
        
        public bool InBounds(PointyHexPoint loc)
        {
            return Grid.Contains(loc);
        }

        #endregion

        #region Mobile Management

        public void PlaceMobile(MapMobile mobile, MapCell cell)
        {
            cell.AddMobile(mobile);
            mobile.transform.parent = cell.transform;
            mobile.transform.position = cell.transform.position;
            mobile.spriteRenderer.sortingOrder = cell.spriteRenderer.sortingOrder + 1;
        }

        public void PlaceMobile(MapMobile mobile, PointyHexPoint point)
        {
            PlaceMobile(mobile, mapGrid[point]);
        }

        public void UnplaceMobile(MapMobile mobile)
        {
            mapGrid[WhereIs(mobile)].RemoveMobile(mobile);
        }

        public IEnumerable<MapMobile> Mobiles()
        {
            List<MapMobile> result = new List<MapMobile>();

            foreach(PointyHexPoint point in mapGrid)
            {
                result.AddRange(mapGrid[point].MobilesPresent());
            }

            return result;
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

        #region Pathfinding
        
        #endregion
    }
}
