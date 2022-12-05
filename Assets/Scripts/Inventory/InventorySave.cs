using ARPGInventory;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventorySave
{
    public Vector2Int size = new Vector2Int();
    public List<ItemSlotSave> itemSlotSaveDatas = new List<ItemSlotSave>();

    public InventorySave(Inventory inventory)
    {
        inventory.itemSlots.ForEach(o => itemSlotSaveDatas.Add(new ItemSlotSave(o.items, o.AnchorCoordinate)));
        size = inventory.Size;
    }

    public Inventory Unload()
    {
        Inventory inventory = new Inventory();
        itemSlotSaveDatas.ForEach(o => inventory.itemSlots.Add(o.Unload()));
        inventory.Size = size;
        return inventory;
    }
}
