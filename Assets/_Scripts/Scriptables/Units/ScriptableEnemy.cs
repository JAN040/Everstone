using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Used to store enemy data
/// </summary>
[CreateAssetMenu(menuName = "Scriptable/Units/New Enemy", fileName = "SO_Enemy_")]
public class ScriptableEnemy : ScriptableUnitBase
{
    public string Name;

    /// <summary>
    /// defines stat distributions and available abilities
    /// </summary>
    public EnemyClass Class;

    /// <summary>
    /// normal, elite, boss
    /// </summary>
    public EnemyType Type;

    //TODO abilities list

    public ScriptableEnemy()
    {
        this.Faction = Faction.Enemies;
    }

    public void SetBaseStats(LocationDifficulty locDiff, Difficulty gameDiff)
    {
        _baseStats = UnitData.GetBaseStats(Class, Type, locDiff, gameDiff);
    }


}