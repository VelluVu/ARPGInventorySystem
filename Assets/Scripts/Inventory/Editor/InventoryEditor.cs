using ARPGInventory;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Inventory))]
public class InventoryEditor : Editor
{
    private const string ADD_ITEM = "ADD ITEM TO INVENTORY";
    private const string ADD_ITEM_TO_COORDINATE = "ADD ITEM TO INVENTORY COORDINATE";
    private const string CLEAR = "CLEAR INVENTORY";
    private const string PARAMETERS_FOR_TESTING = "Parameters for testing the inventory";
    private const string GET_ITEM_BY_ID = "Get Item By ID";
    private const string GET_ITEM_BY_COORDINATE = "Get Item By Coordinate";
    private const string FOUND_ITEM_BY_ID = "Found item named {0} from inventory with ID {1}";
    private const string FOUND_ITEM_BY_COORDINATE = "Found item named {0} from inventory coordinate {1} with ID {2}";
    private const string INVENTORY_SIZE = "Inventory Size";
    private const string INVENTORY_COORDINATE = "Inventory Coordinate";
    private const string SET_NEW_SIZE = "Set new inventory size";

    public ItemSO testItem;
    public Vector2Int newInventorySize = new Vector2Int(64, 32);
    public Vector2Int coordinate = new Vector2Int(0, 0);
    public Dictionary<string, Item> addedItems = new Dictionary<string, Item>();

    public override void OnInspectorGUI()
    {
        var inventory = (Inventory)target;
        base.OnInspectorGUI();
        newInventorySize = EditorGUILayout.Vector2IntField(INVENTORY_SIZE, newInventorySize);
        if (GUILayout.Button(SET_NEW_SIZE))
        {
            inventory.Size = newInventorySize;
        }
        EditorGUILayout.Space(10f);
        EditorGUILayout.LabelField(PARAMETERS_FOR_TESTING);
        coordinate = EditorGUILayout.Vector2IntField(INVENTORY_COORDINATE, coordinate);
        testItem = (ItemSO)EditorGUILayout.ObjectField(testItem, typeof(ItemSO), true);

        GUILayout.Space(100f);
        if (GUILayout.Button(ADD_ITEM))
        {
            Item item = new Item(testItem);
            var isSuccess = inventory.AddItemToFirstAvailableCoordinate(new InventoryItem(item));
            if (isSuccess) addedItems.Add(item.Id, item);
        }

        if (GUILayout.Button(ADD_ITEM_TO_COORDINATE))
        {
            Item item = new Item(testItem);
            var isSuccess = inventory.AddItemToCoordinate(new InventoryItem(item), coordinate);
            if (isSuccess) addedItems.Add(item.Id, item);
        }

        if (GUILayout.Button(GET_ITEM_BY_ID))
        {
            string id = addedItems.Last().Key;
            InventoryItem itemData = inventory.GetItemById(id);
            if (itemData != null)
            {
                Debug.LogFormat(FOUND_ITEM_BY_ID, itemData.name, itemData.id);
                addedItems.Remove(id);
            }
        }

        if (GUILayout.Button(GET_ITEM_BY_COORDINATE))
        {
            InventoryItem itemData = inventory.GetItemFromCoordinate(coordinate);
            if (itemData != null)
            {
                Debug.LogFormat(FOUND_ITEM_BY_COORDINATE, itemData.name, coordinate, itemData.id);
                addedItems.Remove(itemData.id);
            }
        }

        if (GUILayout.Button(CLEAR))
        {
            inventory.Clear();
            addedItems.Clear();
        }   
    }
}
