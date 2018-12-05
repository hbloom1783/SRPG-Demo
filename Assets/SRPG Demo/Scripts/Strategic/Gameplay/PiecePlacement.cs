using UnityEngine;
using UnityEngine.EventSystems;
using SRPGDemo.Strategic.Map;
using System.Linq;
using System.Collections.Generic;
using GridLib.Hex;

namespace SRPGDemo.Strategic.Gameplay
{
    class PlacingPiece : StrategicGameControllerState
    {
        #region Selected piece

        MapPiece piece = null;
        HexRotation pieceRot = new HexRotation(0);
        MapCell lastCell = null;

        public PlacingPiece(MapPiece piece)
        {
            this.piece = piece;
        }

        bool IsValid(MapCell center)
        {
            IEnumerable<MapCell> targetCells = piece.TargetArea(center, pieceRot);

            if (targetCells.Any(x => x.isFilled))
            {
                //Debug.Log("Cell was filled.");
                return false;
            }
            else if (targetCells.Frontier().Any(x => x.isFilled))
            {
                //Debug.Log("Cell was touching.");
                return true;
            }
            else
            {
                //Debug.Log("Cell was not filled or touching.");
                return false;
            }
        }

        void MarkPiece(MapCell center)
        {
            if (center != null)
            {
                foreach (HexCoords loc in center.loc.CompoundRing(1, 5))
                    if (!map.InBounds(loc))
                        map.InitCell(loc);

                if (IsValid(center))
                {
                    foreach (MapCell targetCell in piece.TargetArea(center, pieceRot))
                    {
                        targetCell.highlight = CellHighlight.valid;
                        targetCell.overlayType = CellType.greenPlain;
                    }
                }
                else
                {
                    foreach (MapCell targetCell in piece.TargetArea(center, pieceRot))
                    {
                        targetCell.highlight = CellHighlight.invalid;
                        targetCell.overlayType = CellType.greenPlain;
                    }
                }
            }
        }

        void UnmarkPiece(MapCell center)
        {
            if (center != null)
            {
                foreach (MapCell targetCell in piece.TargetArea(center, pieceRot))
                {
                    targetCell.highlight = CellHighlight.neutral;
                    targetCell.overlayType = CellType.empty;
                }
            }
        }

        #endregion

        #region State implementation

        public override void EnterState()
        {
            map.events.pointerEnter += PointerEnter;
            map.events.pointerExit += PointerExit;
            map.events.pointerClick += PointerClick;

            game.keyDown += KeyDown;
            game.mouseDown += MouseDown;
        }

        #endregion

        #region Input handling

        // While moused over valid hex, display green ghost piece on grid
        // While moused over invalid hex, display red outline
        void PointerEnter(PointerEventData eventData, GameObject obj)
        {
            MarkPiece(obj.GetComponent<MapCell>());
            lastCell = obj.GetComponent<MapCell>();
        }

        void PointerExit(PointerEventData eventData, GameObject obj)
        {
            lastCell = null;
            UnmarkPiece(obj.GetComponent<MapCell>());
        }

        // If valid hex clicked, place piece
        void PointerClick(PointerEventData eventData, GameObject obj)
        {
            MapCell cell = obj.GetComponent<MapCell>();

            if (cell != null)
            {
                IEnumerable<MapCell> targetCells = piece.TargetArea(cell, pieceRot);

                if (IsValid(cell))
                {
                    UnmarkPiece(cell);

                    foreach (MapCell targetCell in targetCells)
                    {
                        targetCell.type = CellType.greenPlain;
                    }

                    ui.pieceButtonPanel.RemoveButton(piece);

                    game.ChangeState(new Idle());
                }
            }
        }

        // If , or . are hit, rotate the piece
        void KeyDown(KeyCode kc)
        {
            switch (kc)
            {
                case KeyCode.Comma:
                    UnmarkPiece(lastCell);
                    pieceRot.CWcount -= 1;
                    MarkPiece(lastCell);
                    break;

                case KeyCode.Period:
                    UnmarkPiece(lastCell);
                    pieceRot.CWcount += 1;
                    MarkPiece(lastCell);
                    break;
            }
        }

        // On RMB, 'put the piece back'
        private void MouseDown(PointerEventData.InputButton button)
        {
            switch (button)
            {
                case PointerEventData.InputButton.Right:
                    UnmarkPiece(map.mouseCell);
                    game.ChangeState(new Idle());
                    break;
            }
        }

        #endregion
    }
}
