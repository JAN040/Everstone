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


    [SerializeField] List<ScriptableEnemy> enemyPool;
    //TODO eventPool ?
    //TODO boss ?
    //TODO nestSetup ?
    


    #region METHODS


    public EncounterType GetNextEncounter()
    {
        int nextProgress = PlayerProgress + 1;

        //If the next encounter is the last, it is either a boss or a nest
        if (nextProgress == this.stageAmount)
            return HasPlayerClearedFirstBoss ? EncounterType.MonsterNest : EncounterType.BossEnemy;

        //check if its an event
        if (Helpers.DiceRoll(LocationData.EVENT_CHANCE))
            return EncounterType.Event;

        //check if its a multi enemy encounter
        if (Helpers.DiceRoll(GetMultiEnemyChance()))
        {
            return EncounterType.MultipleEnemy;
        }

        return EncounterType.SingleEnemy;
    }

    public ScriptableEnemy GetRandomEnemy()
    {
        bool isElite = Helpers.DiceRoll(GetEliteEnemyChance());
        List<ScriptableEnemy> effectivePool = null;

        if (isElite)
        {
            effectivePool = enemyPool.Where(x => x.Type == EnemyType.Elite).ToList();
        }
        else
        {
            effectivePool = enemyPool.Where(x => x.Type == EnemyType.Normal).ToList();
        }

        return effectivePool[Random.Range(0, effectivePool.Count)];
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
