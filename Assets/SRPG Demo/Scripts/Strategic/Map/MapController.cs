using UnityEngine;
using GridLib.Hex;
using System.Collections.Generic;
using System.Linq;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SRPGDemo.Strategic.Map
{
    [AddComponentMenu("SRPG Demo/Strategic/Map Controller")]
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

        #region Cell management

        public MapCell mapCellPrefab = null;

        public override void InitCell(HexCoords loc, MapCell newCell)
        {
            base.InitCell(loc, newCell);
        }

        public MapCell InitCell(HexCoords loc)
        {
            MapCell newCell = Instantiate(mapCellPrefab);

            InitCell(loc, newCell);

            return newCell;
        }

        public MapCell mouseCell { get { return CellAt(mousePosition); } }

        public delegate bool CellMatch(MapCell target, MapCell neighbor);

        public IEnumerable<MapCell> FloodFill(MapCell start, CellMatch match)
        {
            HashSet<MapCell> done = new HashSet<MapCell>();
            Queue<MapCell> todo = new Queue<MapCell>();

            todo.Enqueue(start);

            while (todo.Count > 0)
            {
                MapCell target = todo.Dequeue();

                yield return target;

                foreach (MapCell neighbor in target.neighbors)
                    if (!done.Contains(neighbor) && match(target, neighbor))
                        todo.Enqueue(neighbor);

                done.Add(target);
            }
        }

        #endregion

        #region Map reference

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

            cell.fog = CellFogOfWar.clear;
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

        public IEnumerable<MapUnit> Units()
        {
            return unitIndex.Keys;
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
