using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Linq;

public class AbilityInfoBox : MonoBehaviour
{
    #region VARIABLES


    #region UI References


    [Header("UI References")]
    [SerializeField] GameObject Object_AbilityInfoBox;

    [Space]
    [SerializeField] Image Image_Icon;

    [SerializeField] TextMeshProUGUI Text_Name;
    [SerializeField] TextMeshProUGUI Text_Level;
    [SerializeField] TextMeshProUGUI Text_UpgradeCost;

    [SerializeField] GameObject Object_Description;
    [SerializeField] TextMeshProUGUI Text_Description;

    //[SerializeField] GameObject Object_Effects;
    //[SerializeField] TextMeshProUGUI Text_Effects;

    //[Space]
    //[Header("Buttons")]
    //[SerializeField] GameObject Object_ButtonArea;

    //[SerializeField] GameObject Button_Equip;
    //[SerializeField] GameObject Button_Unequip;
    //[SerializeField] GameObject Button_Use;
    //[SerializeField] GameObject Button_Split;


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


    /// <summary>
    /// Shows an info box with item details and its uses.
    /// </summary>
    public void Init(ScriptableAbility ability)
    {
        AbilityRef = ability;

        Object_AbilityInfoBox.SetActive(true);
        UpdateUI();
    }

    private void UpdateUI()
    {
        Image_Icon.sprite = AbilityRef.MenuImage;

        Text_Name.text   = AbilityRef.Name;

        bool isMaxLvl = AbilityRef.Level == AbilityRef.MaxLevel;
        string maxLvl = isMaxLvl ? "" : $" (Max: {AbilityRef.MaxLevel})";
        Text_Level.text = $"Level: {AbilityRef.Level}{maxLvl}";
        Text_UpgradeCost.text = isMaxLvl ? 
            $"Max level" : $"Upgrade cost:\n{GameManager.Instance.CurrencyToDisplayString(AbilityRef.UpgradeCost)}";

        Text_Description.text = AbilityRef.GetDescription();
        Object_Description.SetActive(!string.IsNullOrEmpty(Text_Description.text));

        //effects
        //Text_Effects.text = AbilityRef.GetSkillDifferencesPerLevel();
        //Object_Effects.SetActive(!string.IsNullOrEmpty(Text_Effects.text));
    }
    

    #region Buttons


    public void CloseClicked()
    {
        Destroy(Object_AbilityInfoBox.gameObject);
    }


    #endregion Buttons


    #endregion METHODS
}
