using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SkillLevelUI : MonoBehaviour
{
    #region VARIABLES


    #region UI References


    [Header("UI References")]
    [SerializeField] TextMeshProUGUI Text_Icon;
    [SerializeField] TextMeshProUGUI Text_NameAndLevel;
    [SerializeField] TextMeshProUGUI Text_Xp;
    [SerializeField] Image Image_XpBar;


    #endregion UI References


    //[Space]
    //[Header("Variables")]
    private SkillLevel SkillLevelRef;

    #endregion VARIABLES


    #region UNITY METHODS


    private void OnEnable()
    {
        UpdateUI();
    }




    #endregion UNITY METHODS


    #region METHODS


    public void Init(SkillLevel skillLevel)
    {
        SkillLevelRef = skillLevel;

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (SkillLevelRef == null)
            return;

        Text_Icon.text = ResourceSystem.GetSkillIconTag(SkillLevelRef.SkillType);
        Text_NameAndLevel.text = $"{SkillLevelRef.GetSkillName()} Lvl {SkillLevelRef.Level}";
        Text_Xp.text = $"{SkillLevelRef.Experience} / {SkillLevelRef.ExpToNextLevel}";
        
        Image_XpBar.fillAmount = SkillLevelRef.GetExperienceNormalized();
    }

    #endregion METHODS
}
