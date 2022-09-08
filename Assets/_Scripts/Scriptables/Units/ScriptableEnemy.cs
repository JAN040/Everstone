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

    /// <summary>
    /// defines stat distributions and available abilities
    /// </summary>
    public UnitClass Class;

    

    //TODO abilities list

    public ScriptableEnemy()
    {
        this.Faction = Faction.Enemies;
    }

    public void SetBaseStats(Difficulty gameDiff, ScriptableAdventureLocation locData)
    {
        _baseStats = GameManager.Instance.UnitData.GetBaseStats(Class, Type, gameDiff, locData);
    }


}