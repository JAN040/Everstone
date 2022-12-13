using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardEntryPanel : MonoBehaviour
{
    #region VARIABLES


    #region UI References


    [Header("UI References")]
    [SerializeField] TextMeshProUGUI RankingText;
    [SerializeField] TextMeshProUGUI PlayerNicknameText;
    [SerializeField] TextMeshProUGUI PointsAmountText;
    [SerializeField] Image PlayerIconImage;


    #endregion UI References


    //[Space]
    //[Header("Variables")]


    #endregion VARIABLES


    #region UNITY METHODS



    #endregion UNITY METHODS


    #region METHODS


    public void Init(int ranking, string nickname, Sprite icon, int pointsAmount)
    {
        RankingText.text = ranking.ToString();
        PlayerNicknameText.text = nickname;
        PointsAmountText.text = pointsAmount.ToString();
        PlayerIconImage.sprite = icon;
    }
    

    #endregion METHODS
}
