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
    [Header("Info Box")]
    [SerializeField] GameObject InfoBox;
    [SerializeField] TextMeshProUGUI InfoBoxTitleText;
    [SerializeField] TextMeshProUGUI InfoBoxMessageText;


    #endregion UI References


    [Space]
    [Header("Variables")]
    public List<RoomInfo> RoomList = new List<RoomInfo>();


    #endregion VARIABLES


    #region UNITY METHODS


    // Start is called before the first frame update
    void Start()
    {
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
    }

    private void ShowInfoBox(string title, string message)
    {
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
        PhotonNetwork.NickName = PlayerNameInput.text;
        PhotonNetwork.CreateRoom(CreateRoomNameInput.text);
    }

    public void JoinRoom()
    {
        JoinRoom(JoinRoomNameInput.text);
    }

    /// <summary>
    /// For joining a room from the list of available rooms
    /// </summary>
    public void JoinRoom(string roomName)
    {
        PhotonNetwork.NickName = PlayerNameInput.text;
        PhotonNetwork.JoinRoom(roomName);
    }

    public void ListRooms()
    {
        //TODO:
    }

    public void ExitClicked()
    {
        PhotonNetwork.Disconnect();
        SceneManagementSystem.Instance.LoadScene(Scenes.MainMenu);
    }


    #region PUN Callbacks


    public override void OnJoinedRoom()
    {
        SceneManagementSystem.Instance.LoadScene(Scenes.MultiplayerRoom);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        RoomList = roomList;

        //TODO: if list component is shown, update its contents
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


    #endregion PUN Callbacks


    #endregion METHODS
}
