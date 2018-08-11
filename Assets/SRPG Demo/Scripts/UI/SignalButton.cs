using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace SRPGDemo.UI
{
    [AddComponentMenu("SRPG/UI/Signal Button")]
    [RequireComponent(typeof(Button))]
    public class SignalButton : MonoBehaviour
    {
        public UiSignal onClickSignal;
    }
}
