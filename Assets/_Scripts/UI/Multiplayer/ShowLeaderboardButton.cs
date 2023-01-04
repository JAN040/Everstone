using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using TMPro;
using System.Linq;

public class ShowLeaderboardButton : MonoBehaviour
{
    #region VARIABLES


    #region UI References


    [Header("UI References")]
    [SerializeField] GameObject LeaderboardEntryPrefab;
    [SerializeField] GameObject LeaderBoardPanel;
    [SerializeField] GameObject LeaderboardEntryContainer;

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
        //check if we are in multiplayer mode, if not, hide the button
        this.gameObject.SetActive(GameManager.Instance.IsMultiplayer);
    }


    #endregion UNITY METHODS


    #region METHODS


    public void OnClicked()
    {
        if (!GameManager.Instance.IsMultiplayer)
            return;

        ShowLeaderboard();
    }

    private void ShowLeaderboard()
    {
        //remove all previous entries from the leaderboard
        foreach (Transform child in LeaderboardEntryContainer.transform)
            Destroy(child.gameObject);

        //grab multiplayer data

        //NOTE: if i ever wanna display players who left mid game, i'd need to track the list locally (for every player?) then display the missing players as such
        var playerList = PhotonNetwork.PlayerList.ToList();
        var startTime = DateTime.Parse((string)PhotonNetwork.CurrentRoom.CustomProperties["StartTime"]);
        var timeLimit = (string)PhotonNetwork.CurrentRoom.CustomProperties["TimeLimit"];
        var pointGoal = (int)PhotonNetwork.CurrentRoom.CustomProperties["PointGoal"];

        if (playerList == null || playerList.Count < 1)
            return;

        List<PlayerData> playerDataList = new List<PlayerData>();
        playerList.ForEach(x =>
        {
            var data = x.CustomProperties;
            int pointAmount = data.ContainsKey("PointAmount") ? (int)data["PointAmount"] : 0;
            playerDataList.Add(new PlayerData(x.NickName, pointAmount, data));
        });
        playerDataList = playerDataList.OrderByDescending(x => x.Score).ToList();

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

            string portraitName = playerData.CustomProperties.ContainsKey("PortraitName") ? (string)playerData.CustomProperties["PortraitName"] : "";

            var entryPrefab = InstantiatePrefab(LeaderboardEntryPrefab, LeaderboardEntryContainer.transform);
            entryPrefab.GetComponent<LeaderboardEntryPanel>().Init(
                ranking,
                playerData.Name,
                ResourceSystem.Instance.GetHeroPortraitByName(portraitName).PortraitImage,
                score
            );

            prevScore = score;
        }

        //actually show the leaderboard
        LeaderBoardPanel.gameObject.SetActive(true);
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
