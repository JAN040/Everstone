using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerIconDropdown : MonoBehaviour
{
    #region VARIABLES

    [SerializeField]
    [Tooltip("Reference to the dropdown menu the player uses to select their picture.")]
    TMP_Dropdown iconDropdown;

    [SerializeField]
    [Tooltip("List of available avatar icons")]
    public List<Sprite> playerIconList;

    #endregion

    #region UNITY METHODS
    // Start is called before the first frame update
    void Start()
    {
        iconDropdown.ClearOptions();
        iconDropdown.AddOptions(playerIconList);
    }

    // Update is called once per frame
    //void Update()
    //{
        
    //}
    #endregion

}
