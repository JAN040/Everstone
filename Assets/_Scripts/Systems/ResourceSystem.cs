using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// One repository for all scriptable objects. Create your query methods here to keep your business logic clean.
/// I make this a MonoBehaviour as sometimes I add some debug/development references in the editor.
/// If you don't feel free to make this a standard class
/// </summary>
public class ResourceSystem : Singleton<ResourceSystem> {
    private List<ScriptableHero> Heroes { get; set; }
    private Dictionary<string, ScriptableHero> _HeroesDict;

    private List<ScriptableAbility> PlayerClassicAbilities { get; set; }
    private List<ScriptableAbility> PlayerSpecialAbilities { get; set; }
    private List<ScriptableStatusEffect> StatusEffects { get; set; }

    private List<ScriptableBackground> HeroBackgrounds { get; set; }
    private Dictionary<string, ScriptableBackground> _HeroBackgroundsDict;

    private List<ScriptableAdventureLocation> AdventureLocations { get; set; }
    private Dictionary<string, ScriptableAdventureLocation> _AdventureLocationsDict;

    private List<ScriptableEnemy> CommonEnemies { get; set; }


    /// <summary>
    /// To add icons: slice up sprite sheet -> right click-> create -> textmeshpro -> sprite asset -> 
    ///     project settings TMP set as default sprite asset. In the asset click "Update sprite asset"
    /// </summary>
    private Dictionary<Icon, string> TMP_IconDict = new Dictionary<Icon, string>()
    {
        { Icon.Everstone,    "everstone" },
        { Icon.Infinity,     "infinity"  },

        { Icon.Attack_Phys, "attack_phys" },
        { Icon.Attack_Arts, "attack_arts" },
        { Icon.Defense,     "defense"     },
        { Icon.Arts_Resist, "resist"      },
        { Icon.Speed,       "speed"       },
        { Icon.Health,      "health"      },
        { Icon.Stamina,     "energy"      },
        { Icon.Mana,        "mana"        }
    };


    //protected override void Awake() {
    //    base.Awake();
    //}

    private void Start()
    {
        AssembleResources();
    }


    private void AssembleResources() {
        Heroes = Resources.LoadAll<ScriptableHero>("Heroes/Classes").ToList();
        _HeroesDict = Heroes.ToDictionary(r => r.ClassName, r => r);

        StatusEffects = Resources.LoadAll<ScriptableStatusEffect>("StatusEffects").ToList();

        AdventureLocations = Resources.LoadAll<ScriptableAdventureLocation>("Locations/Adventure").ToList();
        _AdventureLocationsDict = AdventureLocations.ToDictionary(x => x.locationName, x => x);

        //load enemies
        foreach (var location in AdventureLocations)
        {
            location.SetEnemyPool(Resources.LoadAll<ScriptableEnemy>($"Enemies/{location.locationName}").ToList());
        }

        CommonEnemies = Resources.LoadAll<ScriptableEnemy>("Enemies/_Common").ToList();
    }

    public ScriptableHero GetHero(string t) => _HeroesDict[t];
    public List<string> GetHeroClasses() => _HeroesDict.Keys.OrderBy(x => x).ToList();
    public ScriptableHero GetRandomHero() => Heroes[Random.Range(0, Heroes.Count)];

    public ScriptableBackground GetBackground(string t) => _HeroBackgroundsDict[t];
    public ScriptableBackground GetRandomBackground() => HeroBackgrounds[Random.Range(0, HeroBackgrounds.Count)];
    public List<string> GetHeroBackgrounds()
    {
        HeroBackgrounds = Resources.LoadAll<ScriptableBackground>("Heroes/Backgrounds").ToList();
        _HeroBackgroundsDict = HeroBackgrounds.ToDictionary(x => x.backgroundName, x => x);

        var names = new List<string>();
        
        //add the "None" background first
        var BGnone = HeroBackgrounds.FirstOrDefault(x => x.backgroundName.ToLower() == "none");
        names.Add(BGnone.backgroundName);

        //add the other backgrounds after
        var BGother = _HeroBackgroundsDict.Keys.OrderBy(x => x).ToList();
        BGother.RemoveAll(x => x.ToLower() == "none");
        names.AddRange(BGother);

        return names;
    }

    public List<ScriptableAbility> GetClassicPlayerAbilities()
    {
        PlayerClassicAbilities = Resources.LoadAll<ScriptableAbility>("Heroes/Abilities/Classic").ToList();
        return new List<ScriptableAbility>(PlayerClassicAbilities);
    }

    public List<ScriptableAbility> GetSpecialPlayerAbilities()
    {
        PlayerSpecialAbilities = Resources.LoadAll<ScriptableAbility>("Heroes/Abilities/Special").ToList();
        return new List<ScriptableAbility>(PlayerSpecialAbilities);
    }

    public List<ScriptableAdventureLocation> GetAdventureLocations() => AdventureLocations.OrderBy(x => x.name).OrderBy(x => x.difficulty).ToList();

    public string GetIconTag(Icon icon) => $"<sprite name=\"{TMP_IconDict[icon]}\">";

    public ScriptableStatusEffect GetStatusEffect(StatusEffect effect)
    {
        return StatusEffects.FirstOrDefault(x => x.Effect == effect);
    }
}   
