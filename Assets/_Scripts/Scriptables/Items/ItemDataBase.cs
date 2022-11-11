using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Scriptable/Item/New Item", fileName = "SO_ItemBase_")]
public class ItemDataBase : ScriptableObject
{
    public string Id = "Item_";
    public string DisplayName;
    [TextArea(1,5)]
    public string Description;
    public Sprite MenuIcon;

    [Space]
    public int BuyPrice;
    public bool  CanBeSold = true; //key/quest items shouldnt be sellable

    [Range(1, 999)]
    public int MaxStackSize = 1;   //if MaxStackSize > 1 the item is stackable
    public ItemType   ItemType = ItemType.Loot;
    public ItemRarity Rarity;


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

