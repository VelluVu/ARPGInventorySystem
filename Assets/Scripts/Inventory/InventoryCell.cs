using UnityEngine;

namespace ARPGInventory
{
    public class InventoryCell
    {
        public bool isReserved = false;
        public readonly int coordinateX = 0;
        public readonly int coordinateY = 0;
        public Vector2Int coordinateToTheAnchorPoint;

        public InventoryCell(int coordinateX, int coordinateY, bool isReserved)
        {
            this.coordinateX = coordinateX;
            this.coordinateY = coordinateY;
            this.isReserved = isReserved;
            coordinateToTheAnchorPoint = new Vector2Int(coordinateX, coordinateY);
        }
    }
}