using GridLib.EventHandling;
using UnityEngine;

namespace GridLib.Hex
{
    public class HexGridCell : MonoBehaviour
    {
        #region Set-once from manager

        private HexCoords _loc = null;
        public HexCoords loc
        {
            get { return _loc; }
            set { if (_loc == null) _loc = value; }
        }

        #endregion

        #region Event handling

        private PointerEventCatcher _events = null;
        public PointerEventCatcher events
        {
            get
            {
                if (_events == null) _events = GetComponent<PointerEventCatcher>();
                return _events;
            }
            set
            {
                _events = value;
            }
        }

        #endregion
    }
}
