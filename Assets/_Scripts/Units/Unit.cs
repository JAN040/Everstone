using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

/// <summary>
/// This will share logic for any unit on the field. Could be friend or foe, controlled or not.
/// Things like taking damage, dying, animation triggers etc
/// </summary>
public class Unit : MonoBehaviour
{

    #region VARIABLES

    [Header("Parameters")]

    [SerializeField] float LerpDelta;
    [SerializeField] float MovementSpeed_Base;
    [SerializeField] float BasicAttackForce_Base;
    [SerializeField] Material Material_Dissolve;
    [SerializeField] GameObject StatusIndicatorPrefab;
    /// <summary>
    /// Controls the Dissolve material fade in the death anim.
    /// </summary>
    private float Fade = 1f;


    #region UI References

    [Space]
    [Header("UI References")]

    [SerializeField] Rigidbody2D RigidBody;
    [SerializeField] Collider2D Collider;
    [SerializeField] Button Button; //for selecting the unit as targeted
    [SerializeField] Image Image_Frame;
    [SerializeField] Image Image_DamageEffect;
    [SerializeField] Image Image_Portrait;
    [SerializeField] Image Image_PortraitBackground;
    [SerializeField] Image Image_TargetIcon;
    [SerializeField] Image Image_HealthBar;
    [SerializeField] Image Image_BackHealthBar;
    [SerializeField] Image Image_EnergyBar;

    [Space]
    [SerializeField] RectTransform Transform_Frame;
    [SerializeField] RectTransform Transform_Portrait;

    [Space]
    [SerializeField] Sprite Sprite_Frame_Normal;
    [SerializeField] Sprite Sprite_Frame_Elite;
    [SerializeField] Sprite Sprite_Frame_Boss;

    [Space]
    [SerializeField] Sprite BackHealth_Damage;
    [SerializeField] Sprite BackHealth_Heal;

    [Space]
    [SerializeField] GameObject   UnitEffects;
    [SerializeField] VisualEffect RangedAtkMuzzle;
    [SerializeField] VisualEffect RangedAtkImpact;

    [Space]
    [Header("Effects")]
    [SerializeField] GridLayoutGroup EffectPanelGrid;
    [SerializeField] GameObject EffectPanelPrefab;


    #endregion UI References


    [Space]
    [Header("Script properties")]

    [SerializeField] Material Material_Dissolve_Instance;
    
    [SerializeField] CharacterStats stats;
    public CharacterStats Stats { get => stats; private set => stats = value; }

    [SerializeField] ScriptableUnitBase UnitDataRef;

    private List<ScriptableStatusEffect> ActiveEffects = new List<ScriptableStatusEffect>();

    private UnitGrid UnitGridRef;
    private AdventureManager ManagerRef;

    private ScriptableHero HeroRef;

    /// <summary>
    /// Holds the units idle position, dictated by the grid its in.
    /// </summary>
    public Vector2 IdlePosition;

    [SerializeField] bool isTargeted = false;
    public bool IsTargeted
    {
        get => isTargeted;
        set
        {
            if (value && IsTargetable)
            {
                isTargeted = true;
                Image_TargetIcon.enabled = true;
            }
            else
            {
                isTargeted = false;
                Image_TargetIcon.enabled = false;
            }
        }
    }

    /// <summary>
    /// Non targetable targets cant be hit with a direct attack, only AOE
    /// </summary>
    [SerializeField] bool isTargetable = true;
    public bool IsTargetable
    {
        get => isTargetable;
        set
        {
            isTargetable = value;

            if (!isTargetable && IsTargeted)
            {
                IsTargeted = false;

                //communicate that the target was lost to the manager
                OnSetTarget?.Invoke(null, UnitDataRef.Faction);
            }
        }
    }

    /// <summary>
    /// Determines prefab button clickability
    /// </summary>
    [SerializeField] bool isInteractable = true;
    public bool IsInteractable
    {
        get => isInteractable;
        set
        {
            isInteractable = value;
            isTargetable = value;
            Button.interactable = value;
        }
    }

    /// <summary>
    /// Used for excluding the unit from battle when it dies
    ///     (for the duration of the death animation)
    /// </summary>
    [SerializeField] bool isDead = false;
    public bool IsDead
    {
        get => isDead;
        set
        {
            isDead = value;

            if (isDead)
            {
                this.IsTargetable = false;
                this.IsTargeted = false;
            }
        }
    }

    /// <summary>
    /// Determines whether this unit can perform ranged attacks, or if it is limited to melee only
    /// </summary>
    public bool IsRanged { get; private set; }

    /// <summary>
    /// Determines whether this NpcUnit can only use abilities (cannot basic attack)
    /// </summary>
    private bool CanOnlyUseAbilities { get; set; }
    private float ChanceToUseAbility = 0.4f;

    /// <summary>
    /// Decided by who the player targets (for allies) or randomly (enemies)
    /// </summary>
    public ScriptableUnitBase PreferredTargetOpponent;

    /// <summary>
    /// The target this unit is currently attacking
    /// </summary>
    public ScriptableUnitBase CurrentTargetOpponent;

    [SerializeField] bool IsAttacking = false;

    [SerializeField] AttackType AttackType;

    private float BasicAttackForce { 
        get { return BasicAttackForce_Base * GetMineToHeroSpeedRatio(); }
    }

    private float MovementSpeed { 
        get { return MovementSpeed_Base * GetMineToHeroSpeedRatio(); }
    }

    //healthbar animation
    [SerializeField] float ChipSpeed = 2f;
    [SerializeField] float LerpTimer = 0;


    private List<GameObject> EffectPanelList;


    #endregion VARIABLES



    #region UNITY METHODS

    private void Start()
    {
        Material_Dissolve_Instance = new Material(Material_Dissolve);
        SetDissolveMaterial(Material_Dissolve_Instance);
    }

    private void OnDestroy()
    {
        Stats.OnHealthPointsChanged -= OnUnitHPChanged;
        Stats.OnEnergyChanged       -= OnUnitEnergyChanged;

        if (UnitDataRef == HeroRef)
        {
            foreach (var skill in HeroRef.LevelSystem.Skills.Values)
            {
                skill.OnLevelChanged -= LevelChanged;
            }

            Stats.ClearEventSubscriptions();    //additional clear cause fuck me some bug keeps happening
            OnUnitStatusEffectAdded = null;
        }

        RemoveAllStatusEffects();
    }

    private void Update()
    {
        if (IsDead)
            return;

        //animate HP changes
        UpdateHpUI();

        //update effects
        UpdateStatusEffects();

        if (IsAttacking)
            return;

        //energy gain only when not already attacking
        GainEnergy();

        //start attacking/action if energy full (the hero doesnt auto attack, all other units do)
        if (Stats.Energy >= Stats.MaxEnergy.GetValue() && UnitDataRef != HeroRef)
        {
            //if the unit has abilities and decides to use one, then dont basic attack
            if (UseAbility())
                return;
            
            //auto attack
            BasicAttack();
        }
    }

    private void UpdateStatusEffects()
    {
        foreach (var effect in GetActiveEffects())
        {
            effect.Update();
        }
    }

    public List<ScriptableStatusEffect> GetActiveEffects()
    {
        //make a new list cause AllPlayerAbilities is modified when an effect expires
        return new List<ScriptableStatusEffect>(ActiveEffects);
    }


    private void GainEnergy()
    {
        float modifier = UnitDataRef == HeroRef ? Stats.EnergyRecovery.GetValue() : GetEnergyRecovery();
        Stats.Energy += modifier * Time.deltaTime;
    }

    //Handle melee basic attack & other movement
    private void FixedUpdate()
    {
        if (IsDead)
            return;

        if (IsAttacking && AttackType == AttackType.Melee)
        {
            if (!IsValidTarget(CurrentTargetOpponent))
            {
                this.StopAttacking(true);
                return;
            }

            Vector3 targetPos = CurrentTargetOpponent.Prefab.transform.position;
            Vector3 f = targetPos - transform.position;

            f = f.normalized * BasicAttackForce;
            RigidBody.AddForce(f);

            return;
        }

        if ((Vector2)transform.position != IdlePosition)
        {
            MoveTowardsIdlePosition();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var enemyUnitScript = collision.gameObject?.GetComponent<Unit>();

        //same faction just-in-case check
        if (enemyUnitScript.UnitDataRef.Faction == this.UnitDataRef.Faction) //shouldnt theoretically happen if layers are set correctly
        {
            Debug.LogError($"{UnitDataRef.Name} Collided with {enemyUnitScript.UnitDataRef.Name} but they are of the same faction!");
            return;
        }

        //if we collided with another unit and we are attacking, make the collided unit take damage
        if (this.IsAttacking && enemyUnitScript != null && !enemyUnitScript.IsDead)
        {
            var damageList = GetBasicAttackDamageList();

            var physDmg = damageList?.FirstOrDefault(x => x.Type == DamageType.Physical);
            //when non-boss physical dmg ranged units are in the first row, they melee atk for less dmg
            if (physDmg != null && this.IsRanged && UnitDataRef.Type != EnemyType.Boss)
                physDmg.Amount /= 2;

            enemyUnitScript.TakeDamage(damageList);

            
            //Handle xp gain
            if (IsPlayerHero())
            {
                OnDamageDealtAddXp(enemyUnitScript.UnitDataRef, damageList);
            }

            //stop attacking
            this.StopAttacking();
        }

        StabiliseAfterHit();
    }


    #endregion UNITY METHODS



    public event Action<ScriptableUnitBase, Faction> OnSetTarget;
    public event Action<ScriptableUnitBase> OnUnitDeath;
    //event for the status bar to update stuff
    public event Action<ScriptableStatusEffect> OnUnitStatusEffectAdded;
    //public event Action<ScriptableUnitBase, Stat> OnStatChanged;


    public void Initialize(CharacterStats stats, ScriptableUnitBase unitData, UnitGrid grid, AdventureManager manager, ScriptableHero hero)
    {
        //these stats are unitData.BaseStats modified by stage level, unit type and class
        SetStats(stats);
        SetUnitData(unitData);

        UnitGridRef = grid;
        ManagerRef = manager;
        HeroRef = hero;

        EffectPanelList = new List<GameObject>();

        if (UnitDataRef is ScriptableNpcUnit)
        {
            this.IsRanged = GameManager.Instance.UnitData.IsClassRanged((UnitDataRef as ScriptableNpcUnit).Class);
            this.CanOnlyUseAbilities = GameManager.Instance.UnitData.CanOnlyUseSpecialAbilities((UnitDataRef as ScriptableNpcUnit).Class);
        }

        //assign IsRanged status based on class and equipped weapon
        if (UnitDataRef == HeroRef)
        {
            IsRanged = (UnitDataRef as ScriptableHero).ClassName.ToUpper().Equals("MAGE");

            InventoryItem equipRightArm = GameManager.Instance.PlayerManager.Equipment.GetItemAt((int)EquipmentSlot.RightArm);
            if (equipRightArm != null)
            {   //has some weapon equipped
                var equipData = equipRightArm.ItemData as ItemDataEquipment;
                IsRanged = equipData.EquipmentType.In(EquipmentType.Staff); //only staffs are ranged as of now
            }
        }

        //no-one is targeted at the beginning
        IsTargeted = false;

        //allies cant be targeted
        if (UnitDataRef.Faction == Faction.Allies)
            IsTargetable = false;

        Image_HealthBar.fillAmount     = Stats.GetHpNormalized();
        Image_BackHealthBar.fillAmount = Stats.GetHpNormalized();
        Image_EnergyBar.fillAmount     = Stats.GetEnergyNormalized();

        //set visual effect related objects
        if (UnitDataRef.Faction == Faction.Enemies)
        {
            UnitEffects.GetComponent<MoveWithParent>().Offset = UnitEffects.GetComponent<MoveWithParent>().Offset.FlipX();
            RangedAtkMuzzle.SetFloat("Position_x", - RangedAtkMuzzle.GetFloat("Position_x"));
        }

        if (UnitDataRef == HeroRef)    
        {
            //hero unit has a separate UI element for energy
            Image_EnergyBar.gameObject.SetActive(false);

            foreach (var skill in HeroRef.LevelSystem.Skills.Values)
            {
                skill.OnLevelChanged += LevelChanged;
            }
        }
    }

    private void LevelChanged(int prevLevel, int newLevel, SkillLevel skill)
    {
        CreateStatusIndicator($"{skill.SkillType} lvl {prevLevel} => {newLevel}", Color.white);
        ManagerRef.LogInfo($"{skill.SkillType} leveled up from {prevLevel} to {newLevel}! ({skill.Experience}/{skill.ExpToNextLevel})");
    }

    private void SetUnitData(ScriptableUnitBase unitData)
    {
        UnitDataRef = unitData;

        //set up the portrait
        Image_Portrait.sprite = UnitDataRef.MenuSprite;

        //set up the frame
        switch (unitData.Type)
        {
            case EnemyType.Normal:
                Image_Frame.sprite = Sprite_Frame_Normal;
                break;
            case EnemyType.Elite:
                Image_Frame.sprite = Sprite_Frame_Elite;
                break;
            case EnemyType.Boss:
                Image_Frame.sprite = Sprite_Frame_Boss;
                break;
            default:
                Image_Frame.sprite = Sprite_Frame_Normal;
                break;
        }

        //set up orientation
        FacingDirection shouldBeFacing = UnitDataRef.Faction == Faction.Enemies ?
            FacingDirection.Left
            :
            FacingDirection.Right;

        if (UnitDataRef.FaceDirection != shouldBeFacing)
        {
            Transform_Portrait.localScale = Transform_Portrait.localScale.FlipX();
        }

        if (shouldBeFacing == FacingDirection.Left)
        {
            //the frame faces right by default
            Transform_Frame.localScale = Transform_Frame.localScale.FlipX();
        }
    }

    public void UnitClicked()
    {
        Debug.Log($"Clicked unit {UnitDataRef.Name}");
        //this.TakeDamage(new Damage(10));
        
        //only enemies can be targeted
        if (UnitDataRef.Faction == Faction.Enemies)
        {
            //if its already targeted, then "untarget" it
            if (IsTargeted)
            {
                IsTargeted = false;
                OnSetTarget?.Invoke(null, UnitDataRef.Faction);
                
                return;
            }

            IsTargeted = true;
        }

        OnSetTarget?.Invoke(UnitDataRef, UnitDataRef.Faction);
    }

    public virtual void SetStats(CharacterStats stats)
    {
        Stats = stats;
        Stats.HealthPoints = Stats.MaxHP.GetValue();
        Stats.OnHealthPointsChanged += OnUnitHPChanged;
        Stats.OnEnergyChanged += OnUnitEnergyChanged;
    }

    /// <summary>
    /// For inflicting more than one damage type at once
    /// </summary>
    public bool TakeDamage(List<Damage> damageList)
    {
        bool canEvade = true;
        bool canBlock = true;

        foreach (var damage in damageList)
        {
            canEvade &= damage.CanBeEvaded;
            canBlock &= damage.CanBeBlocked;
        }

        if (canEvade && Helper.DiceRoll(this.Stats.DodgeChance.GetValue()))
        {
            float dmgSum = 0;
            damageList.ForEach(x => dmgSum += x.Amount);

            OnDodge(dmgSum);
            
            return false;
        }

        if (canBlock && Helper.DiceRoll(this.Stats.BlockChance.GetValue()))
            return false;


        damageList.ForEach(x =>
        {
            x.CanBeEvaded = false;  //already tested these, so set them to false
            x.CanBeBlocked = false;
            this.TakeDamage(x);
        });

        return true;
    }

    /// <summary>
    /// Handles taking damage
    /// </summary>
    /// <returns>True on hit, false on dodge</returns>
    public bool TakeDamage(Damage damage)
    {
        //evasion check
        if (damage.CanBeEvaded && Helper.DiceRoll(this.Stats.DodgeChance.GetValue()))
        {
            OnDodge(damage.Amount);

            return false;
        }
        if (damage.CanBeBlocked && Helper.DiceRoll(this.Stats.BlockChance.GetValue()))
            return false;


        float dmgAmount = GetDamageAmountAfterResistances(damage);

        CreateStatusIndicator(dmgAmount.Round().ToString(), damage.GetIndicatorColor());
        ReduceHPByAmount(dmgAmount);

        //Handle xp gain
        if (IsPlayerHero() && dmgAmount > 0 && !IsDead && this.Stats.HealthPoints > 0)
        {
            ManagerRef.AddPlayerXp(dmgAmount.RoundHP(), Skill.Constitution);
        }

        return true;
    }

    public float GetDamageAmountAfterResistances(Damage damage)
    {
        float dmgAmount = 0;
        switch (damage.Type)
        {
            case DamageType.Physical:
                var dmgAfterArmor = damage.Amount - Stats.Armor.GetValue();
                dmgAmount = dmgAfterArmor > 0 ? dmgAfterArmor : 0;
                break;

            case DamageType.Arts:
                //arts resist can be negative (Note: its stored as a percentage value, eg. 0.5 res means 50% dmg red.
                var dmgAfterRes = damage.Amount - (damage.Amount * Stats.ArtsResist.GetValue());
                dmgAmount = dmgAfterRes > 0 ? dmgAfterRes : 0;
                break;

            case DamageType.True:
            case DamageType.Elemental:
                dmgAmount = damage.Amount;
                break;

            default:
                Debug.LogWarning($"Unexpected damage type");
                break;
        }

        return dmgAmount;
    }

    /// <summary>
    /// A TakeDamage wrapper that handles the on-hit animation as well
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="canEvade"></param>
    /// <returns>True on hit, false otherwise</returns>
    public bool TakeRangedDamage(List<Damage> damageList)
    {
        if (damageList == null || damageList.Count == 0)
            return false;

        //only animate if the attack wasnt dodged
        if (TakeDamage(damageList))
        {
            float intensityFactor = Mathf.Pow(2, RangedAtkMuzzle.GetFloat("ColorIntensity"));

            RangedAtkImpact.SetVector4("Color", damageList.First().GetIndicatorColor() * intensityFactor);
            this.RangedAtkImpact.Play();

            return true;
        }

        return false;
    }

    private void OnDodge(float dodgedDamageAmount)
    {
        ManagerRef.LogInfo($"{UnitDataRef.Faction} Unit {UnitDataRef.Name} Dodged. (Chance: {this.Stats.DodgeChance.GetValue()*100:0.0}%)");
        CreateStatusIndicator("Dodged!", Color.white);
        
        //Handle xp gain
        if (IsPlayerHero())
        {
            float xpAmount = dodgedDamageAmount;

            //perfect dodge awards 3x more xp
            xpAmount *= this.Stats.DodgeChance.GetValue() >= 1f ? 3 : 1;
            ManagerRef.AddPlayerXp(xpAmount.RoundHP(), Skill.Agility);
        }
    }

    private void CreateStatusIndicator(string text, Color color)
    {
        var curPos = transform.position;
        var indicator = Instantiate(StatusIndicatorPrefab, new Vector2(curPos.x + 1, curPos.y - 1), Quaternion.identity);
        indicator.GetComponent<StatusChangeIndicator>().SetTextAndColor(text, color, GetIndicatorTargetDirection());
    }

    private FacingDirection GetIndicatorTargetDirection()
    {
        return UnitDataRef.Faction == Faction.Enemies ?
            FacingDirection.Right : FacingDirection.Left;
    }

    public void Heal(float healAmount)
    {
        float totalHeal = healAmount * Stats.HealEfficiency.GetValue();
        CreateStatusIndicator(totalHeal.Round().ToString(), Color.green);

        Stats.HealthPoints += totalHeal;
        ManagerRef.LogInfo($"{UnitDataRef.Faction} Unit {UnitDataRef.Name} healed for {totalHeal.RoundHP()}.");
    }

    public virtual void ReduceHPByAmount(float amount)
    {
        Stats.HealthPoints -= amount;
        Debug.Log($"{UnitDataRef.Faction} Unit {UnitDataRef.Name} took {amount} damage (now has {Stats.HealthPoints} HP)");
        ManagerRef.LogInfo($"{UnitDataRef.Faction} Unit {UnitDataRef.Name} took {amount.RoundHP()} damage.");
    }

    /// <summary>
    /// Reset animation timer and check for death
    /// </summary>
    private void OnUnitHPChanged(float newAmount, float oldAmount)
    {
        LerpTimer = 0;

        if (newAmount <= 0)
        {
            Die();
        }
    }

    private void OnUnitEnergyChanged(float arg1, float arg2)
    {
        Image_EnergyBar.fillAmount = Stats.GetEnergyNormalized();
    }

    protected virtual void Die()
    {
        this.IsDead = true;
        RemoveVelocity();

        //deactivate all effects to avoid weird damage ticks and stuff
        RemoveAllStatusEffects();

        Debug.Log($"Unit {UnitDataRef?.Name} has died.");

        //animate death
        StartCoroutine(PlayDeathAnimation());
    }

    public void RemoveAllStatusEffects()
    {
        foreach (var effect in GetActiveEffects())
        {
            effect.Deactivate();
        }

        //deactivated effects signal an event which then removes them from the ActiveEffects list.
        //  therefore the list should be empty when all effects are deactivated...
        if (ActiveEffects.Count > 0)
        {
            Debug.LogWarning("RemoveAllStatusEffects(): ActiveEffects is still not empty!");
        }

        //also remove the panels
        EffectPanelList.ForEach(x => Destroy(x.gameObject));
        EffectPanelList.Clear();
    }

    public IEnumerator PlayDeathAnimation()
    {
        while (true)
        {
            Fade -= Time.deltaTime / 2;
            if (Fade < 0)
                break;

            Material_Dissolve_Instance.SetFloat("_Fade", Fade);

            yield return null; //wait for a frame
        }

        //animation is over, call manager for cleanup
        OnUnitDeath?.Invoke(UnitDataRef);
    }

    private void SetDissolveMaterial(Material material)
    {
        Image_Frame.material = material;
        Image_DamageEffect.material = material;
        Image_Portrait.material = material;
        Image_PortraitBackground.material = material;
        Image_TargetIcon.material = material;
        Image_HealthBar.material = material;
        Image_BackHealthBar.material = material;
        Image_EnergyBar.material = material;
    }

    private void MoveTowardsIdlePosition()
    {
        var a = (Vector2)transform.position;
        var b = IdlePosition;

        transform.position = Vector2.MoveTowards(a, Vector2.Lerp(a, b, LerpDelta), MovementSpeed);
    }

    //non-hero units use this formula to get the multiplier for Time.deltaTime
    private float GetEnergyRecovery()
    {
        float speedMultiplier = GameManager.Instance.UnitData.SpeedRatioMultiplier;
        var ratio = GetMineToHeroSpeedRatio();

        return speedMultiplier * ratio;
    }

    private float GetMineToHeroSpeedRatio()
    {
        if (this.UnitDataRef == HeroRef)
            return 1f;

        float MAX_SPEED_RATIO = 10f;
        float MIN_SPEED_RATIO = 0.1f;

        float heroSpeed = GameManager.Instance.PlayerManager.PlayerHero.Stats.Speed.GetValue();
        float mySpeed = Stats.Speed.GetValue();

        float ratio = heroSpeed == 0 ? MAX_SPEED_RATIO : mySpeed / heroSpeed;
        Math.Clamp(ratio, MIN_SPEED_RATIO, MAX_SPEED_RATIO);

        return ratio;
    }

    public void BasicAttack()
    {
        if (!IsValidTarget(PreferredTargetOpponent)) //reselect a preffered target if needed
            PreferredTargetOpponent = UnitGridRef.GetRandomUnit(GetOpponentFaction(), this.IsRanged);
        
        CurrentTargetOpponent = PreferredTargetOpponent;

        //if we have a valid target
        if (IsValidTarget(CurrentTargetOpponent))
        {

            //melee attack conditions
            if (UnitGridRef.IsInFirstRow(this.UnitDataRef) &&
                UnitGridRef.IsInFirstRow(this.CurrentTargetOpponent))
            {
                this.IsAttacking = true;
                this.AttackType = AttackType.Melee;
                this.Stats.Energy = 0;
            }
            //ranged attack conditions
            else if (this.IsRanged)
            {
                this.IsAttacking = true;
                this.AttackType = AttackType.Ranged;
                this.Stats.Energy = 0;
                StartCoroutine(RangedAttackRoutine());
            }
        }
    }

    private IEnumerator RangedAttackRoutine()
    {
        //set muzzle angle
        var dir = CurrentTargetOpponent.Prefab.transform.position - UnitDataRef.Prefab.transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        List<Damage> damageList = GetBasicAttackDamageList();
        float intensityFactor = Mathf.Pow(2, RangedAtkMuzzle.GetFloat("ColorIntensity"));
        
        RangedAtkMuzzle.SetFloat("Angle", angle);
        RangedAtkMuzzle.SetVector4("Color", damageList.First().GetIndicatorColor() * intensityFactor);
        //play the muzzle flash animation
        RangedAtkMuzzle.Play();

        //add recoil force
        RigidBody.AddForce(dir.normalized * -1 * BasicAttackForce * 3);


        yield return new WaitForSeconds(0.2f);

        //add some force to the enemy as well
        CurrentTargetOpponent.GetUnit()?.RigidBody?.AddForce(dir.normalized * BasicAttackForce * 3);

        bool isHit = false;
        //handle taking damage & playing the onHit animation
        if (IsValidTarget(CurrentTargetOpponent))
            isHit = CurrentTargetOpponent.GetUnit().TakeRangedDamage(damageList);

        //Handle xp gain
        if (IsPlayerHero() && isHit)
        {
            OnDamageDealtAddXp(CurrentTargetOpponent, damageList);
        }

        yield return new WaitForSeconds(0.1f);

        //stabilise and return to position
        RemoveVelocity();
        this.IsAttacking = false;

        yield return new WaitForSeconds(0.1f);
        CurrentTargetOpponent?.GetUnit()?.RemoveVelocity();
    }

    /// <summary>
    /// Test for ability use. Also takes care of ability casting if true
    /// </summary>
    /// <returns>True if the unit used an ability, false if it has none or didnt use any</returns>
    private bool UseAbility()
    {
        var npcUnitData = UnitDataRef as ScriptableNpcUnit;
        if (npcUnitData == null)
            return false;   //not an NPC unit
            
        if (npcUnitData.Abilities == null || npcUnitData.Abilities.Count == 0)
            return false;   //no abilities
    
        if (CanOnlyUseAbilities || Helper.DiceRoll(ChanceToUseAbility))
        {
            var ability = npcUnitData.GetRandomAbility();
            GameManager.Instance.UnitData.SetupClassAbility(ability, npcUnitData);
            ManagerRef.CastAbility(ability, UnitDataRef);
            this.Stats.Energy = 0;  //consume the energy
            return true;
        }

        return false;
    }

    private List<Damage> GetBasicAttackDamageList()
    {
        List<Damage> damageList = new List<Damage>();

        if (this.UnitDataRef == HeroRef)
        {
            //find out damage based on equipped weapon or class (if no weap equipped)
            InventoryItem equipRightArm = GameManager.Instance.PlayerManager.Equipment.GetItemAt((int)EquipmentSlot.RightArm);
            if (equipRightArm != null)
            {   //has some weapon equipped
                var equipData = equipRightArm.ItemData as ItemDataEquipment;
                switch (equipData.EquipmentType)
                {
                    case EquipmentType.Sword:
                    case EquipmentType.Dagger:
                    case EquipmentType.Axe:
                        damageList.Add(new Damage(Stats.PhysicalDamage.GetValue()));
                        break;

                    case EquipmentType.Staff:
                        damageList.Add(new Damage(0, Stats.ArtsDamage.GetValue()));
                        break;
                    default:
                        Debug.LogWarning($"Unexpected equip type detected in the right arm: {equipData.EquipmentType}");
                        break;
                }
            }
            else
            {   //nothing is equipped, determine damage based on class
                if (HeroRef.ClassName.ToUpper().Equals("MAGE"))
                    damageList.Add(new Damage(0, Stats.ArtsDamage.GetValue()));
                else
                    damageList.Add(new Damage(Stats.PhysicalDamage.GetValue()));
            }
        }
        else
        {
            if (Stats.PhysicalDamage.GetValue() != 0)
                damageList.Add(new Damage(Stats.PhysicalDamage.GetValue()));

            if (Stats.ArtsDamage.GetValue() != 0)
                damageList.Add(new Damage(0, Stats.ArtsDamage.GetValue()));
        }

        damageList.OrderByDescending(x => x.Amount);
        return damageList;
    }

    public void OnDamageDealtAddXp(ScriptableUnitBase unit, List<Damage> damageList)
    {
        foreach (var damage in damageList)
        {
            OnDamageDealtAddXp(unit, damage);
        }
    }

    public void OnDamageDealtAddXp(ScriptableUnitBase unit, Damage damage)
    {
        if (unit == null || damage == null)
            return;

        float xpAmount = unit.GetUnit().GetDamageAmountAfterResistances(damage);
        Skill skill = damage.Type == DamageType.Physical ? Skill.Strength : Skill.Arts;

        ManagerRef.AddPlayerXp(xpAmount.RoundHP(), skill);
    }

    private void StopAttacking(bool stabilise = false)
    {
        IsAttacking = false;

        if (stabilise)
            StabiliseAfterHit();
    }

    private void RemoveVelocity()
    {
        if (RigidBody == null)
            return;

        RigidBody.velocity = Vector2.zero;
        RigidBody.angularVelocity = 0;
    }

    public Faction GetOpponentFaction()
    {
        return UnitDataRef.Faction == Faction.Allies ? Faction.Enemies : Faction.Allies;
    }

    private void StabiliseAfterHit()
    {
        StartCoroutine(StopAfter(0.2f));
    }

    private IEnumerator StopAfter(float t)
    {
        yield return new WaitForSeconds(t);

        RemoveVelocity();
    }

    private bool IsValidTarget(ScriptableUnitBase opponentTarget)
    {
        return opponentTarget != null &&
               opponentTarget.Prefab != null &&
               !opponentTarget.Prefab.GetComponent<Unit>().IsDead &&
               (IsRanged || UnitGridRef.IsInFirstRow(opponentTarget));
    }

    /// <summary>
    /// Handle Hp bar animation
    /// </summary>
    private void UpdateHpUI()
    {
        float fillFront = Image_HealthBar.fillAmount;
        float fillBack = Image_BackHealthBar.fillAmount;
        float hpFraction = Stats.GetHpNormalized();

        if (fillBack > hpFraction)
        {
            //unit took damage
            Image_BackHealthBar.sprite = BackHealth_Damage;
            Image_DamageEffect.color = new Color(1f, 0, 0, 0.4f);   //red with 100/255 transparency
            Image_HealthBar.fillAmount = hpFraction;

            LerpTimer += Time.deltaTime;
            float percentComplete = LerpTimer / ChipSpeed;

            //Debug.Log($"Animating damage taken for unit: {unitRef.Name}, percent complete: {percentComplete}");

            Image_BackHealthBar.fillAmount = Mathf.Lerp(fillBack, hpFraction, percentComplete * percentComplete);
            Image_DamageEffect.color = Color.Lerp(Image_DamageEffect.color, Color.clear, percentComplete * 2);
        }
        else if (fillFront < hpFraction)
        {
            //unit got healed
            Image_BackHealthBar.sprite = BackHealth_Heal;
            Image_DamageEffect.color = new Color(0, 1f, 0, 0.4f);   //green with 100/255 transparency
            Image_BackHealthBar.fillAmount = hpFraction;

            LerpTimer += Time.deltaTime;
            float percentComplete = LerpTimer / ChipSpeed;

            //Debug.Log($"Animating healing for unit: {unitRef.Name}, percent complete: {percentComplete}");

            Image_HealthBar.fillAmount = Mathf.Lerp(fillFront, hpFraction, percentComplete * percentComplete);
            Image_DamageEffect.color = Color.Lerp(Image_DamageEffect.color, Color.clear, percentComplete * 2);
        }
        else
            Image_DamageEffect.color = Color.clear;
    }

    /// <summary>
    /// Adds the effect newEffect to this Unit
    /// </summary>
    /// <param name="newEffect"></param>
    /// <param name="origin">Which unit activated the ability that caused this effect to be applied</param>
    public void AddStatusEffect(ScriptableStatusEffect newEffect)
    {
        if (newEffect == null)
            return;

        if (ActiveEffects == null)
            ActiveEffects = new List<ScriptableStatusEffect>();

        var dupeEffect = ActiveEffects.FirstOrDefault(x => x.Effect == newEffect.Effect && x.IsActive);

        //if the same effect exists in the list and both of the effects are stackable, stack them
        if (dupeEffect != null && newEffect.IsStackable && dupeEffect.IsStackable)
        {
            dupeEffect.StackEffect(newEffect);
            
            return;
        }

        newEffect.OnEffectExpired += EffectExpired;
        newEffect.Activate(this);

        ActiveEffects.Add(newEffect);
        EffectListChanged();

        OnUnitStatusEffectAdded?.Invoke(newEffect);
    }

    public void ApplyTakeDamageEffect(List<Damage> damageList, ScriptableUnitBase origin)
    {
        var origStats = origin.Stats;
        var correctedDmgList = new List<Damage>(damageList);

        foreach (var damage in correctedDmgList)
        {
            //phys dmg & arts damage are percentual values based on the casters stats and need be corrected to actual values
            switch (damage.Type)
            {
                case DamageType.Physical:
                    damage.Amount = damage.Amount * origStats.PhysicalDamage.GetValue();
                    break;

                case DamageType.Arts:
                    damage.Amount = damage.Amount * origStats.ArtsDamage.GetValue();
                    break;

                case DamageType.True:
                case DamageType.Elemental:
                default:
                    break;

            }
        }

        this.TakeDamage(correctedDmgList);
    }

    public void RemoveStatusEffect(ScriptableStatusEffect effect)
    {
        effect.Deactivate();

        //called in EffectExpired
        //ActiveEffects.Remove(effect);
        //EffectListChanged();
    }

    private void EffectListChanged()
    {
        //clear the panel grid
        EffectPanelList.ForEach(x => Destroy(x.gameObject));
        EffectPanelList.Clear();

        //add new effect panels to the grid
        var effectList = this.GetActiveEffects();

        for (int i= 0; i < effectList.Count; i++)
        {
            if (i > 3) break;   //display up to 4 effects max

            SpawnStatusEffectPanel(effectList[i]);
        }
    }

    private void SpawnStatusEffectPanel(ScriptableStatusEffect effect)
    {
        var spawnedPrefab = Instantiate(EffectPanelPrefab, new Vector3(0, 0, 0), Quaternion.identity);

        spawnedPrefab.transform.SetParent(EffectPanelGrid.transform, false);
        spawnedPrefab.transform.localScale = Vector3.one;
        spawnedPrefab.GetComponent<StatusEffectUI>()?.Initialize(effect);

        EffectPanelList.Add(spawnedPrefab);
    }


    private void EffectExpired(ScriptableStatusEffect effect)
    {
        effect.OnEffectExpired -= EffectExpired;
        ActiveEffects.Remove(effect);
        EffectListChanged();
    }

    private bool IsPlayerHero()
    {
        return this.UnitDataRef == HeroRef;
    }
}
