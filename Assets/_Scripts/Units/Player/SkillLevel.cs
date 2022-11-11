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
    public static readonly int MAX_LEVEL = 99;
    public static readonly int STARTING_LEVEL = 1;

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
    public bool IsWeaponSkill { get; private set; }

    protected CharacterStats _statsReference;

    protected int _level = STARTING_LEVEL;
    public int Level
    {
        get { return _level; }
        set
        {
            int prevValue = _level;

            if (value <= _level)
                return;

            int tempLevel = _level;
            _level = value;

            //handle jumping multiple levels cause its possible to start
            //  with level higher than 1
            while (tempLevel < _level)
            {
                ModifyStatsOnLevelUp();
                tempLevel++;
            }

            ExpToNextLevel = GetRequiredExpToNextLevel(_level);

            OnLevelChanged?.Invoke(prevValue, _level, this);
        }
    }
    public int Experience { get; set; }
    public int ExpToNextLevel { get; set; }

    /// <summary>
    /// Modifies bonus exp, aka. how fast the character will level up
    ///     value 0.22 is 22% xp bonus
    /// </summary>
    public Stat Proficiency { get; private set; }

    #endregion PROPERTIES

    /// <summary>
    /// Invoked when the skill levels up.
    /// Parameters: oldLevel, newLevel
    /// </summary>
    public event Action<int, int, SkillLevel> OnLevelChanged;

    /// <param name="type">Which skill this level system is for</param>
    /// <param name="type">Which stats this level system will modify</param>
    /// <param name="level">Starting level</param>
    /// <param name="experience">Starting experience</param>
    /// <param name="proficiency">Percentage modifier that applies to gained xp, instantiates a Stat object</param>
    public SkillLevel(Skill type, CharacterStats stats, int level = 1, int experience = 0, 
                            float proficiency = 0, bool isWeaponSkill = false)
    {
        SkillType = type;
        IsWeaponSkill = isWeaponSkill;
        
        _statsReference = stats;
        Proficiency = new Stat(proficiency, StatType.Proficiency, true);

        if (IsWeaponSkill) //assign level in child class
            return;

        Level = Math.Clamp(level, 1, MAX_LEVEL);
        ExpToNextLevel = GetRequiredExpToNextLevel(Level);
        Experience = Math.Clamp(experience, 0, ExpToNextLevel - 1);
    }


    #region STATIC METHODS



    #endregion STATIC METHODS



    #region METHODS


    public virtual string GetSkillName()
    {
        return $"{SkillType}";
    }

    public void AddExperience(int amount)
    {
        if (this.Level >= MAX_LEVEL)
        {
            Debug.Log($"Player skill '{SkillType}' is already MAX level. No xp added.");
            return;
        }

        //proficiency modification
        amount += (int)(amount * Proficiency.GetValue());

        Experience += amount;

        while (Experience >= ExpToNextLevel)
        {
            Experience -= ExpToNextLevel;
            //updating the level also updates ExpToNextLevel
            LevelUp();
        }
    }

    protected void LevelUp()
    {
        Debug.Log($"Player skill '{SkillType}' leveled up to lvl {Level}.");

        //updating the level also updates ExpToNextLevel
        Level++;
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
    protected int GetRequiredExpToNextLevel(int level)
    {
        return (int)Math.Floor(level + AdditionMultiplier * MathF.Pow(PowerMultiplier, level / DivisionMultiplier)) / 4;
    }

    protected virtual void ModifyStatsOnLevelUp()
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

                this._statsReference.MaxMana.GrowHalf();
                this._statsReference.ManaRecovery.GrowHalf();
                break;

            case Skill.Lockpicking:
                LevelUp_Lockpicking();
                break;

            case Skill.Taming:
                LevelUp_Taming();
                break;

            case Skill.Trading:
                LevelUp_Trading();
                break;

            //All equipment proficiencies are handled in child class
            case Skill.Equipment_Skill:
                Debug.LogError("Tried to modify Equipment_Skill type on SkillLevel object!");
                break;

            default:
                Debug.LogError("Tried to modify stats, but the skillType is unknown!");
                break;
        }
    }

    private void LevelUp_Lockpicking()
    {
        throw new NotImplementedException();
    }

    private void LevelUp_Trading()
    {
        var playerMng = GameManager.Instance.PlayerManager;

        playerMng.SellPriceModifier += playerMng.SellPriceModPerLevelIncrease;
        playerMng.SellPriceModifier_Loot += playerMng.SellPriceModPerLevelIncrease;
        playerMng.ShopItemAmount++;
    }

    private void LevelUp_Taming()
    {
        var playerMng = GameManager.Instance.PlayerManager;

        //every MoreMaxPetsEveryNLevels Taming levels +1 MaxPets count up to 5
        if (Level % playerMng.MoreMaxPetsPerNLevels == 0)
        {
            playerMng.MaxPets++;
        }

        playerMng.PetXpBonus += playerMng.PetXpBonusPerLevel;
    }

    public string GetSkillDescription()
    {
        switch (this.SkillType)
        {
            case Skill.Strength:
                return $"Improves: {Stat.GetDisplayName(StatType.PhysicalDamage)}\n\nGain XP by: Dealing {Stat.GetDisplayName(StatType.PhysicalDamage)}.";

            case Skill.Arts:
                return $"Improves: {Stat.GetDisplayName(StatType.ArtsDamage)}\n\nAlso affects: {Stat.GetDisplayName(StatType.MaxMana)}, {Stat.GetDisplayName(StatType.ManaRecovery)}\n\nGain XP by: Dealing {Stat.GetDisplayName(StatType.ArtsDamage)}.";

            case Skill.Agility:
                return $"Improves: {Stat.GetDisplayName(StatType.Speed)}, {Stat.GetDisplayName(StatType.DodgeChance)}\n\nAlso affects: {Stat.GetDisplayName(StatType.EnergyRecovery)}\n\nGain XP by: Dodging, especially with perfect dodges.";

            case Skill.Constitution:
                return $"Improves: {Stat.GetDisplayName(StatType.MaxHP)}, {Stat.GetDisplayName(StatType.MaxEnergy)}\n\nAlso affects: {Stat.GetDisplayName(StatType.MaxMana)}, {Stat.GetDisplayName(StatType.ManaRecovery)}\n\nGain XP by: Taking damage.";

            case Skill.Lockpicking:
                return $"Makes the lockpicking minigame easier.\n\nGain XP by: Successfully completing the lockpicking minigame.";

            case Skill.Taming:
                return $"Improves: The rate of pet experience gain.\nAlso affects: Max amount of pets you can take into battle.\n\nGain XP by: Having pets deal damage.";

            case Skill.Trading:
                return $"Improves: For how much you can sell items and loot to the shop.\n\nGain XP by: Selling items.";

            case Skill.Equipment_Skill:
            default:
                break;
        }

        return string.Empty;
    }

    public string GetSkillDifferencesPerLevel()
    {
        string res = "Current / Next lvl.\n";
        
        var plrMng = GameManager.Instance.PlayerManager;
        
        var stat_physDmg    = _statsReference.GetStatFromStatType(StatType.PhysicalDamage);
        var stat_artDmg     = _statsReference.GetStatFromStatType(StatType.ArtsDamage);
        var stat_maxMana    = _statsReference.GetStatFromStatType(StatType.MaxMana);
        var stat_maxHp      = _statsReference.GetStatFromStatType(StatType.MaxHP);
        var stat_maxEnergy  = _statsReference.GetStatFromStatType(StatType.MaxEnergy);
        var stat_manaRec    = _statsReference.GetStatFromStatType(StatType.ManaRecovery);
        var stat_energyRec  = _statsReference.GetStatFromStatType(StatType.EnergyRecovery);
        var stat_speed      = _statsReference.GetStatFromStatType(StatType.Speed);
        var stat_dodge      = _statsReference.GetStatFromStatType(StatType.DodgeChance);

        switch (SkillType)
        {
            case Skill.Strength:
                res += Environment.NewLine;
                res += stat_physDmg.GetLevelDiffDisplayValue(false);
                break;

            case Skill.Arts:
                res += Environment.NewLine;
                res += stat_artDmg.GetLevelDiffDisplayValue(false);
                res += Environment.NewLine;
                res += stat_maxMana.GetLevelDiffDisplayValue(true);
                res += Environment.NewLine;
                res += stat_manaRec.GetLevelDiffDisplayValue(true);
                break;

            case Skill.Agility:
                res += Environment.NewLine;
                res += stat_speed.GetLevelDiffDisplayValue(false);
                res += Environment.NewLine;
                res += stat_dodge.GetLevelDiffDisplayValue(false);
                res += Environment.NewLine;
                res += stat_energyRec.GetLevelDiffDisplayValue(true);
                break;

            case Skill.Constitution:
                res += Environment.NewLine;
                res += stat_maxHp.GetLevelDiffDisplayValue(false);
                res += Environment.NewLine;
                res += stat_maxEnergy.GetLevelDiffDisplayValue(false);
                res += Environment.NewLine;
                res += stat_maxMana.GetLevelDiffDisplayValue(true);
                res += Environment.NewLine;
                res += stat_manaRec.GetLevelDiffDisplayValue(true);
                break;

            case Skill.Lockpicking:
                res = $"Minigame difficulty is decided based on chest rarity and game difficulty.\nThe greater the difference between your Lockpicking level and the chest difficulty level, the more/less time you will have to solve the minigame.";
                break;

            case Skill.Taming:
                res += Environment.NewLine;
                res += $"Pet xp bonus: {plrMng.PetXpBonus * 100f}% / {(plrMng.PetXpBonus + plrMng.PetXpBonusPerLevel) * 100f}%";
                res += Environment.NewLine;
                res += $"Max pets: {plrMng.MaxPets} / {((this.Level + 1) % plrMng.MoreMaxPetsPerNLevels == 0 ? plrMng.MaxPets + 1 : plrMng.MaxPets)}";
                break;

            case Skill.Trading:
                float nextLvlSellMod = plrMng.SellPriceModifier + plrMng.SellPriceModPerLevelIncrease;
                if (nextLvlSellMod > 0.99f)
                    nextLvlSellMod = 0.99f;

                res += Environment.NewLine;
                res += $"Sell modifier: {plrMng.SellPriceModifier} / {nextLvlSellMod}";
                res += Environment.NewLine;
                res += $"Sell modifier (loot): {plrMng.SellPriceModifier_Loot} / {plrMng.SellPriceModifier_Loot + plrMng.SellPriceModPerLevelIncrease}";
                break;

            case Skill.Equipment_Skill:
            default:
                break;
        }

        return res;
    }


    #endregion METHODS

}

