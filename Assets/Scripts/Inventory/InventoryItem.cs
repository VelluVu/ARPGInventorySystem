using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ARPGInventory
{
    [System.Serializable]
    public class InventoryItem
    {
        public readonly bool isStackable;
        public readonly int maxStackSize;
        public readonly string name;
        public readonly string id;
        public readonly Vector2Int size;
        public readonly Texture2D texture;

        private Vector2Int anchorCoordinate = new Vector2Int(0,0);
        public Vector2Int AnchorCoordinate { set => SetAnchorCoordinate(value); get => anchorCoordinate; }

        private Vector2Int previousAnchorCoordinate = new Vector2Int(0,0);
        public Vector2Int PreviousAnchorCoordinate { get => previousAnchorCoordinate; }

        public InventoryItem(bool isStackable, int maxStackSize, string name, string id, Vector2Int size, Texture2D texture)
        {
            this.isStackable = isStackable;
            this.maxStackSize = maxStackSize;
            this.name = name;
            this.id = id;
            this.size = size;
            this.texture = texture;
        }

        /// <summary>
        /// IF DOWNLOADED THIS INVENTORY SYSTEM, CREATE NEW CONSTRUCTOR LIKE THIS, SO IT WORKS WITH YOUR ITEM SYSTEM!
        /// </summary>
        /// <param name="item"></param>
        public InventoryItem(Item item)
        {
            isStackable = item.staticData.isStackable;
            maxStackSize = item.staticData.maxStackSize;
            name = item.staticData.Name;
            id = item.Id;
            size = item.staticData.size;
            texture = item.staticData.texture;
        }

        public List<Vector2Int> GetItemAreaCoordinates()
        {
            List<Vector2Int> reservedCoordinates = new List<Vector2Int>();
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    reservedCoordinates.Add(new Vector2Int(x + AnchorCoordinate.x, y + AnchorCoordinate.y));
                }
            }
            return reservedCoordinates;
        }

        public List<Vector2Int> GetLocalItemAreaCoordinates()
        {
            List<Vector2Int> localCoordinates = new List<Vector2Int>();
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    localCoordinates.Add(new Vector2Int(x, y));
                }
            }
            return localCoordinates;
        }

        private void SetAnchorCoordinate(Vector2Int value)
        {
            if (value == anchorCoordinate) return;          
            previousAnchorCoordinate = anchorCoordinate;
            anchorCoordinate = value;
            Debug.LogFormat("Setting new anchor coordinate: {0}, and now previous is coordinate is: {1}", anchorCoordinate, previousAnchorCoordinate);
        }
    }
}