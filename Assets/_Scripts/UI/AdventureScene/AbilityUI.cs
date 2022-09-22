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

    [Space]
    [SerializeField] Animator Animator;

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

    private void OnDestroy()
    {
        if (Ability != null)
            Ability.OnAbilityToggled -= ToggleAbility;
    }

    #endregion UNITY METHODS


    public void Initialize(Unit heroUnit, ScriptableAbility ability)
    {
        PlayerHeroUnit = heroUnit;
        Ability = ability;

        if (Ability != null)
            Ability.OnAbilityToggled += ToggleAbility;

        UpdateUI(true);
    }

    private void ToggleAbility(ScriptableAbility ability, bool isToggled)
    {
        if (Animator != null)
            Animator.SetBool("IsToggled", isToggled);
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

        //for the button to be interactable the ability needs to be off cooldown and have enough mana/energy for its use
        AbilityButton.interactable = (cd <= 0) && HandleAbilityCost(true);

        //cost should only be visible when the skill is ready
        CostPanel.SetActive(cd <= 0);

        CooldownImage.fillAmount = cd;
        CooldownText.text = Ability.GetCooldownText();
    }

    //when ability is clicked
    public void Activate()
    {
        //handle the cost
        if (!HandleAbilityCost(false))
        {
            Debug.LogWarning($"Tried to activate ability {Ability.Name} but it failed because it cost more than the player had.");
            return;
        }

        //handle the cooldown
        float cd = Ability.Cooldown;
        
        if (Ability.IsCDRValid) 
            cd -= cd * PlayerHeroUnit.Stats.CooldownReduction.GetValue();

        Ability.CooldownAtStart = cd;
        Ability.CurrentCooldown = cd;

        Ability.Activate();
    }

    /// <summary>
    /// Handles Ability cost calculations and Energy & Mana stat decrements.
    /// </summary>
    /// <param name="testOnly">
    /// If true, will not decrement Energy & Mana stats but only check if the player has enough
    ///     to actually cast the ability.
    /// </param>
    /// <returns></returns>
    private bool HandleAbilityCost(bool testOnly)
    {
        var stats = PlayerHeroUnit.Stats;
        float eCost;
        float mCost;

        if (Ability.CostType == CostType.Energy)
        {
            eCost = Ability.EnergyCost;

            if (stats.Energy >= eCost)
            {
                if (!testOnly)
                    stats.Energy -= eCost;

                return true;
            }
        }
        else if (Ability.CostType == CostType.Mana)
        {
            mCost = Ability.ManaCost;

            if (stats.Mana >= mCost)
            {
                if (!testOnly)
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
                if (!testOnly)
                {
                    stats.Energy -= eCost;
                    stats.Mana -= mCost;
                }
                
                return true;
            }
        }

        return false;
    }
}
