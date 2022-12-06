using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;
using System.Linq;

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


    #endregion UI References


    //[Space]
    //[Header("Variables")]
    private List<GameObject> PlayerPanelObjectList = new List<GameObject>();


    #endregion VARIABLES


    #region UNITY METHODS


    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            RoomTitleText.text = $"Room '{PhotonNetwork.CurrentRoom?.Name}'";
            StartGameButton.interactable = PhotonNetwork.IsMasterClient;

            StartCoroutine("PeriodicPlayerListRefresh");
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

        //refresh the button since ownership of room can change if master leaves
        StartGameButton.interactable = PhotonNetwork.IsMasterClient;
    }


    public void StartGameClicked()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.LoadLevel((int)Scenes.HeroSelect);
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
    }


    #endregion PUN Callbacks



    #endregion METHODS
}
