using SRPGDemo.Strategic.UI;
using SRPGDemo.Strategic.Map;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SRPGDemo.Strategic.Gameplay
{
    class Idle : StrategicGameControllerState
    {
        #region State implementation

        public override void EnterState()
        {
            map.events.pointerClick += PointerClicked;

            game.keyDown += KeyDown;

            ui.signal += GuiSignal;
        }

        #endregion

        #region Input handling

        // Display info for hexes clicked on
        void PointerClicked(PointerEventData eventData, GameObject cell)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                ui.cellInfoPanel.selectedCell = cell.GetComponent<MapCell>();
            }
        }

        void GuiSignal(GameObject child)
        {
            // If a map piece is selected from the GUI, move to place that piece
            PieceButton pieceButton = child.GetComponent<PieceButton>();
            if (pieceButton != null)
            {
                ui.commandPanel.Hide();
                ui.infoPanel.Hide();
                game.ChangeState(new PlacingPiece(pieceButton.piece));
            }

            // If a unit control is clicked from the GUI, move to move that unit
            UiButton button = child.GetComponent<UiButton>();
            if ((button != null) && (button == ui.cellInfoPanel.moveButton))
            {
                game.ChangeState(new UnitMovement(ui.cellInfoPanel.selectedUnit));
            }
        }

        void KeyDown(KeyCode kc)
        {
            switch (kc)
            {
                case KeyCode.Equals:
                    ui.pieceButtonPanel.AddButton(MapPiece.Generate());
                    break;

                case KeyCode.Escape:
                    game.LoadSceneByName("Main Menu");
                    break;
            }
        }

        #endregion
    }
}
