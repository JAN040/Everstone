﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


#region CHARACTER

[Serializable]
public enum Faction
{
    Allies,
    Enemies,
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
    Normal = 5,
    Advanced = 10,
    Hard = 25,
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
public enum UnitClass
{               //  speed       damage      const       special
    Marksman,   //  fast,       high,       low
    Mage,       //  slow,       high,       low         arts
    Artillery,  //  slow,       V high,     medium      AOE, ELITE
    Controller, //  slow,       low,        medium      CC, debuffs, team buffs
    Bruiser,    //  normal      normal,     normal,     melee, self-heal, self atk buff 
    Battlemage, //  normal      normal,     normal,     arts, self shield, ELITE
    Tank,       //  slow,       low,        high,       armor
    Titan,      //  V slow,     high,       V high      armor, ELITE
    Vanguard,   //  fast,       low,        normal      melee
    Assassin,   //  fast,       high,       low,        melee, invis?
    Healer,     //  slow,       low,        low,        heal team
    Warrior     //  normal,     normal,     normal     
}

public enum UnitRace
{
    Animal,     //can only drop hides
    Humanoid,   //drops hides & can rarely drop equip,
    Human,      //can only drop equipment and currency
    Monster     //kindof "other", idk
}

public enum StatScaling
{
    VeryLow,
    Low,
    Normal,
    High,
    VeryHigh
}

//public enum SpecialAbility
//{
//    CC,             //stuns the enemy (could be freeze or someth when/if i add elements)
//    OpponentDebuff, OpponentTeamDebuff,
//    SelfBuff,       TeamBuff,
//    SelfHeal,       TeamHeal,
//    AoeAtk,         //hits the unit & the ones behind, in front, above and below the target
//    PiercingAtk1,   //hits the unit & one unit behind
//    PiercingAtk2    //hits the unit & all the ones behind
//}

public enum EnemyType
{
    Normal,
    Elite,
    Boss
}

public enum FacingDirection
{
    Left,
    Right
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
    //  further divided by EquipmentType enum
    Equipment_Skill,
}

#endregion CHARACTER


#region ITEMS

[Serializable]
public enum ItemType
{
    None,       //for itemSlot; it means any item can be inserted into the slot
    Equipment,  //view stats, equippable in equipment UI
    Potion,     //usable
    Loot,       //sell price viewable
    Currency,
}

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary,
    None,
    Quest,
}

[Serializable]
public enum EquipmentType
{
    None,

    //weapons
    Sword,
    Dagger,
    Axe,
    Staff,
    Shield,

    //armors
    Helmet,
    Shoulder,
    Chestplate,
    Pants,
    Boots,

    //trinkets
    Necklace,
    Cape,
    Gloves,
    Ring,

    //Rune
    Rune,
}


#endregion ITEMS


public enum Icon
{
    Infinity,
    Everstone,
    Attack_Phys,
    Attack_Arts,
    Defense,
    Arts_Resist,
    Speed,
    Health,
    Stamina,
    Mana,
    Energy_Regen,
    Mana_Regen,
    Health_Regen,
    Strength,
    Agility,
    Taming,
    Lockpicking,
    Trading,
    Block_Chance,
    Dodge,
    Cooldown,
    Accuracy,
    Capacity,
    Luck,
    Coin_Copper,
    Coin_Silver,
    Coin_Gold,
    Poison,
    Electricity,
    Fire,
    Buff,
    Debuff
}

public enum UserLayers
{
    Allies_Layer,
    Enemies_Layer
}


public enum DamageType
{
    Physical,
    Arts,
    /// <summary>
    /// Ignores armor and resistances
    /// </summary>
    True,
    /// <summary>
    /// Poison, burn, etc. Generally Ignores armor and resistances,
    ///     but has a different StatusChangeIndicator text color (not white like True damage)
    /// </summary>
    Elemental
}

public enum ElementType
{
    None,
    Poison,
    Fire,
}

public enum ToggleMode
{
    None,
    Toggled,
    UnToggled
}

public enum Ability
{
    BasicAttack,
    Dodge,
    ShieldBlock,
    OrdinaryAbility
}

public enum StatusEffectType
{
    ModifyStat,
    RegenerateHp,
    Poison,
    Burn,
    DealDamage,
    //etc.
}


public enum AttackType
{
    Melee,
    Ranged,
    Special
}

public enum ItemMoveResult
{
    NoChange,
    Moved,
    Swapped,
    StackedAll,
    StackedWithRemainder
}

public enum EquipmentSlot
{
    None = -1,

    Helmet,
    Shoulder,
    Chestplate,
    Pants,
    Boots,

    Necklace,
    Cape,
    Gloves,
    Ring1,
    Ring2,

    RightArm,
    LeftArm,
}

public enum TargetType
{
    Self,

    SelectedAlly,
    RandomAlly,
    MultipleAllies,
    AllAllies,

    SelectedEnemy,
    RandomEnemy,
    MultipleEnemies,
    AllEnemies,

    Everyone,
}

public enum EffectDisplayValue
{
    None,
    Duration,
    EffectValue,
    Custom
}

public enum BattleState
{
    None,
    InBattle,
    Success,
    GameOver
}

public enum Gender
{
    Male,
    Female,
    Unknown
}