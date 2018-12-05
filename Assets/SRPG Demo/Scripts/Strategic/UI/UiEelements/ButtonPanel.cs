using SRPGDemo.Strategic.Map;
using System.Collections.Generic;
using UnityEngine;

namespace SRPGDemo.Strategic.UI
{
    public class ButtonPanel : MonoBehaviour
    {
        PieceButton buttonPrefab = null;

        Queue<PieceButton> pool = new Queue<PieceButton>();

        public void AddButton(MapPiece piece)
        {
            PieceButton newButton;

            if (pool.Count > 0)
            {
                newButton = pool.Dequeue();
                newButton.gameObject.SetActive(true);
            }
            else newButton = Instantiate(buttonPrefab);

            newButton.LoadRecipe(piece);
        }

        public void RemoveButton(PieceButton button)
        {
            button.gameObject.SetActive(false);
        }
    }
}