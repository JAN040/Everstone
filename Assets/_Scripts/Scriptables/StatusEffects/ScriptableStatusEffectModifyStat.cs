using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable/Status Effects/New ModifyStat Effect", fileName = "SO_Effect_ModifyStat")]
public class ScriptableStatusEffectModifyStat : ScriptableStatusEffect
{
    #region Effect specific variables


    public Sprite MenuImage_Debuff;


    #endregion Effect specific variables


    public ScriptableStatusEffectModifyStat()
    {
        Name = "ModifyStat";
        Effect = StatusEffectType.ModifyStat;
        TickOnDurationEnd = false;
        DisplayValueType = EffectDisplayValue.Custom;
        IsStackable = false;
    }


    protected override void ApplyEffect()
    {
        Modifier.Value = EffectValue;

        ApplyStatModifier(Modifier);
    }

    protected override void RemoveEffect()
    {
        RemoveStatModifier(Modifier);
    }

    public override Sprite GetIcon()
    {
        if (Modifier == null)
            return MenuImage;

        return Modifier.IsPositive() ? MenuImage : MenuImage_Debuff;
    }

    protected override string GetCustomDisplayValue()
    {
        if (Modifier != null)
            return ResourceSystem.GetStatIconTag(Modifier.ModifyingStatType);
        else
            return string.Empty;
    }
}
