using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ARPGInventory
{
    [ExecuteAlways]
    public class ItemVisual : MonoBehaviour
    {
        public bool isDroppedOnCorrectUIItem = false;

        private Image image;
        public Image Image { get => image = image != null ? image : GetComponent<Image>(); }

        private TextMeshProUGUI textMesh;
        public TextMeshProUGUI TextMesh { get => textMesh = textMesh != null ? textMesh : GetComponentsInChildren<TextMeshProUGUI>(true)[0]; }

        private ItemSlot itemSlot;
        public ItemSlot ItemSlot { get => itemSlot; set => SetItem(value); }

        private RectTransform rectangleTransform;
        public RectTransform RectangleTransform { get => rectangleTransform = rectangleTransform != null ? rectangleTransform : gameObject.GetComponent<RectTransform>(); }

        private DragAndDrop dragAndDrop;
        public DragAndDrop DragAndDrop { get => dragAndDrop = dragAndDrop != null ? dragAndDrop : GetComponent<DragAndDrop>(); }

        public Vector2 size = new Vector2(32f, 32f);

        public delegate void ItemVisualDragDelegate(ItemVisual itemVisual, Vector2 newPosition);
        public event ItemVisualDragDelegate onDragEnd;
        public event ItemVisualDragDelegate onDragBegin;

        private void OnEnable()
        {
            AddListeners();
        }

        private void OnDisable()
        {
            RemoveListeners();
        }

        public void AddListeners()
        {
            DragAndDrop.onDragBegin += OnDragBegin;
            DragAndDrop.onDragEnd += OnDragEnd;          
        }

        public void RemoveListeners()
        {
            dragAndDrop.onDragBegin -= OnDragBegin;
            dragAndDrop.onDragEnd -= OnDragEnd;
        }

        private void OnDragBegin(Vector2 originalPosition, Vector2 newPosition)
        {
            onDragBegin?.Invoke(this, newPosition);
        }

        public void UpdateItem()
        {
            ActivateTextComponent();
        }

        private void OnDragEnd(Vector2 originalPosition, Vector2 newPosition)
        {
            onDragEnd?.Invoke(this, newPosition);
            SnapToOriginalPositionIfDroppedOnWrongArea(originalPosition);
        }

        private void SnapToOriginalPositionIfDroppedOnWrongArea(Vector2 originalPosition)
        {
            if (!isDroppedOnCorrectUIItem) RectangleTransform.anchoredPosition = originalPosition;
            isDroppedOnCorrectUIItem = false;
        }

        private void UpdateImageUIRect(Vector2Int itemSize, Vector2Int itemAnchorPoint)
        {
            Vector2 rectSizeDelta = new Vector2(itemSize.x * size.x, itemSize.y * size.y);
            RectangleTransform.sizeDelta = rectSizeDelta;
            Vector2 anchoredPosition = new Vector2(rectSizeDelta.x * 0.5f + itemAnchorPoint.x * size.x, rectSizeDelta.y * -0.5f - itemAnchorPoint.y * size.y);
            RectangleTransform.anchoredPosition = anchoredPosition;
        }

        private void SetSprite(Sprite icon)
        {          
            Image.sprite = icon;
        }

        private void ActivateTextComponent()
        {
            if (!itemSlot.IsStackable)
            {
                if (TextMesh.gameObject.activeSelf)
                    TextMesh.gameObject.SetActive(false);
                return;
            }

            if (!TextMesh.gameObject.activeSelf)
                TextMesh.gameObject.SetActive(true);

            ChangeText(itemSlot.StackSize.ToString());
        }

        private void ChangeText(string newText)
        {
            TextMesh.SetText(newText);
#if UNITY_EDITOR
            TextMesh.enabled = false;
            TextMesh.enabled = true;
#endif
        }

        private void SetItem(ItemSlot inventoryItem)
        {
            itemSlot = inventoryItem;
            UpdateImageUIRect(itemSlot.Size, itemSlot.AnchorCoordinate);
            SetSprite(inventoryItem.ItemIcon);
            ActivateTextComponent();
        }

        public void Configure(Vector2 cellSize, ItemSlot itemSlot)
        {
            size = cellSize;
            ItemSlot = itemSlot;
        }
    }
}