using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable/z_Other/New Status Effect", fileName = "SO_Effect_")]
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


    #endregion PROPERTIES


    //public event Action<ScriptableAbility> OnAbilityActivated;
    public event Action<ScriptableStatusEffect> OnEffectExpired;

    /// <summary>
    /// Gets the effect running
    /// </summary>
    public void Activate()
    {
        if (IsActive)
            return;

        this.IsActive = true;
        this.CurrentDuration = Duration;
        this.DurationAtStart = Duration;
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        this.IsActive = false;
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

        //-1 stands for infinite
        if (CurrentDuration != -1f)
        {
            CurrentDuration -= Time.deltaTime;

            if (CurrentDuration <= 0)
                this.Deactivate();
        }
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

        //for non infinte effects also add up duration
        if (CurrentDuration != -1)
        {
            CurrentDuration += dupeEffect.Duration;
            DurationAtStart += dupeEffect.Duration;
        }
    }
}
