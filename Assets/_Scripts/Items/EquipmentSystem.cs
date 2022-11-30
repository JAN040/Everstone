using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Progress;

[System.Serializable]
public class EquipmentSystem : InventorySystem
{
    #region VARIABLES


    //hardcoded equipment slots cause fuck it
    InventoryItem EquipmentSlot_Helmet {
        get { return InventoryItems[(int)EquipmentSlot.Helmet]; }
        set { InventoryItems[(int)EquipmentSlot.Helmet] = value; }
    }
    InventoryItem EquipmentSlot_Shoulder {
        get { return InventoryItems[(int)EquipmentSlot.Shoulder]; }
        set { InventoryItems[(int)EquipmentSlot.Shoulder] = value; }
    }
    InventoryItem EquipmentSlot_Chestplate {
        get { return InventoryItems[(int)EquipmentSlot.Chestplate]; }
        set { InventoryItems[(int)EquipmentSlot.Chestplate] = value; }
    }
    InventoryItem EquipmentSlot_Pants {
        get { return InventoryItems[(int)EquipmentSlot.Pants]; }
        set { InventoryItems[(int)EquipmentSlot.Pants] = value; }
    }
    InventoryItem EquipmentSlot_Boots {
        get { return InventoryItems[(int)EquipmentSlot.Boots]; }
        set { InventoryItems[(int)EquipmentSlot.Boots] = value; }
    }

    InventoryItem EquipmentSlot_Necklace {
        get { return InventoryItems[(int)EquipmentSlot.Necklace]; }
        set { InventoryItems[(int)EquipmentSlot.Necklace] = value; }
    }
    InventoryItem EquipmentSlot_Cape {
        get { return InventoryItems[(int)EquipmentSlot.Cape]; }
        set { InventoryItems[(int)EquipmentSlot.Cape] = value; }
    }
    InventoryItem EquipmentSlot_Gloves {
        get { return InventoryItems[(int)EquipmentSlot.Gloves]; }
        set { InventoryItems[(int)EquipmentSlot.Gloves] = value; }
    }
    InventoryItem EquipmentSlot_Ring1 {
        get { return InventoryItems[(int)EquipmentSlot.Ring1]; }
        set { InventoryItems[(int)EquipmentSlot.Ring1] = value; }
    }
    InventoryItem EquipmentSlot_Ring2 {
        get { return InventoryItems[(int)EquipmentSlot.Ring2]; }
        set { InventoryItems[(int)EquipmentSlot.Ring2] = value; }
    }

    InventoryItem EquipmentSlot_RightArm {
        get { return InventoryItems[(int)EquipmentSlot.RightArm]; }
        set { InventoryItems[(int)EquipmentSlot.RightArm] = value; }
    }
    InventoryItem EquipmentSlot_LeftArm {
        get { return InventoryItems[(int)EquipmentSlot.LeftArm]; }
        set { InventoryItems[(int)EquipmentSlot.LeftArm] = value; }
    }

    public List<InventoryItem> EquipmentItems 
    { 
        get 
        {
            if (IsRuneSystem)
                return new List<InventoryItem>(InventoryItems);

            return new List<InventoryItem>()
            {
                EquipmentSlot_Helmet,
                EquipmentSlot_Shoulder,
                EquipmentSlot_Chestplate,
                EquipmentSlot_Pants,
                EquipmentSlot_Boots,

                EquipmentSlot_Necklace,
                EquipmentSlot_Cape,
                EquipmentSlot_Gloves,
                EquipmentSlot_Ring1,
                EquipmentSlot_Ring2,

                EquipmentSlot_RightArm,
                EquipmentSlot_LeftArm
            };
        } 
    }

    public bool IsRuneSystem;


    #endregion VARIABLES


    #region UNITY METHODS


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }




#endregion UNITY METHODS


    public EquipmentSystem(int capacity, bool isRuneSystem) : base(capacity)
    {
        IsRuneSystem = isRuneSystem;
    }

    public EquipmentSystem(InventorySystemSaveData data) : base(data)
    {
    }


    #region METHODS


    /// <summary>
    /// Equips the item to the appropriate slot
    /// </summary>
    /// <param name="slot">
    /// What slot to equip the item to (-1 decides the slot based on equip type)
    /// </param>
    /// <returns>A tuple of the unequipped item and the slot where the switch was made (relevant when no slot is provided)</returns>
    public (InventoryItem, int) EquipItem(InventoryItem item, int slot = -1)
    {
        if (item == null)
        {
            if (slot != -1)
            {
                return (UnequipItem(slot), slot);
            }
            else
                return (null, slot);
        }

        ItemDataEquipment itemData = item.ItemData as ItemDataEquipment;
        if (item.ItemData.ItemType != ItemType.Equipment || itemData == null)
            return (item, slot);

        if (slot == -1)
            slot = GetDefaultEquipSlotIndexFromEquipType(itemData.EquipmentType);

        //swap equipments
        InventoryItem unequippedItem = PlaceItemAtSlot(item, slot); ;

        HandleModifiersOnEquipmentChange(unequippedItem, item);

        return (unequippedItem, slot);
    }

    public InventoryItem UnequipItem(int targetIndex)
    {
        InventoryItem unEquipped = PlaceItemAtSlot(null, targetIndex);
        HandleModifiersOnEquipmentChange(unEquipped, null);

        return unEquipped;
    }

    public int GetDefaultEquipSlotIndexFromEquipType(EquipmentType equipType)
    {
        switch (equipType)
        {
            case EquipmentType.Rune:
                int index = FirstFreeSlotIndex();
                if (index == -1)
                    index = 0;  //if all slots are taken equip to the first slot by default
                return index;

            case EquipmentType.Sword:
            case EquipmentType.Dagger:
            case EquipmentType.Hammer:
            case EquipmentType.Staff:
            case EquipmentType.Shield:
                if (equipType == EquipmentType.Shield ||
                    EquipmentSlot_LeftArm == null && EquipmentSlot_RightArm != null)
                    return (int)EquipmentSlot.LeftArm;
                else
                    return (int)EquipmentSlot.RightArm;

            case EquipmentType.Ring:
                return EquipmentSlot_Ring2 == null && EquipmentSlot_Ring1 != null ?
                    (int)EquipmentSlot.Ring2
                    :
                    (int)EquipmentSlot.Ring1;

            case EquipmentType.Helmet:
                return (int)EquipmentSlot.Helmet;

            case EquipmentType.Shoulder:
                return (int)EquipmentSlot.Shoulder;

            case EquipmentType.Chestplate:
                return (int)EquipmentSlot.Chestplate;

            case EquipmentType.Pants:
                return (int)EquipmentSlot.Pants;

            case EquipmentType.Boots:
                return (int)EquipmentSlot.Boots;

            case EquipmentType.Necklace:
                return (int)EquipmentSlot.Necklace;

            case EquipmentType.Cape:
                return (int)EquipmentSlot.Cape;

            case EquipmentType.Gloves:
                return (int)EquipmentSlot.Gloves;

            case EquipmentType.None:
            default:
                return (int)EquipmentSlot.None;
        }
    }

    private void HandleModifiersOnEquipmentChange(InventoryItem unequippedItem, InventoryItem equippedItem)
    {
        var heroStats = GameManager.Instance.PlayerManager.PlayerHero.Stats;

        heroStats.RemoveModifiers((unequippedItem?.ItemData as ItemDataEquipment)?.StatModifiers);
        heroStats.AddModifiers((equippedItem?.ItemData as ItemDataEquipment)?.StatModifiers);
    }

    public bool IsEquipped(InventoryItem itemRef)
    {
        if (InventoryItems == null)
            return false;

        foreach (var item in InventoryItems)
        {
            if (item == itemRef)
                return true;
        }

        return false;
    }

    public bool HasShieldEquipped()
    {
        var leftHandEq = InventoryItems[GetDefaultEquipSlotIndexFromEquipType(EquipmentType.Shield)];
        var equipData = leftHandEq?.ItemData as ItemDataEquipment;

        if (leftHandEq == null || equipData == null)
            return false;

        return equipData.EquipmentType == EquipmentType.Shield;
    }


    #endregion METHODS
}
