using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class AbilityUI : MonoBehaviour
{
    #region VARIABLES


    #region UI References


    [Header("UI References")]

    [SerializeField] Image AbilityImage;
    [SerializeField] Button AbilityButton;
    [SerializeField] Image CooldownImage;
    [SerializeField] TextMeshProUGUI CooldownText;


    #endregion UI References


    [Space]
    [Header("Variables")]

    [SerializeField] ScriptableAbility Ability;
    private ScriptableHero PlayerHero;


    #endregion VARIABLES



    #region UNITY METHODS

    // decrease cooldown every frame, if any
    void Update()
    {
        if (Ability == null)
            return;
        
        if (Ability.Cooldown > 0)
            Ability.Cooldown -= Time.deltaTime;
        
        UpdateUI(false);
    }

    #endregion UNITY METHODS


    public void Initialize(ScriptableHero hero, ScriptableAbility ability)
    {
        PlayerHero = hero;
        Ability = ability;

        UpdateUI(true);
    }

    private void UpdateUI(bool init)
    {
        if (Ability == null)
        {   //blank ability tile
            AbilityButton.interactable = false;
            CooldownImage.fillAmount = 0;
            CooldownText.text = "";

            return;
        }

        if (init)
        {
            AbilityImage.sprite = Ability.MenuImage;
        }

        float cd = Ability.GetCooldownNormalized();

        AbilityButton.interactable = cd <= 0;
        CooldownImage.fillAmount = cd;
        CooldownText.text = GetCooldownText(Ability.CurrentCooldown);
    }

    private string GetCooldownText(float cd)
    {
        //if more than a minute
        if (cd > 60f)
            return $"{ (int)cd / 60 }m";
        else if (cd > 0)
            return $"{ (int)cd }";
        else
            return "";
    }

    //when ability is clicked
    public void Activate()
    {

    }
}
