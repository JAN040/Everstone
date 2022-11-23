using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Scriptable/Item/New Item", fileName = "SO_ItemBase_")]
public class ItemDataBase : ScriptableObject
{
    public string Id
    {
        get
        {
            if (!string.IsNullOrEmpty(id))
                return id;

            return this.name;
        }
        set => id = value;
    }
    private string id;

    public string DisplayName;
    [TextArea(1, 5)]
    public string Description;
    public Sprite MenuIcon;

    [Space]
    public int BuyPrice;
    public bool CanBeSold = true; //key/quest items shouldnt be sellable

    [Range(1, 999)]
    public int MaxStackSize = 1;   //if MaxStackSize > 1 the item is stackable
    public ItemType ItemType = ItemType.Loot;
    public ItemRarity Rarity;


    /// <summary>
    /// Used when creating an instance through ScriptableObject.CreateInstance
    /// </summary>
    public void Init(string id, string displayName, string description, Sprite menuIcon, int buyPrice, int maxStackSize, ItemType itemType, ItemRarity rarity)
    {
        Id = id;
        DisplayName = displayName;
        Description = description;
        MenuIcon = menuIcon;
        BuyPrice = buyPrice;
        MaxStackSize = maxStackSize;
        ItemType = itemType;
        Rarity = rarity;
    }


    public ItemDataBase Clone()
    {
        ItemDataBase clone = CreateInstance<ItemDataBase>();
        clone.Id = Id;
        clone.DisplayName = DisplayName;
        clone.Description = Description;
        clone.MenuIcon = MenuIcon;
        clone.BuyPrice = BuyPrice;
        clone.CanBeSold = CanBeSold;
        clone.MaxStackSize = MaxStackSize;
        clone.ItemType = ItemType;
        clone.Rarity = Rarity;

        return clone;
    }
}

