using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Linq;

public class SkillInfoBox : MonoBehaviour
{
    #region VARIABLES


    #region UI References


    [Header("UI References")]
    [SerializeField] GameObject Object_SkillInfoBox;

    [Space]
    [SerializeField] TextMeshProUGUI Text_Icon;

    [SerializeField] TextMeshProUGUI Text_Name;
    [SerializeField] TextMeshProUGUI Text_Level;
    [SerializeField] TextMeshProUGUI Text_Experience;

    [SerializeField] GameObject Object_Description;
    [SerializeField] TextMeshProUGUI Text_Description;

    [SerializeField] GameObject Object_Effects;
    [SerializeField] TextMeshProUGUI Text_Effects;

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
    private SkillLevel skillLevelRef;
    private SkillLevel SkillLevelRef
    {
        get => skillLevelRef;
        set
        {
            skillLevelRef = value;
            UpdateUI();
        }
    }


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
    public void Init(SkillLevel skillLevel)
    {
        SkillLevelRef = skillLevel;

        Object_SkillInfoBox.SetActive(true);
    }

    private void UpdateUI()
    {
        Text_Icon.text = ResourceSystem.GetSkillIconTag(SkillLevelRef.SkillType);

        Text_Name.text   = SkillLevelRef.GetSkillName();
        Text_Level.text = $"Level: {SkillLevelRef.Level}";

        Text_Experience.text = $"Experience: {SkillLevelRef.Experience}/{SkillLevelRef.ExpToNextLevel}";

        Text_Description.text = SkillLevelRef.GetSkillDescription();
        Object_Description.SetActive(!string.IsNullOrEmpty(Text_Description.text));

        //effects
        Text_Effects.text = SkillLevelRef.GetSkillDifferencesPerLevel();
        Object_Effects.SetActive(!string.IsNullOrEmpty(Text_Effects.text));
    }



    #region Buttons


    public void CloseClicked()
    {
        Destroy(Object_SkillInfoBox.gameObject);
    }


    #endregion Buttons


    #endregion METHODS
}
