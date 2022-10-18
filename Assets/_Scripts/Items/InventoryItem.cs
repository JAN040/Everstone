using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventoryItem
{
    public ItemDataBase ItemData;
    public GameObject Prefab;

    private int stackSize = 1;
    public int StackSize
    {
        get => stackSize;
        set
        {
            stackSize = value;
            OnStackSizeChanged?.Invoke();
        }
    }


    public event Action OnStackSizeChanged;


    public InventoryItem(ItemDataBase itemData)
    {
        this.ItemData = itemData;
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

    public InventoryItem Clone()
    {
        InventoryItem clone = new InventoryItem(this.ItemData.Clone());

        if (Prefab != null)
            clone.Prefab = UnityEngine.Object.Instantiate(Prefab);
        
        clone.StackSize = StackSize;
        //clone.OnStackSizeChanged = OnStackSizeChanged;

        return clone;
    }
}

