using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public class UnitData
{
    public Dictionary<UnitClass, UnitClassData> ClassDataDict = new Dictionary<UnitClass, UnitClassData>()
    {
        {
            UnitClass.Marksman,
            new UnitClassData(UnitClass.Marksman, 
                StatScaling.High, StatScaling.Low, StatScaling.Low, 0f, 
                StatScaling.High, isRanged: true)
        },
        {
            UnitClass.Mage,
            new UnitClassData(UnitClass.Mage,
                StatScaling.Low, StatScaling.Low, StatScaling.VeryLow, 0.5f,
                StatScaling.High, DamageType.Arts, true)
        },
        {
            UnitClass.Artillery,
            new UnitClassData(UnitClass.Artillery,
                StatScaling.Low, StatScaling.VeryHigh, StatScaling.Normal, 0f,
                StatScaling.High, isRanged: true)
        },
        {
            UnitClass.Controller,
            new UnitClassData(UnitClass.Controller,
                StatScaling.Low, StatScaling.Normal, StatScaling.Normal, 0.15f,
                StatScaling.Low)
        },
        {
            UnitClass.Bruiser,
            new UnitClassData(UnitClass.Bruiser,
                StatScaling.Normal, StatScaling.Normal, StatScaling.Normal, 0.10f,
                StatScaling.Normal)
        },
        {
            UnitClass.Battlemage,
            new UnitClassData(UnitClass.Battlemage,
                StatScaling.Normal, StatScaling.Normal, StatScaling.Normal, 0.40f,
                StatScaling.Normal, DamageType.Arts)
        },
        {
            UnitClass.Tank,
            new UnitClassData(UnitClass.Tank,
                StatScaling.Low, StatScaling.High, StatScaling.High, 0.05f,
                StatScaling.High)
        },
        {
            UnitClass.Titan,
            new UnitClassData(UnitClass.Titan,
                StatScaling.VeryLow, StatScaling.VeryHigh, StatScaling.VeryHigh, 0f,
                StatScaling.VeryHigh)
        },
        {
            UnitClass.Vanguard,
            new UnitClassData(UnitClass.Vanguard,
                StatScaling.High, StatScaling.Normal, StatScaling.Low, 0f,
                StatScaling.Low)
        },
        {
            UnitClass.Assassin,
            new UnitClassData(UnitClass.Assassin,
                StatScaling.High, StatScaling.Low, StatScaling.Low, 0f,
                StatScaling.High, isRanged: true, onlyUseSpecialSkills: true)
        },
        {
            UnitClass.Healer,
            new UnitClassData(UnitClass.Healer,
                StatScaling.Low, StatScaling.Low, StatScaling.VeryLow, 0.20f,
                StatScaling.Low, isRanged: true, onlyUseSpecialSkills: true)
        },
        {
            UnitClass.Warrior,
            new UnitClassData(UnitClass.Warrior,
                StatScaling.Normal, StatScaling.Normal, StatScaling.Normal, 0f,
                StatScaling.Normal)
        },
    };

    //bonus stats (in percentages)
    private readonly float EliteBonus = 0.25f;
    private readonly float BossBonus = 1f;

    //game difficulty modifiers
    public Dictionary<Difficulty, float> GameDiffScalingDict = new Dictionary<Difficulty, float>()
    {
        { Difficulty.Custom,   0f   },
        { Difficulty.Casual,  -0.2f },
        { Difficulty.Normal,   0    },
        { Difficulty.Hard,     0.2f },
    };

    //stat scaling modifiers
    public Dictionary<StatScaling, float> StatScalingDict = new Dictionary<StatScaling, float>()
    {
        { StatScaling.VeryLow,  -0.75f },
        { StatScaling.Low,      -0.5f  },
        { StatScaling.Normal,   0      },
        { StatScaling.High,     0.8f   },
        { StatScaling.VeryHigh, 1.5f   },
    };
    

    //per-stage modifier (enemy stats scale by stage)
    private readonly float PerStageMod = 0.02f;

    //base stat amounts
    private readonly float Damage = 15;
    private readonly float Health = 50;
    private readonly float Armor = 4;
    private readonly float Speed = 10;

    //multiply the speed ratio with this and Time.DeltaTime to get enemy energy increase per frame
    public float SpeedRatioMultiplier = 15;


    //loot drop parameters
    private float HumanoidEquipDropChance = 0.3f;

    //chance for common loot is 1 - (sum of chances for other rarities)
    private readonly float RarityChance_Uncommon  = 0.3f;    //cca. 30%
    private readonly float RarityChance_Rare      = 0.1f;    //cca. 10%
    private readonly float RarityChance_Epic      = 0.01f;   //cca. 1%
    private readonly float RarityChance_Legendary = 0.002f;  //cca. 0.2%


    public CharacterStats GetBaseStats(UnitClass @class, EnemyType enemyType, Difficulty gameDiff, ScriptableAdventureLocation locationData)
    {
        return GetEnemyStatsForClass(@class, CalculateStatModifier(enemyType, gameDiff, locationData));
    }

    public bool IsClassRanged(UnitClass @class)
    {
        return ClassDataDict[@class].IsRanged;
    }

    public bool CanOnlyUseSpecialAbilities(UnitClass @class)
    {
        return ClassDataDict[@class].OnlyUseSpecialSkills;
    }

    private float CalculateStatModifier(EnemyType enemyType, Difficulty gameDiff, ScriptableAdventureLocation location)
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
    private CharacterStats GetEnemyStatsForClass(UnitClass @class, float statModifier)
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

    private float GetScaledStatWithVariabilityAndMods(float baseAmnt,  StatScaling scaling, float modifier)
    {
        float res = baseAmnt;
        float mod = StatScalingDict[scaling];

        res += res * mod; //apply stat scaling (based on unit class)
        res += res * Random.Range(-0.05f, 0.05f); //add some variability
        res += res * modifier; //apply the modifiers (stage, game difficulty, etc.)

        return res;
    }

    public void SetupClassAbility(ScriptableAbility ability, ScriptableNpcUnit unit)
    {
        StatusEffect eff;

        switch (ability.Name)
        {
            case "Multi Attack":
                break;

            case "Buff Team Damage":
                break;

            case "Buff Team Defense":
                break;

            case "Debuff Enemy Damage":
                break;

            case "Debuff Enemy Defense":
                break;

            case "Bruiser Buff":
                eff = ability.OnActivedEffects.FirstOrDefault(x => x.EffectType == StatusEffectType.RegenerateHp);
                eff.EffectValue = unit.BaseStats.MaxHP.GetValue() / 50f;    //aka recover 2% of MaxHP every tick
                break;

            case "Shield Self":
                break;

            case "Heal All":
                eff = ability.OnActivedEffects.FirstOrDefault(x => x.EffectType == StatusEffectType.RegenerateHp);
                eff.EffectValue = unit.BaseStats.PhysicalDamage.GetValue();    //aka recover 100% of atk HP to every unit per tick
                break;

            default:
                break;
        }
    }

    public List<InventoryItem> GenerateDropsForUnit(ScriptableNpcUnit unit)
    {
        List<InventoryItem> res = new List<InventoryItem>();

        //add guaranteed drops if any
        if (unit.GuaranteedDrop != null)
            res.Add(new InventoryItem(unit.GuaranteedDrop));

        //roll for special drops
        if (unit.PossibleDropsList != null && unit.PossibleDropsList.Count > 0)
        {
            foreach (var possibleDrop in unit.PossibleDropsList)
            {
                if (Helper.DiceRoll(possibleDrop.Chance))
                {
                    res.Add(new InventoryItem(possibleDrop.Drop));
                }
            }
        }

        //roll the general pool based on unit race (if there are not already 3 drops or more)
        if (res.Count > 2)
            return res;

        res.Add(GetDropByUnitRace(unit.Race));


        return res;
    }

    private InventoryItem GetDropByUnitRace(UnitRace race)
    {
        InventoryItem res = null;

        switch (race)
        {
            case UnitRace.Animal:
                //can only drop hides
                res = RollDropByItemType(ItemType.Loot);
                break;

            case UnitRace.Humanoid:
                //drops hides & can rarely drop equip
                var lootType = Helper.DiceRoll(HumanoidEquipDropChance) ? ItemType.Equipment : ItemType.Loot;
                res = RollDropByItemType(lootType);
                break;

            case UnitRace.Human:
                //can only drop equipment and currency
                break;

            case UnitRace.Monster:
                break;
            default:
                break;
        }

        return res;
    }

    private InventoryItem RollDropByItemType(ItemType itemType)
    {
        var itemData = ResourceSystem.Instance.GetRandomItemByType(itemType, RollItemRarity());
        
        return new InventoryItem(itemData);
    }

    public ItemRarity RollItemRarity()
    {
        if (Helper.DiceRoll(RarityChance_Legendary))
            return ItemRarity.Legendary;

        if (Helper.DiceRoll(RarityChance_Epic))
            return ItemRarity.Epic;

        if (Helper.DiceRoll(RarityChance_Rare))
            return ItemRarity.Rare;

        if (Helper.DiceRoll(RarityChance_Uncommon))
            return ItemRarity.Uncommon;

        return ItemRarity.Common;
    }

}
