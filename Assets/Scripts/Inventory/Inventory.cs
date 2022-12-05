using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using static UnityEditor.Progress;
using static UnityEngine.Networking.UnityWebRequest;

namespace ARPGInventory
{
    public class Inventory : MonoBehaviour
    {
        private const string NO_SPACE_IN_INVENTORY = "Not enough space in inventory";
        private const string INVALID_COORDINATE = "Item placing coordinate {0} is out of inventory bounds";
        private const string INVENTORY_COORDINATE_IS_RESERVED = "Inventory coordinate {0} is reserved";
        private const string UNABLE_TO_FIND_ITEM_BY_ID = "Unable to find item with ID {0} from inventory";
        private const string NO_ITEMS_IN_COORDINATE = "No items found in inventory coordinate {0}";
        private const string GET_ITEM_BY_ID_WAS_SUCCESSFULL = "Succesfully Get item named {0} with ID {1} from inventory";
        private const string GET_ITEM_BY_COORDINATE_WAS_SUCCESFULL = "Succesfully Get item named {0} from inventory Coordinate {1}";
        private const string AVAILABLE_CELL = "Inventory Cell x={0},y={1} is available, and got no items";
        private const string INVENTORY_CLEARED = "Inventory cleared";
        private const string STACKABLE_ITEM = "Stacked item in x={0}, y={1}";
        private const string REMOVABLE_ITEM_IS_NULL = "Unable to remove item, removable item is null";
        private const string GET_DATA_FROM_DRAGGED_ITEMSLOT = "iteration: {0} Getting item data from dragged itemslot with item name:";

        public delegate void InventoryDelegate();
        public event InventoryDelegate onCreate;
        public event InventoryDelegate onChange;
        public event InventoryDelegate onClear;

        public delegate void InventoryItemDataDelegate(ItemSlot inventoryItem);
        public event InventoryItemDataDelegate onItemDataDelete;
        public event InventoryItemDataDelegate onItemSlotChange;

        public List<ItemSlot> itemSlots = new List<ItemSlot>();

        public bool HasItems { get => itemSlots.Any(); }
        public int Width { get => Cells.GetLength(0); }
        public int Height { get => Cells.GetLength(1); }

        Vector2Int size = new Vector2Int(16, 8);
        public Vector2Int Size { get => size; set => SetSize(value); }

        InventoryCell[,] cells;
        public InventoryCell[,] Cells { get => GetInventoryCells(); }

        public bool AddItemDataToInventory(InventoryItem item)
        {
            var result = FindFirstPossiblePosition(item);
            if (!result.isSuccess)
            {
                Debug.LogWarning(NO_SPACE_IN_INVENTORY);
                return false;
            }

            ItemSlot itemSlot = GetItemSlotByCoordinate(item, result.coordinate);
            ChangeCellsDataByItemData(item.size, result.coordinate, true, itemSlot);
            onItemSlotChange?.Invoke(itemSlot);
            onChange?.Invoke();
            return true;
        }

        public bool AddItemDataToCoordinate(InventoryItem item, Vector2Int coordinate)
        {
            item.AnchorCoordinate = coordinate;
            var isPossibleSpot = IsItemAreaAvailableInCoordinate(item, coordinate);
            if (!isPossibleSpot) { return isPossibleSpot; }
            ItemSlot itemSlot = GetItemSlotByCoordinate(item, coordinate);
            ChangeCellsDataByItemData(item.size, coordinate, true, itemSlot);
            onItemSlotChange?.Invoke(itemSlot);
            onChange?.Invoke();
            return isPossibleSpot;
        }

        public DropItemResult TryToDropItemSlotToCoordinate(ItemSlot itemSlotToDropFrom, Vector2Int dropCoordinate)
        {
            var dropItemResult = new DropItemResult();

            if (!IsItemAreaAvailableInCoordinate(itemSlotToDropFrom.items.First(),dropCoordinate)) return dropItemResult;

            Vector2Int itemSlotOriginalCoordinate = itemSlotToDropFrom.AnchorCoordinate;
            dropItemResult.isAvailableDropSpot = true;
            ItemSlot itemSlotInCoordinate = FindItemSlotByCoordinate(dropCoordinate);

            if (itemSlotInCoordinate == null) return DropItemSlotToClearSpot(itemSlotToDropFrom, dropItemResult, dropCoordinate, itemSlotOriginalCoordinate);

            dropItemResult = DropItemsInExistingFromItemSlot(itemSlotToDropFrom, itemSlotInCoordinate, dropItemResult);

            if (!itemSlotToDropFrom.HasItems) return RemoveTheLeftOverItemSlot(itemSlotToDropFrom, dropItemResult, itemSlotOriginalCoordinate, itemSlotInCoordinate);

            return HandleSplitSlot(itemSlotToDropFrom, dropItemResult, itemSlotInCoordinate);
        }

        public bool IsStackingItemInCoordinate(Vector2Int coordinate, InventoryItem item)
        {
            var itemAnchorPoint = cells[coordinate.x, coordinate.y].coordinateToTheAnchorPoint;
            ItemSlot itemSlot = FindItemSlotByCoordinate(itemAnchorPoint);       
            var isStacking = itemSlot.ItemName == item.name && itemSlot.IsStackable && !itemSlot.IsStackFull;
            if(isStacking) Debug.LogWarningFormat(STACKABLE_ITEM, coordinate.x, coordinate.y);
            return isStacking;
        }

        public bool IsItemAreaAvailableInCoordinate(InventoryItem item, Vector2Int coordinate)
        {
            var itemAreaCoordinates = item.GetLocalItemAreaCoordinates();
            var offsetCoordinates = OffsetAreaCoordinatesByVector2Int(itemAreaCoordinates, coordinate);
       
            foreach (var offsetCoordinate in offsetCoordinates)
                if (!IsCoordinateAvailable(offsetCoordinate, item)) return false;

            return true;
        }

        public List<Vector2Int> OffsetAreaCoordinatesByVector2Int(List<Vector2Int> areaCoordinates, Vector2Int offset)
        {
            var offsetCoordinates = new List<Vector2Int>();
            areaCoordinates.ForEach(o => offsetCoordinates.Add(o + offset));
            return offsetCoordinates;
        }

        public bool IsCoordinateAvailable(Vector2Int coordinate, InventoryItem item)
        {
            if (IsCoordinateOutOfBounds(coordinate)) return false;

            if (IsCoordinateReserved(coordinate))
            {
                if (IsCoordinateReservedByThisItem(coordinate, item)) return true;
                return IsStackingItemInCoordinate(coordinate, item);
            }
            
            Debug.LogWarningFormat(AVAILABLE_CELL, coordinate.x, coordinate.y);
            return true;
        }

        public bool IsCoordinateReserved(Vector2Int coordinate)
        {
            InventoryCell cell = Cells[coordinate.x, coordinate.y];
            if (cell.isReserved) Debug.LogWarningFormat(INVENTORY_COORDINATE_IS_RESERVED, coordinate);           
            return cell.isReserved;
        }

        public bool IsCoordinateOutOfBounds(Vector2Int coordinate)
        {
            var isOutOfBounds =
                coordinate.x > (Width - 1) ||
                coordinate.x < 0 ||
                coordinate.y > (Height - 1) ||
                coordinate.y < 0;

            if (isOutOfBounds) Debug.LogWarningFormat(INVALID_COORDINATE, coordinate);
            return isOutOfBounds;
        }

        public ItemPlaceSearchResult FindFirstPossiblePosition(InventoryItem item)
        {
            ItemPlaceSearchResult result = new ItemPlaceSearchResult();
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    result = IsCoordinateAvailable(new Vector2Int(x, y), result, item);
                    if (result.isSuccess) return result;
                }
            }
            return result;
        }

        public ItemPlaceSearchResult IsCoordinateAvailable(Vector2Int coordinate, ItemPlaceSearchResult result, InventoryItem item)
        {          
            item.AnchorCoordinate = coordinate;
            result.coordinate = coordinate;
            var isItemFit = IsItemAreaAvailableInCoordinate(item, coordinate);
            result.isSuccess = isItemFit;
            return result;
        }

        public ItemSlot FindItemSlotByID(string id)
        {
            foreach (var itemSlot in itemSlots)
            {
                InventoryItem itemInInventory = itemSlot.FindItemDataByID(id);
                if (itemInInventory != null)
                    return itemSlot;
            }
            Debug.LogWarningFormat(UNABLE_TO_FIND_ITEM_BY_ID, id);
            return null;
        }

        public ItemSlot FindItemSlotByCoordinate(Vector2Int coordinate)
        {
            return itemSlots.Find(o => o.AnchorCoordinate == coordinate);
        }

        public InventoryItem GetItemDataById(string id)
        {
            ItemSlot itemSlot = FindItemSlotByID(id);
            if (itemSlot == null) { Debug.LogWarningFormat(UNABLE_TO_FIND_ITEM_BY_ID, id); return null; }
            InventoryItem itemData = itemSlot.GetItem();
            RemoveItemSlotIfEmpty(itemSlot);
            onItemDataDelete?.Invoke(itemSlot);
            onChange?.Invoke();
            Debug.LogWarningFormat(GET_ITEM_BY_ID_WAS_SUCCESSFULL, itemData.name, id);
            return itemData;
        }

        public InventoryItem GetItemDataByCoordinate(Vector2Int coordinate)
        {
            if (!IsCoordinateOutOfBounds(coordinate)) return null;
            Vector2Int anchorCoordinate = Cells[coordinate.x, coordinate.y].coordinateToTheAnchorPoint;
            ItemSlot itemSlot = FindItemSlotByCoordinate(anchorCoordinate);
            if (itemSlot == null) { Debug.LogWarningFormat(NO_ITEMS_IN_COORDINATE, coordinate); return null; }
            InventoryItem itemData = itemSlot.GetItem();
            RemoveItemSlotIfEmpty(itemSlot);
            onItemDataDelete?.Invoke(itemSlot);
            onChange?.Invoke();
            Debug.LogWarningFormat(GET_ITEM_BY_COORDINATE_WAS_SUCCESFULL, itemData.name, coordinate);
            return itemData;
        }

        public void Clear()
        {
            for (int x = 0; x < Cells.GetLength(0); x++)
            {
                for (int y = 0; y < Cells.GetLength(1); y++)
                {
                    Cells[x, y].isReserved = false;
                    Cells[x, y].coordinateToTheAnchorPoint = new Vector2Int(x, y);
                }
            }
            itemSlots.ForEach(o => o.items.Clear());
            itemSlots.Clear();
            Debug.LogWarning(INVENTORY_CLEARED);
            onChange?.Invoke();
            onClear?.Invoke();
        }

        private bool IsCoordinateReservedByThisItem(Vector2Int coordinate, InventoryItem item)
        {
            Vector2Int cellCoordinateToAnchorPoint = Cells[coordinate.x, coordinate.y].coordinateToTheAnchorPoint;
            ItemSlot itemSlot = FindItemSlotByCoordinate(cellCoordinateToAnchorPoint);
            var isItemInSlot = itemSlot.items.Contains(item);
            var reservedCoordinates = item.GetItemAreaCoordinates();
            var isCoordinateReservedByThisItem = reservedCoordinates.Contains(coordinate) && isItemInSlot;
            if (isCoordinateReservedByThisItem) Debug.LogFormat("Coordinate {0} is reserved by this item: {1}", coordinate, item.name);
            return isCoordinateReservedByThisItem;
        }

        private DropItemResult DropItemSlotToClearSpot(ItemSlot itemSlot, DropItemResult dropItemResult, Vector2Int inventoryCoordinate, Vector2Int itemOriginalSpot)
        {
            ChangeCellsDataByItemData(itemSlot.Size, itemOriginalSpot);
            ChangeCellsDataByItemData(itemSlot.Size, inventoryCoordinate, true, itemSlot);
            dropItemResult.amountOfItemsDropped = itemSlot.StackSize;
            itemSlot.AnchorCoordinate = inventoryCoordinate;
            onItemSlotChange?.Invoke(itemSlot);
            onChange?.Invoke();
            return dropItemResult;
        }

        private DropItemResult HandleSplitSlot(ItemSlot itemSlot, DropItemResult dropItemResult, ItemSlot itemSlotInCoordinate)
        {
            dropItemResult.isItemsSplit = true;
            onItemSlotChange?.Invoke(itemSlotInCoordinate);
            onItemSlotChange?.Invoke(itemSlot);
            onChange?.Invoke();
            return dropItemResult;
        }

        private DropItemResult RemoveTheLeftOverItemSlot(ItemSlot itemSlot, DropItemResult dropItemResult, Vector2Int itemOriginalSpot, ItemSlot itemSlotInCoordinate)
        {
            ChangeCellsDataByItemData(itemSlotInCoordinate.Size, itemOriginalSpot);
            onItemSlotChange?.Invoke(itemSlotInCoordinate);
            onItemSlotChange?.Invoke(itemSlot);
            itemSlots.Remove(itemSlot);
            onChange?.Invoke();
            return dropItemResult;
        }

        private DropItemResult DropItemsInExistingFromItemSlot(ItemSlot itemSlotToDropFrom, ItemSlot itemSlotInCoordinate, DropItemResult dropItemResult)
        {
            int availableStackingSpace = itemSlotInCoordinate.MaxStackSize - itemSlotInCoordinate.StackSize;
            for (int i = 0; i < availableStackingSpace; i++)
            {
                if (!itemSlotToDropFrom.HasItems) return dropItemResult;
                DropItemFromItemSlot(i, itemSlotToDropFrom, itemSlotInCoordinate, dropItemResult);
            }
            return dropItemResult;
        }

        private DropItemResult DropItemFromItemSlot(int iteration, ItemSlot itemSlotToDropFrom, ItemSlot itemSlotInCoordinate, DropItemResult dropItemResult)
        {
            Debug.LogFormat(GET_DATA_FROM_DRAGGED_ITEMSLOT, iteration);
            itemSlotInCoordinate.AddItem(itemSlotToDropFrom.GetItem());
            dropItemResult.amountOfItemsDropped++;
            return dropItemResult;
        }

        private void RemoveItemSlotIfEmpty(ItemSlot itemSlot)
        {
            if (itemSlot == null) Debug.LogWarningFormat(REMOVABLE_ITEM_IS_NULL);
            if (!itemSlot.HasItems) itemSlots.Remove(itemSlot);
        }

        private void ChangeCellsDataByItemData(Vector2Int itemSize, Vector2Int coordinate, bool isReserved = false, ItemSlot itemSlot = null)
        {
            for (int x = 0; x < itemSize.x; x++)
            {
                for (int y = 0; y < itemSize.y; y++)
                {
                    ChangeCellData(x, y, coordinate, isReserved);
                }
            }
        }

        private void ChangeCellData(int itemCoordinateX, int itemCoordinateY, Vector2Int coordinate, bool isReserved)
        {
            int coordX = coordinate.x + itemCoordinateX;
            int coordY = coordinate.y + itemCoordinateY;
            Cells[coordX, coordY].isReserved = isReserved;
            Cells[coordX, coordY].coordinateToTheAnchorPoint = coordinate;
        }

        private void SetSize(Vector2Int newSize)
        {
            if (newSize == size) return;
            Clear();
            size = newSize;
            cells = CreateInventoryCells();
        }

        private ItemSlot AddNewItemSlot(InventoryItem item, Vector2Int coordinate)
        {
            ItemSlot newItemSlot = new ItemSlot(item, coordinate);
            itemSlots.Add(newItemSlot);
            return newItemSlot;
        }

        private InventoryCell[,] GetInventoryCells()
        {
            if (cells != null) return cells;
            return CreateInventoryCells();
        }

        private InventoryCell[,] CreateInventoryCells()
        {
            cells = new InventoryCell[Size.x, Size.y];
            for (int x = 0; x < Width; x++)
            {
                for (int y = Height - 1; y >= 0; y--)
                {
                    cells[x, y] = new InventoryCell(x, y, false);
                }
            }

            onCreate?.Invoke();
            return cells;
        }

        private ItemSlot GetItemSlotByCoordinate(InventoryItem item, Vector2Int coordinate)
        {
            ItemSlot itemSlot = FindItemSlotByCoordinate(coordinate);
            if (itemSlot != null) itemSlot.AddItem(item);
            else itemSlot = AddNewItemSlot(item, coordinate);
            return itemSlot;
        }
    }

    public struct ItemPlaceSearchResult
    {
        public bool isSuccess;
        public Vector2Int coordinate;
    }

    public struct DropItemResult
    {
        public bool isAvailableDropSpot;
        public bool isItemsSplit;
        public int amountOfItemsDropped;
    }
}