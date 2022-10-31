using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable/Status Effects/New RegenerateHp Effect", fileName = "SO_Effect_RegenerateHp")]
public class ScriptableStatusEffectRegenerateHp : ScriptableStatusEffect
{
    #region PROPERTIES

    
    #region Effect specific variables


    /// <summary>
    /// For poison and similar ticks
    /// </summary>
    [SerializeField] protected int TickAmount;

    protected int CurrentTickAmount;


    #endregion Effect specific variables


    #endregion PROPERTIES


    public ScriptableStatusEffectRegenerateHp()
    {
        Name = "Poison";
        Effect = StatusEffectType.RegenerateHp;
        TickOnDurationEnd = true;
        TickAmount = 5;
        DisplayValueType = EffectDisplayValue.EffectValue;
        IsStackable = true;
    }

    public override void Activate(Unit unitRef)
    {
        base.Activate(unitRef);

        CurrentTickAmount = 0;
    }

    protected override void ApplyEffect()
    {
    }

    protected override void RemoveEffect()
    {
    }

    protected override void Tick()
    {
        //apply poison tick
        UnitRef.Heal(EffectValue);

        CurrentTickAmount++;

        if (CurrentTickAmount >= TickAmount)
        {
            this.Deactivate();
            return;
        }

        //restart the next tick cycle
        base.Tick();
    }
}
