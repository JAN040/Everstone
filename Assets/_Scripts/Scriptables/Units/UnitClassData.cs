using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitClassData
{
    public UnitClass Class;

    public StatScaling Damage;
    public DamageType  DamageType;
    public bool        IsRanged;    //if false, can hit only the first column

    public StatScaling Speed;
    public StatScaling Constitution;
    public StatScaling Armor;
    public float       ArtsResist;

    public List<SpecialSkill> SpecialSkills;
    public bool OnlyUseSpecialSkills;

    public UnitClassData(UnitClass @class, List<SpecialSkill> specialSkills,
        StatScaling speed, StatScaling constitution, StatScaling armor, float artsResist,
        StatScaling damage, DamageType damageType = DamageType.Physical, bool isRanged = false,
        bool onlyUseSpecialSkills = false)
    {
        Class = @class;

        Damage = damage;
        DamageType = damageType;
        IsRanged = isRanged;

        Speed = speed;
        Constitution = constitution;
        Armor = armor;
        ArtsResist = artsResist;

        SpecialSkills = specialSkills;
        OnlyUseSpecialSkills = onlyUseSpecialSkills;
    }
}