using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ScriptableHero;
using UnityEngine;

/// <summary>
/// Used to store character background base data
/// </summary>
[CreateAssetMenu(menuName = "Scriptable/Units/New Hero Background", fileName = "SO_HeroBackground_")]
public class ScriptableBackground : ScriptableObject
{
    [Space]
    [Header("ScriptableBackground")]
    [Space]

    public string backgroundName;

    public int startingCurrencyAmount;

    //TODO: starting equipment ig
    //TODO: special quest ig

    public PointAllocationData pointAllocationData = new PointAllocationData();
}
