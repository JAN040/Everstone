using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class LobbyRoomPanel : MonoBehaviour
{
    #region VARIABLES


    #region UI References


    [Header("UI References")]
    [SerializeField] TextMeshProUGUI NameText;
    [SerializeField] TextMeshProUGUI CapacityText;
    [SerializeField] Button JoinButton;


    #endregion UI References


    //[Space]
    //[Header("Variables")]
    private RoomInfo RoomInfoRef;


    #endregion VARIABLES



    #region METHODS


    public void Init(RoomInfo roomInfo)
    {
        RoomInfoRef = roomInfo;

        UpdateUI();
    }
    
    public void UpdateUI()
    {
        if (RoomInfoRef == null)
            return;

        NameText.text = RoomInfoRef.Name;
        CapacityText.text = $"{RoomInfoRef.PlayerCount}/{RoomInfoRef.MaxPlayers}";
        JoinButton.interactable = RoomInfoRef.PlayerCount < RoomInfoRef.MaxPlayers &&
                                  RoomInfoRef.IsOpen;
    }
    
    public void JoinButtonClicked()
    {
        PhotonNetwork.JoinRoom(RoomInfoRef.Name);
    }


    #endregion METHODS
}
