using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameManager : Singleton<GameManager>
{
    #region VARIABLES

    [SerializeField] private int currency;
    [SerializeField] private Difficulty gameDifficulty;

    /// <summary>
    /// Reference to the PlayerManager script
    /// </summary>
    public PlayerManager PlayerManager { get; private set; }

    /// <summary>
    /// Reference to the UnitData script
    /// </summary>
    public UnitData UnitData { get; private set; }

    /// <summary>
    /// List of all player allies (pets, mercs, mission teammates etc.)
    /// </summary>
    public List<ScriptableUnitBase> Allies { get; set; }

    /// <summary>
    /// Stores the reference to the adventure location the player is currently in (null if not in adventure)
    /// </summary>
    public ScriptableAdventureLocation CurrentLocation { get; private set; }
    public List<ScriptableAdventureLocation> AdventureLocationData { get; set; } = null;

    public int Currency { get => currency; set => currency = value; }
    public Difficulty GameDifficulty { get => gameDifficulty; private set => gameDifficulty = value; }

    public bool IsHardcore { get; private set; }


    #region Player prefs


    public float BattleGameSpeed = 1f;


    #endregion Player prefs


    #endregion VARIABLES


    #region UNITY METHODS

    void Start()
    {
        PlayerManager = new PlayerManager();
        UnitData = new UnitData();
    } 

    #endregion UNITY METHODS


    #region METHODS


    public void SetGameDifficulty(Difficulty newDifficulty, bool isHardcore)
    {
        GameDifficulty = newDifficulty;
        IsHardcore = isHardcore;
    }

    public void SetCurrentLocation(ScriptableAdventureLocation location)
    {
        CurrentLocation = location;
    }

    public void SaveGame()
    {
        GameData data = new GameData()
        {

        };
        //Serialize(data, location);
    }

    public void LoadGame()
    {
        //GameData data = Deserialize(location);
    }

    #endregion METHODS
}
