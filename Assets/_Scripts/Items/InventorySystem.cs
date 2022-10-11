using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class InventorySystem
{
    public List<InventoryItem> InventoryItems;
    private Dictionary<ItemDataBase, InventoryItem> ItemDict;

    /// <summary>
    /// Max amount of items the inventory can hold
    /// </summary>
    public int InventorySize { get; private set; }


    public event Action OnInventoryChanged;

   
    public InventorySystem(int size)
    {
        InventorySize = size;
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
        var existingItemStack = InventoryItems.FirstOrDefault(x => x != null && x.itemData.Id == newItem.itemData.Id);
        
        //if a stack for the item already exists and is not yet full, add the new item to that stack
        if (existingItemStack != null && existingItemStack.itemData.MaxStackSize > existingItemStack.stackSize)
        {
            existingItemStack.AddToStack(newItem.stackSize);
            OnInventoryChanged?.Invoke();

            return true;
        }

        //no such stack exists, try to create a new stack if there is space (null means empty)
        int emptySpace = GetEmptySpaceIndex();

        if (emptySpace == -1)   //no space for the new item
            return false;

        InventoryItems[emptySpace] = newItem;
        OnInventoryChanged?.Invoke();

        return true;
    }

    /// <summary>
    /// Finds an empty space and returns its index
    /// </summary>
    /// <returns>Index of the first empty space. Or -1 if all spaces are taken.</returns>
    private int GetEmptySpaceIndex()
    {
        for (int i = 0; i < InventoryItems.Count; i++)
        {
            if (InventoryItems[i] == null)
                return i;
        }

        return -1;
    }

    /// <returns>The amount of items the inventory is currently holding</returns>
    public int GetItemAmount()
    {
        return InventoryItems.Count;
    }

    /// <summary>
    /// Increase the amount of items the inventory can hold
    /// </summary>
    /// <param name="amount"></param>
    public void AddSpace(int amount)
    {
        InventorySize += amount;

        for (int i = 0; i < amount; i++)
            InventoryItems.Add(null);

        OnInventoryChanged?.Invoke();
    }

    
}
