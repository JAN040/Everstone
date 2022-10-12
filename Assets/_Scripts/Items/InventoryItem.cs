using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventoryItem
{
    public ItemDataBase ItemData;
    public GameObject Prefab;

    public int StackSize = 0;


    public event Action OnStackSizeChanged;


    public InventoryItem(ItemDataBase itemData)
    {
        this.ItemData = itemData;
        AddToStack();
    }

    public void AddToStack(int amount = 1)
    {
        StackSize += amount;
        OnStackSizeChanged?.Invoke();
    }

    public void RemoveFromStack(int amount = 1)
    {
        StackSize -= amount;
        OnStackSizeChanged?.Invoke();
    }
}

