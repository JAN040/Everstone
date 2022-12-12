using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using System;
using Photon.Realtime;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    #region VARIABLES


    #region UI References


    [Header("UI References")]
    [SerializeField] TMP_InputField PlayerNameInput;

    [Space]
    [Header("Create room")]
    [SerializeField] TMP_InputField CreateRoomNameInput;
    [SerializeField] Button CreateRoomButton;

    [Space]
    [Header("Join room")]
    [SerializeField] TMP_InputField JoinRoomNameInput;
    [SerializeField] Button JoinRoomButton;
    [SerializeField] Button ListRoomsButton;

    [Space]
    [Header("List rooms")]
    [SerializeField] GameObject RoomListPanel;
    [SerializeField] GameObject RoomListContainer;
    [SerializeField] GameObject LobbyRoomPanelPrefab;

    [Space]
    [Header("Info Box")]
    [SerializeField] GameObject InfoBox;
    [SerializeField] TextMeshProUGUI InfoBoxTitleText;
    [SerializeField] TextMeshProUGUI InfoBoxMessageText;


    #endregion UI References


    [Space]
    [Header("Variables")]
    public List<RoomInfo> RoomList = new List<RoomInfo>();
    private List<GameObject> RoomPanelObjectList = new List<GameObject>();


    #endregion VARIABLES


    #region UNITY METHODS



    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Joining the lobby...");
        PhotonNetwork.JoinLobby();

        if (PhotonNetwork.IsConnected && !string.IsNullOrEmpty(PhotonNetwork.NickName))
            PlayerNameInput.text = PhotonNetwork.NickName;

        UpdateUI();
    }


    #endregion UNITY METHODS


    #region METHODS


    public void UpdateUI()
    {
        bool isPlayerNameValid = IsNameValid(PlayerNameInput?.text);
        bool isCreateRoomNameValid = IsNameValid(CreateRoomNameInput?.text);
        bool isJoinRoomNameValid = IsNameValid(JoinRoomNameInput?.text);

        CreateRoomButton.interactable = isPlayerNameValid && isCreateRoomNameValid;
        JoinRoomButton.interactable = isPlayerNameValid && isJoinRoomNameValid;
        ListRoomsButton.interactable = isPlayerNameValid;

        if (PhotonNetwork.IsConnected && isPlayerNameValid)
            PhotonNetwork.NickName = PlayerNameInput.text;
    }

    private void ShowInfoBox(string title, string message)
    {
        if (InfoBox == null)
            return;

        InfoBox.gameObject.SetActive(true);

        InfoBoxTitleText.text = title;
        InfoBoxMessageText.text = message;
    }

    private bool IsNameValid(string name)
    {
        return !string.IsNullOrEmpty(name) &&
               name.Length > 2;
    }

    public void CreateRoom()
    {
        if (!PhotonNetwork.IsConnected)
            return;

        PhotonNetwork.NickName = PlayerNameInput.text;
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 10;
        options.EmptyRoomTtl = 0;
        options.IsVisible = true;
        options.IsOpen = true;

        PhotonNetwork.CreateRoom(CreateRoomNameInput.text, options);
    }

    /// <summary>
    /// Join room button click method
    /// </summary>
    public void JoinRoom()
    {
        JoinRoom(JoinRoomNameInput.text);
    }

    /// <summary>
    /// For joining a room from the list of available rooms
    /// </summary>
    public void JoinRoom(string roomName)
    {
        if (!PhotonNetwork.IsConnected)
            return;

        PhotonNetwork.NickName = PlayerNameInput.text;
        PhotonNetwork.JoinRoom(roomName);
    }

    /// <summary>
    /// List all button click method
    /// </summary>
    public void ListRooms()
    {
        if (!PhotonNetwork.IsConnected)
            return;

        UpdateRoomListObject();
        
        RoomListPanel.gameObject.SetActive(true);
    }

    /// <summary>
    /// loop through RoomList and instantiate objects, put them into RoomListContainer
    /// </summary>
    private void UpdateRoomListObject()
    {
        if (RoomPanelObjectList == null)
            RoomPanelObjectList = new List<GameObject>();

        //clean all the previous room panels
        if (RoomPanelObjectList != null && RoomPanelObjectList.Count > 0)
            foreach (var panelObj in RoomPanelObjectList)
                Destroy(panelObj.gameObject);
        
        RoomPanelObjectList.Clear();


        //add new panels from fresh data
        foreach (var roomInfo in RoomList)
        {
            if (roomInfo.RemovedFromList)
                continue;

            var prefab = InstantiatePrefab(LobbyRoomPanelPrefab, RoomListContainer.transform);
            prefab.GetComponent<LobbyRoomPanel>().Init(roomInfo);
            
            RoomPanelObjectList.Add(prefab);
        }
    }

    public void ExitClicked()
    {
        PhotonNetwork.Disconnect();
        SceneManagementSystem.Instance.LoadScene(Scenes.MainMenu);
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



    #region PUN Callbacks

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined the lobby successfully");
    }

    public override void OnJoinedRoom()
    {
        SceneManagementSystem.Instance.LoadScene(Scenes.MultiplayerRoom);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        RoomList = roomList;

        if(RoomListPanel.gameObject.activeInHierarchy)
            UpdateRoomListObject();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        string msg = $"Code {returnCode}:\n{message}";

        ShowInfoBox("Failed to join the room", msg);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        string msg = $"Code {returnCode}:\n{message}";

        ShowInfoBox("Failed to create the room", msg);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"Player {PhotonNetwork.NickName} disconnected. Reason: {cause}");
        StartCoroutine(OnDisconnectedRoutine(cause));
    }


    #endregion PUN Callbacks


    private IEnumerator OnDisconnectedRoutine(DisconnectCause cause)
    {
        //when leaving the lobby by choice, dont show anything
        if (cause != DisconnectCause.DisconnectByClientLogic)
        {
            ShowInfoBox("Disconnected from the server.", $"Reason:\n{cause}");

            yield return new WaitForSeconds(5);

            SceneManagementSystem.Instance.LoadScene(Scenes.MainMenu);
        }
    }


    #endregion METHODS
}
