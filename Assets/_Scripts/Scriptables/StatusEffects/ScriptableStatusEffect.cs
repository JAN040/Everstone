using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//[CreateAssetMenu(menuName = "Scriptable/New Status Effect", fileName = "SO_Effect_")]
public abstract class ScriptableStatusEffect : ScriptableObject
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

    //[NonSerialized]
    //protected List<StatModifier> AppliedStatModifiers = new List<StatModifier>();

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
    [SerializeField] protected bool TickOnDurationEnd;

    public bool IsActive { get; private set; } = false;


    #region Effect specific variables


    protected StatModifier Modifier;
    protected string CustomDisplayValue;


    #endregion Effect specific variables


    #endregion PROPERTIES


    public event Action<ScriptableStatusEffect> OnEffectExpired;

    public virtual void SetEffectValues(float effectValue, float duration, StatModifier modifier)
    {
        EffectValue = effectValue;
        Duration = duration;
        Modifier = modifier;
    }

    /// <summary>
    /// Gets the effect running
    /// </summary>
    public virtual void Activate(Unit unitRef)
    {
        if (IsActive)
            return;

        UnitRef = unitRef;

        this.IsActive = true;
        this.CurrentDuration = Duration;

        ApplyEffect();
    }

    public virtual void Deactivate()
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

    protected virtual void Tick()
    {
        //restart the next tick cycle
        this.CurrentDuration = Duration;
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


    public virtual void StackEffect(ScriptableStatusEffect dupeEffect)
    {
        if (!IsStackable)
            return;

        //add up effect values, extend durations etc.
        EffectValue += dupeEffect.EffectValue;

        //for non infinte non tickable effects also add up duration
        if (CurrentDuration != -1 && !TickOnDurationEnd)
        {
            CurrentDuration += dupeEffect.Duration;
        }
    }

    /// <summary>
    /// Logic like dealing the damage, applying StatModifier, etc
    /// </summary>
    protected abstract void ApplyEffect();

    /// <summary>
    /// Logic that reverses ApplyEffect
    /// </summary>
    protected abstract void RemoveEffect();

    protected void ApplyStatModifier(StatModifier modifier)
    {
        UnitRef.Stats.AddModifier(modifier);
    }

    protected void RemoveStatModifier(StatModifier modifier)
    {
        UnitRef.Stats.RemoveModifier(modifier);
    }

    public virtual Sprite GetIcon()
    {
        return MenuImage;
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
                return GetCustomDisplayValue();

            case EffectDisplayValue.None:
            default:
                return string.Empty;
        }
    }

    protected virtual string GetCustomDisplayValue()
    {
        return string.Empty;
    }
}
