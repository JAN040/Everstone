using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable/Status Effects/New Poison Effect", fileName = "SO_Effect_Poison")]
public class ScriptableStatusEffectPoison : ScriptableStatusEffect
{
    #region PROPERTIES

    
    #region Effect specific variables


    #endregion Effect specific variables


    #endregion PROPERTIES


    public ScriptableStatusEffectPoison()
    {
        Name = "Poison";
        Effect = StatusEffectType.Poison;
        TickOnDurationEnd = true;
        DisplayValueType = EffectDisplayValue.EffectValue;
        IsStackable = true;
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
        UnitRef.TakeDamage(new Damage(EffectValue, ElementType.Poison, false, false));
        EffectValue = (int)EffectValue / 2;

        if (EffectValue <= 0)
        {
            this.Deactivate();
            return;
        }

        //restart the next tick cycle
        base.Tick();
    }
}
