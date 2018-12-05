using UnityEngine;
using UnityEngine.UI;

namespace SRPGDemo.Strategic.UI
{
    [AddComponentMenu("SRPG Demo/Strategic/Gui Button")]
    [RequireComponent(typeof(Button))]
    public class UiButton : UiElement
    {
        public Button unityButton;
        public Text unityText;

        public string text
        {
            get { return unityText.text; }
            set { unityText.text = value; }
        }

        public bool activeOnStartup = true;

        void Start()
        {
            active = activeOnStartup;
        }
    }
}
