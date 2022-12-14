using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;
using System.Linq;
using System;


public class RoomManager : MonoBehaviourPunCallbacks
{
    #region VARIABLES


    #region UI References


    [Header("UI References")]
    [SerializeField] TextMeshProUGUI RoomTitleText;
    
    [Space]
    [SerializeField] GameObject PlayerListContainer;
    [SerializeField] GameObject PlayerPanelPerfab;

    [Space]
    [SerializeField] Button StartGameButton;

    [Space]
    [Header("Settings panel")]
    [SerializeField] GameObject GameSettingsPanel;
    [SerializeField] Button SaveButton;
    [Space]
    [SerializeField] TMP_InputField TimeLimitInput;
    [SerializeField] Toggle HardcoreCheckbox;
    [SerializeField] Toggle KeepInventoryCheckbox;
    [Space]
    [SerializeField] TMP_Dropdown WinCriteriaCombo;
    [SerializeField] TMP_InputField PointGoalInput;
    [SerializeField] TMP_Dropdown GameDifficultyCombo;

    [Space]
    [Header("Info Box")]
    [SerializeField] GameObject InfoBox;
    [SerializeField] TextMeshProUGUI InfoBoxTitleText;
    [SerializeField] TextMeshProUGUI InfoBoxMessageText;


    #endregion UI References


    //[Space]
    //[Header("Variables")]
    private List<GameObject> PlayerPanelObjectList = new List<GameObject>();
    private ExitGames.Client.Photon.Hashtable GameSettings = new ExitGames.Client.Photon.Hashtable();


    #endregion VARIABLES


    #region UNITY METHODS


    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            RoomTitleText.text = $"Room '{PhotonNetwork.CurrentRoom?.Name}'";
            UpdateStartGameButtonInteractibility();

            StartCoroutine("PeriodicPlayerListRefresh");

            if (PhotonNetwork.IsMasterClient)
            {
                //initialize setting values
                GameSettings["TimeLimit"] = "60";
                GameSettings["WinCriteria"] = MultiplayerWinCriteria.Gold;
                GameSettings["PointGoal"] = 10000;
                GameSettings["GameDifficulty"] = Difficulty.Normal;
                GameSettings["KeepInventory"] = true;
                GameSettings["IsHardcore"] = false;

                PhotonNetwork.CurrentRoom.SetCustomProperties(GameSettings);
            }
        }
        else
        {   //something went very wrong, cut our losses and jump back to main menu before more things break
            SceneManagementSystem.Instance.LoadScene(Scenes.MainMenu);
        }
    }

    private void OnDestroy()
    {
        StopCoroutine("PeriodicPlayerListRefresh");
    }


    #endregion UNITY METHODS



    #region METHODS


    private IEnumerator PeriodicPlayerListRefresh()
    {
        //update player list every 2 seconds, cause the callbacks sometimes just dont get called??? retarded
        while (true)
        {
            UpdateRoomListObject();
            
            yield return new WaitForSeconds(2);
        }
    }


    /// <summary>
    /// loop through RoomList and instantiate objects, put them into RoomListContainer
    /// </summary>
    private void UpdateRoomListObject()
    {
        if (PlayerPanelObjectList == null)
            PlayerPanelObjectList = new List<GameObject>();

        if (PhotonNetwork.CurrentRoom == null)
            return;

        //get room player list
        List<Player> playerList = PhotonNetwork.CurrentRoom.Players.Values.ToList();

        //clean all the previous room panels
        if (PlayerPanelObjectList != null && PlayerPanelObjectList.Count > 0)
            foreach (var panelObj in PlayerPanelObjectList)
                Destroy(panelObj.gameObject);

        PlayerPanelObjectList.Clear();


        //add new panels from fresh data
        foreach (var player in playerList)
        {
            var prefab = InstantiatePrefab(PlayerPanelPerfab, PlayerListContainer.transform);
            prefab.GetComponent<RoomPlayerPanel>().Init(player);

            PlayerPanelObjectList.Add(prefab);
        }

        //refresh the button since ownership of room can change if master leaves & player count just changed
        UpdateStartGameButtonInteractibility();
    }


    public void GameSettingsClicked()
    {
        //if this client is the room master, enable setting editing
        EnableDisableSettingPanelOptions(PhotonNetwork.IsMasterClient);

        //show and update the setting panel
        ShowSettings();
    }

    public void SaveSettingsClicked()
    {
        //initialize setting values
        GameSettings["TimeLimit"] = TimeLimitInput.text;
        GameSettings["WinCriteria"] = (MultiplayerWinCriteria)WinCriteriaCombo.value;
        GameSettings["PointGoal"] = int.Parse(PointGoalInput.text);
        GameSettings["GameDifficulty"] = (Difficulty)GameDifficultyCombo.value;
        GameSettings["KeepInventory"] = KeepInventoryCheckbox.isOn;
        GameSettings["IsHardcore"] = HardcoreCheckbox.isOn;

        PhotonNetwork.CurrentRoom.SetCustomProperties(GameSettings);

        //close the panel
        GameSettingsPanel.gameObject.SetActive(false);
    }

    /// <summary>
    /// Shows the room settings panel and updates it from this rooms CustomProperties
    /// </summary>
    private void ShowSettings()
    {
        var settings = PhotonNetwork.CurrentRoom.CustomProperties;
        
        if (settings == null)
            return;

        //update settings
        TimeLimitInput.text = settings["TimeLimit"].ToString();
        HardcoreCheckbox.isOn = (bool)settings["IsHardcore"];
        KeepInventoryCheckbox.isOn = (bool)settings["KeepInventory"];
        WinCriteriaCombo.value = (int)settings["WinCriteria"];
        PointGoalInput.text = $"{settings["PointGoal"]}";
        GameDifficultyCombo.value = (int)settings["GameDifficulty"];
        
        GameSettingsPanel.gameObject.SetActive(true);
    }

    private void EnableDisableSettingPanelOptions(bool isEnabled)
    {
        TimeLimitInput.interactable = isEnabled;
        HardcoreCheckbox.interactable = isEnabled;
        KeepInventoryCheckbox.interactable = isEnabled;
        WinCriteriaCombo.interactable = isEnabled;
        PointGoalInput.interactable = isEnabled;
        GameDifficultyCombo.interactable = isEnabled;
        SaveButton.interactable = isEnabled;
    }

    public void StartGameClicked()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        var roomSettings = PhotonNetwork.CurrentRoom.CustomProperties;
        roomSettings["StartTime"] = DateTime.Now.ToString();
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomSettings);

        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        GameManager.Instance.StartMultiplayerGame();
    }


    public void ExitClicked()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.CurrentRoom != null)
            PhotonNetwork.LeaveRoom();
        
        SceneManagementSystem.Instance.LoadScene(Scenes.MultiplayerLobby);
    }


    private GameObject InstantiatePrefab(GameObject prefab, Transform parentTransform)
    {
        GameObject obj;

        if (parentTransform == null)
            obj = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
        else
            obj = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity, parentTransform);

        //if (parentTransform != null)
        //    obj.transform.SetParent(parentTransform, false);  //when manually setting the parent, in order for transform stretch to work, keepWorldSpace flag needs to be false...

        obj.transform.localScale = Vector3.one;
        obj.transform.localPosition = Vector3.zero;

        return obj;
    }


    /// <summary>
    /// Enables/Disables the Start game button depending on the number of players in the room
    /// </summary>
    private void UpdateStartGameButtonInteractibility()
    {
        StartGameButton.interactable = 
            PhotonNetwork.IsMasterClient &&     //only master can start the game
            PhotonNetwork.PlayerList.Length > 1;//need more than one player in the room
    }


    #region PUN Callbacks


    public override void OnPlayerEnteredRoom(Player player)
    {
        UpdateRoomListObject();
    }

    public override void OnPlayerLeftRoom(Player player)
    {
        UpdateRoomListObject();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"Player {PhotonNetwork.NickName} disconnected. Reason: {cause}");
        StartCoroutine(OnDisconnectedRoutine(cause));
    }


    #endregion PUN Callbacks

    private IEnumerator OnDisconnectedRoutine(DisconnectCause cause)
    {
        ShowInfoBox("Disconnected from the server.", $"Reason:\n{cause}");

        yield return new WaitForSeconds(5);

        SceneManagementSystem.Instance.LoadScene(Scenes.MainMenu);
    }

    private void ShowInfoBox(string title, string message)
    {
        if (InfoBox == null)
            return;

        InfoBox.gameObject.SetActive(true);

        InfoBoxTitleText.text = title;
        InfoBoxMessageText.text = message;
    }


    #endregion METHODS
}
