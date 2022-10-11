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
    public float BuyPrice;
    public bool  CanBeSold = true; //key/quest items shouldnt be sellable

    [Range(1, 999)]
    public int MaxStackSize = 1;   //if MaxStackSize > 1 the item is stackable
    public ItemType   ItemType = ItemType.Loot;
    public ItemRarity Rarity;
}

