using UnityEngine;

namespace SRPGDemo.MainMenu.UI
{
    [AddComponentMenu("SRPG Demo/Strategic/GUI Controller")]
    public class UiController : MonoBehaviour
    {
        #region Singleton

        private static UiController _instance = null;
        public static UiController instance
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

        #region Event handling

        public delegate void GuiElementEvent(GameObject child);

        public event GuiElementEvent signal = null;

        public void ClearEvents()
        {
            signal = null;
        }

        public void SendSignal(GameObject child)
        {
            if (signal != null) signal(child);
        }

        #endregion

        #region Custom methods

        public UiElement newGameButton;
        public UiElement loadGameButton;
        public UiElement quitButton;

        #endregion
    }
}
