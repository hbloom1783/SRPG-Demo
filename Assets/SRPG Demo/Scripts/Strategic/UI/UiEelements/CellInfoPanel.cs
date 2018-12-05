using SRPGDemo.Strategic.Map;
using UnityEngine.UI;

namespace SRPGDemo.Strategic.UI
{
    public class CellInfoPanel : UiElement
    {
        #region Component connections

        public Text infoText = null;
        public UiButton moveButton = null;

        #endregion

        #region Selection updates

        private MapCell _selectedCell = null;
        public MapCell selectedCell
        {
            get { return _selectedCell; }
            set
            {
                _selectedCell = value;
                UpdatePresentation();
            }
        }

        private MapUnit _selectedUnit = null;
        public MapUnit selectedUnit
        {
            get { return _selectedUnit; }
            private set { _selectedUnit = value; }
        }

        private void UpdatePresentation()
        {
            if (selectedCell == null)
            {
                infoText.text = "";
                moveButton.active = false;

                ui.infoPanel.Hide();
            }
            else
            {
                infoText.text = "Data for cell " + selectedCell.name;

                selectedUnit = selectedCell.unitPresent;

                // If there's a unit, give controls for that unit
                if (selectedUnit != null)
                {
                    moveButton.active = true;
                    infoText.text += "\nUnit present!";
                }
                else
                {
                    moveButton.active = false;
                }

                ui.infoPanel.Show();
            }
        }

        #endregion
    }
}
