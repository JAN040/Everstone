using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//holds all player related data
[System.Serializable]
public class PlayerManager
{
    #region VARIABLES


    public ScriptableHero PlayerHero { get; private set; }

    public List<ScriptableAbility> Abilities { get; private set; }
    public List<ScriptableAbility> EquippedAbilities;
    private const int AbilitySlotAmount = 7;

    public InventorySystem Inventory;
    public InventorySystem Storage;
    public EquipmentSystem Equipment;
    public EquipmentSystem Runes;


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
            if (sellPriceModifier > 0.99f)
                sellPriceModifier = 0.99f;
        }
    }
    private float sellPriceModifier;

    /// <summary>
    /// Modifier for selling item types of type ItemType.Loot.
    /// Can be greater than 1 since loot items cant be bought back from shop.
    /// </summary>
    public float SellPriceModifier_Loot { get; set; }

    public readonly float SellPriceModPerLevelIncrease = 0.01f;

    /// <summary>
    /// Max amount of pets that can be added to the formation
    /// </summary>
    public int MaxPets
    {
        get => maxPets;
        set
        {
            maxPets = Math.Clamp(value, 1, 5);
        }
    }
    [SerializeField] private int maxPets;
    
    /// <summary>
    /// Every MoreMaxPetsEveryNLevels amount of Taming levels, MaxPets is increased by one.
    /// </summary>
    public float MoreMaxPetsPerNLevels { get; private set; }
    
    /// <summary>
    /// Bonus XP pets gain when attacking; based on player Taming skill level
    /// </summary>
    public float PetXpBonus { get; set; }

    public readonly float PetXpBonusPerLevel = 0.03f;



    #endregion VARIABLES


    #region METHODS

    public void Init(ScriptableHero hero)
    {
        PlayerHero = hero;
        SellPriceModifier = 0.75f;
        SellPriceModifier_Loot = 0.75f;

        MaxPets = 1;
        MoreMaxPetsPerNLevels = 10;
        PetXpBonus = 0f;
    }

    public void SetAbilities(List<ScriptableAbility> abilities)
    {
        Abilities = new List<ScriptableAbility>();
        Abilities.AddRange(abilities);

        EquippedAbilities = new List<ScriptableAbility>(AbilitySlotAmount);
        for (int i = 0; i < AbilitySlotAmount; i++)
            EquippedAbilities.Add(null);
    }

    public void SetInventory(InventorySystem inventory, InventorySystem storage, EquipmentSystem equipment, EquipmentSystem runes)
    {
        Inventory = inventory;

        if (storage != null)
            Storage = storage;

        if (equipment != null)
            Equipment = equipment;

        if (runes != null)
            Runes = runes;
    }

    public float GetSellPriceModifier(ItemType itemType)
    {
        return itemType == ItemType.Loot ?
            SellPriceModifier_Loot
            :
            SellPriceModifier;
    }

    public void CheckAbilitySpecialCases()
    {
        //Shield block
        var blockAbility = EquippedAbilities.FirstOrDefault(x => x != null && x.Ability == Ability.ShieldBlock);
        if (blockAbility != null && !Equipment.HasShieldEquipped())
        {
            blockAbility.IsSelected = false;

            int index = EquippedAbilities.IndexOf(blockAbility);
            if (index != -1)
                EquippedAbilities[index] = null;
        }
    }


    #endregion METHODS


}
