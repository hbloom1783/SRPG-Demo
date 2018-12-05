using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using GridLib.Hex;
using SRPGDemo.Extensions;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SRPGDemo.Battle.Map
{
    [AddComponentMenu("SRPG Demo/Battle/Map Controller")]
    public class MapController : HexGridManager<MapCell>
    {
        #region Singleton

        private static MapController _instance = null;
        public static MapController instance
        {
            get { return _instance; }
            private set { _instance = value; }
        }

        void Awake()
        {
            if (instance != null)
                Destroy(gameObject);
            else
                instance = this;
        }

        void OnDestroy()
        {
            instance = null;
        }

        #endregion

        #region Map reference

        public override void InitCell(HexCoords loc, MapCell newCell)
        {
            base.InitCell(loc, newCell);
        }

        public HexCoords WhereIs(MapUnit unit)
        {
            return unitIndex[unit];
        }

        public MapUnit UnitAt(HexCoords loc)
        {
            if (InBounds(loc))
                return CellAt(loc).unitPresent;
            else
                return null;
        }

        public MapCell UnitCell(MapUnit unit)
        {
            return CellAt(WhereIs(unit));
        }

        #endregion
        
        #region Unit management

        Dictionary<MapUnit, HexCoords> unitIndex = new Dictionary<MapUnit, HexCoords>();

        public void PlaceUnit(MapUnit unit, MapCell cell)
        {
            if (cell.unitPresent != null)
                throw new ArgumentException("Cell not empty!");

            cell.unitPresent = unit;
            unit.transform.parent = cell.transform;
            unit.transform.position = cell.transform.position;
            unitIndex[unit] = cell.loc;
        }

        public void PlaceUnit(MapUnit unit, HexCoords loc)
        {
            PlaceUnit(unit, CellAt(loc));
        }

        public void UnplaceUnit(MapUnit unit)
        {
            CellAt(WhereIs(unit)).unitPresent = null;
            unit.transform.parent = transform;
            unitIndex.Remove(unit);
        }

        /*public void ReseatUnit(MapUnit unit)
        {
            cells.ForEach(x =>
            {
                x.AddThreat(unit);
            });

            // Establish threat
            unit.GetThreatArea()
                .Select(CellAt)
                .ForEach(x => x.AddThreat(unit));
        }*/

        public IEnumerable<MapUnit> Units()
        {
            return unitIndex.Keys;
        }

        public IEnumerable<MapUnit> Units(UnitTeam team)
        {
            return Units(x => x.team == team);
        }

        public IEnumerable<MapUnit> Units(Func<MapUnit, bool> pred)
        {
            return Units().Where(pred);
        }

        public bool HasUnit(MapUnit unit)
        {
            return unitIndex.ContainsKey(unit);
        }

        #endregion
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(MapController))]
    class MapControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            MapController myScript = (MapController)target;

            if (GUILayout.Button("Initialize"))
            {
                myScript.InitGrid();
            }

            if (GUILayout.Button("Clear"))
            {
                myScript.ClearGrid();
            }
        }
    }
#endif
}
