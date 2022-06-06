using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager
{
    #region VARIABLES

    //holds all player related data
    public ScriptableHero PlayerHero { get; private set; }

    //TODO: inventory system

    #endregion VARIABLES

    
    #region METHODS

    public void SetHero(ScriptableHero hero)
    {
        PlayerHero = hero;
    }

    #endregion METHODS


}
