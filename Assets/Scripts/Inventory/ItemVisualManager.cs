using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ARPGInventory
{
    public class ItemVisualManager : MonoBehaviour, IDropHandler
    {
        private const string ITEM_NAMING_FORMAT = "Item: {0}";
        private const string HAS_EXISTING_ITEM_VISUAL_FORMAT = "Has existing item visual: {0}";

        Vector2 visualCellSize = new Vector2(32.0f, 32.0f);
        public GameObject itemPrefab;
        public List<ItemVisual> itemVisuals = new List<ItemVisual>();

        Vector2Int inventorySize = new Vector2Int(0, 0);
        public Vector2Int InventorySize { get => inventorySize; set => SetInventorySize(value); }

        private ItemVisualStorage itemVisualStorage;
        public ItemVisualStorage ItemVisualStorage { get => itemVisualStorage = itemVisualStorage != null ? itemVisualStorage : GetComponentsInChildren<ItemVisualStorage>(true)[0]; }

        private Inventory inventory;
        public Inventory Inventory { get => inventory = inventory != null ? inventory : FindObjectOfType<Inventory>(); }

        private RectTransform rectangleTransform;
        public RectTransform RectangleTransform { get => rectangleTransform = rectangleTransform != null ? rectangleTransform : GetComponent<RectTransform>(); }

        private void Start()
        {
            RemoveListeners();
            AddListeners();
        }

        public void AddListeners()
        {
            Inventory.onCreate += OnCreateInventory;
            Inventory.onClear += OnClearInventory;
            Inventory.onItemChange += OnItemDataChange;
            Inventory.onItemDelete += OnItemDataDeleted;
        }

        public void RemoveListeners()
        {
            Inventory.onCreate -= OnCreateInventory;
            Inventory.onClear -= OnClearInventory;
            Inventory.onItemChange -= OnItemDataChange;
            Inventory.onItemDelete -= OnItemDataDeleted;
        }

        public void ClearItemVisuals()
        {
            itemVisuals.ForEach(o => DestroyImmediate(o.gameObject));
            itemVisuals.Clear();
            foreach (Transform child in ItemVisualStorage.transform)
            {
                DestroyImmediate(child.gameObject);
            }
        }

        private void OnItemDataDeleted(ItemSlot itemSlot)
        {
            ItemVisual visualToRemove = itemVisuals.Find(o => o.ItemSlot == itemSlot);
            itemVisuals.Remove(visualToRemove);
        }

        private void OnItemDataChange(ItemSlot itemSlot)
        {
            HandleItemVisualForItemSlot(itemSlot);
        }

        private void OnCreateInventory()
        {
            HandleItemVisuals();
        }

        private void HandleItemVisuals()
        {
            ClearItemVisuals();
            InventorySize = Inventory.Size;
            if (!Inventory.HasItems) return;
            Inventory.itemSlots.ForEach(o => HandleItemVisualForItemSlot(o));
        }

        private void OnClearInventory()
        {
            ClearItemVisuals();
        }

        private void HandleItemVisualForItemSlot(ItemSlot itemSlot)
        {
            var itemVisual = FindItemVisualByItemSlot(itemSlot);
            if (itemVisual != null)
            {
                if(InventoryUI.IsDebugging) Debug.LogFormat(HAS_EXISTING_ITEM_VISUAL_FORMAT, itemVisual.name);
                if (!itemVisual.ItemSlot.HasItems)
                {
                    RemoveEmptyItemVisual(itemVisual);
                    return;                 
                }
                itemVisual.UpdateItem();
                return;
            }
            CreateNewItemVisual(itemSlot);       
        }

        private void CreateNewItemVisual(ItemSlot itemSlot)
        {
            var instantiatedGameObject = Instantiate(itemPrefab, ItemVisualStorage.transform);
            instantiatedGameObject.name = FormatItemVisualName(itemSlot.ItemName);
            var itemVisual = instantiatedGameObject.GetComponent<ItemVisual>();
            itemVisual.Configure(visualCellSize, itemSlot);
            itemVisuals.Add(itemVisual);
        }

        private string FormatItemVisualName(string name)
        {
            return string.Format(ITEM_NAMING_FORMAT, name);
        }

        public ItemVisual FindItemVisualByItemSlot(ItemSlot itemSlot)
        {
            return itemVisuals.Find(o => o.ItemSlot == itemSlot);
        }

        public void RemoveEmptyItemVisual(ItemVisual itemVisualToRemove)
        {
            itemVisuals.Remove(itemVisualToRemove);
            DestroyImmediate(itemVisualToRemove.gameObject);
        }

        public void OnDrop(PointerEventData eventData)
        {
            var draggedObject = eventData.pointerDrag;
            if (draggedObject == null) return;

            ItemVisual itemVisual = draggedObject.GetComponent<ItemVisual>();
            if (itemVisual == null) return;

            itemVisual.isDroppedOnCorrectUIItem = true;

            Vector2 localMousePosition = HelperFunctionLibrary.GetLocalMousePositionOfRectTransform(eventData.position, ItemVisualStorage.RectTransform);
            Vector2 visualPosition = HelperFunctionLibrary.LocalPositionToVisualCoordinate(localMousePosition, visualCellSize);
            Vector2Int cellCoordinate = HelperFunctionLibrary.PositionToCellCoordinate(visualPosition);
            DropItemResult dropResult = Inventory.DragDropItemToCoordinate(itemVisual.ItemSlot, cellCoordinate);

            if (draggedObject == null) return;

            RectTransform draggedObjectRectTransform = draggedObject.GetComponent<RectTransform>();

            if (!dropResult.isAvailableDropSpot)
            {
                MoveItemVisualBackToOriginalPosition(draggedObjectRectTransform, itemVisual);
                return;
            }

            if (!dropResult.isItemsSplit)
            {
                SnapObjectToNewPosition(visualPosition, draggedObjectRectTransform, itemVisual);
                return;
            }

            if (dropResult.isItemsSplit)
            {
                MoveItemVisualBackToOriginalPosition(draggedObjectRectTransform, itemVisual);
                return;
            }
        }

        private void MoveItemVisualBackToOriginalPosition(RectTransform draggedObjectRectTransform, ItemVisual itemVisual)
        {           
            draggedObjectRectTransform.anchoredPosition = itemVisual.DragAndDrop.originalPosition;
        }

        private void SnapObjectToNewPosition(Vector2 cellCoordinate, RectTransform draggedObjectRectTransform, ItemVisual itemVisual)
        {
            Vector2 snapToPosition = HelperFunctionLibrary.SnapToCellPosition(cellCoordinate, draggedObjectRectTransform.sizeDelta, visualCellSize);
            draggedObjectRectTransform.anchoredPosition = snapToPosition;
            itemVisual.DragAndDrop.originalPosition = snapToPosition;
        }

        private void SetInventorySize(Vector2Int newInventorySize)
        {
            if (newInventorySize == InventorySize) return;

            inventorySize = newInventorySize;
            float inventorySizeX = (float)Inventory.Size.x;
            float inventorySizeY = (float)Inventory.Size.y;

            float oldInventoryUISizeX = inventorySizeX * visualCellSize.x;
            float oldInventoryUISizeY = inventorySizeY * visualCellSize.y;
            Vector2 oldInventoryUISize = new Vector2(oldInventoryUISizeX, oldInventoryUISizeY);

            float newInventoryUISizeX = (float)newInventorySize.x * visualCellSize.x;
            float newInventoryUISizeY = (float)newInventorySize.y * visualCellSize.y;
            Vector2 newInventoryUISize = new Vector2(newInventoryUISizeX, newInventoryUISizeY);

            RectTransform InventoryRectTransform = GetComponentInParent<InventoryUI>().GetComponent<RectTransform>();
            InventoryRectTransform.sizeDelta -= oldInventoryUISize;
            InventoryRectTransform.sizeDelta += newInventoryUISize;
            
        }      
    }
}