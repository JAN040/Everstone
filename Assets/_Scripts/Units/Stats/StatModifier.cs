using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatModifier
{
	public ModifierType Type { get; private set; }
	
	[Tooltip("Example: Percentage value 0.2 translates to 20% buff")]
	public float Value { get; set; }
    public StatType ModifyingStatType { get; private set; }

	public StatModifier(float value, StatType modifyingStatType, ModifierType type = ModifierType.Flat)
    {
        this.Value = value;
        ModifyingStatType = modifyingStatType;
        this.Type = type;
    }
}



public enum ModifierType
{
    Flat,
    Percent
}