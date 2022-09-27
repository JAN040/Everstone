using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

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
    [SerializeField] GameObject PlayerAttackButton;
    [SerializeField] GameObject PlayerDodgeButton;

    [Space]
    [SerializeField] Button     PauseButton;
    [SerializeField] Sprite     PauseButton_ContinueImage;
    [SerializeField] Sprite     PauseButton_PauseImage;

    [SerializeField] TextMeshProUGUI GameSpeedButtonText;


    [Space]
    [SerializeField] TextMeshProUGUI TimerText;

    #endregion UI References


    [Space]
    [Header("Variables")]

    [SerializeField] Image background;
    [SerializeField] bool IsPaused = false;
    [SerializeField] float gameSpeed;

    [SerializeField] ScriptableAdventureLocation CurrentLocation;

    private ScriptableHero PlayerHero;
    [SerializeField] List<ScriptableUnitBase> AlliedUnitsList;
    [SerializeField] List<ScriptableNpcUnit> EnemyUnitsList;

    private UnitGrid UnitGrid = new UnitGrid();

    [SerializeField] ScriptableNpcUnit targetedEnemy = null;
    private ScriptableNpcUnit TargetedEnemy
    {
        get
        {
            if (targetedEnemy == null)
                return UnitGrid.GetDefaultTarget(Faction.Enemies, false) as ScriptableNpcUnit;

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
                    SetStatusBarUnit(UnitGrid.GetDefaultTarget(Faction.Enemies, false), Faction.Enemies);
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
            
            SetStatusBarUnit(selectedAlly, Faction.Allies);
        }
    }

    public float GameSpeed
    {
        get => gameSpeed;
        set
        {
            gameSpeed = value;
            Time.timeScale = gameSpeed;
            GameSpeedButtonText.text = $"{gameSpeed:0.0}";

            //set the preference to the manager, so that next time the battle starts the same speed will be used
            GameManager.Instance.BattleGameSpeed = gameSpeed;
        }
    }

    float Timer = 0f;


    //selected abilities
    private List<ScriptableAbility> PlayerClassicAbilities = null;
    
    //selected + special cases like basic attack & dodge
    private List<ScriptableAbility> AllPlayerAbilities = new List<ScriptableAbility>();

    [SerializeField] float AbilityAntiSpamCD = 2f;

    #endregion 	VARIABLES


    #region 	UNITY METHODS


    // Start is called before the first frame update
    void Start()
    {
        CurrentLocation = GameManager.Instance?.CurrentLocation;
        Sprite currLocationSprite = CurrentLocation?.background;

        if (currLocationSprite != null)
            background.sprite = GameManager.Instance?.CurrentLocation?.background;

        InitStage(true, true);

        SelectedAlly = PlayerHero;
        TargetedEnemy = null;

        var playerStats = PlayerHero.Prefab.GetComponent<Unit>().Stats;
        playerStats.Mana = playerStats.MaxMana.GetValue();
        PlayerEnergyBar.GetComponent<PlayerEnergyBar>().Initialize(PlayerHero, PlayerClassicAbilities);
    }

    // Update is called once per frame
    void Update()
    {
        Timer += Time.deltaTime;
        TimerText.text = $"{(int)Timer}s";
    }

    private void OnDestroy()
    {
        ResetTimeScale();

        foreach (var ability in AllPlayerAbilities)
        {
            ability.OnAbilityActivated -= AbilityActivated;
            if (ability.ToggleMode != ToggleMode.None)
                ability.OnAbilityToggled -= AbilityToggled;
        }
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

        Time.timeScale = isPaused ? 0 : GameSpeed;
    }

    /// <summary>
    /// Should be used when leaving the battle scene
    /// </summary>
    public void ResetTimeScale()
    {
        Time.timeScale = 1;
    }

    private void TogglePausedMenu()
    {
        if (PauseMenu == null)
            return;

        //pause button icon swap
        Transform icon = PauseButton.transform.Find("Icon");
        if (icon != null && icon.GetComponent<Image>() != null)
            icon.GetComponent<Image>().sprite = IsPaused ? PauseButton_ContinueImage : PauseButton_PauseImage;

        PauseMenu.SetActive(IsPaused);
    }

    public void OnGameSpeedChange()
    {
        var tempSpeed = GameSpeed;
        tempSpeed += 0.5f;

        if (tempSpeed == 2.5f) //2.5x speed setting kinna useless
            tempSpeed = 3f;

        if (tempSpeed > 3f)
            tempSpeed = 0.5f;

        GameSpeed = tempSpeed;
    }

    private void InitStage(bool isFirstTime, bool initAllies)
    {
        //get encounter type
        EncounterType encounter = CurrentLocation.GetNextEncounter();


        if (isFirstTime || initAllies)
        {   //Handle allies (and hero)
            AlliedUnitsList = new List<ScriptableUnitBase>();
            InitPlayerHero();

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

            if (isFirstTime)
                InitPlayerAbilities();
        }

        //reset ally status bar to the hero
        SelectedAlly = PlayerHero;

        //TODO: set energy to 'starting energy' stat
        PlayerHero.Prefab.GetComponent<Unit>().Stats.Energy = 0;
        ResetAbilityToggles();

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

        GameSpeed = GameManager.Instance.BattleGameSpeed;
    }

    private void ResetPlayerStatusEffects()
    {
        PlayerHero.GetUnit().RemoveAllStatusEffects();
    }

    private void ResetAbilityToggles()
    {
        PlayerClassicAbilities.ForEach(x =>
        {
            if (x.ToggleMode == ToggleMode.Toggled)
                x.ToggleMode = ToggleMode.UnToggled;
        });
    }

    private void InitPlayerHero()
    {
        PlayerHero = GameManager.Instance.PlayerManager.PlayerHero;
    }

    private void InitPlayerAbilities()
    {
        var pManager = GameManager.Instance.PlayerManager;

        if (pManager.ClassicAbilities != null)
        {
            var selectedAbilities = pManager.ClassicAbilities.Where(x => x.IsSelected).ToList();
            if (selectedAbilities.Count > 0)
            {
                PlayerClassicAbilities = new List<ScriptableAbility>();
                PlayerClassicAbilities.AddRange(selectedAbilities);

                //remove the last element till we only have 7 selected abilities
                while (PlayerClassicAbilities.Count > 7)
                    PlayerClassicAbilities.RemoveAt(PlayerClassicAbilities.Count - 1);

                AllPlayerAbilities.AddRange(PlayerClassicAbilities);
            }
        }

        var atkAbility   = pManager.SpecialAbilities.FirstOrDefault(x => x.Ability == Ability.BasicAttack);
        var dodgeAbility = pManager.SpecialAbilities.FirstOrDefault(x => x.Ability == Ability.Dodge);
        var pUnit = PlayerHero.Prefab.GetComponent<Unit>();

        if (atkAbility != null)
            AllPlayerAbilities.Add(atkAbility);
        
        if (dodgeAbility != null)
            AllPlayerAbilities.Add(dodgeAbility);
         
        PlayerAttackButton.GetComponent<AbilityUI>().Initialize(pUnit, atkAbility);
        PlayerDodgeButton.GetComponent<AbilityUI>().Initialize(pUnit, dodgeAbility);

        foreach (var ability in AllPlayerAbilities)
        {
            if (ability.ToggleMode != ToggleMode.None)
                ability.OnAbilityToggled += AbilityToggled;
            else
                ability.OnAbilityActivated += AbilityActivated;
        }
    }

    public void Test_ResetStage()
    {
        ResetAllies();
        ResetEnemies();

        InitStage(false, true);
    }

    private void ResetAllies()
    {
        //reset ally leftover status effects -> called when the hero prefab is destroyed
        //ResetPlayerStatusEffects();

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
            InitUnitScript(ally);

            //TODO: load the saved ally positions if any, if not, place like this:

            //place the prefab in the grid (allies are teleported)
            if (ally is ScriptableNpcUnit)
                AddEnemyToGridByClass(ally as ScriptableNpcUnit, true);
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
            InitUnitScript(enemy);

            //place the prefab in the grid
            AddEnemyToGridByClass(enemy, false);
        }

        UnitGrid.Restructure(Faction.Enemies);
        UnitGrid.SetupEnemyEntrance();
    }

    private void InitUnitScript(ScriptableUnitBase unit)
    {
        var unitScript = unit.GetUnit();
        if (unitScript == null)
            return;

        unitScript.Initialize(unit.BaseStats, unit, UnitGrid, PlayerHero);
        unitScript.OnSetTarget += SetTarget;
        unitScript.OnUnitDeath += HandleUnitDeath;

        if (unit is ScriptableNpcUnit)
            unitScript.IsRanged = GameManager.Instance.UnitData.IsClassRanged((unit as ScriptableNpcUnit).Class);

        //TODO: assign this based on the equipped weapon!
        if (unit is ScriptableHero)
            unitScript.IsRanged = (unit as ScriptableHero).ClassName.Equals("Mage");
    }

    private void AddEnemyToGridByClass(ScriptableNpcUnit unit, bool teleport)
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
            
            TargetedEnemy = unit as ScriptableNpcUnit;

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
            EnemyUnitsList.Remove(unit as ScriptableNpcUnit);

            if (TargetedEnemy == unit)
                TargetedEnemy = null;
            else if (EnemyStatusBar.GetComponent<UnitStatusBar>().UnitRef == unit)
                SetStatusBarUnit(UnitGrid.GetDefaultTarget(Faction.Enemies, false), Faction.Enemies);
        }
    }

    private void SetStatusBarUnit(ScriptableUnitBase unit, Faction faction)
    {
        var bar = faction == Faction.Allies ? AllyStatusBar : EnemyStatusBar;
        
        bar.GetComponent<UnitStatusBar>().UnitRef = unit;
    }

    private void AbilityActivated(ScriptableAbility ability)
    {
        AddAbilityAntiSpamCooldown();

        switch (ability.Ability)
        {
            case Ability.BasicAttack:
                CastAbility_BasicAttack(ability);
                break;

            case Ability.Dodge:
                CastAbility_Dodge(ability);
                break;

            case Ability.PoisonSelf:
                CastAbility_PoisonSelf(ability);
                break;

            default:
                Debug.LogWarning($"Activated ability ({ability.Name}) that has no implementation in the AdventureManager!");
                break;
        }
    }

    

    private void AbilityToggled(ScriptableAbility ability, bool isToggled)
    {
        AddAbilityAntiSpamCooldown();

        switch (ability.Ability)
        {
            case Ability.ShieldBlock:
                ToggleAbility_ShieldBlock(ability, isToggled);
                break;
            default:
                break;
        }
    }

    private void AddAbilityAntiSpamCooldown()
    {
        AllPlayerAbilities.ForEach(x => x.AddAntiSpamCooldown(AbilityAntiSpamCD));
    }



    #region Ability Cast Methods


    private void CastAbility_BasicAttack(ScriptableAbility ability)
    {
        PlayerHero.GetUnit()?.BasicAttack();
    }

    private void CastAbility_Dodge(ScriptableAbility ability)
    {
        var statusEffect = ResourceSystem.Instance.GetStatusEffect(StatusEffect.EvasionBuff);
        //dodge chance increase & "perfect dodge" duration
        statusEffect.SetEffectValues(ability.EffectValue, ability.EffectValue_2);

        PlayerHero.GetUnit()?.AddStatusEffect(statusEffect);
    }

    private void CastAbility_PoisonSelf(ScriptableAbility ability)
    {
        var statusEffect = ResourceSystem.Instance.GetStatusEffect(StatusEffect.Poison);
        //poison amount
        statusEffect.SetEffectValues(ability.EffectValue);

        PlayerHero.GetUnit()?.AddStatusEffect(statusEffect);
    }


    #endregion Ability Cast Methods



    #region Ability Toggle Methods


    private void ToggleAbility_ShieldBlock(ScriptableAbility ability, bool isToggled)
    {
        if (isToggled)
        {
            var statusEffect = ResourceSystem.Instance.GetStatusEffect(StatusEffect.ShieldBlock);
            //percent of shield stats, energy regen % red.
            statusEffect.SetEffectValues(ability.EffectValue, ability.EffectValue_2);

            PlayerHero.GetUnit()?.AddStatusEffect(statusEffect);
        }
        else
        {
            PlayerHero.GetUnit().RemoveStatusEffect(StatusEffect.ShieldBlock);
        }
    }


    #endregion Ability Toggle Methods
}
