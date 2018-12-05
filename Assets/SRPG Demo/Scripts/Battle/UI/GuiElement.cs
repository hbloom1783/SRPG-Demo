using UnityEngine;

namespace SRPGDemo.Battle.UI
{
    public class GuiElement : MonoBehaviour
    {
        #region Shorthands

        protected GuiController ui { get { return GuiController.instance; } }

        public void SendSignal()
        {
            ui.SendSignal(gameObject);
        }

        #endregion

        #region Activity

        public bool activeOnStartup;

        public void Activate()
        {
            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
        }

        #endregion

        #region Monobehaviour

        void Start()
        {
            gameObject.SetActive(activeOnStartup);
        }

        #endregion
    }
}
