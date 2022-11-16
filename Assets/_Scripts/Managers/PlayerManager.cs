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
    public InventorySystem ShopInventory;
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
    /// The amount of items available in the shop. Increases with Trading skill level.
    /// </summary>
    public int ShopItemAmount
    {
        get => shopItemAmount; 
        set
        {
            if (shopItemAmount > value)
                return;

            if (shopItemAmount < value)
                ShopInventory.AddSpace(value - shopItemAmount);

            shopItemAmount = value;
        }
    }

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
    private int shopItemAmount = 10;

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

        if (ShopInventory == null)
        {
            ShopInventory = new InventorySystem(ShopItemAmount, true);
            RefreshShopInventory();
        }
    }

    /// <summary>
    /// To be called every time the player returns from adventure scene
    /// </summary>
    public void RefreshShopInventory()
    {
        List<InventoryItem> shopItemList = new List<InventoryItem>(ShopItemAmount + GameManager.Instance.PlayerInventorySpace);

        float chance_equip = 1f;
        float chance_potion = 0f; //no potions atm
        //float chance_loot = 0.4f; loot items shouldnt be sold

        for (int i = 0; i < ShopItemAmount; i++)
        {
            ItemType itemType = RollItemType(chance_equip, chance_potion);
            ItemDataBase itemData = ResourceSystem.Instance.GetRandomItemByType(itemType, GameManager.Instance.UnitData.RollItemRarity());
            
            if (itemData == null)
                continue;

            InventoryItem item = new InventoryItem(itemData, true);
            shopItemList.Add(item);
        }

        for (int i = ShopItemAmount; i < ShopItemAmount + Inventory.InventorySize; i++)
        {
            shopItemList.Add(null);
        }

        ShopInventory.SetInventoryItemList(shopItemList);
    }

    private ItemType RollItemType(float chance_equip, float chance_potion)
    {
        if (Helper.DiceRoll(chance_equip))
            return ItemType.Equipment;

        if (Helper.DiceRoll(chance_potion))
            return ItemType.Potion;

        //if (Helper.DiceRoll(chance_equip))
        //    return ItemType.Equipment;

        return ItemType.Potion;
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
