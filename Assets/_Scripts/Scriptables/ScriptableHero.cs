using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Create a hero type. Used to store hero class base data
/// </summary>
[CreateAssetMenu(menuName = "Scriptable/Units/New Hero Class", fileName = "SO_HeroClass_")]
public class ScriptableHero : ScriptableUnitBase {

    [Space]
    [Header("ScriptableHero")]
    [Space]

    public string ClassName;

    [SerializeField]
    public BaseSkillPointAllocation[] BaseSkillPointAllocations = new BaseSkillPointAllocation[]
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
    public WeaponProficiency[] WeaponProficiencies = new WeaponProficiency[]
    {
        //non editable in character creation scene
        new WeaponProficiency(WeaponType.Dagger, 0),
        new WeaponProficiency(WeaponType.Sword,  0),
        new WeaponProficiency(WeaponType.Axe,    0),
        new WeaponProficiency(WeaponType.Shield, 0),    
        new WeaponProficiency(WeaponType.Staff,  0),    
    };

    [Serializable]
    public class BaseSkillPointAllocation
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
    public class WeaponProficiency
    {
        public WeaponType weapon;
        public int points;

        public WeaponProficiency(WeaponType weaponType, int points)
        {
            this.weapon = weaponType;
            this.points = points;
        }
    }
}


