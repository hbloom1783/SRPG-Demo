using SRPGDemo.Strategic.Map;
using SRPGDemo.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SRPGDemo.Strategic.UI
{
    public class PieceButtonPanel : MonoBehaviour
    {
        #region Pool connection

        public ObjectPool pieceButtonPool = null;
        private int buttonCount = 0;

        #endregion

        #region Button inventory

        public List<PieceButton> buttons = new List<PieceButton>();

        public void AddButton(MapPiece piece)
        {
            if (pieceButtonPool == null) Debug.Log("PieceButtonPanel has no pool!");
            else
            {
                PieceButton newButton = pieceButtonPool.GetObject(piece).GetComponent<PieceButton>();
                newButton.transform.SetParent(transform, false);
                
                buttonCount += 1;
                buttons.Insert(0, newButton);
                UiController.instance.commandPanel.Show();
            }
        }

        public void RemoveButton(PieceButton button)
        {
            buttons.Remove(button);

            pieceButtonPool.ReturnObject(button.gameObject);
        }

        public void RemoveButton(MapPiece piece)
        {
            buttons
                .Where(x => x.piece == piece)
                .ToList()
                .ForEach(RemoveButton);
        }

        #endregion
    }
}
