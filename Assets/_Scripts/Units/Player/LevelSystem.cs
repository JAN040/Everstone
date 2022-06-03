using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


/// <summary>
/// LevelSystem groups all the levelable skills for this character.
/// </summary>
[Serializable]
public class LevelSystem
{
    #region PROPERTIES

    public Dictionary<Skill, SkillLevel> Skills { get; private set; }

    private CharacterStats Stats;

    #endregion PROPERTIES


    public LevelSystem(Dictionary<Skill, SkillLevel> skills, CharacterStats stats)
    {
        if (skills == null || stats == null)
        {
            throw new NullReferenceException("Cant instantiate LevelSystem without valid skills & stats references");
        }
        Skills = skills;
        Stats = stats;
    }
    
    public void AddExperienceToSkill(int amount, Skill skill)
    {
        Skills[skill].AddExperience(amount);
    }

}

