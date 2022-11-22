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
    [Tooltip("Defines stat distributions and available abilities")]
    public UnitClass Class;

    [Tooltip("Shared drop pool is decided based on the race type")]
    public UnitRace Race;

    [Tooltip("When the unit is defeated, this item will always be dropped")]
    public ItemDataBase GuaranteedDrop;

    [System.Serializable]
    public struct PossibleDrops
    {
        public ItemDataBase Drop;
        [Range(0,1)]
        public float Chance;
    }

    [Tooltip("When the unit is defeated, these items might appear as drops, based on defined drop chances (0-1)")]
    public List<PossibleDrops> PossibleDropsList;

    /// <summary>
    /// Decided based on Class
    /// </summary>
    [System.NonSerialized]
    public List<ScriptableAbility> Abilities;


    public ScriptableNpcUnit()
    {
        this.Faction = Faction.Enemies;
    }


    #region METHODS


    public void InitStatsAndAbilities(Difficulty gameDiff, ScriptableAdventureLocation locData)
    {
        _stats = GameManager.Instance.UnitData.GetBaseStats(Class, Type, gameDiff, locData);
        Abilities = ResourceSystem.Instance.GetUnitClassAbilities(Class);
    }

    public ScriptableAbility GetRandomAbility()
    {
        if (Abilities == null || Abilities.Count == 0)
            return null;

        return Abilities[Random.Range(0, Abilities.Count)];
    }


    #endregion METHODS
}