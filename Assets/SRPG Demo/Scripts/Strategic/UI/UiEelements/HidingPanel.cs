using UnityEngine;
using UnityEngine.UI;

namespace SRPGDemo.Strategic.UI
{
    public class HidingPanel : MonoBehaviour
    {
        public int hideDX = 0;
        public int hideDY = 0;
        public bool startHidden = true;

        public Text textToChange = null;
        public string shownText = "Hide";
        public string hiddenText = "Show";

        private bool hidden = false;

        private RectTransform rtf { get { return (RectTransform)transform; } }

        public void HideShow()
        {
            if (hidden)
                Show();
            else
                Hide();
        }

        public void Hide()
        {
            if (!hidden)
            {
                transform.localPosition = new Vector3(
                    rtf.localPosition.x + hideDX,
                    rtf.localPosition.y + hideDY);
                hidden = true;
                UpdateText();
            }
        }

        public void Show()
        {
            if (hidden)
            {
                transform.localPosition = new Vector3(
                    rtf.localPosition.x - hideDX,
                    rtf.localPosition.y - hideDY);
                hidden = false;
                UpdateText();
            }
        }

        private void UpdateText()
        {
            if (textToChange != null) switch (hidden)
                {
                    case false:
                        textToChange.text = shownText;
                        break;
                    case true:
                        textToChange.text = hiddenText;
                        break;
                }
        }

        void Start()
        {
            if (startHidden) Hide();
            UpdateText();
        }
    }
}