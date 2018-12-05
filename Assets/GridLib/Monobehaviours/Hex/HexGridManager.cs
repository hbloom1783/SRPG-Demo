using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using GridLib.Extensions;
using GridLib.Matrix;
using GridLib.EventHandling;

namespace GridLib.Hex
{
    public class HexGridManager<TCell> : MonoBehaviour where TCell : HexGridCell
    {
        #region Grid inventory

        protected Dictionary<HexCoords, TCell> gridContents = new Dictionary<HexCoords, TCell>();
        
        public IEnumerable<HexCoords> coords { get { return gridContents.Keys; } }

        public IEnumerable<TCell> cells { get { return gridContents.Values; } }

        public bool InBounds(HexCoords loc)
        {
            return coords.Contains(loc);
        }

        public TCell CellAt(HexCoords loc)
        {
            if (InBounds(loc)) return gridContents[loc];
            else return null;
        }

        public TCell this[HexCoords loc] { get { return CellAt(loc); } }

        public void DeleteCell(HexCoords loc)
        {
            DeleteCell(CellAt(loc));
        }

        public void DeleteCell(TCell cell)
        {
            if (cell != null)
            {
                if (cell.loc != null) gridContents.Remove(cell.loc);

                if (Application.isPlaying)
                    Destroy(cell.gameObject);
                else
                    DestroyImmediate(cell.gameObject);
            }
        }

        public virtual void InitCell(HexCoords loc, TCell newCell)
        {
            // If a cell exists at this loc, clear it
            if (InBounds(loc)) DeleteCell(loc);

            // Initialize cell
            newCell.name = loc.ToString();
            newCell.loc = loc;
            newCell.transform.parent = transform;
            newCell.transform.position = GridToWorld(loc);

            // Try to set up events
            if (newCell.events != null) newCell.events.parent = events;

            // Store cell
            gridContents[loc] = newCell;
        }

        #endregion

        #region Initialization

        void Start()
        {
            if (Application.isPlaying)
                InitGrid();
        }

        public void ClearGrid()
        {
            GetComponentsInChildren<TCell>().ForEach(DeleteCell);
        }

        public void InitGrid()
        {
            ClearGrid();

            // Call our grid initializers
            GetComponents<HexGridInitializer<TCell>>()
                .ForEach(x => x.InitGrid(this, Application.isPlaying));
        }

        #endregion

        #region World-space mapping

        public Vector3 gridX = new Vector3(1, 0, 0);
        public Vector3 gridY = new Vector3(0, 1, 0);

        readonly private static Vector3 squareX = new Vector3(1, 0, 0);
        readonly private static Vector3 squareY = new Vector3(0, 1, 0);

        public void SetSquareDefault()
        {
            gridX = squareX;
            gridY = squareY;
        }

        readonly private static Vector3 pointyX = new Vector3(1, 0, 0);
        readonly private static Vector3 pointyY = new Vector3(0.5f, Mathf.Sqrt(3) / 2, 0);

        public void SetPointyDefault()
        {
            gridX = pointyX;
            gridY = pointyY;
        }

        readonly private static Vector3 flatX = new Vector3(Mathf.Sqrt(3) / 2, 0.5f, 0);
        readonly private static Vector3 flatY = new Vector3(0, 1, 0);

        public void SetFlatDefault()
        {
            gridX = flatX;
            gridY = flatY;
        }

        private Matrix3 gridTransform { get { return new Matrix3(gridX, gridY); } }

        public Vector3 GridToWorld(HexCoords loc)
        {
            return gridTransform * new Vector3(loc.x, loc.y, 0);
        }

        public HexCoords WorldToGrid(Vector3 pos)
        {
            return HexCoords.Round(gridTransform.inverse * pos);
        }

        public HexCoords mousePosition { get { return WorldToGrid(Camera.main.ScreenToWorldPoint(Input.mousePosition)); } }

        #endregion

        #region Event handling

        private PointerEventCentralizer _events = null;
        public PointerEventCentralizer events
        {
            get
            {
                if (_events == null) _events = GetComponent<PointerEventCentralizer>();
                return _events;
            }
            set
            {
                _events = value;
            }
        }

        #endregion
    }
}
