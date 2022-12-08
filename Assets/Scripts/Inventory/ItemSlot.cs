using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ARPGInventory
{
    [System.Serializable]
    public class ItemSlot
    {
        private const string GET_STACKED_ITEMS = "Items are stacked, returning the last item of the stack.";
        private const string ITEM_ADDED_SUCCESFULLY = "Item named {0} ID {1} was successfully added to coordinate {2} in inventory";

        public List<InventoryItem> items = new List<InventoryItem>();

        public bool IsStackFull { get => GetIsStackFull(); }
        public bool IsStackable { get => GetIsStackable(); }
        public bool HasItems { get => items.Any(); }
        public int StackSize { get => items.Count; }
        public int MaxStackSize { get => GetMaxStackSize(); }
        public string ItemName { get => items.First().name; }
        public Vector2Int Size { get => items.First().size; }
        public Sprite ItemIcon { get => items.First().icon; }

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

        public void RemoveItemByID(string id)
        {
            InventoryItem itemDataToRemove = items.Find(o => o.id == id);
            items.Remove(itemDataToRemove);
        }

        public InventoryItem FindItemByID(string id)
        {
            return items.Find(o => o.id == id);
        }

        public InventoryItem FindItem(InventoryItem itemData)
        {
            return items.Find(o => o == itemData);
        }

        public bool AddItem(InventoryItem item)
        {          
            if (HasItems && (IsStackFull || item == null)) return false;
            item.AnchorCoordinate = this.AnchorCoordinate;
            items.Add(item);
            if(Inventory.IsDebugging) Debug.LogWarningFormat(ITEM_ADDED_SUCCESFULLY, item.name, item.id, AnchorCoordinate);
            return true;
        }

        public void RemoveItem(InventoryItem itemData)
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
                if (Inventory.IsDebugging) Debug.LogWarningFormat(GET_STACKED_ITEMS);
            }
            else
            {
                inventoryItemData = items.First();
            }
            RemoveItem(inventoryItemData);
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