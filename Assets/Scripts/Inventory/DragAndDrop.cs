using ARPGInventory;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ARPGInventory
{
    public class DragAndDrop : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        private Canvas canvas;
        public Canvas Canvas { get => canvas = canvas != null ? canvas : GetComponentsInParent<Canvas>()[0]; }

        private CanvasGroup canvasGroup;
        public CanvasGroup CanvasGroup { get => canvasGroup = canvasGroup != null ? canvasGroup : GetComponent<CanvasGroup>(); }

        private RectTransform rectTransform;
        public RectTransform RectTransform { get => rectTransform = rectTransform != null ? rectTransform : GetComponent<RectTransform>(); }

        public Vector2 originalPosition;

        public delegate void DragDelegate(Vector2 originalPosition, Vector2 newPosition);
        public event DragDelegate onDragEnd;
        public event DragDelegate onDragBegin;

        public void OnBeginDrag(PointerEventData eventData)
        {
            onDragBegin?.Invoke(originalPosition, originalPosition);
            CanvasGroup.blocksRaycasts = false;
            CanvasGroup.alpha = 0.7f;
        }

        public void OnDrag(PointerEventData eventData)
        {
            RectTransform.anchoredPosition += eventData.delta / Canvas.scaleFactor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            onDragEnd?.Invoke(originalPosition, Input.mousePosition);
            CanvasGroup.alpha = 1.0f;
            CanvasGroup.blocksRaycasts = true;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            originalPosition = RectTransform.anchoredPosition;
        }
    }
}