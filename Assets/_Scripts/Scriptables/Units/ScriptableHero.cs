using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Progress;

/// <summary>
/// Used to store hero class base data
/// </summary>
[CreateAssetMenu(menuName = "Scriptable/Units/New Hero Class", fileName = "SO_HeroClass_")]
public class ScriptableHero : ScriptableUnitBase
{

    //inherited: public UnitBase Prefab;


    [Space]
    [Header("ScriptableHero")]
    [Space]

    //pre-assigned values - GAME DATA
    //inherited: private CharacterStats _baseStats 
    public string ClassName;
    public PointAllocationData pointAllocationData = new PointAllocationData();

    [Space]
    [Header("Runtime Data")]
    [Space]

    //data about character levels
    private LevelSystem levelSystem;
    public LevelSystem LevelSystem { get => levelSystem; private set => levelSystem = value; }

    public void SetLevelSystem(LevelSystem levelSystem)
    {
        LevelSystem = levelSystem;
    }

    //TODO: ability system
}


