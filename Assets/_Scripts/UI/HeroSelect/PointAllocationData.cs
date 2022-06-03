using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class PointAllocationData
{
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

    public int GetBaseSkillPoints(Skill skill)
    {
        var allocation = BaseSkillPointAllocations.FirstOrDefault(x => x.skill == skill);
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
        public WeaponType weapon;
        public int points;

        public WeaponProficiencyAllocation(WeaponType weaponType, int points)
        {
            this.weapon = weaponType;
            this.points = points;
        }
    }
}
