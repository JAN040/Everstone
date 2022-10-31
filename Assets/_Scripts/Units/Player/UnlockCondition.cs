using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Specifies what level the player must reach in order to meet the criteria for something
/// </summary>
[System.Serializable]
public class UnlockCondition
{
    #region VARIABLES


    [Space]
    [Header("Variables")]
    public Skill Skill;
    public int Level;


    #endregion VARIABLES


    #region METHODS


    /// <returns>True if the provided level system meets this conditions criteria</returns>
    public bool IsMet(LevelSystem lvlSystem)
    {
        return lvlSystem.Skills[Skill.ToString()].Level >= Level;
    }


    #endregion METHODS
}
