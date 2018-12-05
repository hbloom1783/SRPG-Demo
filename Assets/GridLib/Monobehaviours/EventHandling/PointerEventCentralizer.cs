using UnityEngine;
using UnityEngine.EventSystems;

namespace GridLib.EventHandling
{
    public class PointerEventCentralizer : MonoBehaviour
    {
        public delegate void ChildEvent(PointerEventData eventData, GameObject child);

        public event ChildEvent pointerEnter;
        public event ChildEvent pointerExit;
        public event ChildEvent pointerDown;
        public event ChildEvent pointerUp;
        public event ChildEvent pointerClick;
        public event ChildEvent pointerBeginDrag;
        public event ChildEvent pointerDrag;
        public event ChildEvent pointerEndDrag;
        public event ChildEvent pointerDrop;

        public void ClearEvents()
        {
            pointerEnter = null;
            pointerExit = null;
            pointerDown = null;
            pointerUp = null;
            pointerClick = null;
            pointerBeginDrag = null;
            pointerDrag = null;
            pointerEndDrag = null;
            pointerDrop = null;
        }

        private void DebugPrint(PointerEventData eventData)
        {
            const bool debug = false;

#pragma warning disable CS0162 // Unreachable code detected
            if (debug) Debug.Log(gameObject.name + ": " + eventData);
#pragma warning restore CS0162 // Unreachable code detected
        }

        public void HandleEnter(PointerEventData eventData, GameObject child)
        {
            DebugPrint(eventData);
            if (pointerEnter != null) pointerEnter(eventData, child);
        }

        public void HandleExit(PointerEventData eventData, GameObject child)
        {
            if (pointerExit != null) pointerExit(eventData, child);
        }

        public void HandleDown(PointerEventData eventData, GameObject child)
        {
            if (pointerDown != null) pointerDown(eventData, child);
        }

        public void HandleUp(PointerEventData eventData, GameObject child)
        {
            if (pointerUp != null) pointerUp(eventData, child);
        }

        public void HandleClick(PointerEventData eventData, GameObject child)
        {
            if (pointerClick != null) pointerClick(eventData, child);
        }

        public void HandleBeginDrag(PointerEventData eventData, GameObject child)
        {
            if (pointerBeginDrag != null) pointerBeginDrag(eventData, child);
        }

        public void HandleDrag(PointerEventData eventData, GameObject child)
        {
            if (pointerDrag != null) pointerDrag(eventData, child);
        }

        public void HandleEndDrag(PointerEventData eventData, GameObject child)
        {
            if (pointerEndDrag != null) pointerEndDrag(eventData, child);
        }

        public void HandleDrop(PointerEventData eventData, GameObject child)
        {
            if (pointerDrop != null) pointerDrop(eventData, child);
        }
    }
}
