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

    [Header("UI References")]
    [SerializeField] GameObject PauseMenu;
    [SerializeField] GameObject UnitPrefab;
    [SerializeField] GameObject AllyStatusBar;
    [SerializeField] GameObject EnemyStatusBar;
    [SerializeField] GameObject PlayerEnergyBar;

    [Space]
    [SerializeField] Button     PauseButton;
    [SerializeField] Sprite     PauseButton_ContinueImage;
    [SerializeField] Sprite     PauseButton_PauseImage;


    #endregion UI References

    [Space]
    [Header("Variables")]
    [SerializeField] Image background;
    [SerializeField] bool IsPaused = false;

    [SerializeField] ScriptableAdventureLocation CurrentLocation;

    private ScriptableHero PlayerHero;
    [SerializeField] List<ScriptableUnitBase> AlliedUnitsList;
    [SerializeField] List<ScriptableEnemy> EnemyUnitsList;

    private UnitGrid UnitGrid = new UnitGrid();

    [SerializeField] ScriptableEnemy targetedEnemy = null;
    private ScriptableEnemy TargetedEnemy
    {
        get
        {
            if (targetedEnemy == null)
                return UnitGrid.GetDefaultTarget(Faction.Enemies) as ScriptableEnemy;

            return targetedEnemy;
        }
        set
        {
            var enemyUnit = targetedEnemy?.Prefab?.GetComponent<Unit>();
            if (value == null)
            {
                if (enemyUnit == null || enemyUnit.IsDead)
                {   //if the unit died; select a default target for the status bar
                    SetStatusBarUnit(UnitGrid.GetDefaultTarget(Faction.Enemies), Faction.Enemies);
                }
                //if the target was deselected manually, but the unit is still alive, keep the status bar as-is
            }

            targetedEnemy = value;

            foreach (var ally in AlliedUnitsList)
            {
                ally.Prefab.GetComponent<Unit>().PreferredTargetOpponent = targetedEnemy;
            }
        }
    }

    //who to display in unit status bars
    [SerializeField] ScriptableUnitBase selectedAlly;
    public ScriptableUnitBase SelectedAlly
    {
        get => selectedAlly;
        set
        {
            selectedAlly = value;
            //we always want to show heros stats instead of nothing when
            //  the up-to-now selected unit dies
            if (selectedAlly == null)
                selectedAlly = PlayerHero;
            
            SetStatusBarUnit(value, Faction.Allies);
        }
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

        SelectedAlly = PlayerHero;
        TargetedEnemy = null;
        PlayerEnergyBar.GetComponent<PlayerEnergyBar>().Initialize(PlayerHero, null);
    }

    // Update is called once per frame
    void Update()
    {

    }

    #endregion 	UNITY METHODS


    public void TogglePause()
    {
        Pause(!IsPaused);

        TogglePausedMenu();
    }

    //DONT FORGET TO UNPAUSE AFTER ANY BUTTON CLICKS
    // (quitting the scene doesnt reset the timescale)
    //Update() still gets called even when timescale = 0
    //use time.unscaledDeltaTime to measure time even when timescale = 0
    public void Pause(bool isPaused)
    {
        IsPaused = isPaused;

        Time.timeScale = isPaused ? 0 : 1;
    }

    private void TogglePausedMenu()
    {
        if (PauseMenu == null)
            return;

        //pause button image swap
        PauseButton.image.sprite = IsPaused ? PauseButton_ContinueImage : PauseButton_PauseImage;

        PauseMenu.SetActive(IsPaused);
    }

    private void InitStage(bool initAllies)
    {
        //get encounter type
        EncounterType encounter = CurrentLocation.GetNextEncounter();

        if (initAllies)
        {   //Handle allies (and hero)
            AlliedUnitsList = new List<ScriptableUnitBase>();
            PlayerHero = GameManager.Instance.PlayerManager.PlayerHero;

            if (PlayerHero != null)
                AlliedUnitsList.Add(PlayerHero);

            if (GameManager.Instance.Allies != null)
                AlliedUnitsList.AddRange(GameManager.Instance.Allies);

            //TODO: remove! this is testing only
            var extraAllies = CurrentLocation.RollEnemies(encounter);
            while (extraAllies.Count > 5) //make sure there arent too many
                extraAllies.RemoveAt(0);
            foreach (var extra in extraAllies)
            {
                extra.Faction = Faction.Allies;
                extra.SetBaseStats(GameManager.Instance.GameDifficulty, CurrentLocation);
            }
            AlliedUnitsList.AddRange(extraAllies);

            SpawnAllies();
        }

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

            //TODO: load the saved ally positions if any, if not, place like this:

            //place the prefab in the grid (allies are teleported)
            if (ally is ScriptableEnemy)
                AddEnemyToGridByClass(ally as ScriptableEnemy, true);
            else
                UnitGrid.AddToFront(Faction.Allies, ally, true);
        }

        UnitGrid.Restructure(Faction.Allies);
    }

    private void SpawnEnemies()
    {
        foreach (var enemy in EnemyUnitsList)
        {
            enemy.SetBaseStats(GameManager.Instance.GameDifficulty, CurrentLocation);

            //create prefab
            CreateUnitPrefab(enemy);

            //place the prefab in the grid
            AddEnemyToGridByClass(enemy, false);
        }

        UnitGrid.Restructure(Faction.Enemies);
        UnitGrid.SetupEnemyEntrance();
    }

    private void AddEnemyToGridByClass(ScriptableEnemy unit, bool teleport)
    {
        switch (unit.Class)
        {
            case UnitClass.Warrior:
            case UnitClass.Bruiser:
            case UnitClass.Battlemage:
            case UnitClass.Tank:
            case UnitClass.Titan:
            case UnitClass.Vanguard:
                UnitGrid.AddToFront(unit.Faction, unit, teleport);
                break;

            case UnitClass.Marksman:
            case UnitClass.Mage:
            case UnitClass.Artillery:
            case UnitClass.Controller:
            case UnitClass.Healer:
            case UnitClass.Assassin:
                UnitGrid.AddToBack(unit.Faction, unit, teleport);
                break;

            default:
                Debug.Log($"Unknown Class: {unit.Class} of enemy: {unit.Name}");
                break;
        }
    }

    private void CreateUnitPrefab(ScriptableUnitBase unit)
    {
        float spawnX;
        UserLayers layer;

        if (unit.Faction == Faction.Enemies)
        {
            spawnX = 10;
            layer = UserLayers.Enemies_Layer;
        }
        else
        {
            spawnX = -10;
            layer = UserLayers.Allies_Layer;
        }

        unit.Prefab = Instantiate(UnitPrefab, new Vector2(spawnX, -0.25f), Quaternion.identity);

        unit.Prefab.GetComponent<Unit>().Initialize(unit.BaseStats, unit, UnitGrid, PlayerHero);
        unit.Prefab.GetComponent<Unit>().OnSetTarget += SetTarget;
        unit.Prefab.GetComponent<Unit>().OnUnitDeath += HandleUnitDeath;
        unit.Prefab.GetComponent<Unit>().OnStatChanged += UnitStatChanged;

        unit.Prefab.layer = LayerMask.NameToLayer(layer.ToString());
    }

    #endregion Unit Spawning

    /// <summary>
    /// Needs faction parameter for when the unit is null
    /// </summary>
    public void SetTarget(ScriptableUnitBase unit, Faction faction)
    {
        if (unit == null)
        {
            if (faction == Faction.Allies)
                SelectedAlly = null;
            else
                TargetedEnemy = null;
            
            return;
        }

        if (faction == Faction.Enemies)
        {
            foreach (var enemy in EnemyUnitsList)
            {
                enemy.Prefab.GetComponent<Unit>().IsTargeted = false;
            }
            
            TargetedEnemy = unit as ScriptableEnemy;

            unit.Prefab.GetComponent<Unit>().IsTargeted = true;
        }
        else
        {
            SelectedAlly = unit;
        }
    }

    private void HandleUnitDeath(ScriptableUnitBase unit)
    {
        if (unit == PlayerHero)
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
        {
            if (SelectedAlly == unit)
                SelectedAlly = null;

            AlliedUnitsList.Remove(unit);
        }
        else
        {
            if (TargetedEnemy == unit)
                TargetedEnemy = null;

            EnemyUnitsList.Remove(unit as ScriptableEnemy);
        }

        UnitGrid.Remove(unit);
    }

    /// <summary>
    /// The method that handles the OnStatChanged event of Unit.
    /// When any of its stats are changed, we should update the StatusBar
    ///     (if the unit is selected, and therefore shown in the StatusBar)
    /// </summary>
    private void UnitStatChanged(ScriptableUnitBase unit, Stat stat)
    {
        //only do an update if the unit is selected
        if (unit.Faction == Faction.Allies && SelectedAlly == unit)
        {
            AllyStatusBar.GetComponent<UnitStatusBar>()?.UpdateStats();
        }
        else if (unit.Faction == Faction.Enemies && targetedEnemy == unit)
        {
            EnemyStatusBar.GetComponent<UnitStatusBar>()?.UpdateStats();
        }
    }

    private void SetStatusBarUnit(ScriptableUnitBase unit, Faction faction)
    {
        var bar = faction == Faction.Allies ? AllyStatusBar : EnemyStatusBar;
        
        bar.GetComponent<UnitStatusBar>().UnitRef = unit;
    }
}
