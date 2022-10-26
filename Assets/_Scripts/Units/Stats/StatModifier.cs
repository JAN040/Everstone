using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatModifier
{
    [SerializeField] ModifierType _type;
    [SerializeField] float _value;
    [SerializeField] StatType _modifyingStatType;

    public ModifierType Type { get => _type; private set => _type = value; }

    [Tooltip("Example: Percentage value 0.2 translates to 20% buff")]
    public float Value { get => _value; set => _value = value; }
    public StatType ModifyingStatType { get => _modifyingStatType; private set => _modifyingStatType = value; }

    public StatModifier(float value, StatType modifyingStatType, ModifierType type = ModifierType.Flat)
    {
        if (value == 0)
            Debug.LogWarning($"Creating a modifier with a value of 0..! (stat {modifyingStatType})");

        this.Value = value;
        ModifyingStatType = modifyingStatType;
        this.Type = type;
    }

    /// <returns>True if the change made to the stat with this modifier is considered positive, false otherwise</returns>
    public bool IsPositive()
    {
        return this.Value > 0;
    }
}


public enum ModifierType
{
    Flat,
    Percent
}