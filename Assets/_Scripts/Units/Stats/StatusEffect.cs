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
        EffectValue += PerLevelValueChange;
        Duration += PerLevelDurationChange;
    }


    public string GetEffectDescription(bool isAbilityMaxLevel)
    {
        var res = string.Empty;
        var data = ResourceSystem.Instance.GetStatusEffect(this.EffectType);
        
        if (data == null)
            return res;
        
        data.SetEffectValues(EffectValue, Duration, StatModifier);

        return data.GetEffectDescription(isAbilityMaxLevel, PerLevelValueChange, PerLevelDurationChange);
    }


    #endregion METHODS
}
