using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// One repository for all scriptable objects. Create your query methods here to keep your business logic clean.
/// Always instantiate ScriptableObjects when retrieving them, because sometimes the modified data gets 
///     saved to the actual ScriptableObject file??? cringe
/// </summary>
public class ResourceSystem : Singleton<ResourceSystem> {
    private List<ScriptableHero> Heroes { get; set; }
    private Dictionary<string, ScriptableHero> _HeroesDict;

    private List<ScriptableAbility> PlayerAbilities { get; set; }
    private List<ScriptableStatusEffect> StatusEffects { get; set; }

    private List<ScriptableBackground> HeroBackgrounds { get; set; }
    private Dictionary<string, ScriptableBackground> _HeroBackgroundsDict;

    private List<ScriptableAdventureLocation> AdventureLocations { get; set; }

    private Dictionary<string, ScriptableAdventureLocation> _AdventureLocationsDict;

    private List<ScriptableNpcUnit> CommonEnemies { get; set; }

    public List<ItemDataBase> Items_Other { get; private set; }
    public List<ItemDataEquipment> Items_Equipment { get; private set; }


    /// <summary>
    /// To add icons: slice up sprite sheet -> right click-> create -> textmeshpro -> sprite asset -> 
    ///     project settings TMP set as default sprite asset. In the asset click "Update sprite asset"
    /// </summary>
    private static Dictionary<Icon, string> TMP_IconDict = new Dictionary<Icon, string>()
    {
        { Icon.Everstone,    "everstone"    },
        { Icon.Infinity,     "infinity"     },

        { Icon.Attack_Phys, "attack_phys"   },
        { Icon.Attack_Arts, "attack_arts"   },
        { Icon.Defense,     "defense"       },
        { Icon.Arts_Resist, "resist"        },
        { Icon.Speed,       "speed"         },
        { Icon.Health,      "health"        },
        { Icon.Stamina,     "energy"        },
        { Icon.Energy_Regen,"energy_regen"  },
        { Icon.Health_Regen,"health_regen"  },
        { Icon.Mana,        "mana"          },
        { Icon.Mana_Regen,  "mana_regen"    },
        { Icon.Strength,    "strength"      },
        { Icon.Block_Chance,"block_chance"  },
        { Icon.Dodge,       "dodge"         },
        { Icon.Agility,     "agility"       },
        { Icon.Cooldown,    "cooldown"      },
        { Icon.Taming,      "taming"        },
        { Icon.Lockpicking, "lockpicking"   },
        { Icon.Trading,     "trading"       },
        { Icon.Electricity, "electricity"   },
        { Icon.Accuracy,    "accuracy"      },
        { Icon.Capacity,    "capacity"      },
        { Icon.Poison,      "poison"        },
        { Icon.Luck,        "luck"          },
        { Icon.Coin_Copper, "coin_copper"   },
        { Icon.Coin_Silver, "coin_silver"   },
        { Icon.Coin_Gold,   "coin_gold"     },
        { Icon.Fire,        "fire"          },
    };


    #region STATIC METHODS


    public static string GetStatIconTag(StatType statType)
    {
        switch (statType)
        {
            case StatType.PhysicalDamage:
                return GetIconTag(Icon.Attack_Phys);
            case StatType.Armor:
                return GetIconTag(Icon.Defense);
            case StatType.ArtsDamage:
                return GetIconTag(Icon.Attack_Arts);
            case StatType.ArtsResist:
                return GetIconTag(Icon.Arts_Resist);
            case StatType.MaxHP:
                return GetIconTag(Icon.Health);
            case StatType.MaxEnergy:
                return GetIconTag(Icon.Stamina);
            case StatType.EnergyRecovery:
                return GetIconTag(Icon.Energy_Regen);
            case StatType.MaxMana:
                return GetIconTag(Icon.Mana);
            case StatType.ManaRecovery:
                return GetIconTag(Icon.Mana_Regen);
            case StatType.CooldownReduction:
                return GetIconTag(Icon.Cooldown);
            case StatType.Speed:
                return GetIconTag(Icon.Speed);
            case StatType.DodgeChance:
                return GetIconTag(Icon.Dodge);
            case StatType.HealEfficiency:
                return GetIconTag(Icon.Health_Regen);
            case StatType.BlockChance:
                return GetIconTag(Icon.Block_Chance);
            
            case StatType.WeaponAccuracy:
            case StatType.Proficiency:
            default:
                return "";
        }
    }

    public static string GetSkillIconTag(Skill skill)
    {
        switch (skill)
        {
            case Skill.Strength:
                return GetIconTag(Icon.Strength);

            case Skill.Arts:
                return GetIconTag(Icon.Attack_Arts);
                
            case Skill.Agility:
                return GetIconTag(Icon.Agility);
                
            case Skill.Constitution:
                return GetIconTag(Icon.Health);
                
            case Skill.Lockpicking:
                return GetIconTag(Icon.Lockpicking);
                
            case Skill.Taming:
                return GetIconTag(Icon.Taming);
                
            case Skill.Trading:
                return GetIconTag(Icon.Trading);
                
            case Skill.Equipment_Skill:
            default:
                return "";
        }
    }

    public static string GetIconTag(Icon icon) => $"<sprite name=\"{TMP_IconDict[icon]}\">";

    public static Color GetRarityColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common:
                return Color.white;

            case ItemRarity.Uncommon:
                return new Color(50 / 205f, 255 / 255f, 50 / 255f); //lime green
                //return Color.green;

            case ItemRarity.Rare:
                return new Color(0/255f, 150/255f, 255/255f); //light blue

            case ItemRarity.Epic:
                return new Color(191/255f, 64/255f, 191/255f); //light purple

            case ItemRarity.Legendary:
                return Color.red;

            case ItemRarity.Quest:
                return Color.yellow;

            case ItemRarity.None:
            default:
                return Color.gray;
        }
    }


    #endregion STATIC METHODS


    private void Start()
    {
        AssembleResources();
    }


    private void AssembleResources() {
        Heroes = Resources.LoadAll<ScriptableHero>("Heroes/Classes").ToList();
        _HeroesDict = Heroes.ToDictionary(r => r.ClassName, r => r);
        
        HeroBackgrounds = Resources.LoadAll<ScriptableBackground>("Heroes/Backgrounds").ToList();
        _HeroBackgroundsDict = HeroBackgrounds.ToDictionary(x => x.backgroundName, x => x);


        StatusEffects = Resources.LoadAll<ScriptableStatusEffect>("Heroes/Abilities/StatusEffects").ToList();
        PlayerAbilities = Resources.LoadAll<ScriptableAbility>("Heroes/Abilities").ToList();


        var locationData = Resources.LoadAll<ScriptableAdventureLocation>("Locations/Adventure").ToList();
        AdventureLocations = new List<ScriptableAdventureLocation>();
       
        foreach (var location in locationData.OrderBy(x => x.name).OrderBy(x => x.difficulty))
            AdventureLocations.Add(Instantiate(location));
            
        _AdventureLocationsDict = AdventureLocations.ToDictionary(x => x.locationName, x => x);


        //load enemies
        foreach (var location in AdventureLocations)
        {
            location.SetEnemyPool(Resources.LoadAll<ScriptableNpcUnit>($"Enemies/{location.locationName}").ToList());
        }

        CommonEnemies = Resources.LoadAll<ScriptableNpcUnit>("Enemies/_Common").ToList();

        //load items
        Items_Equipment = Resources.LoadAll<ItemDataEquipment>("Items/Equipment").ToList();
        Items_Other     = Resources.LoadAll<ItemDataBase>("Items/Other").ToList();
    }

  
    public ScriptableHero GetHero(string t) => Instantiate(_HeroesDict[t]);
    public List<string> GetHeroClasses() => _HeroesDict.Keys.OrderBy(x => x).ToList();
    //public ScriptableHero GetRandomHero() => Instantiate(Heroes[Random.Range(0, Heroes.Count)]);

    public ScriptableBackground GetBackground(string t) => Instantiate(_HeroBackgroundsDict[t]);
    //public ScriptableBackground GetRandomBackground() => HeroBackgrounds[Random.Range(0, HeroBackgrounds.Count)];
    public List<string> GetHeroBackgrounds()
    {
        var names = new List<string>();
        
        //add the "None" background first
        var BGnone = HeroBackgrounds.FirstOrDefault(x => x.backgroundName.ToLower() == "none");
        if (BGnone != null)
            names.Add(BGnone.backgroundName);

        //add the other backgrounds after
        var BGother = _HeroBackgroundsDict.Keys.OrderBy(x => x).ToList();
        BGother.RemoveAll(x => x.ToLower() == "none");
        names.AddRange(BGother);

        return names;
    }

    public List<ScriptableAbility> GetPlayerAbilities()
    {
        var res = new List<ScriptableAbility>();

        foreach (var ability in PlayerAbilities)
            res.Add(Instantiate(ability));

        return res;
    }

    public List<ScriptableAdventureLocation> GetAdventureLocations()
    {
        return AdventureLocations.OrderBy(x => x.name).OrderBy(x => x.difficulty).ToList();
    }

    public ScriptableStatusEffect GetStatusEffect(StatusEffectType effect)
    {
        return Instantiate(StatusEffects.FirstOrDefault(x => x.Effect == effect));
    }
}   
