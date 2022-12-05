using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
	#region VARIABLES


    #region UI References


    //[Header("UI References")]


    #endregion UI References


    //[Space]
    //[Header("Variables")]


    #endregion VARIABLES


    #region UNITY METHODS


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    #endregion UNITY METHODS


    #region METHODS

    
    public void ExitClicked()
    {
        PhotonNetwork.LeaveRoom();
        SceneManagementSystem.Instance.LoadScene(Scenes.MultiplayerLobby);
    }


    #endregion METHODS
}
