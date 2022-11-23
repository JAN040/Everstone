using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;


/// <summary>
/// LevelSystem groups all the levelable skills for this character.
/// </summary>
[Serializable]
public class LevelSystem
{
    #region PROPERTIES

    /// <summary>
    /// Dictionary of skill names and SkillLevel objects.
    ///     Used to keep track of all levelable skills
    /// </summary>
    public Dictionary<string, SkillLevel> Skills { get; private set; }

    [SerializeField] private CharacterStats Stats;

    #endregion PROPERTIES


    /// <summary>
    /// Json deserializer constructor
    /// </summary>
    [JsonConstructor]
    public LevelSystem(Dictionary<string, SkillLevel> skills)
    {
        Skills = skills;
    }

    public LevelSystem(Dictionary<string, SkillLevel> skills, CharacterStats stats)
    {
        if (skills == null || stats == null)
        {
            throw new NullReferenceException("Cant instantiate LevelSystem without valid skills & stats references");
        }

        Skills = skills;
        Stats = stats;
    }


    #region METHODS


    public void AddExperienceToSkill(int amount, Skill skill)
    {
        Skills[skill.ToString()].AddExperience(amount);
    }

    public List<UnlockCondition> GetUnfulfilledConditions(List<UnlockCondition> unlockConditions)
    {
        var unfulfilledConditions = new List<UnlockCondition>();

        foreach (var condition in unlockConditions)
        {
            if (Skills[condition.Skill.ToString()].Level < condition.Level)
            {
                unfulfilledConditions.Add(condition);
            }
        }

        return unfulfilledConditions;
    }

    //public void AddExperienceToWeaponSkill(int amount, WeaponType weaponType)
    //{
    //    Skills[weaponType.ToString()].AddExperience(amount);
    //}


    #endregion METHODS
}

