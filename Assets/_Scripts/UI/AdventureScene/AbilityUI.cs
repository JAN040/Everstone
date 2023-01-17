using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Logic for the Ability button in the adventure scene
/// </summary>
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

    [SerializeField] KeyCode Hotkey;
    [SerializeField] bool HoldEnabled = false;

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

        if (Input.GetKeyDown(Hotkey))
        {
            if (HoldEnabled)
            {
                Debug.Log($"Button hold: {Hotkey}");
                StartCoroutine(HoldAction());
            }
            else
            {
                Activate();
            }
        }
        if (Input.GetKeyUp(Hotkey) && HoldEnabled)
        {
            Debug.Log($"Button release: {Hotkey}");
            StopAllCoroutines();
        }
    }

    private void OnDestroy()
    {
        //if (Ability != null)
        //    Ability.OnAbilityToggled -= ToggleAbility;
        StopAllCoroutines();
    }

    #endregion UNITY METHODS


    public void Initialize(Unit heroUnit, ScriptableAbility ability)
    {
        PlayerHeroUnit = heroUnit;
        Ability = ability;

        //if (Ability != null)
        //    Ability.OnAbilityToggled += ToggleAbility;

        UpdateUI(true);
    }

    //private void ToggleAbility(ScriptableAbility ability, bool isToggled)
    //{
        
    //}

    private void UpdateUI(bool init)
    {
        //blank ability tile
        if (Ability == null)
        {
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

            if (Ability.Name.ToUpper().Equals("BASIC ATTACK"))
            {
                InventoryItem equipRightArm = GameManager.Instance.PlayerManager.Equipment.GetItemAt((int)EquipmentSlot.RightArm);
                if (equipRightArm != null)
                {
                    AbilityImage.sprite = equipRightArm.ItemData.MenuIcon;
                }
            }

            Ability.SetCostText(CostText_Energy, CostText_Mana);
        }

        float cd = Ability.GetCooldownNormalized();

        //for the button to be interactable the ability needs to be off cooldown and have enough mana/energy for its use
        AbilityButton.interactable = (cd <= 0) && HandleAbilityCost(true);

        //cost should only be visible when the skill is ready
        CostPanel.SetActive(cd <= 0);

        CooldownImage.fillAmount = cd;
        CooldownText.text = Ability.GetCooldownText();

        //Update toggled animation
        if (Animator != null && Ability.ToggleMode != ToggleMode.None)
            Animator.SetBool("IsToggled", Ability.ToggleMode == ToggleMode.Toggled);

        if (PlayerHeroUnit.IsDead)
            AbilityButton.interactable = false;
    }

    //when ability is clicked
    public void Activate()
    {
        if (AbilityButton.interactable == false)
            return;

        //handle the cost
        if (!HandleAbilityCost(false))
        {
            Debug.LogWarning($"Tried to activate ability {Ability.Name} but it failed because it cost more than the player had.");
            return;
        }

        if (PlayerHeroUnit.IsDead)
            return;

        //handle the cooldown
        float cd = Ability.Cooldown;
        
        if (Ability.IsCDRValid) 
            cd -= cd * PlayerHeroUnit.Stats.CooldownReduction.GetValue();

        Ability.CooldownAtStart = cd;
        Ability.CurrentCooldown = cd;

        Ability.Activate();

        //in case the cost changes (toggleable abilities)
        Ability.SetCostText(CostText_Energy, CostText_Mana);
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
        (float eCost, float mCost) = Ability.GetCost();

        if (stats.Energy >= eCost && stats.Mana >= mCost)
        {
            if (!testOnly)
            {
                stats.Energy -= eCost;
                stats.Mana -= mCost;
            }
                
            return true;
        }

        return false;
    }

    IEnumerator HoldAction()
    {
        while (true)
        {
            Debug.Log($"Hotkey '{Hotkey}' Action!");
            Activate();

            yield return new WaitForSeconds(1f);
        }
    }
}
