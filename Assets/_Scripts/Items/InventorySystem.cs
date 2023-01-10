using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

[Serializable]
public class InventorySystem
{
    protected List<InventoryItem> InventoryItems;

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


    public InventorySystem(InventorySystemSaveData data)
    {
        InventoryItems = new List<InventoryItem>();

        foreach (var itemData in data.itemDataList)
        {
            var invItem = new InventoryItem(itemData);

            if (invItem.ItemData == null)
                InventoryItems.Add(null);
            else
                InventoryItems.Add(invItem);
        }

        IsShop = data.isShop;

        if (this is EquipmentSystem)
        {
            (this as EquipmentSystem).IsRuneSystem = data.isRuneSystem;
        }
    }


    public InventorySystemSaveData GetSaveData()
    {
        List<InventoryItemSaveData> itemData = new List<InventoryItemSaveData>();

        if (InventoryItems == null)
            itemData = null;
        else
            foreach (var item in InventoryItems)
            {
                if (item == null)
                    itemData.Add(null);
                else
                    itemData.Add(item.GetSaveData());
            }

        bool isRuneSystem = this is EquipmentSystem ?
            (this as EquipmentSystem).IsRuneSystem
            :
            false;

        InventorySystemSaveData data = new InventorySystemSaveData(
            itemData,
            IsShop,
            isRuneSystem
        );

        return data;
    }


    /// <summary>
    /// Try to add the item to the list.
    /// </summary>
    /// <returns>True on success, false on failure (inventory is full)</returns>
    public bool AddItem(InventoryItem newItem)
    {
        if (newItem == null)
        {
            Debug.LogWarning("Tried adding a null item to inventory... Possible semantic error");
            return false;
        }

        //try to stack the item to existing stacks
        foreach (var item in InventoryItems)
        {
            if (item != null &&
                item.ItemData.Id.Equals(newItem.ItemData.Id) &&
                item.ItemData.MaxStackSize > item.StackSize)
            {   //found an item we can stack to
                int remainder = item.AddToStack(newItem.StackSize);
                newItem.StackSize = remainder;
                
                if (remainder == 0) //if we stacked the entirety of the item break, else search for another stack
                    break;
            }
        }

        if (newItem.StackSize != 0)
        {
            //if we havent stacked the entire item, try to create a new stack if there is space (null means empty)
            int emptySpace = FirstFreeSlotIndex();

            if (emptySpace == -1)   //no space for the new item
                return false;

            InventoryItems[emptySpace] = newItem;
        }

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

    /// <returns>Returns the index of the item in parameter, or -1 if no such item exists in this inventory</returns>
    public int GetItemIndex(InventoryItem item)
    {
        for (int i = 0; i < InventoryItems.Count; i++)
        {
            if (InventoryItems[i] == item)
                return i;
        }

        return -1;
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

        Debug.Log($"Moving item [{itemIndex}: {invItem.ItemData.DisplayName}] to [{targetIndex}: {(invItemTarget == null ? "empty" : invItemTarget.ItemData.DisplayName)}]");

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

            GameManager.Instance.Currency -= invItem.GetBuyPrice();
            invItem.IsShopOwned = false;

            HandleItemTrade(invItem);
        }
        //selling to shop
        if (this != targetInventory && targetInventory.IsShop)
        {
            GameManager.Instance.Currency += invItem.GetSellPrice();
            invItem.IsShopOwned = true;

            HandleItemTrade(invItem);
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

        Debug.Log($"After MoveItemToTarget:\nPrev inventory list: {GetItemsDisplayString()}\nTarget inventory list: {targetInventory.GetItemsDisplayString()}");

        return hasSwapped ? ItemMoveResult.Swapped : ItemMoveResult.Moved;
    }

    private void HandleItemTrade(InventoryItem invItem)
    {
        if (!invItem.WasTradedAlready)
            AddXpOnItemTrade(invItem);

        invItem.WasTradedAlready = true;
    }

    private void AddXpOnItemTrade(InventoryItem invItem)
    {
        GameManager.Instance.PlayerManager.PlayerHero.LevelSystem
            .AddExperienceToSkill(
                (int)(invItem.GetBuyPrice() * PlayerManager.SELLPRICE_TO_XP_RATE),
                Skill.Trading
            );
    }

    public bool IsEmpty()
    {
        return GetItemAmount() == 0;
    }

    public int GetItemAmount()
    {
        int count = 0;
        foreach (var item in InventoryItems)
        {
            if (item != null)
                count++;
        }

        return count;
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


    /// <summary>
    /// Entirely empties the inventory. Meant for when the player dies without keepInventory
    /// </summary>
    public void RemoveAllItems()
    {
        for (int i = 0; i < InventoryItems.Count; i++)
        {
            InventoryItems[i] = null;
        }

        OnInventoryChanged?.Invoke(this);
    }

    public void RemoveItemAt(int itemIndex)
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

            if (this.IsShop && item.ItemData.ItemType == ItemType.Loot)
            {
                UnityEngine.Object.Destroy(item.Prefab.gameObject);
                InventoryItems[i] = null;
            }
        }
    }

    public void SetInventoryItemList(List<InventoryItem> inventoryItems)
    {
        InventoryItems = inventoryItems;
        OnInventoryChanged?.Invoke(this);
    }

    public string GetItemsDisplayString()
    {
        string res = "[";

        for (int i= 0; i < InventoryItems.Count; i++)
        {
            string name = InventoryItems[i]?.ItemData?.DisplayName;
            res += $"{i}: {(name == null ? "empty" : name)}, ";
        }

        res += "]";

        return res;
    }

    public bool HasFreeSpace()
    {
        return FirstFreeSlotIndex() != -1;
    }

    public void MoveItemsToFront()
    {
        //loop through all items
        for (int i = 0; i < InventoryItems.Count; i++)
        {
            if (InventoryItems[i] == null)  //skip empty spots
                continue;

            //search for empty spots from the front
            for (int j = 0; j < InventoryItems.Count; j++)
            {
                //if found an empty spot that is more to the front than current spot, move the item
                if (InventoryItems[j] == null && j < i)
                {
                    InventoryItems[j] = InventoryItems[i];
                    InventoryItems[i] = null;
                }
            }
        }

        OnInventoryChanged?.Invoke(this);
    }
}
