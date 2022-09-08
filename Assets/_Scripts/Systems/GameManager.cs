using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Nice, easy to understand enum-based game manager. For larger and more complex games, look into
/// state machines. But this will serve just fine for most games.
/// </summary>
public class GameManager : Singleton<GameManager>
{
    [SerializeField] private int currency;
    [SerializeField] private Dictionary<string, int> AdventureLocationProgress;
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

    //public static event Action<GameState> OnBeforeStateChanged;
    //public static event Action<GameState> OnAfterStateChanged;

    //public GameState State { get; private set; }
    public int Currency { get => currency; set => currency = value; }
    public Difficulty GameDifficulty { get => gameDifficulty; private set => gameDifficulty = value; }



    // Kick the game off with the first state
    void Start()
    {
        PlayerManager = new PlayerManager();
        UnitData      = new UnitData();
    }

    #region METHODS

    //public void ChangeState(GameState newState)
    //{
    //    OnBeforeStateChanged?.Invoke(newState);

    //    State = newState;
    //    switch (newState)
    //    {
    //        case GameState.Starting:
    //            //HandleStarting();
    //            break;
    //        case GameState.SpawningHeroes:
    //            //HandleSpawningHeroes();
    //            break;
    //        case GameState.SpawningEnemies:
    //            //HandleSpawningEnemies();
    //            break;
    //        case GameState.HeroTurn:
    //            //HandleHeroTurn();
    //            break;
    //        case GameState.EnemyTurn:
    //            break;
    //        case GameState.Win:
    //            break;
    //        case GameState.Lose:
    //            break;
    //        default:
    //            throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
    //    }

    //    OnAfterStateChanged?.Invoke(newState);

    //    //Debug.Log($"New state: {newState}");
    //}

    public void SetGameDifficulty(Difficulty newDifficulty)
    {
        GameDifficulty = newDifficulty;
    }

    public void SetCurrentLocation(ScriptableAdventureLocation location)
    {
        CurrentLocation = location;
    }

    private void HandleStarting()
    {
        // Do some start setup, could be environment, cinematics etc

        // Eventually call ChangeState again with your next state

        //ChangeState(GameState.SpawningHeroes);
    }

    private void HandleSpawningHeroes()
    {
        //UnitManager.Instance.SpawnHeroes();

        //ChangeState(GameState.SpawningEnemies);
    }

    private void HandleSpawningEnemies()
    {

        // Spawn enemies

        //ChangeState(GameState.HeroTurn);
    }

    private void HandleHeroTurn()
    {
        // If you're making a turn based game, this could show the turn menu, highlight available units etc

        // Keep track of how many units need to make a move, once they've all finished, change the state. This could
        // be monitored in the unit manager or the units themselves.
    }

    #endregion METHODS
}

/// <summary>
/// This is obviously an example and I have no idea what kind of game you're making.
/// You can use a similar manager for controlling your menu states or dynamic-cinematics, etc
/// </summary>
[Serializable]
public enum GameState {
    Starting,
    SpawningHeroes,
    SpawningEnemies,
    HeroTurn,
    EnemyTurn,
    Win,
    Lose,
}