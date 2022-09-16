using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

public class AdventureManager : MonoBehaviour
{
    #region VARIABLES


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

    [Space]
    [SerializeField] TextMeshProUGUI Timer;

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
            var currentTargetedEnemyUnit = (targetedEnemy != null && targetedEnemy.Prefab != null) ?
                targetedEnemy.Prefab.GetComponent<Unit>()
                :
                null;

            if (value == null)
            {
                if (currentTargetedEnemyUnit == null || currentTargetedEnemyUnit.IsDead)
                {   //if the unit died; select a default target for the status bar
                    SetStatusBarUnit(UnitGrid.GetDefaultTarget(Faction.Enemies), Faction.Enemies);
                }
                //if the target was deselected manually, but the unit is still alive, keep the status bar as-is
            }
            else
                SetStatusBarUnit(value, Faction.Enemies);

            targetedEnemy = value;

            foreach (var ally in AlliedUnitsList)
            {
                ally.Prefab.GetComponent<Unit>().PreferredTargetOpponent = targetedEnemy;
            }
        }
    }

    [SerializeField] ScriptableUnitBase selectedAlly;
    /// <summary>
    /// who to display in the ally unit status bar
    /// </summary>
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

    float timer = 0f;

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

        var playerStats = PlayerHero.Prefab.GetComponent<Unit>().Stats;
        playerStats.Mana = playerStats.MaxMana.GetValue();
        PlayerEnergyBar.GetComponent<PlayerEnergyBar>().Initialize(PlayerHero, null);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        Timer.text = $"{(int)timer}s";
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

        //reset ally status bar to the hero
        SelectedAlly = PlayerHero;

        //TODO: set energy to 'starting energy' stat
        PlayerHero.Prefab.GetComponent<Unit>().Stats.Energy = 0;

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

        TargetedEnemy = null;

        //TODO: notify about stage number (current progress)
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
        UnitGrid.Remove(unit);

        if (unit.Faction == Faction.Allies)
        {
            AlliedUnitsList.Remove(unit);

            if (SelectedAlly == unit)
                SelectedAlly = null;
        }
        else
        {
            EnemyUnitsList.Remove(unit as ScriptableEnemy);

            if (TargetedEnemy == unit)
                TargetedEnemy = null;
            else if (EnemyStatusBar.GetComponent<UnitStatusBar>().UnitRef == unit)
                SetStatusBarUnit(UnitGrid.GetDefaultTarget(Faction.Enemies), Faction.Enemies);
        }
    }

    

    private void SetStatusBarUnit(ScriptableUnitBase unit, Faction faction)
    {
        var bar = faction == Faction.Allies ? AllyStatusBar : EnemyStatusBar;
        
        bar.GetComponent<UnitStatusBar>().UnitRef = unit;
    }
}
