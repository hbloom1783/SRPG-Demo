using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SRPGDemo.UI
{
    public enum UiSignal
    {
        endTurn,
        jump,
    }

    [AddComponentMenu("SRPG/UI Controller")]
    public class UiController : MonoBehaviour
    {
        public Text marqueeText;
        public Button endTurnButton;
        public Button jumpButton;

        void Awake()
        {
            marqueeText.gameObject.SetActive(false);
            endTurnButton.gameObject.SetActive(false);
            jumpButton.gameObject.SetActive(false);
        }

        public void ButtonOnClickSignal(SignalButton button)
        {
            Controllers.game.ReceiveUiSignal(button.onClickSignal);
        }
    }
}
