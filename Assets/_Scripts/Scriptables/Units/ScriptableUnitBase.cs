using System;
using UnityEngine;

/// <summary>
/// Keeping all relevant information about a unit on a scriptable means we can gather and show
/// info on the menu screen, without instantiating the unit prefab.
/// </summary>
public abstract class ScriptableUnitBase : ScriptableObject {

    public string Name;

    public Faction Faction;

    /// <summary>
    /// normal, elite, boss
    ///     ...even allies can have a type, after all it affects stats and the frame type
    /// </summary>
    public EnemyType Type = EnemyType.Normal;

    // Used in menus
    public Sprite MenuSprite;

    [Tooltip("Specify the direction the portrait is facing. This helps the game flip the sprite in the right direction")]
    public FacingDirection FaceDirection;

    [NonSerialized]
    [TextArea(3,5)]
    public string Description;

    // Used in game
    [NonSerialized]
    public GameObject Prefab;

    /// <summary>
    /// Keeping base stats as a struct on the scriptable keeps it flexible and easily editable.
    /// We can pass this struct to the spawned prefab unit and alter them depending on conditions.
    /// </summary>
    [SerializeField] protected CharacterStats _stats 
        = new CharacterStats(0,0,50,0,0,100,10,2);
    public CharacterStats Stats => _stats;

}



