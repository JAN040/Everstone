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
    private InventoryItem EquipmentSlot_Helmet {
        get { return InventoryItems[(int)EquipmentSlot.Helmet]; }
        set { InventoryItems[(int)EquipmentSlot.Helmet] = value; }
    }
    private InventoryItem EquipmentSlot_Shoulder {
        get { return InventoryItems[(int)EquipmentSlot.Shoulder]; }
        set { InventoryItems[(int)EquipmentSlot.Shoulder] = value; }
    }
    private InventoryItem EquipmentSlot_Chestplate {
        get { return InventoryItems[(int)EquipmentSlot.Chestplate]; }
        set { InventoryItems[(int)EquipmentSlot.Chestplate] = value; }
    }
    private InventoryItem EquipmentSlot_Pants {
        get { return InventoryItems[(int)EquipmentSlot.Pants]; }
        set { InventoryItems[(int)EquipmentSlot.Pants] = value; }
    }
    private InventoryItem EquipmentSlot_Boots {
        get { return InventoryItems[(int)EquipmentSlot.Boots]; }
        set { InventoryItems[(int)EquipmentSlot.Boots] = value; }
    }

    private InventoryItem EquipmentSlot_Necklace {
        get { return InventoryItems[(int)EquipmentSlot.Necklace]; }
        set { InventoryItems[(int)EquipmentSlot.Necklace] = value; }
    }
    private InventoryItem EquipmentSlot_Cape {
        get { return InventoryItems[(int)EquipmentSlot.Cape]; }
        set { InventoryItems[(int)EquipmentSlot.Cape] = value; }
    }
    private InventoryItem EquipmentSlot_Gloves {
        get { return InventoryItems[(int)EquipmentSlot.Gloves]; }
        set { InventoryItems[(int)EquipmentSlot.Gloves] = value; }
    }
    private InventoryItem EquipmentSlot_Ring1 {
        get { return InventoryItems[(int)EquipmentSlot.Ring1]; }
        set { InventoryItems[(int)EquipmentSlot.Ring1] = value; }
    }
    private InventoryItem EquipmentSlot_Ring2 {
        get { return InventoryItems[(int)EquipmentSlot.Ring2]; }
        set { InventoryItems[(int)EquipmentSlot.Ring2] = value; }
    }

    private InventoryItem EquipmentSlot_RightArm {
        get { return InventoryItems[(int)EquipmentSlot.RightArm]; }
        set { InventoryItems[(int)EquipmentSlot.RightArm] = value; }
    }
    private InventoryItem EquipmentSlot_LeftArm {
        get { return InventoryItems[(int)EquipmentSlot.LeftArm]; }
        set { InventoryItems[(int)EquipmentSlot.LeftArm] = value; }
    }

    public List<InventoryItem> EquipmentItems 
    { 
        get 
        {
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


    public EquipmentSystem() : base((int)Enum.GetValues(typeof(EquipmentSlot)).Cast<EquipmentSlot>().Max() + 1)
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
        InventoryItem unequippedItem = InventoryItems[slot];
        InventoryItems[slot] = item;

        HandleModifiersOnEquipmentChange(unequippedItem, item);

        return (unequippedItem, slot);
    }

    public InventoryItem UnequipItem(int targetIndex)
    {
        InventoryItem res = InventoryItems[targetIndex];
        HandleModifiersOnEquipmentChange(res, null);
        InventoryItems[targetIndex] = null;

        return res;
    }

    public int GetDefaultEquipSlotIndexFromEquipType(EquipmentType equipType)
    {
        switch (equipType)
        {
            case EquipmentType.Sword:
            case EquipmentType.Dagger:
            case EquipmentType.Axe:
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
        var heroStats = GameManager.Instance.PlayerManager.PlayerHero.BaseStats;

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


    #endregion METHODS
}
