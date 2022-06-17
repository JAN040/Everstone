using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


#region CHARACTER

[Serializable]
public enum Faction
{
    Heroes,
    Pets,
    Enemies
}

[Serializable]
public enum Difficulty
{
    Custom,
    Casual,
    Normal,
    Hard
}

/// <summary>
/// Enemy base stats will be multiplied by the enum value/10 regardless of reached stage
///     eg. for difficulty = 3, all enemy stats will gain 30% bonus
/// </summary>
[Serializable]
public enum LocationDifficulty
{
    Easy = 0,
    Normal = 2,
    Advanced = 5,
    Hard = 10,
    Expert = 50
}

public enum EncounterType
{
    SingleEnemy,
    MultipleEnemy,
    BossEnemy,
    MonsterNest,
    Event,
}

[Serializable]
public enum EnemyClass
{               //  speed       damage      const       special
    Marksman,   //  fast,       high,       low
    Mage,       //  slow,       high,       low         arts
    Artillery,  //  normal,     V high,     medium      arts, ELITE
    Controller, //  slow,       low,        medium      CC, debuffs, team buffs
    Bruiser,    //  normal      normal,     normal,     melee, self-heal, self atk buff 
    Battlemage, //  normal      normal,     normal,     arts, self shield, ELITE
    Tank,       //  slow,       low,        high,       armor
    Titan,      //  V slow,     high,       V high      armor, ELITE
    Vanguard,   //  fast,       low,        normal      melee
    Assassin,   //  fast,       high,       low,        melee, invis
    Healer,     //  slow,       low,        low,        heal team
}

public enum EnemyType
{
    Normal,
    Elite,
    Boss
}

//public enum HeroClass
//{
//    Warrior,
//    Mage,
//    Rogue
//}

public enum Skill
{
    Strength,
    Arts,
    Agility,
    Constitution,
    Lockpicking,
    Taming,
    Trading,

    //skills only active when a specific type of equipment is equipped
    //  further divided by WeaponType enum
    Equipment_Skill,
}

#endregion CHARACTER


#region ITEMS

[Serializable]
public enum ItemType
{
    Weapon,
    Potion,
}

[Serializable]
public enum WeaponType
{
    None,
    Sword,
    Dagger,
    Axe,
    Staff,
    Shield,
} 

#endregion ITEMS


public enum Icon
{
    Infinity,
    Everstone
}