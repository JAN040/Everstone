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
    [SerializeField] TextMeshProUGUI Text_Name;
    [SerializeField] TextMeshProUGUI Text_Level;
    [SerializeField] TextMeshProUGUI Text_Xp;
    [SerializeField] Image Image_XpBar;
    [SerializeField] GameObject SkillInfoBoxPrefab;


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
        Text_Name.text = SkillLevelRef.GetSkillName();
        Text_Level.text = $"Lvl {SkillLevelRef.Level}";
        Text_Xp.text = $"{SkillLevelRef.Experience} / {SkillLevelRef.ExpToNextLevel}";
        
        Image_XpBar.fillAmount = SkillLevelRef.GetExperienceNormalized();
    }


    public void IconButtonClicked()
    {
        var obj = Instantiate(SkillInfoBoxPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        obj.transform.localScale = Vector3.one;
        obj.transform.localPosition = Vector3.zero;

        obj.GetComponent<SkillInfoBox>().Init(SkillLevelRef);
    }


    #endregion METHODS
}
