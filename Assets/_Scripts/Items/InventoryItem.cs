using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    public ItemDataBase itemData;
    public int stackSize = 0;

    public InventoryItem(ItemDataBase itemData)
    {
        this.itemData = itemData;
        AddToStack();
    }

    public void AddToStack(int amount = 1)
    {
        stackSize += amount;
    }

    public void RemoveFromStack(int amount = 1)
    {
        stackSize -= amount;
    }
}

