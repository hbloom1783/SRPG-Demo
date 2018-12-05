using GridLib.Hex;
using SRPGDemo.Strategic.Map;
using SRPGDemo.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SRPGDemo.Strategic.UI
{
    public class PieceButton : UiElement, IHasRecipe<MapPiece>
    {
        public MapPiece piece;

        public Sprite emptySprite = null;
        public Sprite moonSprite = null;
        public Sprite greenSprite = null;

        [Serializable]
        public struct Cell
        {
            public Vector2 loc;
            public Image image;
        }

        public Cell[] cellMap;

        private Dictionary<HexCoords, Image> _cellDict = null;
        public Dictionary<HexCoords, Image> cellDict
        {
            get
            {
                if (_cellDict == null) InitCellDict();
                return _cellDict;
            }
        }

        private void InitCellDict()
        {
            _cellDict = new Dictionary<HexCoords, Image>();

            foreach (Cell cell in cellMap)
            {
                HexCoords coords = new HexCoords(
                    (int)cell.loc.x,
                    (int)cell.loc.y);

                _cellDict[coords] = cell.image;
            }
        }

        private Sprite SwitchSprite()
        {
            return greenSprite;
        }

        private Sprite SwitchSprite(CellType type)
        {
            switch(type)
            {
                default:
                case CellType.empty:
                    return emptySprite;

                case CellType.greenCity:
                case CellType.greenPlain:
                    return greenSprite;

                case CellType.moonCrashSite:
                case CellType.moonPlain:
                    return moonSprite;
            }
        }

        public void LoadRecipe(MapPiece piece)
        {
            this.piece = piece;

            foreach (Image cell in cellDict.Values)
            {
                cell.sprite = null;
                cell.gameObject.SetActive(false);
            }

            foreach (HexCoords coords in piece.cells.Keys)
            {
                cellDict[coords].gameObject.SetActive(true);
                cellDict[coords].sprite = SwitchSprite(piece.cells[coords]);
            }
        }
    }
}
