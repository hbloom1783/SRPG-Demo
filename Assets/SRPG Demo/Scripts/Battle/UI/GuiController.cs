using UnityEngine;

namespace SRPGDemo.Battle.UI
{
    [AddComponentMenu("SRPG Demo/Battle/Gui Controller")]
    public class GuiController : MonoBehaviour
    {
        #region Singleton

        private static GuiController _instance = null;
        public static GuiController instance
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

        public GuiButton endTurnButton;
        public GuiButton jumpButton;

        public GuiText marqueeText;
        public GuiText playerDebugText;
        public GuiText enemyDebugText;

        #endregion
    }
}
