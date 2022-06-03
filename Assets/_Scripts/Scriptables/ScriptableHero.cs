using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Used to store hero class base data
/// </summary>
[CreateAssetMenu(menuName = "Scriptable/Units/New Hero Class", fileName = "SO_HeroClass_")]
public class ScriptableHero : ScriptableUnitBase {

    [Space]
    [Header("ScriptableHero")]
    [Space]

    public string ClassName;

    public PointAllocationData pointAllocationData = new PointAllocationData();
}


