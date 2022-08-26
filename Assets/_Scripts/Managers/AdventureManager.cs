using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdventureManager : MonoBehaviour
{
    #region 	VARIABLES

    #region UI References

    [SerializeField] GameObject PauseMenu;
    [SerializeField] GameObject UnitPrefab;

    #endregion UI References


    [SerializeField] Image background;
    [SerializeField] bool IsPaused = false;

    private ScriptableAdventureLocation CurrentLocation;

    private ScriptableHero Hero;
    private List<ScriptableUnitBase> AlliedUnitsList;
    private List<ScriptableEnemy> EnemyUnitsList;

    private UnitGrid UnitGrid = new UnitGrid();

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

    private void InitStage(bool IsFirstInit)
    {
        if (IsFirstInit)
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
        ResetEnemies();

        InitStage(false);
    }

    private void ResetEnemies()
    {
        foreach (var enemy in EnemyUnitsList)
        {
            Destroy(enemy.Prefab);
            
            //enemy.Prefab = null;
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
            ally.Prefab = Instantiate(UnitPrefab, new Vector2(-20, -20), Quaternion.identity);
            ally.Prefab.GetComponent<Unit>().Initialize(ally.BaseStats, ally);

            //place the prefab in the grid
            UnitGrid.AddToFront(Faction.Allies, ally);
        }
    }

    private void SpawnEnemies()
    {
        foreach (var enemy in EnemyUnitsList)
        {
            //create prefab
            enemy.Prefab = Instantiate(UnitPrefab, new Vector2(-20, -20), Quaternion.identity);
            enemy.Prefab.GetComponent<Unit>().Initialize(enemy.BaseStats, enemy);

            //place the prefab in the grid
            switch (enemy.Class)
            {
                case EnemyClass.Warrior:
                case EnemyClass.Bruiser:
                case EnemyClass.Battlemage:
                case EnemyClass.Tank:
                case EnemyClass.Titan:
                case EnemyClass.Vanguard:
                    UnitGrid.AddToFront(Faction.Enemies, enemy);
                    break;

                case EnemyClass.Marksman:
                case EnemyClass.Mage:
                case EnemyClass.Artillery:
                case EnemyClass.Controller:
                case EnemyClass.Healer:
                case EnemyClass.Assassin:
                    UnitGrid.AddToBack(Faction.Enemies, enemy);
                    break;

                default:
                    Debug.Log($"Unknown Class: {enemy.Class} of enemy: {enemy.Name}");
                    break;
            }
        }

        //TODO: load the saved ally positions
        UnitGrid.Restructure(Faction.Enemies);
    }

    #endregion Unit Spawning
}
