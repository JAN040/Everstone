using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StatusEffect
{
    #region VARIABLES


    public StatusEffectType EffectType;

    [Space]
    [Header("Target")]
    public TargetType Target;

    [Tooltip("Specifiy amount of random targets when TargetType is MultipleAllies or MultipleEnemies")]
    public int MultipleTargetCount = 2;

    public float EffectValue;
    public float Duration;

    [NonSerialized] //gotten from the resource system. Has data like effect icon, etc.
    public ScriptableStatusEffect EffectData;


    #endregion VARIABLES





    #region METHODS



    #endregion METHODS
}
