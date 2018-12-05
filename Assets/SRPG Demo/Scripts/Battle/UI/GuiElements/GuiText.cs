using UnityEngine;
using UnityEngine.UI;
using SRPGDemo.Utility;

namespace SRPGDemo.Battle.UI
{
    [AddComponentMenu("SRPG Demo/Battle/Gui Text")]
    [RequireComponent(typeof(Text))]
    public class GuiText : GuiElement
    {
        public CachedReference<Text> unityText;

        public GuiText()
        {
            unityText = new CachedReference<Text>(GetComponent<Text>);
        }

        public string text
        {
            get { return unityText.cache.text; }
            set { unityText.cache.text = value; }
        }

        public Color color
        {
            get { return unityText.cache.color; }
            set { unityText.cache.color = value; }
        }
    }
}
