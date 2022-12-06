using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class RoomPlayerPanel : MonoBehaviour
{
    #region VARIABLES


    #region UI References


    [Header("UI References")]
    [SerializeField] TextMeshProUGUI NameText;
    [SerializeField] Image OwnerIcon;

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

    
    /// <param name="isLocal">Enables to differentiate between this player and other players in the room</param>
    public void Init(Player player)
    {
        if (player == null)
            return;

        if (NameText != null)
            NameText.text = player.NickName;

        if (player.IsLocal)
            NameText.color = Color.green;

        OwnerIcon.gameObject.SetActive(player.IsMasterClient);
    }

    #endregion METHODS
}
