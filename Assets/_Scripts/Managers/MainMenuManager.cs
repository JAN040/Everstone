using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class MainMenuManager : MonoBehaviourPunCallbacks
{
    #region VARIABLES


    #region UI References


    [Header("UI References")]
    [SerializeField] Button ContinueButton;

    [SerializeField] GameObject MultiplayerInfoBox;
    [SerializeField] TextMeshProUGUI MultiplayerInfoBoxText;


    #endregion UI References


    //[Space]
    //[Header("Variables")]
    private const string CONNECTING_TEXT = "Connecting to the game server...";
    private const string CONNECTION_FAILED_TEXT = "Connection to the server failed!";


    #endregion VARIABLES


    #region UNITY METHODS


    // Start is called before the first frame update
    void Start()
    {
        ContinueButton.interactable = SaveSystem.SaveFileExists();
    }

    #endregion UNITY METHODS


    #region METHODS

    
    public void OnContinueClicked()
    {
        GameManager.Instance.LoadGame();
    }


    #region Multiplayer


    public void OnMultiplayerClicked()
    {
        MultiplayerInfoBox.gameObject.SetActive(true);
        MultiplayerInfoBoxText.text = CONNECTING_TEXT;

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = Application.version;
        PhotonNetwork.ConnectUsingSettings();
    }

    public void InfoBoxCancelClicked()
    {
        MultiplayerInfoBox.gameObject.SetActive(false);
        PhotonNetwork.Disconnect();
    }

    public override void OnConnectedToMaster()
    {
        SceneManagementSystem.Instance.LoadScene(Scenes.MultiplayerLobby);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        MultiplayerInfoBox.gameObject.SetActive(true);
        MultiplayerInfoBoxText.text = CONNECTION_FAILED_TEXT + $"\nReason:\n {cause}";
    }

    


    #endregion Multiplayer


    #endregion METHODS
}
