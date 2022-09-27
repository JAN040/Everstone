using System.Collections.Generic;
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
    /// If the boss was already reached, then getting to the last stage
    ///     will be a super hard monster nest encounter
    /// </summary>
    [SerializeField] private bool _hasPlayerClearedFirstBoss = false;
    public bool HasPlayerClearedFirstBoss
    {
        get => _hasPlayerClearedFirstBoss; 
        set
        {
            if (_hasPlayerClearedFirstBoss)
                return;

            if (value)
            {
                this.IncreaseDifficulty();
                _hasPlayerClearedFirstBoss = value;
            }
        }
    }


    public List<ScriptableNpcUnit> EnemyPool { get; private set; }
    //TODO eventPool ?
    //TODO boss ?
    //TODO nestSetup ?
    


    #region METHODS

    public void SetEnemyPool(List<ScriptableNpcUnit> pool)
    {
        if (pool != null && pool.Count != 0)
            EnemyPool = pool;
    }

    public EncounterType GetNextEncounter()
    {
        int nextProgress = PlayerProgress + 1;

        //If the next encounter is the last, it is either a boss or a nest
        if (nextProgress == this.stageAmount)
            return HasPlayerClearedFirstBoss ? EncounterType.MonsterNest : EncounterType.BossEnemy;

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
    public List<ScriptableNpcUnit> RollEnemies(EncounterType encounterType)
    {
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

                //roll to see if we add even more (max 6 enemies)
                while (Helper.DiceRoll(LocationData.MULTI_ENEMY_ADD_MORE_ENEMIES_CHANCE) && result.Count < 6)
                    result.Add(GetRandomEnemy());

                break;

            case EncounterType.BossEnemy:
                Debug.Log("TODO: BossEnemy");
                break;
            case EncounterType.MonsterNest:
                Debug.Log("TODO: MonsterNest");
                break;
            case EncounterType.Event:
            default:
                //this shouldnt really happen, events should be handled separately
                Debug.Log($"Unexpected EncounterType: {encounterType} in RollEnemies");
                break;
        }

        return result;
    }

    public ScriptableNpcUnit GetRandomEnemy()
    {
        bool isElite = Helper.DiceRoll(GetEliteEnemyChance());
        List<ScriptableNpcUnit> effectivePool = isElite ?
            effectivePool = EnemyPool.Where(x => x.Type == EnemyType.Elite).ToList()
            :
            effectivePool = EnemyPool.Where(x => x.Type == EnemyType.Normal).ToList();

        return Instantiate(effectivePool[Random.Range(0, effectivePool.Count)]);
    }



    #region PRIVATE

    private float GetEliteEnemyChance()
    {
        float maxChance = ((int)GameManager.Instance.GameDifficulty) / 10f +
                            LocationData.BASE_ELITE_ENEMY_CHANCE;
        float stageMulitplier = (playerProgress + 1) / stageAmount;


        return maxChance * stageMulitplier;
    }

    private float GetMultiEnemyChance()
    {
        float difficultyBonus = ((int)GameManager.Instance.GameDifficulty) / 10f;

        return LocationData.BASE_MULTI_ENEMY_CHANCE + difficultyBonus;
    }

    private void IncreaseDifficulty()
    {
        difficulty = difficulty.NextOrSame();
    }

    #endregion PRIVATE


    #endregion METHODS
}
