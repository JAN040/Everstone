using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquippedAbilityPanelUI : MonoBehaviour
{
    #region VARIABLES


    #region UI References


    [Header("UI References")]
    [SerializeField] Image AbilityIcon;


    #endregion UI References


    //[Space]
    //[Header("Variables")]
    private ScriptableAbility AbilityRef;


    #endregion VARIABLES


    #region UNITY METHODS


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    #endregion UNITY METHODS


    #region METHODS

    
    public void SetAbility(ScriptableAbility ability)
    {
        AbilityRef = ability;

        AbilityIcon.gameObject.SetActive(AbilityRef != null);

        if (AbilityRef == null)
            return;

        AbilityIcon.sprite = AbilityRef.MenuImage;
    }


    #endregion METHODS
}
