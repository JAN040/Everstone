using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using Newtonsoft.Json;


/// <summary>
/// A float wrapper, adding functionality like:
///     -differentiating stats by type
///     -modifier support
///     -growth parameter support (TBA)
/// </summary>
[System.Serializable]
public class Stat
{

    #region VARIABLES

    [JsonProperty] private List<StatModifier> statModifiers = new List<StatModifier>();
    
    /// <summary> Tracks how many times this stat has grown </summary>
    [JsonProperty] private float growthLevel;
    
    [JsonIgnore][SerializeField] private StatGrowthParameters growthParameters;

    public StatType Type { get => type; private set => type = value; }
    [JsonIgnore][SerializeField] private StatType type;
    
    public bool CanBeNegative { get => canBeNegative; private set => canBeNegative = value; }
    [JsonIgnore][SerializeField] private bool canBeNegative;

    /// <summary>
    /// Not used yet.. should be used to fine tune the stat growth function
    /// </summary>
    public StatGrowthParameters GrowthParameters { get => growthParameters; private set => growthParameters = value; }
    public float BaseValue
    {
        get => baseValue;
        set
        {
            bool isPositive = baseValue < value; 

            baseValue = value;
            
            OnStatChanged?.Invoke(this, isPositive);
        }
    }
    [JsonIgnore] [SerializeField] private float baseValue;

    /// <summary>
    /// Fires when Stat gets changed. Carries params:
    /// -Stat: Stat that was changed
    /// -bool: True if the change was positive, false if negative
    /// </summary>
    public event Action<Stat, bool> OnStatChanged;

    #endregion


    public Stat(float baseValue, StatType type, bool canBeNegative = false, StatGrowthParameters growthParams = null)
    {
        this.baseValue = baseValue;
        Type = type;
        CanBeNegative = canBeNegative;

        growthLevel = 1f;
        GrowthParameters = growthParams;
        if (GrowthParameters == null)
            GrowthParameters = new StatGrowthParameters();
    }


    #region STATIC METHODS


    public static string GetDisplayName(StatType statType)
    {
        switch (statType)
        {
            case StatType.PhysicalDamage:
                return "Physical Damage";

            case StatType.Armor:
                return "Armor";

            case StatType.ArtsDamage:
                return "Arts Damage";

            case StatType.ArtsResist:
                return "Arts Resist";

            case StatType.MaxHP:
                return "Max HP";

            case StatType.MaxEnergy:
                return "Max Energy";

            case StatType.EnergyRecovery:
                return "Energy Recovery";

            case StatType.MaxMana:
                return "Max Mana";

            case StatType.ManaRecovery:
                return "Mana Recovery";

            case StatType.CooldownReduction:
                return "Cooldown Reduction";

            case StatType.Speed:
                return "Speed";

            case StatType.WeaponAccuracy:
                return "Weapon Accuracy";

            case StatType.DodgeChance:
                return "Dodge Chance";

            case StatType.Proficiency:
                return "Proficiency";

            case StatType.HealEfficiency:
                return "Heal Efficiency";

            case StatType.BlockChance:
                return "Block Chance";

            default:
                break;
        }

        return string.Empty;
    }


    #endregion STATIC METHODS



    #region METHODS

    public string GetDisplayValue(float value = float.NaN)
    {
        if (float.IsNaN(value))
            value = GetValue();

        bool showOneDecimal = this.Type.In(StatType.HealEfficiency,
                                           StatType.EnergyRecovery,
                                           StatType.ManaRecovery);

        bool isPercent = this.Type.In(StatType.ArtsResist,
                                      StatType.BlockChance,
                                      StatType.CooldownReduction,
                                      StatType.DodgeChance,
                                      StatType.WeaponAccuracy);
        //for percentual values
        if (isPercent)
            value *= 100;        

        return showOneDecimal ? $"{value:0.#}" : $"{value:0}{(isPercent ? "%" : "")}";
    }

    public string GetLevelDiffDisplayValue(bool isHalfGrowth)
    {
        return $"{ResourceSystem.GetStatIconTag(this.Type)} {this.GetDisplayValue(BaseValue)} / {this.GetDisplayValue(BaseValue + this.GetGrowthAmount(isHalfGrowth))}";
    }

    /// <summary>
    /// Meant for modifying base stats on level up
    /// </summary>
    /// <returns></returns>
    //public float GetBaseValue()
    //{
    //    return this.baseValue;
    //}


    /// <summary>
    /// Get the value of the stat with flat and percent modifiers
    ///     taken into account
    /// </summary>
    public float GetValue()
    {
        float value = BaseValue;
        (float flatModifiers, float perc1mods, float perc2mods) = GetModifierValues();

        //step 1: add flat bonus
        value += flatModifiers;

        //step 2: add the percent bonus
        value += value * perc1mods;

        //step 2.5: add the percent (level 2) bonus
        value += value * perc2mods;

        //step 3: is negative check
        if (value < 0 && !CanBeNegative)
            value = 0;

        return value;
    }

    /// <summary>
    /// Grows the stat by one growth level by default
    /// </summary>
    /// <param name="growByLevels">How many levels to grow by. Default is 1</param>
    public void Grow(int growByLevels = 1)
    {
        if (growByLevels < 1)
            return;

        while (growByLevels > 0)
        {
            float bonus = GetGrowthAmount(false);
            BaseValue += bonus;
            growthLevel += 1f;
            Debug.Log($"Grew stat {Type} by '{bonus}' to '{BaseValue}' (growthLevel {growthLevel})");

            growByLevels--;
        }
    }

    /// <summary>
    /// Grows the stat by half a level (for skills stats that are affected secondarily)
    /// </summary>
    public void GrowHalf()
    {
        float bonus = GetGrowthAmount(true);
        BaseValue += bonus;
        growthLevel += 0.5f;

        Debug.Log($"Grew stat {Type} by '{bonus}' to '{BaseValue}' (growthLevel {growthLevel})");
    }

    /// <summary>
    /// Serves as a way to preview what the stats will be on level up and as a Grow helper method.
    /// </summary>
    public float GetGrowthAmount(bool isHalfGrowth)
    {
        float bonus = BaseValue * 0.01f * GetGrowthFixAmount();
        
        if (isHalfGrowth)
            bonus /= 2;

        return bonus;
    }

    private float GetGrowthFixAmount()
    {
        float amount = (SkillLevel.MAX_LEVEL - growthLevel) * 0.1f;
        //this becomes negative when growthLevel > MAX_LEVEL.. lazy fix
        if (amount <= 0)
            return 1;
        else
            return amount;
    }

    /// <returns>True on success</returns>
    public bool AddModifier(StatModifier modifier)
    {
        if (modifier == null)
            return false;

        if (modifier.ModifyingStatType != this.Type)
        {
            Debug.LogWarning($"Trying to add modifier of type {modifier.ModifyingStatType} to stat of type {this.Type}");
            return false;
        }

        if (statModifiers == null)
            statModifiers = new List<StatModifier>();
        
        var oldValue = GetValue();

        statModifiers.Add(modifier);
        
        Debug.Log($"Added {modifier.Type} modifier of '{modifier.Value}' to Stat '{modifier.ModifyingStatType}'. Prev. val: '{oldValue}', New val: '{GetValue()}");

        OnStatChanged?.Invoke(this, modifier.IsPositive());

        return true;
    }

    /// <returns>True on success</returns>
    public bool RemoveModifier(StatModifier modifier)
    {
        if (modifier == null || statModifiers == null || statModifiers.Count == 0)
        {
            return false;
        }

        var oldValue = GetValue();

        if (!statModifiers.Remove(modifier))
        {
            Debug.LogError($"Failed to remove modifier {modifier.Value}, {modifier.Type}, for statType {modifier.ModifyingStatType}");
            return false;
        }

        Debug.Log($"Removed {modifier.Type} modifier of '{modifier.Value}' to Stat '{modifier.ModifyingStatType}'. Prev. val: '{oldValue}', New val: '{GetValue()}");
        
        OnStatChanged?.Invoke(this, !modifier.IsPositive()); //removing a positive modifier is bad

        return true;
    }

    private (float, float, float) GetModifierValues()
    {
        float flatModifiers = 0;
        float percentModifiers = 0;
        float percent2Modifiers = 0;

        if (statModifiers != null)
        {
            foreach (StatModifier modifier in statModifiers)
            {
                if (modifier.Type == ModifierType.Flat)
                    flatModifiers += modifier.Value;
                
                else if (modifier.Type == ModifierType.Percent)
                    percentModifiers += modifier.Value;

                else if (modifier.Type == ModifierType.Percent_L2)
                    percent2Modifiers += modifier.Value;
            }
        }

        return (flatModifiers, percentModifiers, percent2Modifiers);
    }

    # endregion METHODS
}


public enum StatType
{
    PhysicalDamage,
    /// <summary>
    /// Flat PhysicalDamage reduction
    /// </summary>
    Armor,
    ArtsDamage,
    /// <summary>
    /// Percent ArtsDamage reduction
    /// </summary>
    ArtsResist,
    MaxHP,
    MaxEnergy,
    /// <summary>
    /// The rate at which Energy recovers (per second)
    /// </summary>
    EnergyRecovery,
    MaxMana,
    /// <summary>
    /// The rate at which Mana recovers (per second)
    /// </summary>
    ManaRecovery,
    /// <summary>
    /// Percent reduction of extra skill cooldowns 
    /// </summary>
    CooldownReduction,
    Speed,
    WeaponAccuracy,
    DodgeChance,
    /// <summary>
    /// Modifies xp gain for levelable stats
    /// </summary>
    Proficiency,
    /// <summary>
    /// Percent modifies heal amount when healed
    /// </summary>
    HealEfficiency,

    BlockChance
}


