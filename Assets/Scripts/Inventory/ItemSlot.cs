using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ARPGInventory
{
    [System.Serializable]
    public class ItemSlot
    {
        private const string GetStackedItemsMessage = "Items are stacked, returning the last item of the stack.";
        private const string ITEM_ADDED_SUCCESFULLY = "Item named {0} ID {1} was successfully added to coordinate {2} in inventory";

        public List<InventoryItem> items = new List<InventoryItem>();

        public bool IsStackFull { get => GetIsStackFull(); }
        public bool IsStackable { get => GetIsStackable(); }
        public bool HasItems { get => items.Any(); }
        public int StackSize { get => items.Count; }
        public int MaxStackSize { get => GetMaxStackSize(); }
        public string ItemName { get => items.First().name; }
        public Vector2Int Size { get => items.First().size; }
        public Texture2D ItemTexture { get => items.First().texture; }

        private Vector2Int anchorCoordinate = new Vector2Int();
        public Vector2Int AnchorCoordinate { get => anchorCoordinate; set => SetAnchorCoordinate(value); }

        public ItemSlot(InventoryItem item, Vector2Int anchorCoordinate)
        {
            this.AnchorCoordinate = anchorCoordinate;         
            AddItem(item);         
        }

        public ItemSlot(List<InventoryItem> items, Vector2Int anchorCoordinate)
        {
            this.AnchorCoordinate = anchorCoordinate;
            items.ForEach(o => AddItem(o));        
        }

        public void RemoveItemDataByID(string id)
        {
            InventoryItem itemDataToRemove = items.Find(o => o.id == id);
            items.Remove(itemDataToRemove);
        }

        public InventoryItem FindItemDataByID(string id)
        {
            return items.Find(o => o.id == id);
        }

        public InventoryItem FindItemData(InventoryItem itemData)
        {
            return items.Find(o => o == itemData);
        }

        public bool AddItem(InventoryItem item)
        {          
            if (HasItems && (IsStackFull || item == null)) return false;
            item.AnchorCoordinate = this.AnchorCoordinate;
            items.Add(item);
            Debug.LogWarningFormat(ITEM_ADDED_SUCCESFULLY, item.name, item.id, AnchorCoordinate);
            return true;
        }

        public void RemoveItemData(InventoryItem itemData)
        {
            items.Remove(itemData);
        }

        public InventoryItem GetItem()
        {
            if (!HasItems) return null;
            InventoryItem inventoryItemData = null;
            if (IsStackable)
            {
                inventoryItemData = items.Last();
                Debug.LogWarningFormat(GetStackedItemsMessage);
            }
            else
            {
                inventoryItemData = items.First();
            }
            RemoveItemData(inventoryItemData);
            return inventoryItemData;
        }

        private bool GetIsStackable()
        {
            return items.First().isStackable;
        }

        private bool GetIsStackFull()
        {
            return StackSize >= MaxStackSize;
        }

        private int GetMaxStackSize()
        {
            int maxStack = 1;
            if (HasItems) maxStack = items.First().maxStackSize;
            return maxStack;
        }
        private void SetAnchorCoordinate(Vector2Int value)
        {
            if (anchorCoordinate == value) return;
            anchorCoordinate = value;
            if (!HasItems) return;
            items.ForEach(o => o.AnchorCoordinate = anchorCoordinate);
        }
    }
}