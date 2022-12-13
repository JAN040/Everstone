using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D;

/// <summary>
/// One repository for all scriptable objects. Create your query methods here to keep your business logic clean.
/// Always instantiate ScriptableObjects when retrieving them, because sometimes the modified data gets 
///     saved to the actual ScriptableObject file??? cringe
/// </summary>
public class ResourceSystem : Singleton<ResourceSystem> {

    [SerializeField] SpriteAtlas icon_sprite_atlas;
    [SerializeField] Sprite CurrencyItemSprite;


    private List<ScriptableHero> Heroes { get; set; }
    private Dictionary<string, ScriptableHero> _HeroesDict;

    private List<HeroPortrait> HeroPortraits { get; set; } 

    private List<ScriptableAbility> PlayerAbilities { get; set; }
    private List<ScriptableStatusEffect> StatusEffects { get; set; }

    /// <summary>
    /// Abilities mostly used by special enemy classes like bruiser, controller, etc.
    /// </summary>
    private List<ScriptableAbility> UnitClassAbilities { get; set; }

    private List<ScriptableBackground> HeroBackgrounds { get; set; }
    private Dictionary<string, ScriptableBackground> _HeroBackgroundsDict;

    private List<ScriptableAdventureLocation> AdventureLocations { get; set; }

    private Dictionary<string, ScriptableAdventureLocation> _AdventureLocationsDict;

    private List<ScriptableNpcUnit> CommonEnemies { get; set; }

    public List<ItemDataBase> Items_Loot { get; private set; }
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
        { Icon.Buff,        "buff"          },
        { Icon.Debuff,      "debuff"        },
    };



    #region STATIC METHODS


    public static Sprite GetStatIconImage(StatType statType)
    {
        if (Instance == null)
            return null;

        return Instance.icon_sprite_atlas.GetSprite(TMP_IconDict[GetStatIcon(statType)]);
    }

    public static string GetStatIconTag(StatType statType)
    {
        return GetIconTag(GetStatIcon(statType));
    }

    public ScriptableAbility GetAbilityByName(string name)
    {
        foreach (var ability in PlayerAbilities)
        {
            if (ability.Name.Equals(name))
                return Instantiate(ability);
        }

        return null;
    }

    private static Icon GetStatIcon(StatType statType)
    {
        switch (statType)
        {
            case StatType.PhysicalDamage:
                return Icon.Attack_Phys;
            case StatType.Armor:
                return Icon.Defense;
            case StatType.ArtsDamage:
                return Icon.Attack_Arts;
            case StatType.ArtsResist:
                return Icon.Arts_Resist;
            case StatType.MaxHP:
                return Icon.Health;
            case StatType.MaxEnergy:
                return Icon.Stamina;
            case StatType.EnergyRecovery:
                return Icon.Energy_Regen;
            case StatType.MaxMana:
                return Icon.Mana;
            case StatType.ManaRecovery:
                return Icon.Mana_Regen;
            case StatType.CooldownReduction:
                return Icon.Cooldown;
            case StatType.Speed:
                return Icon.Speed;
            case StatType.DodgeChance:
                return Icon.Dodge;
            case StatType.HealEfficiency:
                return Icon.Health_Regen;
            case StatType.BlockChance:
                return Icon.Block_Chance;

            case StatType.WeaponAccuracy:
            case StatType.Proficiency:
            default:
                Debug.LogWarning($"Unexpected skill enum: {statType}");
                return Icon.Everstone;
        }
    }

    public static string GetSkillIconTag(Skill skill)
    {
        return GetIconTag(GetSkillIcon(skill));
    }

    private static Icon GetSkillIcon(Skill skill)
    {
        switch (skill)
        {
            case Skill.Strength:
                return Icon.Strength;

            case Skill.Arts:
                return Icon.Attack_Arts;

            case Skill.Agility:
                return Icon.Agility;

            case Skill.Constitution:
                return Icon.Health;

            case Skill.Lockpicking:
                return Icon.Lockpicking;

            case Skill.Taming:
                return Icon.Taming;

            case Skill.Trading:
                return Icon.Trading;

            case Skill.Equipment_Skill:
            default:
                Debug.LogWarning($"Unexpected skill enum: {skill}");
                return Icon.Everstone;
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

    public static string GetWinCriteriaDisplayString(MultiplayerWinCriteria criteria)
    {
        switch (criteria)
        {
            case MultiplayerWinCriteria.Gold:
                return "Gold Amount";

            case MultiplayerWinCriteria.StageProgress:
                return "Stage Progress";

            case MultiplayerWinCriteria.SumLevelCount:
                return "Level Sum";

            case MultiplayerWinCriteria.None:
            default:
                break;
        }

        return "";
    }


    #endregion STATIC METHODS



    private void Start()
    {
        AssembleResources();
    }


    private void AssembleResources() {
        Heroes = Resources.LoadAll<ScriptableHero>("Heroes/Classes").ToList();
        _HeroesDict = Heroes.ToDictionary(r => r.ClassName, r => r);

        HeroPortraits = Resources.LoadAll<HeroPortrait>("Heroes/Portraits").ToList();

        HeroBackgrounds = Resources.LoadAll<ScriptableBackground>("Heroes/Backgrounds").ToList();
        _HeroBackgroundsDict = HeroBackgrounds.ToDictionary(x => x.backgroundName, x => x);


        StatusEffects = Resources.LoadAll<ScriptableStatusEffect>("Heroes/StatusEffects").ToList();
        PlayerAbilities = Resources.LoadAll<ScriptableAbility>("Heroes/Abilities").ToList();

        UnitClassAbilities = Resources.LoadAll<ScriptableAbility>("Enemies/Abilities").ToList();

        var locationData = Resources.LoadAll<ScriptableAdventureLocation>("Locations/Adventure").ToList();
        AdventureLocations = new List<ScriptableAdventureLocation>();
       
        foreach (var location in locationData.OrderBy(x => x.name).OrderBy(x => x.difficulty))
            AdventureLocations.Add(Instantiate(location));
            
        _AdventureLocationsDict = AdventureLocations.ToDictionary(x => x.locationName, x => x);

        CommonEnemies = Resources.LoadAll<ScriptableNpcUnit>("Enemies/_Common").ToList();

        //load items
        Items_Equipment = Resources.LoadAll<ItemDataEquipment>("Items/Equipment").ToList();
        Items_Loot     = Resources.LoadAll<ItemDataBase>("Items/Loot").ToList();
    }

  
    public ScriptableHero GetHeroByName(string t) => Instantiate(_HeroesDict[t]);
    public List<string> GetHeroClassNames() => _HeroesDict.Keys.OrderBy(x => x).ToList();
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

    public List<ScriptableAbility> GetUnitClassAbilities(UnitClass unitClass)
    {
        var res = new List<ScriptableAbility>();
        
        switch (unitClass)
        {
            case UnitClass.Artillery:
                res.Add(GetClassAbilityFromName("Multi Attack"));
                break;

            case UnitClass.Controller:
                res.Add(GetClassAbilityFromName("Buff Team Damage"));
                res.Add(GetClassAbilityFromName("Buff Team Defense"));
                res.Add(GetClassAbilityFromName("Debuff Enemy Damage"));
                res.Add(GetClassAbilityFromName("Debuff Enemy Defense"));
                break;

            case UnitClass.Bruiser:
                res.Add(GetClassAbilityFromName("Bruiser Buff"));
                break;

            case UnitClass.Battlemage:
                res.Add(GetClassAbilityFromName("Shield Self"));
                break;

            case UnitClass.Healer:
                res.Add(GetClassAbilityFromName("Heal All"));
                break;

            case UnitClass.Marksman:
            case UnitClass.Mage:
            case UnitClass.Tank:
            case UnitClass.Titan:
            case UnitClass.Vanguard:
            case UnitClass.Assassin:
            case UnitClass.Warrior:
            default:
                break;
        }

        return res;
    }

    private ScriptableAbility GetClassAbilityFromName(string name)
    {
        return Instantiate(UnitClassAbilities.FirstOrDefault(x => x.Name.Equals(name)));
    }

    /// <summary>
    /// Returns a list of ordered (cloned) ScriptableAdventureLocation objects 
    /// </summary>
    public List<ScriptableAdventureLocation> GetAdventureLocations()
    {
        return AdventureLocations.OrderBy(x => x.name).OrderBy(x => x.difficulty).ToList();
    }

    public ScriptableAdventureLocation GetAdventureLocationByName(string name)
    {
        foreach (var location in AdventureLocations)
        {
            if (location.locationName.Equals(name))
                return location;
        }

        return null;
    }

    public ScriptableStatusEffect GetStatusEffect(StatusEffectType effect)
    {
        return Instantiate(StatusEffects.FirstOrDefault(x => x.Effect == effect));
    }

    public ItemDataBase GetRandomItemByType(ItemType itemType, ItemRarity rarity)
    {
        List<ItemDataBase> itemCandidates = null;

        switch (itemType)
        {
            case ItemType.Equipment:
                itemCandidates = GetCastedEquipmentItems().Where(x => x.Rarity == rarity).ToList();
                break;

            case ItemType.Loot:
                itemCandidates = Items_Loot.Where(x => x.Rarity == rarity).ToList();
                break;

            case ItemType.Potion:
                //TODO
                break;

            case ItemType.None:
            default:
                return null;
        }

        if (itemCandidates == null || itemCandidates.Count == 0)
            return null;
        else
            return itemCandidates[Random.Range(0, itemCandidates.Count)];
    }


    private List<ItemDataBase> GetCastedEquipmentItems()
    {
        List<ItemDataBase> res = new List<ItemDataBase>();

        Items_Equipment.ForEach(x => res.Add(x));

        return res;
    }

    public ItemDataBase GetItemById(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
            return null;

        if (itemId.Equals("Item_Currency"))
            return GetCurrencyItem();

        foreach (var item in Items_Loot)
            if (item.Id.Equals(itemId))
                return item;

        foreach (var item in Items_Equipment)
            if (item.Id.Equals(itemId))
                return item;

        Debug.LogWarning($"Couldnt find item by Id! ({itemId})");
        return null;
    }

    public ItemDataBase GetCurrencyItem()
    {
        var res = ScriptableObject.CreateInstance("ItemDataBase") as ItemDataBase;
        res.Init("Item_Currency", 
            "Money", 
            "A pile of money.", 
            CurrencyItemSprite,
            1,
            999999999,
            ItemType.Currency,
            ItemRarity.Uncommon
        );

        return res;
    }

    //public List<ItemDataBase> GetAllItemsOfType(ItemType itemType)
    //{
    //    switch (itemType)
    //    {
    //        case ItemType.Equipment:
    //            return new List<ItemDataBase>(Items_Equipment);

    //        case ItemType.Loot:
    //            return Items_Loot[Random.Range(0, Items_Loot.Count)];

    //        case ItemType.Potion:
    //            break;

    //        case ItemType.None:
    //        default:
    //            return null;
    //    }

    //    return null;
    //}

    public List<ScriptableNpcUnit> GetAdventureLocationEnemyPool(string locationName)
    {
        List<ScriptableNpcUnit> res = new List<ScriptableNpcUnit>();
        
        var tempList = Resources.LoadAll<ScriptableNpcUnit>($"Enemies/{locationName}").ToList();
        tempList.ForEach(x => res.Add(Instantiate(x)));

        return res;
    }

    public List<HeroPortrait> GetHeroPortraits()
    {
        return HeroPortraits;
    }

    public HeroPortrait GetHeroPortraitByName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return null;

        foreach (var portrait in HeroPortraits)
        {
            if (portrait.name.Equals(name))
                return portrait;
        }

        return null;
    }


    
}   
