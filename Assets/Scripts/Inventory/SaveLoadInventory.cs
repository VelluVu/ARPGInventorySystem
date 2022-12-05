using ARPGInventory;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ARPGInventory
{
    public static class SaveLoadInventory
    {
        private const string INVALID_PATH = "No Save File in path: {0}";
        static readonly string savePath = Application.persistentDataPath + "/";
        static readonly string saveFileName = "/inventory.json";
        static string fullSavePath = "";

        static List<string> saves = new List<string>();

        public static void Save(Inventory inventory, string saveFolderName)
        {
            InventorySave inventorySaveData = new InventorySave(inventory);
            var inventoryInJsonFormat = JsonUtility.ToJson(inventorySaveData);
            fullSavePath = savePath + saveFolderName + saveFileName;
        }

        public static void Load()
        {
            if (!File.Exists(fullSavePath))
            {
                Debug.LogWarningFormat(INVALID_PATH, fullSavePath);
            }
        }
    }
}