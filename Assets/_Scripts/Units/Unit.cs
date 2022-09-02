using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] float MovementSpeed;
    [SerializeField] float BasicAttackForce;
    [SerializeField] Material Material_Dissolve;

    #region UI References

    [Space]
    [Header("UI References")]

    [SerializeField] Rigidbody2D RigidBody;
    [SerializeField] Collider2D Collider;
    [SerializeField] Button Button; //for selecting the unit as targeted
    [SerializeField] Image Image_Frame;
    [SerializeField] Image Image_Portrait;
    [SerializeField] Image Image_PortraitBackground;
    [SerializeField] Image Image_TargetIcon;
    [SerializeField] Image Image_HealthBar;
    [SerializeField] Image Image_EnergyBar;

    [Space]
    [SerializeField] RectTransform Transform_Frame;
    [SerializeField] RectTransform Transform_Portrait;

    [Space]
    [SerializeField] Sprite Sprite_Frame_Normal;
    [SerializeField] Sprite Sprite_Frame_Elite;
    [SerializeField] Sprite Sprite_Frame_Boss;

    #endregion UI References

    [Space]
    [Header("Script properties")]

    [SerializeField] Material Material_Dissolve_Instance;
    
    [SerializeField] CharacterStats stats;
    protected CharacterStats Stats { get => stats; private set => stats = value; }

    [SerializeField] ScriptableUnitBase UnitDataRef;

    private UnitGrid UnitGridRef;

    private ScriptableHero HeroRef;

    /// <summary>
    /// Holds the units idle position, dictated by the grid its in.
    /// </summary>
    public Vector2 IdlePosition;

    private bool isTargeted = false;
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
    private bool isTargetable = true;
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
                OnSetTarget?.Invoke(null);
            }
        }
    }

    /// <summary>
    /// Determines prefab button clickability
    /// </summary>
    private bool isInteractable = true;
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
    private bool isDead = false;
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

    private bool IsAttacking = false;

    private ScriptableUnitBase TargetOpponent;

    #endregion VARIABLES

    #region UNITY METHODS

    private void Start()
    {
        Material_Dissolve_Instance = new Material(Material_Dissolve);
        SetDissolveMaterial(Material_Dissolve_Instance);
    }

    private void Update()
    {
        //increase energy amount if not dead
        if (IsDead || IsAttacking)
            return;

        Stats.Energy += GetEnergyRecovery();

        //start attacking/action if energy full
        if (Stats.Energy >= Stats.MaxEnergy.GetValue())
        {
            //the hero doesnt auto attack, all other units do
            if (UnitDataRef != HeroRef)
            {
                //auto attack
                BasicAttack();

                Stats.Energy = 0;
            }
        }
    }

    //Handle movement
    private void FixedUpdate()
    {
        if (IsDead)
            return;

        if (IsAttacking)
        {
            Vector3 targetPos = TargetOpponent.Prefab.transform.position;
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

    private void OnDestroy()
    {
        Stats.OnHealthPointsChanged -= OnUnitHPChanged;
        Stats.OnEnergyChanged -= OnUnitEnergyChanged;
    }

    #endregion UNITY METHODS

    public event Action<ScriptableUnitBase> OnSetTarget;
    public event Action<ScriptableUnitBase> OnUnitDeath;


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

        Image_HealthBar.fillAmount = Stats.GetHpNormalized();
        Image_EnergyBar.fillAmount = Stats.GetEnergyNormalized();
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
        this.TakeDamage(new Damage(10));
        IsTargeted = true;

        OnSetTarget?.Invoke(UnitDataRef);
    }

    public virtual void SetStats(CharacterStats stats)
    {
        Stats = stats;
        Stats.OnHealthPointsChanged += OnUnitHPChanged;
        Stats.OnEnergyChanged += OnUnitEnergyChanged;
    }



    public virtual void TakeDamage(Damage damage)
    {
        float dmgAmount = 0;

        switch (damage.Type)
        {
            case DamageType.Physical:
                dmgAmount = CalcDmgAmountPhysical(damage.PhysicalDamage);
                break;
            case DamageType.Arts:
                dmgAmount = CalcDmgAmountArts(damage.ArtsDamage);
                break;
            case DamageType.Mixed:
                dmgAmount = CalcDmgAmountPhysical(damage.PhysicalDamage) + CalcDmgAmountArts(damage.ArtsDamage);
                break;
            case DamageType.True:
                dmgAmount = damage.TrueDamage;
                break;
            default:
                Debug.LogWarning($"Unset damage type");
                break;
        }

        ReduceHPByAmount(dmgAmount);
    }


    private float CalcDmgAmountPhysical(float physicalDamage)
    {
        var tempDamage = physicalDamage - Stats.Armor.GetValue();
        return tempDamage > 0 ? tempDamage : 0;
    }
    private float CalcDmgAmountArts(float artsDamage)
    {
        //arts resist can be negative
        return artsDamage - artsDamage * Stats.ArtsResist.GetValue();
    }

    public virtual void Heal(float healAmount)
    {
        Stats.HealthPoints += healAmount * Stats.HealEfficiency.GetValue();
    }

    public virtual void ReduceHPByAmount(float amount)
    {
        Debug.Log($"Unit {this.UnitDataRef.Name} took {amount} damage.");
        Stats.HealthPoints -= amount;
    }

    /// <summary>
    /// Handle animations and check for death
    /// </summary>
    private void OnUnitHPChanged(float newAmount, float oldAmount)
    {
        Image_HealthBar.fillAmount = Stats.GetHpNormalized();

        if (newAmount <= 0)
        {
            Die();
        }

        //TODO: update hp bar
        //TODO: animate hp bar
    }

    private void OnUnitEnergyChanged(float arg1, float arg2)
    {
        Image_EnergyBar.fillAmount = Stats.GetEnergyNormalized();
    }

    protected virtual void Die()
    {
        this.IsDead = true;
        //TODO reset targeted enemy in the manager

        Debug.Log($"Unit {UnitDataRef?.Name} has died.");

        //animate death
        StartCoroutine(PlayDeathAnimation());
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
        Image_Portrait.material = material;
        Image_PortraitBackground.material = material;
        Image_TargetIcon.material = material;
        Image_HealthBar.material = material;
        Image_EnergyBar.material = material;
    }

    private void MoveTowardsIdlePosition()
    {
        var a = (Vector2)transform.position;
        var b = IdlePosition;

        transform.position = Vector2.MoveTowards(a, Vector2.Lerp(a, b, LerpDelta), MovementSpeed);
    }

    private float GetEnergyRecovery()
    {
        float MAX_SPEED_RATIO = 10f;
        float MIN_SPEED_RATIO = 0.1f;

        float heroSpeed = HeroRef.Prefab.GetComponent<Unit>().Stats.Speed.GetValue();
        float mySpeed = Stats.Speed.GetValue();
        float ratio;

        ratio = heroSpeed == 0 ? MAX_SPEED_RATIO : mySpeed / heroSpeed;

        Math.Clamp(ratio, MIN_SPEED_RATIO, MAX_SPEED_RATIO);

        return mySpeed * ratio * Time.deltaTime;
    }

    private void BasicAttack()
    {
        this.IsAttacking = true;

        TargetOpponent = UnitGridRef.GetDefaultTarget(Faction.Allies);
    }

}
