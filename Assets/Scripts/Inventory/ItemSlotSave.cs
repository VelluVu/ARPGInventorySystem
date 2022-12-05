using ARPGInventory;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemSlotSave
{
    public List<InventoryItem> itemDatas = new List<InventoryItem>();
    public Vector2Int anchorCoordinate = new Vector2Int();

    public ItemSlotSave(List<InventoryItem> itemDatas, Vector2Int anchorCoordinate)
    {
        this.itemDatas = itemDatas;
        this.anchorCoordinate = anchorCoordinate;
    }

    public ItemSlot Unload()
    {
        ItemSlot itemSlot = new ItemSlot(itemDatas, anchorCoordinate);   
        return itemSlot;
    }
}