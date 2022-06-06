using System;
using UnityEngine;

/// <summary>
/// Keeping all relevant information about a unit on a scriptable means we can gather and show
/// info on the menu screen, without instantiating the unit prefab.
/// </summary>
public abstract class ScriptableUnitBase: ScriptableObject {
    
    public Faction Faction;

    // Used in menus
    public Sprite MenuSprite;
    [TextArea(3,5)]
    public string Description;

    // Used in game
    public UnitBase Prefab;

    /// <summary>
    /// Keeping base stats as a struct on the scriptable keeps it flexible and easily editable.
    /// We can pass this struct to the spawned prefab unit and alter them depending on conditions.
    /// </summary>
    [SerializeField] private CharacterStats _baseStats 
        = new CharacterStats(0,0,0,0,50,30,5,5,10);
    public CharacterStats BaseStats => _baseStats;

}



