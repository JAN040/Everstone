using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor.VersionControl;
using UnityEngine;

public static class UnitData
{
    public static Dictionary<UnitClass, UnitClassData> ClassDataDict = new Dictionary<UnitClass, UnitClassData>()
    {
        {
            UnitClass.Marksman,
            new UnitClassData(UnitClass.Marksman, null,
                StatScaling.High, StatScaling.Low, StatScaling.Low, 0, 
                StatScaling.High, isRanged: true)
        },
        {
            UnitClass.Mage,
            new UnitClassData(UnitClass.Mage, new List<SpecialSkill>(){ SpecialSkill.SelfBuff, SpecialSkill.SelfHeal },
                StatScaling.Low, StatScaling.Low, StatScaling.VeryLow, 50,
                StatScaling.High, DamageType.Arts, true)
        },
        {
            UnitClass.Artillery,
            new UnitClassData(UnitClass.Artillery, new List<SpecialSkill>(){ SpecialSkill.AoeAtk},
                StatScaling.Low, StatScaling.VeryHigh, StatScaling.Normal, 0,
                StatScaling.High, isRanged: true)
        },
        {
            UnitClass.Controller,
            new UnitClassData(UnitClass.Controller, new List<SpecialSkill>(){ SpecialSkill.CC, SpecialSkill.OpponentTeamDebuff, SpecialSkill.TeamBuff},
                StatScaling.Low, StatScaling.Normal, StatScaling.Normal, 15,
                StatScaling.Low)
        },
        {
            UnitClass.Bruiser,
            new UnitClassData(UnitClass.Bruiser, new List<SpecialSkill>(){ SpecialSkill.SelfHeal},
                StatScaling.Normal, StatScaling.Normal, StatScaling.Normal, 10,
                StatScaling.Normal)
        },
        {
            UnitClass.Battlemage,
            new UnitClassData(UnitClass.Battlemage, new List<SpecialSkill>(){ SpecialSkill.OpponentTeamDebuff},
                StatScaling.Normal, StatScaling.Normal, StatScaling.Normal, 40,
                StatScaling.Normal, DamageType.Arts)
        },
        {
            UnitClass.Tank,
            new UnitClassData(UnitClass.Tank, null,
                StatScaling.Low, StatScaling.High, StatScaling.High, 5,
                StatScaling.Low)
        },
        {
            UnitClass.Titan,
            new UnitClassData(UnitClass.Titan, new List<SpecialSkill>(){ SpecialSkill.CC},
                StatScaling.VeryLow, StatScaling.VeryHigh, StatScaling.VeryHigh, 0,
                StatScaling.High)
        },
        {
            UnitClass.Vanguard,
            new UnitClassData(UnitClass.Vanguard, null,
                StatScaling.High, StatScaling.Normal, StatScaling.Low, 0,
                StatScaling.Low)
        },
        {
            UnitClass.Assassin,
            new UnitClassData(UnitClass.Assassin, new List<SpecialSkill>(){ SpecialSkill.SelfBuff},
                StatScaling.High, StatScaling.Low, StatScaling.Low, 0,
                StatScaling.High, isRanged: true, onlyUseSpecialSkills: true)
        },
        {
            UnitClass.Healer,
            new UnitClassData(UnitClass.Healer, new List<SpecialSkill>(){ SpecialSkill.TeamHeal},
                StatScaling.Low, StatScaling.Low, StatScaling.VeryLow, 20,
                StatScaling.Low, isRanged: true, onlyUseSpecialSkills: true)
        },
        {
            UnitClass.Warrior,
            new UnitClassData(UnitClass.Warrior, null,
                StatScaling.Normal, StatScaling.Normal, StatScaling.Normal, 0,
                StatScaling.Normal)
        },
    };

    //bonus stats (in percentages)
    private static readonly float EliteBonus = 0.25f;
    private static readonly float BossBonus = 1f;

    //game difficulty modifiers
    public static Dictionary<Difficulty, float> GameDiffScalingDict = new Dictionary<Difficulty, float>()
    {
        { Difficulty.Custom,   0f   },
        { Difficulty.Casual,  -0.2f },
        { Difficulty.Normal,   0    },
        { Difficulty.Hard,     0.2f },
    };

    //stat scaling modifiers
    public static Dictionary<StatScaling, float> StatScalingDict = new Dictionary<StatScaling, float>()
    {
        { StatScaling.VeryLow,  -0.75f },
        { StatScaling.Low,      -0.5f  },
        { StatScaling.Normal,   0      },
        { StatScaling.High,     0.8f   },
        { StatScaling.VeryHigh, 1.5f   },
    };
    

    //per-stage modifier (enemy stats scale by stage)
    private static readonly float PerStageMod = 0.02f;

    //base stat amounts
    private static readonly float Damage = 10;
    private static readonly float Health = 50;
    private static readonly float Armor = 4;
    private static readonly float Speed = 10;


    public static CharacterStats GetBaseStats(UnitClass unitClass, EnemyType enemyType, Difficulty gameDiff, ScriptableAdventureLocation locationData)
    {
        return GetEnemyStatsForClass(unitClass, CalculateStatModifier(enemyType, gameDiff, locationData));
    }

    private static float CalculateStatModifier(EnemyType enemyType, Difficulty gameDiff, ScriptableAdventureLocation location)
    {
        float res = 0f;

        switch (enemyType)
        {
            case EnemyType.Elite:
                res += EliteBonus;
                break;
            case EnemyType.Boss:
                res += BossBonus;
                break;
            default:
                break;
        }

        res += GameDiffScalingDict[gameDiff];

        res += (float)location.difficulty / 10f;

        res += location.PlayerProgress * PerStageMod;

        return res;
    }

    /// <param name="statModifier">Based on location & game difficulties</param>
    private static CharacterStats GetEnemyStatsForClass(UnitClass @class, float statModifier)
    {
        UnitClassData classData = ClassDataDict[@class];

        float damage = GetScaledStatWithVariabilityAndMods(Damage, classData.Damage, statModifier);
        float health = GetScaledStatWithVariabilityAndMods(Health, classData.Constitution, statModifier);
        float speed = GetScaledStatWithVariabilityAndMods(Speed, classData.Speed, statModifier);
        float armor = GetScaledStatWithVariabilityAndMods(Armor, classData.Armor, statModifier);

        if (classData.DamageType == DamageType.Physical)
        {
            return new CharacterStats(damage, 0, health, armor,
                                      classData.ArtsResist, 100, speed, 0);
        }
        else
        {
            return new CharacterStats(0, damage, health, armor,
                                       classData.ArtsResist, 100, speed, 0);
        }
    }

    private static float GetScaledStatWithVariabilityAndMods(float baseAmnt,  StatScaling scaling, float modifier)
    {
        float res = baseAmnt;
        float mod = StatScalingDict[scaling];

        res += res * mod; //apply stat scaling (based on unit class)
        res += res * Random.Range(-0.05f, 0.05f); //add some variability
        res += res * modifier; //apply the modifiers (stage, game difficulty, etc.)

        return res;
    }
}
