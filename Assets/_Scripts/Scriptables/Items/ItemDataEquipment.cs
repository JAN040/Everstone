using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Scriptable/Item/New Equipment", fileName = "SO_ItemEquip_")]
public class ItemDataEquipment : ItemDataBase
{
    [Space]
    [Header("Equipment")]

    //Further divide the item in case ItemType == ItemType.Equipment
    public EquipmentType EquipmentType = EquipmentType.None;

    public List<StatModifier> StatModifiers;

    public ItemDataEquipment()
    {
        ItemType = ItemType.Equipment;
        MaxStackSize = 1; //aka non stackable
        CanBeSold = true;
        Id = "Equip_";
    }

    public new ItemDataEquipment Clone()
    {
        ItemDataEquipment clone = CreateInstance<ItemDataEquipment>();
        clone.Id = Id;
        clone.DisplayName = DisplayName;
        clone.Description = Description;
        clone.MenuIcon = MenuIcon;
        clone.BuyPrice = BuyPrice;
        clone.CanBeSold = CanBeSold;
        clone.MaxStackSize = MaxStackSize;
        clone.ItemType = ItemType;
        clone.Rarity = Rarity;

        clone.EquipmentType = EquipmentType;
        clone.StatModifiers = new List<StatModifier>(StatModifiers);
        
        return clone;
    }
}
