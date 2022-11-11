using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

[Serializable]
public class InventorySystem
{
    protected List<InventoryItem> InventoryItems;
    //private Dictionary<ItemDataBase, InventoryItem> ItemDict;

    /// <summary>
    /// Max amount of items the inventory can hold
    /// </summary>
    public int InventorySize { get { return InventoryItems.Count; } }

    public bool IsShop;

    public event Action<InventorySystem> OnInventoryChanged;

   
    public InventorySystem(int size, bool isShop = false)
    {
        IsShop = isShop;
        InventoryItems = new List<InventoryItem>(size);
        
        for (int i = 0; i < size; i++)
            InventoryItems.Add(null);
    }


    /// <summary>
    /// Try to add the item to the list.
    /// </summary>
    /// <returns>True on success, false on failure (inventory is full)</returns>
    public bool AddItem(InventoryItem newItem)
    {
        var existingItemStack = InventoryItems.FirstOrDefault(x => x != null && x.ItemData.Id == newItem.ItemData.Id);
        
        //if a stack for the item already exists and is not yet full, add the new item to that stack
        if (existingItemStack != null && existingItemStack.ItemData.MaxStackSize > existingItemStack.StackSize)
        {
            existingItemStack.AddToStack(newItem.StackSize);
            OnInventoryChanged?.Invoke(this);

            return true;
        }

        //no such stack exists, try to create a new stack if there is space (null means empty)
        int emptySpace = GetEmptySpaceIndex();

        if (emptySpace == -1)   //no space for the new item
            return false;

        InventoryItems[emptySpace] = newItem;
        OnInventoryChanged?.Invoke(this);

        return true;
    }

    /// <summary>
    /// Places item into the specified slot index. 
    /// </summary>
    /// <returns>What was previously at that spot (null if empty) or item on failure</returns>
    public InventoryItem PlaceItemAtSlot(InventoryItem item, int slotIndex)
    {
        if (slotIndex >= InventorySize)
            return item;

        var prevItem = InventoryItems[slotIndex];

        InventoryItems[slotIndex] = item;
        OnInventoryChanged?.Invoke(this);

        return prevItem;
    }

    /// <summary>
    /// Finds an empty space and returns its index
    /// </summary>
    /// <returns>Index of the first empty space. Or -1 if all spaces are taken.</returns>
    protected int GetEmptySpaceIndex()
    {
        for (int i = 0; i < InventoryItems.Count; i++)
        {
            if (InventoryItems[i] == null)
                return i;
        }

        return -1;
    }

    /// <returns>The amount of items the inventory is currently holding</returns>
    //public int GetItemAmount()
    //{
    //    return InventoryItems.Count;
    //}

    /// <summary>
    /// Increase the amount of items the inventory can hold
    /// </summary>
    /// <param name="amount"></param>
    public void AddSpace(int amount)
    {
        InventoryItems.Capacity += amount;

        for (int i = 0; i < amount; i++)
            InventoryItems.Add(null);

        OnInventoryChanged?.Invoke(this);
    }

    public InventoryItem GetItemAt(int index)
    {
        if (index < InventorySize)
            return InventoryItems[index];

        return null;
    }

    public List<InventoryItem> GetItems()
    {
        return InventoryItems;
    }

    /// <summary>
    /// Moves item at itemIndex position to targetIndex position. Handles any necessary swapping & stacking
    ///  and notifies the caller what it did through ItemMoveResult enum.
    /// </summary>
    public ItemMoveResult MoveItemToTarget(int itemIndex, InventorySystem targetInventory, int targetIndex)
    {
        var invItem = InventoryItems[itemIndex];
        var invItemTarget = targetInventory.InventoryItems[targetIndex];

        if (itemIndex >= InventorySize || targetIndex >= targetInventory.InventorySize)
        {
            Debug.LogWarning($"Failed to move item from {itemIndex} to {targetIndex}. Index out of range (InventorySize: {targetInventory.InventorySize})");
            return ItemMoveResult.NoChange;
        }

        if (invItem == null)
        {
            Debug.LogWarning($"Failed to move item from {itemIndex} to {targetIndex}. There is no item at index {itemIndex}");
            return ItemMoveResult.NoChange;
        }

        //if the item is moved to the same item stack and neither stack is full, join the stacks
        if (invItemTarget != null &&
            invItem.ItemData.Id == invItemTarget.ItemData.Id &&
            invItemTarget.ItemData.MaxStackSize > 1 &&
            invItemTarget.StackSize < invItemTarget.ItemData.MaxStackSize &&
            invItem.StackSize < invItem.ItemData.MaxStackSize
        )
        {
            return StackItemToTarget(itemIndex, targetInventory, targetIndex);
        }

        #region Shop

        //buying from shop
        if (this != targetInventory && this.IsShop)
        {
            //if not enough currency reject transaction
            if (!invItem.CanAfford())
                return ItemMoveResult.NoChange;

            GameManager.Instance.Currency -= invItem.ItemData.BuyPrice;
            invItem.IsShopOwned = false;
        }
        //selling to shop
        if (this != targetInventory && targetInventory.IsShop)
        {
            GameManager.Instance.Currency += invItem.GetSellPrice();
            invItem.IsShopOwned = true;
        }

        #endregion Shop


        bool hasSwapped = invItemTarget != null;

        //swap items
        if (targetInventory is EquipmentSystem)
            (targetInventory as EquipmentSystem).EquipItem(invItem, targetIndex);
        else
            targetInventory.PlaceItemAtSlot(invItem, targetIndex);

        if (this is EquipmentSystem)
            (this as EquipmentSystem).EquipItem(invItemTarget, itemIndex);
        else
            this.PlaceItemAtSlot(invItemTarget, itemIndex);

        //already called in EquipItem/PlaceItemAtSlot
        //OnInventoryChanged?.Invoke(this);
        //if (targetInventory != this)
        //    targetInventory.OnInventoryChanged.Invoke(targetInventory);

        return hasSwapped ? ItemMoveResult.Swapped : ItemMoveResult.Moved;
    }

    public ItemMoveResult StackItemToTarget(int itemIndex, InventorySystem targetInventory, int targetIndex)
    {
        var invItem = InventoryItems[itemIndex];
        var invItemTarget = targetInventory.InventoryItems[targetIndex];
        int spaceOnStack = invItemTarget.ItemData.MaxStackSize - invItemTarget.StackSize;
        int moveAmount = invItem.StackSize > spaceOnStack ? spaceOnStack : invItem.StackSize;

        invItem.RemoveFromStack(moveAmount);
        invItemTarget.AddToStack(moveAmount);


        if (invItem.StackSize <= 0)
        {
            RemoveItemAt(itemIndex);
            //called inside removeItemAt
            //OnInventoryChanged?.Invoke()
            return ItemMoveResult.StackedAll;
        }
        else
        {
            OnInventoryChanged?.Invoke(this);
            return ItemMoveResult.StackedWithRemainder;
        }
    }

    private void RemoveItemAt(int itemIndex)
    {
        if (itemIndex >= InventorySize)
            return;

        InventoryItems[itemIndex] = null;

        OnInventoryChanged?.Invoke(this);
    }

    /// <summary>
    /// Get the index of the first free inventory slot, or -1 if the inventory is full.
    /// </summary>
    public int FirstFreeSlotIndex()
    {
        if (InventoryItems == null)
            return -1;

        for (int i = 0; i < InventoryItems.Count; i++)
        {
            if (InventoryItems[i] == null)
                return i;
        }

        return -1;
    }

    public void Refresh()
    {
        for (int i= 0; i < InventoryItems.Count; i++)
        {
            var item = InventoryItems[i];
            if (item == null)
                continue;

            if (item.StackSize <= 0)
            {
                UnityEngine.Object.Destroy(item.Prefab.gameObject);
                InventoryItems[i] = null;
            }
        }
    }

    public void SetInventoryItemList(List<InventoryItem> inventoryItems)
    {
        InventoryItems = inventoryItems;
    }
}
