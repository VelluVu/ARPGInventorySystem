using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item: ", menuName = "Items/Item")]
public class ItemSO : ScriptableObject
{
    public string Name = "Some Item";
    public bool isStackable = false;
    public int maxStackSize = 1;
    public ItemUsageType usage = ItemUsageType.Consumable;
    public Vector2Int size = new Vector2Int(1,1);
    public Texture2D texture;
    public List<Stat> stats;
}
