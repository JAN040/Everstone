using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventoryItem
{
    public ItemDataBase itemData;
    public GameObject Prefab;

    public int stackSize = 0;


    public event Action OnStackSizeChanged;


    public InventoryItem(ItemDataBase itemData)
    {
        this.itemData = itemData;
        AddToStack();
    }

    public void AddToStack(int amount = 1)
    {
        stackSize += amount;
        OnStackSizeChanged?.Invoke();
    }

    public void RemoveFromStack(int amount = 1)
    {
        stackSize -= amount;
        OnStackSizeChanged?.Invoke();
    }
}

