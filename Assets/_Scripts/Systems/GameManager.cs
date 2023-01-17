using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;


[System.Serializable]
public class GameManager : Singleton<GameManager>, IInRoomCallbacks, IConnectionCallbacks, IOnEventCallback
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

            UpdateMultiplayerScore(currency, MultiplayerWinCriteria.Gold);
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

    public bool IsMultiplayer { get { return PhotonNetwork.IsConnected && PhotonNetwork.InRoom; } }


    #region Player prefs


    public float BattleGameSpeed = 1f;
    public int NumOfCopperForOneSilver = 10;
    public int NumOfSilverForOneGold = 100;
    private int playerInventorySpace = 10;


    #endregion Player prefs


    #region PUN


    public const byte PLAYERREACHEDPOINTGOAL_EVENTCODE = 1;
    public const byte TIMEROVER_EVENTCODE = 2;
    public const byte ALLPLAYERSLEFT_EVENTCODE = 3;

    public const string GAMEOVERTYPE = "GameOverType";
    public const string WINNINGPLAYER = "WinningPlayer";

    private DateTime GameStartTime;
    private int GameLength;
    public DisconnectCause DisconnectCause = DisconnectCause.None;

    public bool IsMultiplayerGameOver = false;


    #endregion PUN


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

        StopAllCoroutines();
    }

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }


    #endregion UNITY METHODS



    #region METHODS



    #region Multiplayer


    public void ResetMultiplayerData()
    {
        IsMultiplayerGameOver = false;
        GameLength = 0;
        DisconnectCause = DisconnectCause.None;
    }


    /// <summary>
    /// Called on master client when he starts the room
    /// </summary>
    public void StartMultiplayerGame()
    {
        PhotonNetwork.LoadLevel((int)Scenes.HeroSelect);

        SetupGameMasterManager();
    }

    private void SetupGameMasterManager()
    {
        GameStartTime = DateTime.Parse((string)PhotonNetwork.CurrentRoom.CustomProperties["StartTime"]);
        GameLength = int.Parse((string)PhotonNetwork.CurrentRoom.CustomProperties["TimeLimit"]);

        StartCoroutine(GameTimerRoutine());
    }


    /// <summary>
    /// A routine that checks for game timeout every minute
    /// </summary>
    private IEnumerator GameTimerRoutine()
    {
        while (true)
        {
            //check for game timeout
            TimeSpan timeSpan = DateTime.Now - GameStartTime;
            if (timeSpan.Minutes >= GameLength)
            {
                SendGameOverEvent(TIMEROVER_EVENTCODE);

                break;
            }

            yield return new WaitForSecondsRealtime(60);
        }
    }

    /// <summary>
    /// Updates the score for the local player, if the game mode is actually tracking this criteria.
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="criteria"></param>
    public void UpdateMultiplayerScore(int amount, MultiplayerWinCriteria criteria)
    {
        if (!IsMultiplayer)
            return;

        var roomData = PhotonNetwork.CurrentRoom.CustomProperties;
        if ((MultiplayerWinCriteria)roomData["WinCriteria"] != criteria)
            return;

        var playerData = PhotonNetwork.LocalPlayer.CustomProperties;
        playerData["PointAmount"] = amount;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerData);

        //check if the player just reached the point goal
        int pointGoal = (int)roomData["PointGoal"];
        if (amount >= pointGoal)
        {
            if (!roomData.ContainsKey(WINNINGPLAYER))
            {
                //write data about the game being over
                roomData[WINNINGPLAYER] = PhotonNetwork.LocalPlayer;
                roomData[GAMEOVERTYPE] = MultiplayerGameOverType.PlayerReachedPointGoal;

                PhotonNetwork.CurrentRoom.SetCustomProperties(roomData);
                
                //send event about the game being over to all players
                SendGameOverEvent(PLAYERREACHEDPOINTGOAL_EVENTCODE);
            }
        }
    }

    public void PlayerLevelChanged()
    {
        if (!IsMultiplayer)
            return;

        int playerLevelSum = 0;

        //in character select when setting up the levelSystem this can be null
        if (PlayerManager?.PlayerHero?.LevelSystem?.Skills == null)
            return;

        var skillList = PlayerManager.PlayerHero.LevelSystem.Skills.Values.ToList();

        skillList.ForEach(x => playerLevelSum += x.Level);
        UpdateMultiplayerScore(playerLevelSum, MultiplayerWinCriteria.SumLevelCount);
    }

    public void StageProgressChanged()
    {
        int progressSum = 0;

        AdventureLocationData.ForEach(x => progressSum += x.PlayerProgress);
        UpdateMultiplayerScore(progressSum, MultiplayerWinCriteria.StageProgress);
    }

   

    /// <summary>
    /// Called when any of the players reach the point goal, effectively winning the match
    /// Sends an event to all players
    /// </summary>
    private void SendGameOverEvent(byte eventCode)
    { 
        PhotonNetwork.RaiseEvent(eventCode, null, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
    }

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;

        if (eventCode.In(PLAYERREACHEDPOINTGOAL_EVENTCODE,
                         ALLPLAYERSLEFT_EVENTCODE,
                         TIMEROVER_EVENTCODE)
        )
        {
            //prevents reloading the game finished scene from ALLPLAYERSLEFT triggerring
            //  after the game was already concluded
            if (IsMultiplayerGameOver)
                return;

            IsMultiplayerGameOver = true;
            LoadMultiplayerGameOverScene();
        }
    }


    private void LoadMultiplayerGameOverScene()
    {
        //PhotonNetwork.AutomaticallySyncScene = true;

        //change to the multiplayer ending scene
        if (SceneManagementSystem.Instance.IsSwitchingLocation) //if another scene is loading, queue the scene to be loaded after
            SceneManagementSystem.Instance.ContinueTransitioningTo = Scenes.MultiplayerGameOver;
        else
            SceneManagementSystem.Instance.LoadScene(Scenes.MultiplayerGameOver);
    }

    #region PUN Callbacks


    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        
        //check the player count
        if (PhotonNetwork.PlayerList.Length < 2)
        {
            SendGameOverEvent(ALLPLAYERSLEFT_EVENTCODE);
        }
    }

    public void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.LocalPlayer == newMasterClient)
            SetupGameMasterManager();
    }

    public void OnPlayerEnteredRoom(Player newPlayer)
    {
    }

    public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
    }


    //connection callbacks
    public void OnDisconnected(DisconnectCause cause)
    {
        if (cause != DisconnectCause.DisconnectByClientLogic)
            DisconnectCause = cause;
        
        SceneManagementSystem.Instance.LoadScene(Scenes.MainMenu);
    }

    public void OnConnected()
    {
    }

    public void OnConnectedToMaster()
    {
    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
    }

    public void OnCustomAuthenticationFailed(string debugMessage)
    {
    }


    #endregion PUN Callbacks


    #endregion Multiplayer


    public void ResetLocationData()
    {
        CurrentAdventureLocation = null;
        AdventureLocationData = ResourceSystem.Instance.GetAdventureLocations();
    }

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
        //no saving in multiplayer
        if (PhotonNetwork.IsConnected)
            return;

        GameData data = new GameData(Instance.GetSaveData());

        SaveSystem.SaveGame(data);
    }


    public void LoadGame()
    {
        LoadSaveData(SaveSystem.LoadGame()?.GameManagerData);

        //load the scene where the player left the game
        SceneManagementSystem.Instance.LoadSceneWithText(Instance.CurrentScene, "Loading...");

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

        //handle stage progress detoriation (only in singleplayer)
        if (IsMultiplayer)
            StageProgressChanged();
        else
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
