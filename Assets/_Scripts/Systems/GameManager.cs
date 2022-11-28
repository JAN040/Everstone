using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class GameManager : Singleton<GameManager>
{
    #region VARIABLES


    #region UI References


    [Header("UI References")]
    [SerializeField] GameObject GameOverMenu;


    #endregion UI References


    [Header("Variables")]

    [SerializeField] private int currency;
    [SerializeField] private Difficulty gameDifficulty;
    public BattleState GameState = BattleState.None;

    [SerializeField] PlayerManager playerManager;
    /// <summary>
    /// Reference to the PlayerManager script
    /// </summary>
    public PlayerManager PlayerManager
    {
        get
        {
            if (playerManager == null)
                playerManager = new PlayerManager();

            return playerManager;
        }
        private set => playerManager = value;
    }

    /// <summary>
    /// Reference to the UnitData script
    /// </summary>
    public UnitData UnitData { get; private set; }

    /// <summary>
    /// List of all player allies (pets, mercs, mission teammates etc.)
    /// </summary>
    public List<ScriptableNpcUnit> Allies { get; set; }

    /// <summary>
    /// Stores the reference to the adventure location the player is currently in (null if not in adventure)
    /// </summary>
    public ScriptableAdventureLocation CurrentAdventureLocation { get; private set; }
    public List<ScriptableAdventureLocation> AdventureLocationData
    {
        get
        {
            if (adventureLocationData == null)
                adventureLocationData = ResourceSystem.Instance.GetAdventureLocations();

            return adventureLocationData;
        }
        set => adventureLocationData = value;
    }
    private List<ScriptableAdventureLocation> adventureLocationData = null;


    public int Currency
    {
        get => currency;
        set
        {
            var oldAmount = currency;
            currency = value;

            if (oldAmount != currency)
                OnCurrencyChanged?.Invoke(oldAmount, currency);
        }
    }

    public Difficulty GameDifficulty { get => gameDifficulty; private set => gameDifficulty = value; }

    public bool IsHardcore { get; private set; }
    public bool KeepInventory { get; private set; }

    public int CampStorageSpace = 70;

    public int PlayerInventorySpace
    {
        get => playerInventorySpace;
        set
        {
            if (value > playerInventorySpace)
            {
                PlayerManager.Inventory.AddSpace(value - playerInventorySpace);
            }

            playerInventorySpace = value;
        }
    }

    public bool ShowGameOverScreenOnSaveLoad { get; private set; } = false;

    public Scenes CurrentScene;


    #region Player prefs


    public float BattleGameSpeed = 1f;
    public int NumOfCopperForOneSilver = 10;
    public int NumOfSilverForOneGold = 100;
    private int playerInventorySpace = 10;




    #endregion Player prefs


    #endregion VARIABLES


    #region STATIC METHODS


    public static bool CanAfford(int amountToCompare)
    {
        if (Instance == null)
            return false;

        return Instance.Currency >= amountToCompare;
    }


    #endregion STATIC METHODS


    /// <summary>
    /// Fires when Currency amount changes. Data: old amount, new amount
    /// </summary>
    public event Action<int, int> OnCurrencyChanged;





    #region UNITY METHODS

    void Start()
    {
        //PlayerManager = new PlayerManager();
        UnitData = new UnitData();
    }

    private void OnDestroy()
    {
        if (GameState == BattleState.InBattle)
        {
            //time to punish for leaving in middle of combat >:)
            ShowGameOverScreenOnSaveLoad = true;
        }
    }

    #endregion UNITY METHODS


    #region METHODS


    public void SetGameDifficulty(Difficulty newDifficulty, bool keepInventory, bool isHardcore)
    {
        GameDifficulty = newDifficulty;
        KeepInventory = keepInventory;
        IsHardcore = isHardcore;
    }

    public void SetCurrentLocation(ScriptableAdventureLocation location)
    {
        CurrentAdventureLocation = location;
    }


    #region SAVE/LOAD

    public static void SaveGame()
    {
        GameData data = new GameData(Instance.GetSaveData());

        SaveSystem.SaveGame(data);
    }


    public void LoadGame()
    {
        LoadSaveData(SaveSystem.LoadGame()?.GameManagerData);

        //load the scene where the player left the game
        SceneManagementSystem.Instance.LoadScene(Instance.CurrentScene);

        if (Instance.ShowGameOverScreenOnSaveLoad)
        {
            InstantiatePrefab(Instance.GameOverMenu, null);
        }
    }

    /// <summary>
    /// Save system helper method
    /// </summary>
    public GameManagerSaveData GetSaveData()
    {
        List<AdventureLocationSaveData> locDataList = new List<AdventureLocationSaveData>();
        foreach (var location in AdventureLocationData)
        {
            locDataList.Add(location.GetSaveData());
        }

        GameManagerSaveData data = new GameManagerSaveData(
            currency,
            gameDifficulty,
            GameState,

            PlayerManager.GetSaveData(),
            null,
            CurrentAdventureLocation?.GetSaveData(),
            locDataList,

            IsHardcore,
            KeepInventory,
            CampStorageSpace,
            PlayerInventorySpace,
            ShowGameOverScreenOnSaveLoad,
            CurrentScene,
            BattleGameSpeed,
            NumOfCopperForOneSilver,
            NumOfSilverForOneGold
        );

        return data;
    }

    /// <summary>
    /// Save system helper method
    /// </summary>
    public void LoadSaveData(GameManagerSaveData data)
    {
        List<ScriptableAdventureLocation> adventureLocations = new List<ScriptableAdventureLocation>();
        foreach (var locData in data.adventureLocationData)
        {
            adventureLocations.Add(ScriptableAdventureLocation.GetAdventureLocationFromSaveData(locData));
        }

        PlayerManager = new PlayerManager(data.playerManagerData);

        //TODO: after pets/merc are implemented
        //Allies = data.allies;
        Allies = null;

        CurrentAdventureLocation = ScriptableAdventureLocation.GetAdventureLocationFromSaveData(data.currentAdventureLocation);
        AdventureLocationData = adventureLocations;

        this.currency = data.currency;
        this.gameDifficulty = data.gameDifficulty;
        GameState = data.gameState;
        IsHardcore = data.isHardcore;
        KeepInventory = data.keepInventory;
        CampStorageSpace = data.campStorageSpace;
        PlayerInventorySpace = data.playerInventorySpace;
        ShowGameOverScreenOnSaveLoad = data.showGameOverScreenOnSaveLoad;
        CurrentScene = data.currentScene;
        BattleGameSpeed = data.battleGameSpeed;
        NumOfCopperForOneSilver = data.numOfCopperForOneSilver;
        NumOfSilverForOneGold = data.numOfSilverForOneGold;
    }

    #endregion SAVE/LOAD


    /// <summary>
    /// Convert currency amount to a pretty display string with coin icons
    /// </summary>
    /// <param name="amount">Amount to convert</param>
    /// <param name="compactMode">If true, no spaces will be added between coins</param>
    public string CurrencyToDisplayString(int amount, bool compactMode = false)
    {
        int copperAmnt = amount % NumOfCopperForOneSilver;

        //convert all u can to silver
        int tempAmnt = amount / NumOfCopperForOneSilver;

        int silverAmnt = tempAmnt % NumOfSilverForOneGold;
        int goldAmnt = tempAmnt / NumOfSilverForOneGold;

        string copperIcon = ResourceSystem.GetIconTag(Icon.Coin_Copper);
        string silverIcon = ResourceSystem.GetIconTag(Icon.Coin_Silver);
        string goldIcon = ResourceSystem.GetIconTag(Icon.Coin_Gold);

        if (compactMode)    //no spaces
            return $"{(goldAmnt > 0 ? $"{goldIcon} {goldAmnt}" : "")}{(silverAmnt > 0 ? $"{silverIcon}{silverAmnt}" : "")}{copperIcon}{copperAmnt}";
        else
            return $"{(goldAmnt > 0 ? $"{goldIcon} {goldAmnt}  " : "")}{(silverAmnt > 0 ? $"{silverIcon} {silverAmnt}  " : "")}{copperIcon} {copperAmnt}";
    }

    public void EndAdventure()
    {
        //leave battle
        GameState = BattleState.None;
        SceneManagementSystem.Instance.LoadScene(Scenes.Outskirts);

        //handle shop reset
        PlayerManager.RefreshShopInventory();

        //handle stage progress detoriation
        DetoriateOtherStageProgress();

        CurrentAdventureLocation = null;
    }

    private void DetoriateOtherStageProgress()
    {
        foreach (var location in AdventureLocationData)
        {
            if (location != CurrentAdventureLocation && location.PlayerProgress > 0)
                location.PlayerProgress--;
        }
    }

    private GameObject InstantiatePrefab(GameObject prefab, Transform parentTransform)
    {
        var obj = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);

        if (parentTransform != null)
            obj.transform.SetParent(parentTransform, true);

        obj.transform.localScale = Vector3.one;
        obj.transform.localPosition = Vector3.zero;

        return obj;
    }

    public ScriptableAdventureLocation HighestClearedAdventureLocation()
    {
        ScriptableAdventureLocation highestCleared = null;

        foreach (var location in AdventureLocationData)
        {
            if (location.HasPlayerClearedFirstBoss)
                highestCleared = location;
        }

        return highestCleared;
    }

    #endregion METHODS
}
