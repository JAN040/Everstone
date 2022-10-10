using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class InventorySystem
{
    public List<InventoryItem> InventoryItems;
    private Dictionary<ItemDataBase, InventoryItem> ItemDict;

    public int InventorySize { get; private set; }
   
    public InventorySystem(int size)
    {
        InventorySize = size;
    }


    /// <summary>
    /// Try to add the item to the list.
    /// </summary>
    /// <returns>True on success, false on failure (inventory is full)</returns>
    public bool AddItem(InventoryItem newItem)
    {
        var existingItemStack = InventoryItems.FirstOrDefault(x => x.itemData.Id == newItem.itemData.Id);
        
        //if a stack for the item already exists and is not yet full, add the new item to that stack
        if (existingItemStack != null && existingItemStack.itemData.MaxStackSize > existingItemStack.stackSize)
        {
            existingItemStack.AddToStack(newItem.stackSize);
            
            return true;
        }
        //no such stack exists, try to create a new stack if there is space
        else if (InventoryItems.Count < InventorySize)
        {
            InventoryItems.Add(newItem);

            return true;
        }

        //no space for the new item
        return false;
    }

}
