using UnityEngine;
using UnityEngine.UI;
using SRPGDemo.Utility;

namespace SRPGDemo.Battle.UI
{
    [AddComponentMenu("SRPG Demo/Battle/Gui Button")]
    [RequireComponent(typeof(Button))]
    public class GuiButton : GuiElement
    {
        public CachedReference<Button> unityButton;
        public CachedReference<Text> unityText;

        public GuiButton()
        {
            unityButton = new CachedReference<Button>(GetComponent<Button>);
            unityText = new CachedReference<Text>(GetComponentInChildren<Text>);
        }

        public string text
        {
            get { return unityText.cache.text; }
            set { unityText.cache.text = value; }
        }
    }
}
