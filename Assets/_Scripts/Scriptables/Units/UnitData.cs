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
    private readonly float BossBonus = 3f;

    //game difficulty modifiers
    public Dictionary<Difficulty, float> GameDiffScalingDict = new Dictionary<Difficulty, float>()
    {
        { Difficulty.Custom,   0f   },
        { Difficulty.Easy,  -0.2f },
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



    #region Loot drop parameters


    private float HumanoidEquipDropChance = 0.3f;

    //chance for common loot is 1 - (sum of chances for other rarities)
    private readonly float RarityChance_Uncommon = 0.3f;    //cca. 30%
    private readonly float RarityChance_Rare = 0.1f;    //cca. 10%
    private readonly float RarityChance_Epic = 0.01f;   //cca. 1%
    private readonly float RarityChance_Legendary = 0.002f;  //cca. 0.2%

    private readonly float LocationDiffChanceQuotient = 1000;

    private readonly int CurrencyMinDrop = 1;
    private readonly int CurrencyMaxDrop = 1000;


    #endregion Loot drop parameters


    public CharacterStats GetBaseStats(UnitClass @class, EnemyType enemyType, Difficulty gameDiff, ScriptableAdventureLocation locationData)
    {
        return GetEnemyStatsForClass(@class, CalculateStatModifier(enemyType, gameDiff, locationData), enemyType);
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
    private CharacterStats GetEnemyStatsForClass(UnitClass @class, float statModifier, EnemyType enemyType)
    {
        UnitClassData classData = ClassDataDict[@class];

        float damage = GetScaledStatWithVariabilityAndMods(Damage, classData.Damage, statModifier);
        float health = GetScaledStatWithVariabilityAndMods(Health, classData.Constitution, statModifier);
        float speed = GetScaledStatWithVariabilityAndMods(Speed, classData.Speed, statModifier);
        float armor = GetScaledStatWithVariabilityAndMods(Armor, classData.Armor, statModifier);

        if (enemyType == EnemyType.Elite)
            health *= 2;
        else if (enemyType == EnemyType.Boss)
            health *= 5;

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
                eff.EffectValue = unit.Stats.MaxHP.GetValue() / 50f;    //aka recover 2% of MaxHP every tick
                break;

            case "Shield Self":
                break;

            case "Heal All":
                eff = ability.OnActivedEffects.FirstOrDefault(x => x.EffectType == StatusEffectType.RegenerateHp);
                eff.EffectValue = unit.Stats.PhysicalDamage.GetValue() / 2f;    //aka recover 50% of atk HP to every unit per tick
                break;

            default:
                break;
        }
    }

    public List<InventoryItem> GenerateDropsForUnit(ScriptableNpcUnit unit)
    {
        List<InventoryItem> res = new List<InventoryItem>();

        if (unit == null) return res;

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
                //res = RollDropByItemType(ItemType.Loot);
                break;

            case UnitRace.Humanoid:
                //can rarely drop equip
                if(Helper.DiceRoll(HumanoidEquipDropChance))
                    res = RollDropByItemType(ItemType.Equipment);
                break;

            case UnitRace.Human:
                //can only drop equipment and currency
                if(Helper.DiceRoll(HumanoidEquipDropChance))
                    res = RollDropByItemType(ItemType.Equipment);
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
        ItemDataBase itemData;
        var adventureLocation = GameManager.Instance.CurrentAdventureLocation;
        var locDifficulty = adventureLocation != null ? adventureLocation.difficulty : LocationDifficulty.Easy;

        if (itemType == ItemType.Currency)
        {
            itemData = ResourceSystem.Instance.GetCurrencyItem();
            var item = new InventoryItem(itemData);
            item.StackSize = RollCurrencyAmount(locDifficulty);

            return item;
        }

        itemData = ResourceSystem.Instance.GetRandomItemByType(itemType, RollItemRarity(locDifficulty));
        
        return new InventoryItem(itemData);
    }

    private int RollCurrencyAmount(LocationDifficulty locationDifficulty)
    {
        int amount = Random.Range(CurrencyMinDrop, CurrencyMaxDrop);
        var rarity = RollItemRarity(locationDifficulty);

        switch (rarity)
        {
            case ItemRarity.Uncommon:
                amount *= 2;
                break;

            case ItemRarity.Rare:
                amount *= 4;
                break;

            case ItemRarity.Epic:
                amount *= 8;
                break;

            case ItemRarity.Legendary:
                amount *= 16;
                break;

            case ItemRarity.Common:
            case ItemRarity.None:
            case ItemRarity.Quest:
            default:
                break;
        }

        return amount;
    }

    public ItemRarity RollItemRarity(LocationDifficulty difficulty)
    {
        float chanceMod = 0;
        chanceMod = GetChanceModifier(difficulty);

        if (Helper.DiceRoll(RarityChance_Legendary + chanceMod))
            return ItemRarity.Legendary;

        if (Helper.DiceRoll(RarityChance_Epic      + chanceMod * 2))
            return ItemRarity.Epic;

        if (Helper.DiceRoll(RarityChance_Rare      + chanceMod * 4))
            return ItemRarity.Rare;

        if (Helper.DiceRoll(RarityChance_Uncommon  + chanceMod * 8))
            return ItemRarity.Uncommon;

        return ItemRarity.Common;
    }

    private float GetChanceModifier(LocationDifficulty difficulty)
    {
        return (int)difficulty / LocationDiffChanceQuotient;
    }
}
