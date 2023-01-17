using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Used to store Adventure location data like the background and encounterable enemies, events, etc.
/// </summary>

[CreateAssetMenu(menuName = "Scriptable/Locations/New Adventure Location", fileName = "SO_AdventureLocation_")]
public class ScriptableAdventureLocation : ScriptableObject
{
    public string locationName;

    public Sprite icon;

    public Sprite background;

    public LocationDifficulty difficulty;

    /// <summary>
    /// Indicates max progress, ie. how many stages need be cleared to encounter the boss
    /// </summary>
    public int stageAmount;

    /// <summary>
    /// Tracks till what stage the player got; when they adventure here again
    ///     this is where they will start
    /// </summary>
    [SerializeField] private int playerProgress = 0;
    public int PlayerProgress { get => playerProgress; set => playerProgress = value; }

    /// <summary>
    /// Used for some calculations like elite enemy chance etc
    /// </summary>
    [NonSerialized] public int CurrentProgress;

    /// <summary>
    /// If the boss was already reached, then getting to the last stage
    ///     will be a super hard monster nest encounter
    /// </summary>
    [SerializeField] private bool _hasPlayerClearedFirstBoss;   //afraid of removing this since it might break my scriptable objects
    public bool HasPlayerClearedFirstBoss
    {
        get => LoopCount > 0;
    }

    private int _loopCount = 0;
    public int LoopCount { 
        get 
        {
            return _loopCount;
        }
        set 
        {
            if (value != _loopCount + 1)
                return;

            if (value > LOOP_COUNT_LIMIT)
                return;

            _loopCount = value;
            this.IncreaseDifficulty();
        }
    }

    private const int LOOP_COUNT_LIMIT = 5;

    public List<ScriptableNpcUnit> EnemyPool { get; private set; }

    private const float COMMON_ENEMY_CHANCE = 0.05f;

    //TODO eventPool ?
    //TODO boss ?
    //TODO nestSetup ?


    #region STATIC METHODS

    /// <summary>
    /// Save load helper methods
    /// </summary>
    public static ScriptableAdventureLocation GetAdventureLocationFromSaveData(AdventureLocationSaveData data)
    {
        if (data == null)
            return null;

        var location = ResourceSystem.Instance.GetAdventureLocationByName(data.locationName);
        location.PlayerProgress = data.playerProgress;
        location._loopCount = data.LoopCount;
        location.difficulty = data.Difficulty;

        return location;
    } 

    #endregion STATIC METHODS



    #region METHODS

    public AdventureLocationSaveData GetSaveData()
    {
        AdventureLocationSaveData data = new AdventureLocationSaveData(
            locationName,
            playerProgress,
            LoopCount,
            difficulty
        );

        return data;
    }

    public void SetEnemyPool(List<ScriptableNpcUnit> pool)
    {
        if (pool != null && pool.Count != 0)
            EnemyPool = pool;
    }

    public EncounterType GetNextEncounter(int currentProgress)
    {
        int nextProgress = currentProgress + 1;

        if (difficulty == LocationDifficulty.Easy && currentProgress <= 10)
            return EncounterType.SingleEnemy;

        //If the next encounter is the last, it is either a boss or a nest
        if (nextProgress == this.stageAmount)
            return EncounterType.BossEnemy;

        //check if its an event
        if (Helper.DiceRoll(LocationData.EVENT_CHANCE))
            return EncounterType.Event;

        //check if its a multi enemy encounter
        if (Helper.DiceRoll(GetMultiEnemyChance()))
        {
            return EncounterType.MultipleEnemy;
        }

        return EncounterType.SingleEnemy;
    }

    /// <summary>
    /// Decide the enemies for the stage, from the ScriptableAdventureLocation enemy pool
    /// </summary>
    public List<ScriptableNpcUnit> RollEnemies(EncounterType encounterType, int currentProgress)
    {
        CurrentProgress = currentProgress;
        var result = new List<ScriptableNpcUnit>();

        switch (encounterType)
        {
            case EncounterType.SingleEnemy:
                result.Add(GetRandomEnemy());
                break;

            case EncounterType.MultipleEnemy:

                //for there to be multiple there have to be at least two
                result.Add(GetRandomEnemy());
                result.Add(GetRandomEnemy());

                var multiEnemyChance = this.GetMultiEnemyChance() / 2;

                //roll to see if we add even more (max 6 enemies)
                while (Helper.DiceRoll(multiEnemyChance) && result.Count < 6)
                {
                    result.Add(GetRandomEnemy());
                    multiEnemyChance /= 2;
                }

                break;

            case EncounterType.BossEnemy:
                for (int i = 0; i <= LoopCount; i++)
                {
                    result.Add(GetRandomBoss());
                }
                Debug.Log($"Boss room (loopCount: {LoopCount})");
                break;
            case EncounterType.Event:
            default:
                //this shouldnt really happen, events should be handled separately
                Debug.Log($"Unexpected EncounterType: {encounterType} in RollEnemies");
                break;
        }

        return result;
    }


    private ScriptableNpcUnit GetRandomBoss()
    {
        List<ScriptableNpcUnit> effectivePool = EnemyPool.Where(x => x.Type == EnemyType.Boss).ToList();

        return Instantiate(effectivePool[UnityEngine.Random.Range(0, effectivePool.Count)]);
    }

    public ScriptableNpcUnit GetRandomEnemy()
    {
        ScriptableNpcUnit res = null;

        bool isElite = Helper.DiceRoll(GetEliteEnemyChance());

        //check to make the early game in forest more manageable
        if (difficulty == LocationDifficulty.Easy && CurrentProgress <= 10)
            isElite = false;

        List<ScriptableNpcUnit> effectivePool;
        if (isElite)
        {
            effectivePool = EnemyPool.Where(x => x.Type == EnemyType.Elite).ToList();
            res = Instantiate(effectivePool[UnityEngine.Random.Range(0, effectivePool.Count)]);
        }
        else
        {
            //first check if the enemy will be one of the common (shared between locations) ones
            if (Helper.DiceRoll(COMMON_ENEMY_CHANCE))
            {
                res = ResourceSystem.Instance.GetRandomCommonEnemy();
            }
            else
            {
                effectivePool = EnemyPool.Where(x => x.Type == EnemyType.Normal).ToList();
                res = Instantiate(effectivePool[UnityEngine.Random.Range(0, effectivePool.Count)]);
            }
        }

        return res;
    }



    #region PRIVATE

    private float GetEliteEnemyChance()
    {
        float maxChance = ((int)GameManager.Instance.GameDifficulty) / 10f +
                            LocationData.BASE_ELITE_ENEMY_CHANCE;
        float stageMulitplier = ((playerProgress + 1) / stageAmount) / 2;

        return maxChance + stageMulitplier;
    }

    private float GetMultiEnemyChance()
    {
        float difficultyBonus = (((int)GameManager.Instance.GameDifficulty) - 1) / 10f;

        return LocationData.BASE_MULTI_ENEMY_CHANCE + difficultyBonus;
    }

    private void IncreaseDifficulty()
    {
        difficulty = difficulty.NextOrSame();
    }

    #endregion PRIVATE


    #endregion METHODS
}
