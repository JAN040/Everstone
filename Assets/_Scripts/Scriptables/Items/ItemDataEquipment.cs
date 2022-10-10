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

    public StatModifier[] StatModifiers;

    public ItemDataEquipment()
    {
        ItemType = ItemType.Equipment;
        MaxStackSize = 1; //aka non stackable
        CanBeSold = true;
        Id = "Equip_";
    }
}
