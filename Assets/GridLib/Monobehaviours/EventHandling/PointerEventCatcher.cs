using UnityEngine;
using UnityEngine.EventSystems;

namespace GridLib.EventHandling
{
    [RequireComponent(typeof(Collider2D))]
    public class PointerEventCatcher :
        MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerClickHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IDropHandler
    {
        public PointerEventCentralizer parent = null;

        private void DebugPrint(PointerEventData eventData)
        {
            const bool debug = false;

#pragma warning disable CS0162 // Unreachable code detected
            if (debug) Debug.Log(gameObject.name + ": " + eventData);
#pragma warning restore CS0162 // Unreachable code detected
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            DebugPrint(eventData);
            if (parent != null) parent.HandleEnter(eventData, gameObject);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            DebugPrint(eventData);
            if (parent != null) parent.HandleExit(eventData, gameObject);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            DebugPrint(eventData);
            if (parent != null) parent.HandleDown(eventData, gameObject);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            DebugPrint(eventData);
            if (parent != null) parent.HandleUp(eventData, gameObject);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            DebugPrint(eventData);
            if (parent != null) parent.HandleClick(eventData, gameObject);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            DebugPrint(eventData);
            if (parent != null) parent.HandleBeginDrag(eventData, gameObject);
        }

        public void OnDrag(PointerEventData eventData)
        {
            DebugPrint(eventData);
            if (parent != null) parent.HandleDrag(eventData, gameObject);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            DebugPrint(eventData);
            if (parent != null) parent.HandleEndDrag(eventData, gameObject);
        }

        public void OnDrop(PointerEventData eventData)
        {
            DebugPrint(eventData);
            if (parent != null) parent.HandleDrop(eventData, gameObject);
        }
    }
}
