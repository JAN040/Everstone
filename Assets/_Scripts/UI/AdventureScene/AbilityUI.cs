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

    [SerializeField] GameObject CostPanel;
    [SerializeField] TextMeshProUGUI CostText_Energy;
    [SerializeField] TextMeshProUGUI CostText_Mana;


    #endregion UI References


    [Space]
    [Header("Variables")]

    [SerializeField] ScriptableAbility Ability;
    private Unit PlayerHeroUnit;


    #endregion VARIABLES



    #region UNITY METHODS

    // decrease cooldown every frame, if any
    void Update()
    {
        if (Ability == null)
            return;
        
        if (Ability.CurrentCooldown > 0)
            Ability.CurrentCooldown -= Time.deltaTime;
        
        UpdateUI(false);
    }

    #endregion UNITY METHODS


    public void Initialize(Unit heroUnit, ScriptableAbility ability)
    {
        PlayerHeroUnit = heroUnit;
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

            if (CostText_Energy != null)
                CostText_Energy.text = "";
            if (CostText_Mana != null)
                CostText_Mana.text = "";

            return;
        }

        if (init)
        {
            AbilityImage.sprite = Ability.MenuImage;
            Ability.SetCostText(CostText_Energy, CostText_Mana);
        }

        float cd = Ability.GetCooldownNormalized();

        AbilityButton.interactable = cd <= 0;
        CostPanel.SetActive(cd <= 0);   //cost should only be visible when the skill is ready

        CooldownImage.fillAmount = cd;
        CooldownText.text = Ability.GetCooldownText();
    }

   
    //when ability is clicked
    public void Activate()
    {
        //handle the cost
        if (!HandleAbilityCost())
        {
            Debug.LogWarning($"Tried to activate ability {Ability.Name} but it failed because it cost more than the player had.");
            return;
        }

        Ability.Activate();

        //handle the cooldown
        float cd = Ability.Cooldown;
        
        if (Ability.IsCDRValid) 
            cd -= cd * PlayerHeroUnit.Stats.CooldownReduction.GetValue();
        
        Ability.CurrentCooldown = cd;
    }

    private bool HandleAbilityCost()
    {
        var stats = PlayerHeroUnit.Stats;
        float eCost;
        float mCost;

        if (Ability.CostType == CostType.Energy)
        {
            eCost = Ability.EnergyCost;

            if (stats.Energy >= eCost)
            {
                stats.Energy -= eCost;
                return true;
            }
        }
        else if (Ability.CostType == CostType.Mana)
        {
            mCost = Ability.ManaCost;

            if (stats.Mana >= mCost)
            {
                stats.Mana -= mCost;
                return true;
            }
        }
        else if (Ability.CostType == CostType.EnergyAndMana)
        {
            eCost = Ability.EnergyCost;
            mCost = Ability.ManaCost;

            if (stats.Energy >= eCost && stats.Mana >= mCost)
            {
                stats.Energy -= eCost;
                stats.Mana -= mCost;
                
                return true;
            }
        }

        return false;
    }
}
