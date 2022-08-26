using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This will share logic for any unit on the field. Could be friend or foe, controlled or not.
/// Things like taking damage, dying, animation triggers etc
/// </summary>
public class Unit : MonoBehaviour {

    #region UI References

    [SerializeField] Image Image_Frame;
    [SerializeField] Image Image_Portrait;
    [SerializeField] Image Image_TargetIcon;
    [SerializeField] Image Image_HealthBar;
    [SerializeField] Image Image_EnergyBar;
    
    [Space]
    [SerializeField] RectTransform Transform_Frame;
    [SerializeField] RectTransform Transform_Portrait;

    [Space]
    [Header("Raw number stats")]
    [SerializeField] Sprite Sprite_Frame_Normal;
    [SerializeField] Sprite Sprite_Frame_Elite;
    [SerializeField] Sprite Sprite_Frame_Boss;

    #endregion UI References

    #region UNITY METHODS

    private void Update()
    {
        //increase energy amount
        //start attacking/action if energy full
    }

    private void OnDestroy()
    {
        Stats.OnHealthPointsChanged -= OnUnitHPChanged;
    }

    #endregion UNITY METHODS


    public event Action OnUnitClicked;


    public CharacterStats Stats { get; private set; }

    private ScriptableUnitBase UnitDataRef { get; set; }

    /// <summary>
    /// Holds the units idle position, dictated by the grid its in.
    /// </summary>
    public Vector2 IdlePosition;


    public void Initialize(CharacterStats stats, ScriptableUnitBase unitData)
    {
        //these stats are unitData.BaseStats modified by stage level, unit type and class
        SetStats(stats);
        SetUnitData(unitData);

        //no-one is targeted at the beginning
        Image_TargetIcon.enabled = false;
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
        Debug.Log("Clicked unit");
        OnUnitClicked?.Invoke();
    }

    public virtual void SetStats(CharacterStats stats)
    {
        Stats = stats;
        Stats.OnHealthPointsChanged += OnUnitHPChanged;
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
        Stats.HealthPoints -= amount;
        Debug.Log($"Unit {this.name} took {amount} damage.");
    }

    /// <summary>
    /// Handle animations and check for death
    /// </summary>
    private void OnUnitHPChanged(float newAmount, float oldAmount)
    {
        if (newAmount <= 0)
        {
            Die();
        }

        //TODO: update hp bar
        //TODO: animate hp bar
    }

    protected virtual void Die()
    {
        Debug.Log($"Unit {this.name} has died.");
        
        //TODO: animate death

        //destroy object

        //notify Combat manager
    }
}