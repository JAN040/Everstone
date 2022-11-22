using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    #region VARIABLES


    #region UI References


    [Header("UI References")]
    [SerializeField] Button ContinueButton;


    #endregion UI References


    //[Space]
    //[Header("Variables")]


    #endregion VARIABLES


    #region UNITY METHODS


    // Start is called before the first frame update
    void Start()
    {
        ContinueButton.interactable = SaveSystem.SaveFileExists();
    }

    #endregion UNITY METHODS


    #region METHODS

    
    public void OnContinueClicked()
    {
        GameManager.Instance.LoadGame();
    }


    #endregion METHODS
}
