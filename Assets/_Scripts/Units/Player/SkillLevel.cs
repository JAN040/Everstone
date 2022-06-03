using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SkillLevel
{
    #region PROPERTIES

    /// <summary>
    /// Dictionary of skill names, aka Strength, Agility etc.
    ///     to avoid hardcoded values
    /// </summary>
    //public static Dictionary<Skill, string> SkillNames { get; } = new Dictionary<Skill, string>()
    //{
    //    { Skill.Strength, "Strength" },
    //    { Skill.Arts, "Arts" },
    //    { Skill.Agility, "Agility" },
    //    { Skill.Constitution, "Constitution" },
    //    { Skill.Lockpicking, "Lockpicking" },
    //    { Skill.Taming, "Taming" },
    //    { Skill.Trading, "Trading" },

    //    //skills only active when a specific type of equipment
    //    //  is equipped
    //    { Skill.Equipment_Skill, "Weapon Mastery" },
    //};

    //public static Dictionary<WeaponType, string> Weapon_SkillNames { get; } = new Dictionary<WeaponType, string>()
    //{ 
    //    { WeaponType.Sword, "Sword  Mastery" },
    //    { WeaponType.Dagger, "Dagger  Mastery" },
    //    { WeaponType.Axe, "Axe  Mastery" },
    //    { WeaponType.Shield, "Shield  Mastery" }
    //};

    /// <summary>
    /// Highest allowed level. Used in stat gain calculation
    /// </summary>
    public static readonly int MAX_LEVEL = 100;

    [Header("Default xp growth parameters")]

    [Range(1f, 300f)]
    public static float AdditionMultiplier = 300;
    [Range(2f, 4f)]
    public static float PowerMultiplier = 2;
    [Range(7f, 14f)]
    public static float DivisionMultiplier = 7;


    public Skill SkillType { get; private set; }

    /// <summary>
    /// Further divides weapon skills When SkillType = Skill.Weapon_SkillType
    /// </summary>
    public WeaponType Weapon_SkillType { get; private set; }

    private CharacterStats _statsReference;

    private int _level;
    public int Level
    {
        get { return _level; }
        set
        {
            _level = value;
            ExpToNextLevel = GetRequiredExpToNextLevel(_level);
        }
    }
    public int Experience { get; set; }
    public int ExpToNextLevel { get; set; }

    /// <summary>
    /// Modifies bonus exp, aka. how fast the character will level up
    /// </summary>
    public Stat Proficiency { get; private set; }

    #endregion PROPERTIES

    public event EventHandler OnExperienceChanged;
    public event EventHandler OnLevelChanged;

    /// <param name="type">Which skill this level system is for</param>
    /// <param name="type">Which stats this level system will modify</param>
    /// <param name="level">Starting level</param>
    /// <param name="experience">Starting experience</param>
    /// <param name="proficiency">Percentage modifier that applies to gained xp, instantiates a Stat object</param>
    public SkillLevel(Skill type, CharacterStats stats, int level = 1, int experience = 0, float proficiency = 0, WeaponType weapon_SkillType = WeaponType.None)
    {
        SkillType = type;
        Weapon_SkillType = weapon_SkillType;
        
        _statsReference = stats;
        Level = Math.Clamp(level, 0, MAX_LEVEL);
        Experience = Math.Clamp(experience, 0, ExpToNextLevel - 1);

        Proficiency = new Stat(proficiency, StatType.Proficiency, false);
    }

    public string GetSkillName()
    {
        if (SkillType == Skill.Equipment_Skill)
        {
            return $"{Weapon_SkillType} Mastery";
        }
        else
            return $"{SkillType}";
    }

    public void AddExperience(int amount)
    {
        if (this.Level >= MAX_LEVEL)
        {
            Debug.Log($"Player skill '{SkillType}' is already MAX level. No xp added.");
            return;
        }

        Experience += amount;

        while (Experience >= ExpToNextLevel)
        {
            Experience -= ExpToNextLevel;
            //updating the level also updates ExpToNextLevel
            LevelUp();
            
            //TODO: UI notify about the level up
            Debug.Log($"Player skill '{SkillType}' leveled up to lvl {Level}.");
        }
        
        OnExperienceChanged?.Invoke(this, EventArgs.Empty);
    }

    private void LevelUp()
    {
        //updating the level also updates ExpToNextLevel
        Level++;
        OnLevelChanged?.Invoke(this, EventArgs.Empty);

        //TODO: modify stats
        ModifyStatsOnLevelUp();
    }

    public float GetExperienceNormalized()
    {
        return (float)Experience / ExpToNextLevel;
    }

    //level up the skill by 'amount' levels
    public void AddLevels(int amount)
    {
        if (amount < 1)
            return;

        while (amount > 0)
        {
            LevelUp();
            amount--;
        }
    }

    /// <summary>
    /// Calculates the xp required to reach level: 'level' + 1
    /// </summary>
    /// <param name="level">Currently reached level</param>
    /// <returns></returns>
    private int GetRequiredExpToNextLevel(int level)
    {
        return (int)Math.Floor(level + AdditionMultiplier * MathF.Pow(PowerMultiplier, level / DivisionMultiplier)) / 4;
    }

    private void ModifyStatsOnLevelUp()
    {
        switch (SkillType)
        {
            case Skill.Strength:
                this._statsReference.PhysicalDamage.Grow();
                break;
            case Skill.Arts:
                this._statsReference.ArtsDamage.Grow();

                this._statsReference.MaxMana.GrowHalf();
                this._statsReference.ManaRecovery.GrowHalf();
                break;
            case Skill.Agility:
                this._statsReference.Speed.Grow();
                this._statsReference.DodgeChance.Grow();

                this._statsReference.EnergyRecovery.GrowHalf();
                break;
            case Skill.Constitution:
                this._statsReference.MaxHP.Grow();
                this._statsReference.MaxEnergy.Grow();

                this._statsReference.EnergyRecovery.GrowHalf();
                this._statsReference.MaxMana.GrowHalf();
                this._statsReference.ManaRecovery.GrowHalf();
                break;
            case Skill.Lockpicking:
                break;
            case Skill.Taming:
                LevelUp_Taming();
                break;
            case Skill.Trading:
                break;
            //All equipment proficiencies are handled here
            case Skill.Equipment_Skill:
                if (Weapon_SkillType == WeaponType.Shield)
                {
                    this._statsReference.BlockChance.Grow();
                }
                this._statsReference.WeaponProficiencies[this.Weapon_SkillType].Grow(Level);
                break;
            default:
                Debug.LogError("Tried to modify stats, but the skillType is unknown!");
                break;
        }
    }

    private void LevelUp_Taming()
    {
        //every 10 Taming levels +1 MaxPets count up to 8
        if (Level % 10 == 0)
        {
            int maxPets = (int)_statsReference.MaxPets + 1;
            Math.Clamp(maxPets, 1, 8);
            this._statsReference.MaxPets = maxPets;
        }
    }
}

