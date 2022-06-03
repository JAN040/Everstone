using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This will share logic for any unit on the field. Could be friend or foe, controlled or not.
/// Things like taking damage, dying, animation triggers etc
/// </summary>
public class UnitBase : MonoBehaviour {

    public CharacterStats Stats { get; private set; }

    public UnitBase(CharacterStats stats)
    {
        SetStats(stats);
    }


    private void OnDestroy()
    {
        Stats.OnHealthPointsChanged -= OnUnitHPChanged;
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