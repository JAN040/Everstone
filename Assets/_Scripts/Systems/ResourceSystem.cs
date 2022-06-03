using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// One repository for all scriptable objects. Create your query methods here to keep your business logic clean.
/// I make this a MonoBehaviour as sometimes I add some debug/development references in the editor.
/// If you don't feel free to make this a standard class
/// </summary>
public class ResourceSystem : StaticInstance<ResourceSystem> {
    public List<ScriptableHero> Heroes { get; private set; }
    private Dictionary<string, ScriptableHero> _HeroesDict;


    /// <summary>
    /// To add icons: slice up sprite sheet -> right click-> create -> textmeshpro -> sprite asset -> 
    ///     project settings TMP set as default sprite asset. In the asset click "Update sprite asset"
    /// </summary>
    private Dictionary<Icon, string> TMP_IconDict = new Dictionary<Icon, string>()
    {
        {Icon.Everstone, "everstone" },
        {Icon.Infinity, "infinity" },
    };


    protected override void Awake() {
        base.Awake();
        AssembleResources();
    }

    private void AssembleResources() {
        Heroes = Resources.LoadAll<ScriptableHero>("Heroes").ToList();
        _HeroesDict = Heroes.ToDictionary(r => r.ClassName, r => r);
    }

    public ScriptableHero GetHero(string t) => _HeroesDict[t];
    public List<string> GetHeroClasses() => _HeroesDict.Keys.OrderBy(x => x).ToList();
    public ScriptableHero GetRandomHero() => Heroes[Random.Range(0, Heroes.Count)];


    public string GetIconTag(Icon icon) => $"<sprite name=\"{TMP_IconDict[icon]}\">";
}   
