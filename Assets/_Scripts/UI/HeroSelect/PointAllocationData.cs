using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[Serializable]
public class PointAllocationData
{
    #region STATIC

    public static int GetTotalPoints(Skill skill, List<PointAllocationData> dataList)
    {
        var total = 0;
        foreach (var data in dataList)
        {
            total += data.GetBaseSkillPoints(skill);
        }

        return total;
    }

    public static int GetTotalWeaponPoints(WeaponType weaponType, List<PointAllocationData> dataList)
    {
        var total = 0;
        foreach (var data in dataList)
        {
            total += data.GetWeaponProficiency(weaponType);
        }

        return total;
    } 

    #endregion STATIC


    [SerializeField]
    private BaseSkillPointAllocation[] BaseSkillPointAllocations = new BaseSkillPointAllocation[]
    {
        new BaseSkillPointAllocation(Skill.Constitution, 0),
        new BaseSkillPointAllocation(Skill.Strength,     0),
        new BaseSkillPointAllocation(Skill.Agility,      0),
        new BaseSkillPointAllocation(Skill.Arts,         0),    

        //non editable in character creation scene
        new BaseSkillPointAllocation(Skill.Lockpicking,  0),
        new BaseSkillPointAllocation(Skill.Taming,       0),
        new BaseSkillPointAllocation(Skill.Trading,      0),
    };

    [SerializeField]
    private WeaponProficiencyAllocation[] WeaponProficiencies = new WeaponProficiencyAllocation[]
    {
        //non editable in character creation scene
        new WeaponProficiencyAllocation(WeaponType.Dagger, 0),
        new WeaponProficiencyAllocation(WeaponType.Sword,  0),
        new WeaponProficiencyAllocation(WeaponType.Axe,    0),
        new WeaponProficiencyAllocation(WeaponType.Shield, 0),
        new WeaponProficiencyAllocation(WeaponType.Staff,  0),
    };

    /// <summary>
    /// Returns the base skill point allocation or zero, if no BaseSkillPointAllocation entry 
    ///     exists for the specified Skill parameter
    /// </summary>
    public int GetBaseSkillPoints(Skill skill)
    {
        var allocation = BaseSkillPointAllocations.FirstOrDefault(x => x.skill == skill);
        return allocation == null ? 0 : allocation.points;
    }

    /// <summary>
    /// Returns the base weapon proficiency allocation or zero, if no WeaponProficiencyAllocation entry 
    ///     exists for the specified WeaponType parameter
    /// </summary>
    public int GetWeaponProficiency(WeaponType weaponType)
{
        var allocation = WeaponProficiencies.FirstOrDefault(x => x.weaponType == weaponType);
        return allocation == null ? 0 : allocation.points;
    }

    [Serializable]
    private class BaseSkillPointAllocation
    {
        public Skill skill;
        public int points;

        public BaseSkillPointAllocation(Skill skill, int points)
        {
            this.skill = skill;
            this.points = points;
        }
    }

    [Serializable]
    private class WeaponProficiencyAllocation
    {
        public WeaponType weaponType;
        public int points;

        public WeaponProficiencyAllocation(WeaponType weaponType, int points)
        {
            this.weaponType = weaponType;
            this.points = points;
        }
    }
}
