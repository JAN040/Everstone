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