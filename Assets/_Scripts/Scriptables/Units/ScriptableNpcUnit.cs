using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Used to store non-player character unit data
/// </summary>
[CreateAssetMenu(menuName = "Scriptable/Units/New Enemy", fileName = "SO_Enemy_")]
public class ScriptableNpcUnit : ScriptableUnitBase
{

    /// <summary>
    /// defines stat distributions and available abilities
    /// </summary>
    public UnitClass Class;



    //TODO abilities list
    public List<ScriptableAbility> Abilities;

    public ScriptableNpcUnit()
    {
        this.Faction = Faction.Enemies;
    }

    public void InitStatsAndAbilities(Difficulty gameDiff, ScriptableAdventureLocation locData)
    {
        _baseStats = GameManager.Instance.UnitData.GetBaseStats(Class, Type, gameDiff, locData);
        Abilities = ResourceSystem.Instance.GetUnitClassAbilities(Class);
    }

    public ScriptableAbility GetRandomAbility()
    {
        if (Abilities == null || Abilities.Count == 0)
            return null;

        return Abilities[Random.Range(0, Abilities.Count)];
    }
}