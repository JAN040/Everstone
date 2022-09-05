using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

public class AdventureManager : MonoBehaviour
{
    #region 	VARIABLES

    #region UI References

    [SerializeField] GameObject PauseMenu;
    [SerializeField] GameObject UnitPrefab;

    #endregion UI References


    [SerializeField] Image background;
    [SerializeField] bool IsPaused = false;
    [SerializeField] ScriptableEnemy targetedEnemy = null;

    private ScriptableAdventureLocation CurrentLocation;

    private ScriptableHero Hero;
    private List<ScriptableUnitBase> AlliedUnitsList;
    private List<ScriptableEnemy> EnemyUnitsList;

    private UnitGrid UnitGrid = new UnitGrid();

    private ScriptableEnemy TargetedEnemy { 
        get 
        {
            if (targetedEnemy == null)
                return UnitGrid.GetDefaultTarget(Faction.Enemies) as ScriptableEnemy;
            
            return targetedEnemy;
        } 
        set => targetedEnemy = value; 
    }
    #endregion 	VARIABLES


    #region 	UNITY METHODS

    // Start is called before the first frame update
    void Start()
    {
        CurrentLocation = GameManager.Instance?.CurrentLocation;
        Sprite currLocationSprite = CurrentLocation?.background;

        if (currLocationSprite != null)
            background.sprite = GameManager.Instance?.CurrentLocation?.background;

        InitStage(true);
    }

    // Update is called once per frame
    void Update()
    {

    }

    #endregion 	UNITY METHODS


    public void TogglePause()
    {
        IsPaused = !IsPaused;

        //DONT FORGET TO UNPAUSE AFTER ANY BUTTON CLICKS
        // (quitting the scene doesnt reset the timescale)
        //Update() still gets called even when timescale = 0
        //use time.unscaledDeltaTime to measure time even when timescale = 0
        Time.timeScale = IsPaused ? 0 : 1;

        TogglePausedMenu();
    }

    private void TogglePausedMenu()
    {
        if (PauseMenu == null)
            return;

        PauseMenu.SetActive(IsPaused);
    }

    private void InitStage(bool initAllies)
    {
        if (initAllies)
        {   //Handle allies (and hero)
            AlliedUnitsList = new List<ScriptableUnitBase>();
            Hero = GameManager.Instance.PlayerManager.PlayerHero;

            if (Hero != null)
                AlliedUnitsList.Add(Hero);

            if (GameManager.Instance.Allies != null)
                AlliedUnitsList.AddRange(GameManager.Instance.Allies);

            SpawnAllies();
        }

        //get encounter type
        EncounterType encounter = CurrentLocation.GetNextEncounter();

        if (encounter == EncounterType.Event)
        {
            //handle events
        }
        else
        {
            EnemyUnitsList = CurrentLocation.RollEnemies(encounter);
        }

        //spawn enemies/chests
        SpawnEnemies();


        //notify about stage number (current progress)
    }


    public void Test_ResetStage()
    {
        ResetAllies();
        ResetEnemies();

        InitStage(true);
    }

    private void ResetAllies()
    {
        foreach (var ally in AlliedUnitsList)
        {
            Destroy(ally.Prefab);
        }
        AlliedUnitsList.Clear();
        UnitGrid.Clear(Faction.Allies);
    }

    private void ResetEnemies()
    {
        foreach (var enemy in EnemyUnitsList)
        {
            Destroy(enemy.Prefab);
        }
        EnemyUnitsList.Clear();
        UnitGrid.Clear(Faction.Enemies);
    }


    #region Unit Spawning

    private void SpawnAllies()
    {
        foreach (var ally in AlliedUnitsList)
        {
            //create prefab
            CreateUnitPrefab(ally);

            //place the prefab in the grid (allies are teleported)
            UnitGrid.AddToFront(Faction.Allies, ally, true);
        }
    }

    private void SpawnEnemies()
    {
        foreach (var enemy in EnemyUnitsList)
        {
            enemy.SetBaseStats(GameManager.Instance.GameDifficulty, CurrentLocation);
            
            //create prefab
            CreateUnitPrefab(enemy);

            //place the prefab in the grid
            switch (enemy.Class)
            {
                case UnitClass.Warrior:
                case UnitClass.Bruiser:
                case UnitClass.Battlemage:
                case UnitClass.Tank:
                case UnitClass.Titan:
                case UnitClass.Vanguard:
                    UnitGrid.AddToFront(Faction.Enemies, enemy);
                    break;

                case UnitClass.Marksman:
                case UnitClass.Mage:
                case UnitClass.Artillery:
                case UnitClass.Controller:
                case UnitClass.Healer:
                case UnitClass.Assassin:
                    UnitGrid.AddToBack(Faction.Enemies, enemy);
                    break;

                default:
                    Debug.Log($"Unknown Class: {enemy.Class} of enemy: {enemy.Name}");
                    break;
            }
        }

        //TODO: load the saved ally positions
        UnitGrid.Restructure(Faction.Enemies);
        UnitGrid.SetupEnemyEntrance();
    }

    private void CreateUnitPrefab(ScriptableUnitBase unit)
    {
        var spawnX = unit.Faction == Faction.Enemies ? 10 : -10; 
        unit.Prefab = Instantiate(UnitPrefab, new Vector2(spawnX, -0.25f), Quaternion.identity);
        unit.Prefab.GetComponent<Unit>().Initialize(unit.BaseStats, unit, UnitGrid, Hero);
        unit.Prefab.GetComponent<Unit>().OnSetTarget += SetTargetEnemy;
        unit.Prefab.GetComponent<Unit>().OnUnitDeath += HandleUnitDeath;
    }

    public void SetTargetEnemy(ScriptableUnitBase unit)
    {
        foreach (var enemy in EnemyUnitsList)
        {
            enemy.Prefab.GetComponent<Unit>().IsTargeted = false;
        }

        if (unit == null)
        {
            targetedEnemy = null;
        }
        else
        {
            targetedEnemy = unit as ScriptableEnemy;

            unit.Prefab.GetComponent<Unit>().IsTargeted = true;
        }
    }

    private void HandleUnitDeath(ScriptableUnitBase unit)
    {
        if (unit == Hero)
        {
            //TODO: game over
            Debug.Log("Hero died. Game over.");
        }

        if (unit.Faction == Faction.Enemies)
        {
            //TODO: handle loot 
        }

        Destroy(unit.Prefab);

        if (unit.Faction == Faction.Allies)
            AlliedUnitsList.Remove(unit);
        else
        {
            EnemyUnitsList.Remove(unit as ScriptableEnemy);

            if (TargetedEnemy == unit)
                TargetedEnemy = null;
        }

        UnitGrid.Remove(unit);
    }

    #endregion Unit Spawning
}
