using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable/Ability/New Status Effect", fileName = "SO_Effect_")]
public class ScriptableStatusEffect : ScriptableObject
{
    #region PROPERTIES


    public StatusEffect Effect;

    public Sprite MenuImage;

    public string Name;

    [TextArea(3, 5)]
    public string Description;

    /// <summary>
    /// The amount (in seconds) after which the effect diminishes.
    ///     Value '-1' stands for infinite.
    /// </summary>
    public float Duration = 0;

    private float currentDuration;
    /// <summary>
    /// Duration timer. Effect will expire when it hits 0.
    /// </summary>
    public float CurrentDuration
    {
        get => currentDuration;
        set
        {
            currentDuration = value;
        }
    }

    /// <summary>
    /// Amount of duration when the ability was activated.
    /// Not necessarily the same as Cooldown ? idk will see
    /// </summary>
    [NonSerialized]
    public float DurationAtStart;

    [NonSerialized]
    public GameObject Prefab;

    [NonSerialized]
    private Unit UnitRef;

    [NonSerialized]
    private List<StatModifier> AppliedStatModifiers = new List<StatModifier>();

    [Space]
    [Header("Effect values")]

    /// <summary>
    /// The value (poison amount, stack amount) of the effect.
    /// Since the effect logic is hardcoded, keeping the values separated makes it easier to modify.
    /// </summary>
    public float EffectValue;
    public float EffectValue_2;
    public float EffectValue_3;
    public float EffectValue_4;
    public float EffectValue_5;

    /// <summary>
    /// The value to display in the StatusEffectPanel
    /// </summary>
    public string DisplayValue;

    /// <summary>
    /// If true, adding the same effect type to the same unit twice will combine their effects
    ///     instead of keeping the strongest one.
    /// </summary>
    [Space]
    [Header("Flags")]

    public bool IsStackable;
    
    public bool IsActive { get; private set; } = false;

   


    #region Effect specific variables

    /// <summary>
    /// Dodge effect downgrades after cca 0.1s to provide a lesser buff
    /// </summary>
    private float secondsToModifierDowngrade;

    /// <summary>
    /// For poison and similar ticks
    /// </summary>
    private bool TickOnDurationEnd;

    private bool TickAmount;

    private bool CurrentTickAmount;


    #endregion Effect specific variables


    #endregion PROPERTIES


    //public event Action<ScriptableAbility> OnAbilityActivated;
    public event Action<ScriptableStatusEffect> OnEffectExpired;


    /// <summary>
    /// Gets the effect running
    /// </summary>
    public void Activate(Unit unitRef)
    {
        if (IsActive)
            return;

        UnitRef = unitRef;

        this.IsActive = true;
        this.CurrentDuration = Duration;
        this.DurationAtStart = Duration;

        ApplyEffect();
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        this.IsActive = false;

        RemoveEffect();

        Destroy(this.Prefab);
        OnEffectExpired?.Invoke(this);
    }

    /// <summary>
    /// Has to be called every frame while the effect is active
    /// </summary>
    public void Update()
    {
        if (!IsActive)
            return;

        EffectSpecificUpdate();

        //-1 stands for infinite
        if (CurrentDuration != -1f)
        {
            CurrentDuration -= Time.deltaTime;

            if (CurrentDuration <= 0)
            {
                if (TickOnDurationEnd)
                    this.Tick();
                else
                    this.Deactivate();
            }
        }
    }

    private void EffectSpecificUpdate()
    {
        switch (Effect)
        {
            case StatusEffect.Poison:
                break;

            case StatusEffect.EvasionBuff:
                secondsToModifierDowngrade -= Time.deltaTime;
                if (secondsToModifierDowngrade <= 0)
                {
                    //downgrade perfect dodge from 100 to 
                    var modifier = AppliedStatModifiers.FirstOrDefault(x => x.ModifyingStatType == StatType.DodgeChance);
                    modifier.Value = EffectValue;
                    DisplayValue = $"{(int)(EffectValue * 100f)}";
                }
                break;

            case StatusEffect.ShieldBlock:
                break;

            case StatusEffect.Slow:
                break;

            default:
                break;
        }
    }

    private void Tick()
    {
        switch (Effect)
        {
            case StatusEffect.Poison:
                //apply poison tick
                UnitRef.TakeDamage(new Damage(EffectValue, ElementType.Poison), false);
                EffectValue = (int) EffectValue / 2;

                if (EffectValue <= 0)
                {
                    this.Deactivate();
                    return;
                }
                else
                {
                    DisplayValue = EffectValue.ToString();
                }

                break;

            case StatusEffect.EvasionBuff:
                break;
            case StatusEffect.ShieldBlock:
                break;
            case StatusEffect.Slow:
                break;
            default:
                break;
        }

        //restart the next tick cycle
        this.CurrentDuration = Duration;
        this.DurationAtStart = Duration;
    }

    public float GetRemainingDurationNormalized()
    {
        if (CurrentDuration == -1)
            return 1;

        if (DurationAtStart <= 0 || !IsActive)
            return 0;
        else
            return CurrentDuration / DurationAtStart;
    }

    public void SetEffectValues(float effectValue, float effectValue2 = 0, float effectValue3 = 0, float effectValue4 = 0, float effectValue5 = 0)
    {
        EffectValue   = effectValue;
        EffectValue_2 = effectValue2;
        EffectValue_3 = effectValue3;
        EffectValue_4 = effectValue4;
        EffectValue_5 = effectValue5;
    }

    public void StackEffect(ScriptableStatusEffect dupeEffect)
    {
        //note to future self: feel free to add custom logic for specific effects, should you need it

        //add up effect values, extend durations etc.
        EffectValue += dupeEffect.EffectValue;

        //for non infinte non tickable effects also add up duration
        if (CurrentDuration != -1 && !TickOnDurationEnd)
        {
            CurrentDuration += dupeEffect.Duration;
            DurationAtStart += dupeEffect.Duration;
        }

        if (Effect == StatusEffect.Poison)
            DisplayValue = EffectValue.ToString();
    }

    private void ApplyEffect()
    {
        switch (Effect)
        {
            case StatusEffect.Poison:
                TickOnDurationEnd = true;
                DisplayValue = EffectValue.ToString();

                break;

            case StatusEffect.EvasionBuff:
                //add 100% dodge chance
                ApplyStatModifier(new StatModifier(1, StatType.DodgeChance, ModifierType.Flat));

                secondsToModifierDowngrade = EffectValue_2;
                DisplayValue = "100";

                break;

            case StatusEffect.ShieldBlock:
                //TODO: when equipment is done, get actual shield stats for the armor modifiers etc..
                ApplyStatModifiers(new List<StatModifier>() {
                    new StatModifier(20, StatType.Armor, ModifierType.Flat),
                    new StatModifier(- EffectValue_2, StatType.EnergyRecovery, ModifierType.Percent),
                });
                DisplayValue = ResourceSystem.GetIconTag(Icon.Infinity);

                break;

            case StatusEffect.Slow:
                break;

            default:
                break;
        }
    }

    private void ApplyStatModifiers(List<StatModifier> statModifiers)
    {
        statModifiers.ForEach(x => ApplyStatModifier(x));
    }

    private void ApplyStatModifier(StatModifier modifier)
    {
        UnitRef.Stats.AddModifier(modifier);
        AppliedStatModifiers.Add(modifier);
    }

    private void RemoveStatModifier(StatModifier modifier)
    {
        UnitRef.Stats.RemoveModifier(modifier);
        AppliedStatModifiers.Remove(modifier);
    }

    private void RemoveAllStatModifiers()
    {
        UnitRef.Stats.RemoveModifiers(AppliedStatModifiers);
        AppliedStatModifiers.Clear();
    }

    private void RemoveEffect()
    {
        RemoveAllStatModifiers();
    }

    
}
