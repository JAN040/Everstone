using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable/Other/New Effect", fileName = "SO_Effect_")]
public class ScriptableEffect : ScriptableObject
{
    #region PROPERTIES


    public Effect Effect;

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

            if (currentDuration < 0)
                currentDuration = 0;
        }
    }

    /// <summary>
    /// Amount of duration when the ability was activated.
    /// Not necessarily the same as Cooldown ? idk will see
    /// </summary>
    [NonSerialized]
    public float DurationAtStart;


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

    //[Space]
    //[Header("Flags")]


    #endregion PROPERTIES


    //public event Action<ScriptableAbility> OnAbilityActivated;
    //public event Action<bool> OnAbilityToggled;
}
