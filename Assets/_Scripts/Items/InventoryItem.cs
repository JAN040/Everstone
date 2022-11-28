using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventoryItem
{
    #region Variables


    public ItemDataBase ItemData { get; private set; }
    public GameObject Prefab;

    /// <summary>
    /// Indicates whether transfering this item from ShopInventory to the Players inventory system should charge BuyPrice
    /// </summary>
    [NonSerialized] public bool IsShopOwned = false;
    public bool WasTradedAlready = false; //a flag that prevents gaining infinite merchant xp by reselling the same item


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


    #endregion Variables


    public event Action OnStackSizeChanged;


    public InventoryItem(ItemDataBase itemData, bool isShopOwned = false)
    {
        if (itemData == null)
            Debug.LogWarning("Trying to create an item with null data!");

        this.ItemData = itemData;
        this.IsShopOwned = isShopOwned;
    }

    /// <summary>
    /// Save system constructor
    /// </summary>
    public InventoryItem(InventoryItemSaveData data)
    {
        this.ItemData = ResourceSystem.Instance.GetItemById(data.itemId);
        this.IsShopOwned = data.isShopOwned;
        this.StackSize = data.stackSize;
        this.WasTradedAlready = data.wasTradedAlready;
    }


    #region METHODS


    public InventoryItemSaveData GetSaveData()
    {
        if (ItemData == null)
            return null;

        return new InventoryItemSaveData(ItemData.Id, IsShopOwned, StackSize, WasTradedAlready);
    }


    /// <summary>
    /// Tries to add amount to stack. If stack cant take the entire amount (gets full), returns the remainder
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    public int AddToStack(int amount = 1)
    {
        int remainder = 0;
        int amountCanAdd = amount;

        //if adding everything would cause the stack to overflow
        if (StackSize + amount > ItemData.MaxStackSize)
        {
            amountCanAdd = ItemData.MaxStackSize - StackSize;
            remainder = amount - amountCanAdd;
        }

        StackSize += amountCanAdd;
        OnStackSizeChanged?.Invoke();

        return remainder;
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
        clone.IsShopOwned = IsShopOwned;
        //clone.OnStackSizeChanged = OnStackSizeChanged;

        return clone;
    }

    /// <returns>True, if this inventory item represents an Equipment of type Rune</returns>
    public bool IsRune()
    {
        var equipData = ItemData as ItemDataEquipment;
        if (equipData == null)
            return false;

        return equipData.EquipmentType == EquipmentType.Rune;
    } 

    public int GetSellPrice()
    {
        float sellModifier = GameManager.Instance.PlayerManager.GetSellPriceModifier(ItemData.ItemType);

        return (ItemData.BuyPrice * StackSize * sellModifier).Round();
    }

    public int GetBuyPrice()
    {
        return ItemData.BuyPrice * StackSize;
    }

    public bool CanAfford()
    {
        return ItemData.BuyPrice <= GameManager.Instance.Currency;
    }


    #endregion METHODS
}

