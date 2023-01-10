using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Linq;
using UnityEngine.UIElements;

public class StatInfoBox : MonoBehaviour
{
    #region VARIABLES


    #region UI References


    [Header("UI References")]
    [SerializeField] GameObject Object_SkillInfoBox;

    [Space]
    [SerializeField] TextMeshProUGUI Text_Icon;

    [SerializeField] TextMeshProUGUI Text_Name;
    [SerializeField] TextMeshProUGUI Text_Amount;

    [SerializeField] GameObject Object_Description;
    [SerializeField] TextMeshProUGUI Text_Description;


    #endregion UI References


    private StatType StatTypeRef;


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
    public void Init(StatType stat)
    {
        StatTypeRef = stat;
        UpdateUI();

        Object_SkillInfoBox.SetActive(true);
    }

    private void UpdateUI()
    {
        Text_Icon.text = ResourceSystem.GetStatIconTag(StatTypeRef);

        Text_Name.text = Stat.GetDisplayName(StatTypeRef);
        Text_Amount.text = $"{GameManager.Instance.PlayerManager.PlayerHero.Stats.GetStatFromStatType(StatTypeRef).GetDisplayValue()}";

        Text_Description.text = Stat.GetDescription(StatTypeRef);
        Object_Description.SetActive(!string.IsNullOrEmpty(Text_Description.text));
    }



    #region Buttons


    public void CloseClicked()
    {
        Destroy(Object_SkillInfoBox.gameObject);
    }


    #endregion Buttons


    #endregion METHODS
}
