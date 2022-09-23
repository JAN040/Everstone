using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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

    #endregion UI References


    [Space]
    [Header("Script properties")]

    [SerializeField] Material Material_Dissolve_Instance;
    
    [SerializeField] CharacterStats stats;
    public CharacterStats Stats { get => stats; private set => stats = value; }

    [SerializeField] ScriptableUnitBase UnitDataRef;

    private List<ScriptableStatusEffect> ActiveEffects = new List<ScriptableStatusEffect>();

    private UnitGrid UnitGridRef;

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
    /// Controls the Dissolve material fade in the death anim.
    /// </summary>
    private float Fade = 1f;

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
    /// Decided by who the player targets (for allies) or randomly (enemies)
    /// </summary>
    public ScriptableUnitBase PreferredTargetOpponent;

    /// <summary>
    /// The target this unit is currently attacking
    /// </summary>
    public ScriptableUnitBase CurrentTargetOpponent;

    [SerializeField] bool IsAttacking = false;

    private float BasicAttackForce { 
        get { return BasicAttackForce_Base * GetMineToHeroSpeedRatio(); }
    }

    private float MovementSpeed { 
        get { return MovementSpeed_Base * GetMineToHeroSpeedRatio(); }
    }

    //healthbar animation
    [SerializeField] float ChipSpeed = 2f;
    [SerializeField] float LerpTimer = 0;



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

        RemoveAllStatusEffects();
    }

    private void Update()
    {
        //increase energy amount if not dead
        if (IsDead || IsAttacking)
            return;

        //animate HP changes
        UpdateHpUI();

        //energy gain
        GainEnergy();

        //update effects
        UpdateStatusEffects();

        //start attacking/action if energy full
        if (Stats.Energy >= Stats.MaxEnergy.GetValue())
        {
            //the hero doesnt auto attack, all other units do
            if (UnitDataRef != HeroRef)
            {
                //auto attack
                BasicAttack();
            }
        }
    }

    private void UpdateStatusEffects()
    {
        //make a new list cause AllPlayerAbilities is modified when an effect expires
        foreach (var effect in new List<ScriptableStatusEffect>(ActiveEffects))
        {
            effect.Update();
        }
    }


    private void GainEnergy()
    {
        float modifier = UnitDataRef == HeroRef ? Stats.EnergyRecovery.GetValue() : GetEnergyRecovery();
        Stats.Energy += modifier * Time.deltaTime;
    }

    //Handle movement
    private void FixedUpdate()
    {
        if (IsDead)
            return;

        if (IsAttacking)
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
            var damage = new Damage(Stats.PhysicalDamage.GetValue(), Stats.ArtsDamage.GetValue());
            enemyUnitScript.TakeDamage(damage, true);

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


    public void Initialize(CharacterStats stats, ScriptableUnitBase unitData, UnitGrid grid, ScriptableHero hero)
    {
        //these stats are unitData.BaseStats modified by stage level, unit type and class
        SetStats(stats);
        SetUnitData(unitData);

        UnitGridRef = grid;
        HeroRef = hero;

        //no-one is targeted at the beginning
        IsTargeted = false;

        //allies cant be targeted
        if (UnitDataRef.Faction == Faction.Allies)
            IsTargetable = false;

        Image_HealthBar.fillAmount     = Stats.GetHpNormalized();
        Image_BackHealthBar.fillAmount = Stats.GetHpNormalized();
        Image_EnergyBar.fillAmount     = Stats.GetEnergyNormalized();

        if (unitData == HeroRef)    //hero unit has a separate UI element for energy
            Image_EnergyBar.gameObject.SetActive(false);
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
    /// If i ever need to inflict more than one damage type at once
    /// </summary>
    public void TakeDamage(List<Damage> damageList, bool canEvade)
    {
        if (canEvade && Helper.DiceRoll(this.Stats.DodgeChance.GetValue()))
        {
            Debug.Log($"Dodged. DodgeChance: {this.Stats.DodgeChance.GetValue()}");
            CreateStatusIndicator("Dodged!", Color.white);
            return;
        }

        damageList.ForEach(x => this.TakeDamage(x, false));
    }

    public void TakeDamage(Damage damage, bool canEvade)
    {
        //evasion check
        if (canEvade && Helper.DiceRoll(this.Stats.DodgeChance.GetValue()))
        {
            Debug.Log($"Dodged. DodgeChance: {this.Stats.DodgeChance.GetValue()}");
            CreateStatusIndicator("Dodged!", Color.white);
            return;
        }

        float dmgAmount = 0;

        switch (damage.Type)
        {
            case DamageType.Physical:
                var dmgAfterArmor = damage.Amount - Stats.Armor.GetValue();
                dmgAmount = dmgAfterArmor > 0 ? dmgAfterArmor : 0;
                break;

            case DamageType.Arts:
                //arts resist can be negative (Note: its stored as a whole value, eg. 5 res means 0.05 dmg red., thats why division by 100)
                var dmgAfterRes = damage.Amount - (damage.Amount * (Stats.ArtsResist.GetValue() / 100f));
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

        CreateStatusIndicator(dmgAmount.Round().ToString(), damage.GetIndicatorColor());

        ReduceHPByAmount(dmgAmount);
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
    }

    public virtual void ReduceHPByAmount(float amount)
    {
        Stats.HealthPoints -= amount;
        Debug.Log($"{UnitDataRef.Faction} Unit {UnitDataRef.Name} took {amount} damage (now has {Stats.HealthPoints} HP)");
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
        foreach (var effect in new List<ScriptableStatusEffect>(ActiveEffects))
        {
            effect.Deactivate();
        }

        //deactivated effects signal an event which then removes them from the ActiveEffects list.
        //  therefore the list should be empty when all effects are deactivated...
        if (ActiveEffects.Count > 0)
        {
            Debug.LogWarning("RemoveAllStatusEffects(): ActiveEffects is still not empty!");
        }
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

        float heroSpeed = GameManager.Instance.PlayerManager.PlayerHero.BaseStats.Speed.GetValue();
        float mySpeed = Stats.Speed.GetValue();

        float ratio = heroSpeed == 0 ? MAX_SPEED_RATIO : mySpeed / heroSpeed;
        Math.Clamp(ratio, MIN_SPEED_RATIO, MAX_SPEED_RATIO);

        return ratio;
    }

    public void BasicAttack()
    {
        if (!IsValidTarget(PreferredTargetOpponent)) //reselect a preffered target if needed
            PreferredTargetOpponent = UnitGridRef.GetDefaultTarget(GetOpponentFaction());
        
        CurrentTargetOpponent = PreferredTargetOpponent;

        //if we have a valid target
        if (IsValidTarget(CurrentTargetOpponent))
        {
            this.IsAttacking = true;
            this.Stats.Energy = 0;
        }
    }

    private void StopAttacking(bool stabilise = false)
    {
        IsAttacking = false;

        if (stabilise)
            StabiliseAfterHit();
    }

    private void RemoveVelocity()
    {
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
               !opponentTarget.Prefab.GetComponent<Unit>().IsDead;
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

    public void AddStatusEffect(ScriptableStatusEffect newEffect)
    {
        if (newEffect == null)
            return;

        if (ActiveEffects == null)
            ActiveEffects = new List<ScriptableStatusEffect>();

        var dupeEffect = ActiveEffects.FirstOrDefault(x => x.Effect == newEffect.Effect && x.IsActive); 
        
        //if the same effect exists in the list
        if (dupeEffect != null)
        {
            //if the effects are stackable, stack them, otherwise keep the newer effect
            if (newEffect.IsStackable && dupeEffect.IsStackable)
            {
                dupeEffect.StackEffect(newEffect);
                
                return;   
            }
            else
            {
                dupeEffect.Deactivate();
            }
        }

        newEffect.OnEffectExpired += EffectExpired;
        newEffect.Activate(this);

        ActiveEffects.Add(newEffect);

        //TODO: add modifiers ??

        OnUnitStatusEffectAdded?.Invoke(newEffect);
    }

    public void RemoveStatusEffect(StatusEffect effect)
    {
        var effectToRemove = ActiveEffects.FirstOrDefault(x => x.Effect == effect);
        
        if (effectToRemove == null)
        {
            Debug.LogWarning($"Couldnt find an active effect '{effect}' on unit {UnitDataRef.Name}");
            return;
        }

        effectToRemove.Deactivate();
    }


    private void EffectExpired(ScriptableStatusEffect effect)
    {
        effect.OnEffectExpired -= EffectExpired;
        ActiveEffects.Remove(effect);

        //TODO: remove modifiers
    }
}
