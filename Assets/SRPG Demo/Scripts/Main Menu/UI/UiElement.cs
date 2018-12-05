using UnityEngine;

namespace SRPGDemo.MainMenu.UI
{
    public class UiElement : MonoBehaviour
    {
        protected UiController ui { get { return UiController.instance; } }

        public bool active
        {
            set
            {
                gameObject.SetActive(value);

                Canvas.ForceUpdateCanvases();
            }
        }

        public void SendSignal()
        {
            ui.SendSignal(gameObject);
        }
    }
}
