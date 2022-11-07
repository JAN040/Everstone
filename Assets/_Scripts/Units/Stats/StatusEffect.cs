using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StatusEffect
{
    #region VARIABLES


    public StatusEffectType EffectType;

    [Space]
    [Header("Target")]
    public TargetType Target;

    [Range(2, 5), Tooltip("Specifiy amount of random targets when TargetType is MultipleAllies or MultipleEnemies")]
    public int MultipleTargetCount = 2;

    public float EffectValue;
    public float PerLevelValueChange;
    public float Duration;
    public float PerLevelDurationChange;

    public List<Damage> DamageList;
    public StatModifier StatModifier;

    [NonSerialized] //gotten from the resource system. Has data like effect icon, etc.
    public ScriptableStatusEffect EffectData;
    

    #endregion VARIABLES





    #region METHODS


    public void LevelUp()
    {
        if (EffectType == StatusEffectType.DealDamage)
        {
            foreach (var damage in DamageList)
            {
                damage.Amount += damage.GetPerLevelAmountChange();
            }

            return;
        }

        EffectValue += PerLevelValueChange;
        Duration += PerLevelDurationChange;
    }


    public string GetEffectDescription(bool isAbilityMaxLevel)
    {
        if (EffectType == StatusEffectType.DealDamage)
        {
            string dmgAmounts = GetDamageAmounts();
            bool multipleTarget = Target.In(TargetType.MultipleAllies, TargetType.MultipleEnemies);

            return $"Deal {dmgAmounts} to {Target}{(multipleTarget ? $" ({MultipleTargetCount})" : "")}";
        }


        var res = string.Empty;
        var data = ResourceSystem.Instance.GetStatusEffect(this.EffectType);
        
        if (data == null)
            return res;
        
        data.SetEffectValues(EffectValue, Duration, StatModifier);

        return data.GetEffectDescription(isAbilityMaxLevel, PerLevelValueChange, PerLevelDurationChange);
    }

    private string GetDamageAmounts()
    {
        string res = string.Empty;
        foreach (var damage in DamageList)
        {
            switch (damage.Type)
            {
                case DamageType.Physical:
                    res += $"{damage.Amount * 100}% of PhysAtk\n";
                    break;
                case DamageType.Arts:
                    res += $"{damage.Amount * 100}% of ArtsAtk\n";
                    break;
                case DamageType.True:
                    res += $"{damage.Amount} True damage"; 
                    break;
                case DamageType.Elemental:
                    res += $"{damage.Amount} {damage.ElementType} element damage"; 
                    break;
                default:
                    break;
            }
        }

        return res;
    }


    #endregion METHODS
}
