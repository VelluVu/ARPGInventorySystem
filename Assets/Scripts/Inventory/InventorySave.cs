using ARPGInventory;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// IN PROGRESS
/// </summary>
[System.Serializable]
public class InventorySave
{
    public Vector2Int inventorySize = new Vector2Int();
    public List<ItemSlotSave> itemSlotSaves = new List<ItemSlotSave>();

    public InventorySave(Inventory inventory)
    {
        inventory.itemSlots.ForEach(o => itemSlotSaves.Add(new ItemSlotSave(o.items, o.AnchorCoordinate)));
        inventorySize = inventory.Size;
    }

    public Inventory Load()
    {
        Inventory inventory = new Inventory();
        itemSlotSaves.ForEach(o => inventory.itemSlots.Add(o.Unload()));
        inventory.Size = inventorySize;
        return inventory;
    }
}
