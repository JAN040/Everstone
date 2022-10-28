using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable/Ability/New Status Effect", fileName = "SO_Effect_")]
public class ScriptableStatusEffect : ScriptableObject
{
    #region PROPERTIES

    [Space]
    [Header("Properties")]
    public StatusEffectType Effect;

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
    //[NonSerialized]
    //public float DurationAtStart;

    [NonSerialized]
    public GameObject Prefab;

    [NonSerialized]
    protected Unit UnitRef;

    [NonSerialized]
    protected List<StatModifier> AppliedStatModifiers = new List<StatModifier>();

    [Space]
    [Header("Effect values")]

    /// <summary>
    /// The value (poison amount, stack amount) of the effect.
    /// Since the effect logic is hardcoded, keeping the values separated makes it easier to modify.
    /// </summary>
    public float EffectValue;
    public bool IsEffectValuePercentual = false;
    

    /// <summary>
    /// Defines what to display in the StatusEffectPanel
    /// </summary>
    public EffectDisplayValue DisplayValueType;

    [Space]
    [Header("Flags"), Tooltip("If true, adding the same effect type to the same unit twice will combine their effects instead of keeping the strongest one.")]
    public bool IsStackable;

    [Tooltip("Indicates the effect doesn't necessarily deactivate when its duration finishes, but instead executes arbitrary code")]
    [SerializeField] bool TickOnDurationEnd;

    public bool IsActive { get; private set; } = false;


    #region Effect specific variables


    /// <summary>
    /// For poison and similar ticks
    /// </summary>
    protected bool TickAmount;

    protected bool CurrentTickAmount;

    protected string CustomDisplayValue;


    #endregion Effect specific variables


    #endregion PROPERTIES


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
        //this.DurationAtStart = Duration;

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
    public virtual void Update()
    {
        if (!IsActive)
            return;

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

    public string GetDisplayValue()
    {
        switch (DisplayValueType)
        {
            case EffectDisplayValue.Duration:
                if (CurrentDuration == -1)
                    return ResourceSystem.GetIconTag(Icon.Infinity);
                else
                    return CurrentDuration.ToString();

            case EffectDisplayValue.EffectValue:
                if (IsEffectValuePercentual)
                    return (EffectValue * 100).ToString();
                else
                    return EffectValue.ToString();

            case EffectDisplayValue.Custom:
                return CustomDisplayValue;

            case EffectDisplayValue.None:
            default:
                return string.Empty;
        }
    }

    

    protected virtual void Tick()
    {
        //switch (Effect)
        //{
        //    case StatusEffectType.Poison:
        //        //apply poison tick
        //        UnitRef.TakeDamage(new Damage(EffectValue, ElementType.Poison), false);
        //        EffectValue = (int) EffectValue / 2;

        //        if (EffectValue <= 0)
        //        {
        //            this.Deactivate();
        //            return;
        //        }
        //        else
        //        {
        //            //DisplayValueType = EffectValue.ToString();
        //        }

        //        break;

        //    case StatusEffectType.EvasionBuff:
        //        break;
        //    case StatusEffectType.ShieldBlock:
        //        break;
        //    case StatusEffectType.Slow:
        //        break;
        //    default:
        //        break;
        //}

        //restart the next tick cycle
        this.CurrentDuration = Duration;
        //this.DurationAtStart = Duration;
    }

    public float GetRemainingDurationNormalized()
    {
        if (CurrentDuration == -1)
            return 1;

        if (CurrentDuration <= 0 || !IsActive)
            return 0;
        else
            return CurrentDuration / Duration;
    }

    //, float effectValue2 = 0, float effectValue3 = 0, float effectValue4 = 0, float effectValue5 = 0
    public void SetEffectValue(float effectValue)
    {
        EffectValue   = effectValue;
        //EffectValue_2 = effectValue2;
        //EffectValue_3 = effectValue3;
        //EffectValue_4 = effectValue4;
        //EffectValue_5 = effectValue5;
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
            //DurationAtStart += dupeEffect.Duration;
        }

        //if (Effect == StatusEffectType.Poison)
        //    DisplayValueType = EffectValue.ToString();
    }

    protected virtual void ApplyEffect()
    {
        switch (Effect)
        {
            case StatusEffectType.Poison:
                TickOnDurationEnd = true;
                //DisplayValueType = EffectValue.ToString();

                break;

            case StatusEffectType.EvasionBuff:
                //add 100% dodge chance
                ApplyStatModifier(new StatModifier(1, StatType.DodgeChance, ModifierType.Flat));

                //secondsToModifierDowngrade = EffectValue_2;
                //DisplayValueType = "100";

                break;

            case StatusEffectType.ShieldBlock:
                //TODO: when equipment is done, get actual shield stats for the armor modifiers etc..
                ApplyStatModifiers(new List<StatModifier>() {
                    new StatModifier(20, StatType.Armor, ModifierType.Flat),
                    //new StatModifier(- EffectValue_2, StatType.EnergyRecovery, ModifierType.Percent),
                });
                //DisplayValueType = ResourceSystem.GetIconTag(Icon.Infinity);

                break;

            case StatusEffectType.Slow:
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
