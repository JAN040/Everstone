using System;
using System.Collections.Generic;
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

    private PlayerManager playerManager;
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
    public List<ScriptableUnitBase> Allies { get; set; }

    /// <summary>
    /// Stores the reference to the adventure location the player is currently in (null if not in adventure)
    /// </summary>
    public ScriptableAdventureLocation CurrentAdventureLocation { get; private set; }
    public List<ScriptableAdventureLocation> AdventureLocationData { 
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
    public int PlayerInventorySpace = 10;
    private bool ShowGameOverScreenOnSaveLoad = false;


    #region Player prefs


    public float BattleGameSpeed = 1f;
    public int NumOfCopperForOneSilver = 10;
    public int NumOfSilverForOneGold = 100;


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
        PlayerManager = new PlayerManager();
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

    public void SaveGame()
    {
        GameData data = new GameData()
        {

        };
        //Serialize(data, location);
    }

    public void DeleteCurrentSave()
    {

    }

    public void LoadGame()
    {
        //GameData data = Deserialize(location);

        if (ShowGameOverScreenOnSaveLoad)
        {
            InstantiatePrefab(GameOverMenu, null);
        }
    }

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
