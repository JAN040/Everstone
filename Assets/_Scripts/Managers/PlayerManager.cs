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
    public List<ScriptableAbility> EquippedAbilities;   //tracks the order of selected abilities
    private const int ABILITY_SLOT_AMOUNT = 7;

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

    public const float SELLPRICEMOD_PER_LEVEL_INCREASE = 0.01f;

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
    [SerializeField] int shopItemAmount = 10;

    /// <summary>
    /// Every MoreMaxPetsEveryNLevels amount of Taming levels, MaxPets is increased by one.
    /// </summary>
    public const float MOREMAXPETS_PER_N_LEVELS = 10;

    /// <summary>
    /// Bonus XP pets gain when attacking; based on player Taming skill level
    /// </summary>
    public float PetXpBonus { get; set; }

    public const float PET_XP_BONUS_PER_LEVEL = 0.03f;


    // IMPORTANT: dont forget to include any new fields to the save system!


    #endregion VARIABLES


    public PlayerManager()
    {
    }

    /// <summary>
    /// Save system helper method
    /// </summary>
    public PlayerManager(PlayerManagerSaveData data)
    {
        Abilities = new List<ScriptableAbility>();
        EquippedAbilities = new List<ScriptableAbility>();

        foreach (var abilityData in data.abilities)
        {
            if (abilityData == null)
                Abilities.Add(null);
            else
                Abilities.Add(ScriptableAbility.GetAbilityFromSaveData(abilityData));
        }

        foreach (var abilityData in data.equippedAbilities)
        {
            if (abilityData == null)
                EquippedAbilities.Add(null);
            else
                EquippedAbilities.Add(ScriptableAbility.GetAbilityFromSaveData(abilityData));
        }

        PlayerHero = ScriptableHero.GetHeroFromSaveData(data.playerHero);
        //Abilities = data.abilities;
        //EquippedAbilities = data.equippedAbilities;
        Inventory = new InventorySystem(data.inventory);
        Storage = new InventorySystem(data.storage);
        ShopInventory = new InventorySystem(data.shopInventory);
        Equipment = new EquipmentSystem(data.equipment);
        Runes = new EquipmentSystem(data.runes);

        SellPriceModifier = data.sellPriceModifier;
        SellPriceModifier_Loot = data.sellPriceModifier_Loot;
        ShopItemAmount = data.shopItemAmount;
        MaxPets = data.maxPets;
        PetXpBonus = data.petXpBonus;
    }


    #region METHODS

    public void Init(ScriptableHero hero)
    {
        PlayerHero = hero;
        SellPriceModifier = 0.75f;
        SellPriceModifier_Loot = 0.75f;

        MaxPets = 1;
        PetXpBonus = 0f;
    }

    public void SetAbilities(List<ScriptableAbility> abilities)
    {
        Abilities = new List<ScriptableAbility>();
        Abilities.AddRange(abilities);

        EquippedAbilities = new List<ScriptableAbility>(ABILITY_SLOT_AMOUNT);
        for (int i = 0; i < ABILITY_SLOT_AMOUNT; i++)
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

        //item generation uses probabilities for one higher than the max cleared adventure.
        LocationDifficulty locationDiff = GetItemGenerationDifficulty();

        float chance_equip = 1f;
        float chance_potion = 0f; //no potions atm
        //float chance_loot = 0.4f; loot items shouldnt be sold

        for (int i = 0; i < ShopItemAmount; i++)
        {
            ItemType itemType = RollItemType(chance_equip, chance_potion);
            ItemDataBase itemData = ResourceSystem.Instance.GetRandomItemByType(itemType, GameManager.Instance.UnitData.RollItemRarity(locationDiff));

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

    private LocationDifficulty GetItemGenerationDifficulty()
    {
        LocationDifficulty locDiff = LocationDifficulty.Easy;
        var highestClearedAdventure = GameManager.Instance.HighestClearedAdventureLocation();
        
        if (highestClearedAdventure != null)
            locDiff = highestClearedAdventure.difficulty;
        
        locDiff = locDiff.NextOrSame();

        return locDiff;
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
        //this makes it so that the currency stack always has its correct value displayed
        if (itemType == ItemType.Currency)
            return 1;

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

    /// <summary>
    /// Save system helper method
    /// </summary>
    public PlayerManagerSaveData GetSaveData()
    {
        List<AbilitySaveData> abilitySaveDatas = new List<AbilitySaveData>();
        List<AbilitySaveData> equippedAbilitySaveDatas = new List<AbilitySaveData>();

        foreach (var ability in Abilities)
        {
            if (ability == null) 
                abilitySaveDatas.Add(null);
            else
                abilitySaveDatas.Add(ability.GetSaveData());
        }

        foreach (var ability in EquippedAbilities)
        {
            if (ability == null)
                equippedAbilitySaveDatas.Add(null);
            else
                equippedAbilitySaveDatas.Add(ability.GetSaveData());
        }

        PlayerManagerSaveData data = new PlayerManagerSaveData(
            PlayerHero.GetSaveData(),
            abilitySaveDatas,
            equippedAbilitySaveDatas,
            Inventory.GetSaveData(),
            Storage.GetSaveData(),
            ShopInventory.GetSaveData(),
            Equipment.GetSaveData(),
            Runes.GetSaveData(),

            SellPriceModifier,
            SellPriceModifier_Loot,
            ShopItemAmount,
            MaxPets,
            PetXpBonus
        );

        return data;
    }

    #endregion METHODS


}
