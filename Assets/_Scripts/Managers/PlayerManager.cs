using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//holds all player related data
[System.Serializable]
public class PlayerManager
{
    #region VARIABLES


    public ScriptableHero PlayerHero { get; private set; }

    public List<ScriptableAbility> ClassicAbilities { get; private set; } = new List<ScriptableAbility>();
    public List<ScriptableAbility> SpecialAbilities { get; private set; } = new List<ScriptableAbility>();

    public InventorySystem Inventory;
    public InventorySystem Storage;
    public EquipmentSystem Equipment;


    /// <summary>
    /// Modifier for selling item types of type other then ItemType.Loot.
    /// Max value of 1 (to prevent buying and selling same stuff for profit)
    /// </summary>
    public float SellPriceModifier
    {
        get => sellPriceModifier;
        set
        {
            sellPriceModifier = value;
            if (sellPriceModifier > 1f)
                sellPriceModifier = 1f;
        }
    }
    private float sellPriceModifier;

    /// <summary>
    /// Modifier for selling item types of type ItemType.Loot.
    /// Can be greater than 1 since loot items cant be bought back from shop.
    /// </summary>
    public float SellPriceModifier_Loot { get; set; }


    #endregion VARIABLES


    #region METHODS

    public void Init(ScriptableHero hero)
    {
        PlayerHero = hero;
        SellPriceModifier = 0.75f;
        SellPriceModifier_Loot = 0.75f;
    }

    public void SetAbilities(List<ScriptableAbility> classicAbilities, List<ScriptableAbility> specialAbilities)
    {
        ClassicAbilities.AddRange(classicAbilities);
        SpecialAbilities.AddRange(specialAbilities);
    }

    public void SetInventory(InventorySystem inventory, InventorySystem storage, EquipmentSystem equipment)
    {
        Inventory = inventory;

        if (storage != null)
            Storage = storage;

        if (equipment != null)
            Equipment = equipment;
    }

    public float GetSellPriceModifier(ItemType itemType)
    {
        return itemType == ItemType.Loot ?
            SellPriceModifier_Loot
            :
            SellPriceModifier;
    }

    #endregion METHODS


}
