using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Photon.Pun;
using System.Linq;
using Photon.Realtime;

public class MultiplayerGameOverManager : MonoBehaviour
{
    #region VARIABLES


    #region UI References


    [Header("UI References")]

    [Header("Leaderboard")]
    [SerializeField] GameObject LeaderboardEntryPrefab;
    [SerializeField] GameObject LeaderboardEntryContainer;

    [SerializeField] TextMeshProUGUI TitleText;
    [SerializeField] TextMeshProUGUI WinningPlayerText;
    [SerializeField] TextMeshProUGUI TimerText;
    [SerializeField] TextMeshProUGUI WinCriteriaLabelText;


    #endregion UI References


    //[Space]
    //[Header("Variables")]


    #endregion VARIABLES



    #region UNITY METHODS


    // Start is called before the first frame update
    void Start()
    {
        InitLeaderboard();
    }


    #endregion UNITY METHODS



    #region METHODS


    private void InitLeaderboard()
    {
        //grab multiplayer data

        //NOTE: if i ever wanna display players who left mid game, i'd need to track the list locally (for every player?) then display the missing players as such
        var startTime = DateTime.Parse((string)PhotonNetwork.CurrentRoom.CustomProperties["StartTime"]);
        var timeLimit = (string)PhotonNetwork.CurrentRoom.CustomProperties["TimeLimit"];
        var pointGoal = (int)PhotonNetwork.CurrentRoom.CustomProperties["PointGoal"];
        
        //get a list of players and create an ordered list of playerData
        var playerList = PhotonNetwork.PlayerList.ToList();

        if (playerList == null || playerList.Count < 1)
            return;

        List<PlayerData> playerDataList = new List<PlayerData>();
        playerList.ForEach(x =>
        {
            var data = x.CustomProperties;
            playerDataList.Add(new PlayerData(x.NickName, (int)data["PointAmount"], data));
        });
        playerDataList.OrderByDescending(x => x.Score);

        //set winner text
        WinningPlayerText.text = playerDataList.First().Name;

        //set timer & goal
        TimeSpan timeSpan = DateTime.Now - startTime;
        TimerText.text = $"Timer: {timeSpan.Minutes}/{timeLimit}m   Goal: {pointGoal}";

        //set win criteria label text
        WinCriteriaLabelText.text = ResourceSystem.GetWinCriteriaDisplayString(
            (MultiplayerWinCriteria)PhotonNetwork.CurrentRoom.CustomProperties["WinCriteria"]
        );

        int ranking = 0;
        int prevScore = -1;

        //fill the leaderboard
        foreach (var playerData in playerDataList)
        {
            int score = playerData.Score;

            if (score != prevScore) //players with the same score have the same ranking
                ranking++;

            var entryPrefab = InstantiatePrefab(LeaderboardEntryPrefab, LeaderboardEntryContainer.transform);
            entryPrefab.GetComponent<LeaderboardEntryPanel>().Init(
                ranking,
                playerData.Name,
                ResourceSystem.Instance.GetHeroPortraitByName((string)playerData.CustomProperties["PortraitName"]).PortraitImage,
                score
            );

            prevScore = score;
        }
    }


    public void OnExitClicked()
    {
        PhotonNetwork.Disconnect();
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

    private class PlayerData
    {
        public string Name;
        public int Score;
        public ExitGames.Client.Photon.Hashtable CustomProperties;

        public PlayerData(string name, int score, ExitGames.Client.Photon.Hashtable customProperties)
        {
            Name = name;
            Score = score;
            CustomProperties = customProperties;
        }
    }

    #endregion METHODS
}