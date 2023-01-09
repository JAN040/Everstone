using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameData
{
    public GameManagerSaveData GameManagerData;

    public GameData(GameManagerSaveData gameManagerData)
    {
        if (gameManagerData == null)
            Debug.LogWarning($"Creating game data with NULL gameManager reference...!");

        GameManagerData = gameManagerData;
    }
}


[Serializable]
public class GameManagerSaveData
{
    public int currency;
    public Difficulty gameDifficulty;
    public BattleState gameState;

    public PlayerManagerSaveData playerManagerData;

    public List<string> allies; //name of allied ScriptableNpcUnit
    public AdventureLocationSaveData currentAdventureLocation;
    public List<AdventureLocationSaveData> adventureLocationData;
    public bool isHardcore;
    public bool keepInventory;
    public int campStorageSpace;
    public int playerInventorySpace;
    public bool showGameOverScreenOnSaveLoad;
    public Scenes currentScene;
    public float battleGameSpeed;
    public int numOfCopperForOneSilver;
    public int numOfSilverForOneGold;

    public GameManagerSaveData(int currency, Difficulty gameDifficulty, BattleState gameState, PlayerManagerSaveData playerManagerData, List<string> allies, AdventureLocationSaveData currentAdventureLocation, List<AdventureLocationSaveData> adventureLocationData, bool isHardcore, bool keepInventory, int campStorageSpace, int playerInventorySpace, bool showGameOverScreenOnSaveLoad, Scenes currentScene, float battleGameSpeed, int numOfCopperForOneSilver, int numOfSilverForOneGold)
    {
        this.currency = currency;
        this.gameDifficulty = gameDifficulty;
        this.gameState = gameState;
        this.playerManagerData = playerManagerData;
        this.allies = allies;
        this.currentAdventureLocation = currentAdventureLocation;
        this.adventureLocationData = adventureLocationData;
        this.isHardcore = isHardcore;
        this.keepInventory = keepInventory;
        this.campStorageSpace = campStorageSpace;
        this.playerInventorySpace = playerInventorySpace;
        this.showGameOverScreenOnSaveLoad = showGameOverScreenOnSaveLoad;
        this.currentScene = currentScene;
        this.battleGameSpeed = battleGameSpeed;
        this.numOfCopperForOneSilver = numOfCopperForOneSilver;
        this.numOfSilverForOneGold = numOfSilverForOneGold;
    }
}

[Serializable]
public class PlayerManagerSaveData
{
    public PlayerHeroSaveData playerHero;
    public List<AbilitySaveData> abilities;
    public List<AbilitySaveData> equippedAbilities;
    public InventorySystemSaveData inventory;
    public InventorySystemSaveData storage;
    public InventorySystemSaveData shopInventory;
    public InventorySystemSaveData equipment;
    public InventorySystemSaveData runes;
    public float sellPriceModifier;
    public float sellPriceModifier_Loot;
    public int shopItemAmount;
    public int maxPets;
    public float petXpBonus;

    public PlayerManagerSaveData(PlayerHeroSaveData playerHero, List<AbilitySaveData> abilities, List<AbilitySaveData> equippedAbilities, InventorySystemSaveData inventory, InventorySystemSaveData storage, InventorySystemSaveData shopInventory, InventorySystemSaveData equipment, InventorySystemSaveData runes, float sellPriceModifier, float sellPriceModifier_Loot, int shopItemAmount, int maxPets, float petXpBonus)
    {
        this.playerHero = playerHero;
        this.abilities = abilities;
        this.equippedAbilities = equippedAbilities;
        this.inventory = inventory;
        this.storage = storage;
        this.shopInventory = shopInventory;
        this.equipment = equipment;
        this.runes = runes;
        this.sellPriceModifier = sellPriceModifier;
        this.sellPriceModifier_Loot = sellPriceModifier_Loot;
        this.shopItemAmount = shopItemAmount;
        this.maxPets = maxPets;
        this.petXpBonus = petXpBonus;
    }
}

public class InventorySystemSaveData
{
    public List<InventoryItemSaveData> itemDataList;
    public bool isShop;
    public bool isRuneSystem;

    public InventorySystemSaveData(List<InventoryItemSaveData> itemData, bool isShop, bool isRuneSystem)
    {
        this.itemDataList = itemData;
        this.isShop = isShop;
        this.isRuneSystem = isRuneSystem;
    }
}

public class InventoryItemSaveData
{
    public string itemId;
    public bool isShopOwned;
    public int stackSize;
    public bool wasTradedAlready;

    public InventoryItemSaveData(string itemId, bool isShopOwned, int stackSize, bool wasTradedAlready)
    {
        this.itemId = itemId;
        this.isShopOwned = isShopOwned;
        this.stackSize = stackSize;
        this.wasTradedAlready = wasTradedAlready;
    }
}

public class AdventureLocationSaveData
{
    public string locationName;
    public int playerProgress;
    public int LoopCount;
    public LocationDifficulty Difficulty;

    public AdventureLocationSaveData(string name, int playerProgress, int LoopCount, LocationDifficulty difficulty)
    {
        this.locationName = name;
        this.playerProgress = playerProgress;
        this.LoopCount = LoopCount;
        this.Difficulty = difficulty;
    }
}

public class PlayerHeroSaveData
{
    public string className;
    public string playerName;
    public string background;
    public string portraitName;
    public CharacterStats stats;
    public LevelSystem levelSystem;

    public PlayerHeroSaveData(string className, string playerName, string background, string portraitName, CharacterStats stats, LevelSystem levelSystem)
    {
        this.className = className;
        this.playerName = playerName;
        this.background = background;
        this.portraitName = portraitName;
        this.stats = stats;
        this.levelSystem = levelSystem;
    }
}

public class AbilitySaveData
{
    public string abilityName;
    public int level;
    public int upgradeCost;
    public bool isSelected;

    public AbilitySaveData(string name, int level, int upgradeCost, bool isSelected)
    {
        abilityName = name;
        this.level = level;
        this.upgradeCost = upgradeCost;
        this.isSelected = isSelected;
    }
}