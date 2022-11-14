using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventoryItem
{
    #region Variables


    public ItemDataBase ItemData;
    public GameObject Prefab;

    /// <summary>
    /// Indicates whether transfering this item from ShopInventory to the Players inventory system should charge BuyPrice
    /// </summary>
    [NonSerialized] public bool IsShopOwned = false;

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
        this.ItemData = itemData;
        this.IsShopOwned = isShopOwned;
    }


    #region METHODS


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
        return (ItemData.BuyPrice * StackSize * GameManager.Instance.PlayerManager.GetSellPriceModifier(ItemData.ItemType)).Round();
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

