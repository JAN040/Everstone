using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Used to store the portrait sprite and its facing direction
/// </summary>
[CreateAssetMenu(menuName = "Scriptable/Units/New Hero Portrait", fileName = "SO_HeroPortrait_")]
public class HeroPortrait : ScriptableObject
{
    #region VARIABLES


    #region UI References


    [Header("UI References")]
    public Sprite PortraitImage;


    #endregion UI References


    [Space]
    [Header("Variables")]
    public FacingDirection FaceDirection;
    public Gender Gender;


    #endregion VARIABLES


    #region METHODS

    

    #endregion METHODS
}
