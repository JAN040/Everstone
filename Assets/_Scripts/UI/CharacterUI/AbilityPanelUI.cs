using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


/// <summary>
/// Logic for the ability panel in the Abilities Tab of the CharacterUI
/// </summary>
public class AbilityPanelUI : MonoBehaviour
{
    #region VARIABLES


    #region UI References


    [Header("UI References")]
    [SerializeField] Image Image_Icon;
    [SerializeField] TextMeshProUGUI Text_Name;
    [SerializeField] TextMeshProUGUI Text_Level;
    [SerializeField] TextMeshProUGUI Text_UpgradeCost;
    [SerializeField] GameObject AbilityInfoBoxPrefab;

    [SerializeField] Button Button_Upgrade;
    [SerializeField] Button Button_Equip;

    [Space]
    [Header("UI References")]
    [SerializeField] GameObject LockPanel;
    [SerializeField] Button Button_Unlock;
    [SerializeField] TextMeshProUGUI Text_UnlockConditions;
    [SerializeField] GameObject UnlockConditions_Panel;


    #endregion UI References


    //[Space]
    //[Header("Variables")]
    private ScriptableAbility AbilityRef;
    private CharacterUI CharacterUIRef;


    #endregion VARIABLES


    #region UNITY METHODS


    private void OnEnable()
    {
        UpdateUI();
    }


    #endregion UNITY METHODS


    #region METHODS


    public void Init(ScriptableAbility ability, CharacterUI characterUI)
    {
        AbilityRef = ability;
        CharacterUIRef = characterUI;

        UpdateUI();
    }

    public void UpdateUI()
    {
        if (AbilityRef == null)
            return;

        Image_Icon.sprite = AbilityRef.MenuImage;
        Text_Name.text = AbilityRef.Name;
        Text_Level.text = $"Lvl {AbilityRef.Level}";
        Text_UpgradeCost.text = GetUpgradeCostText();
        
        Button_Upgrade.interactable = !IsMaxLevel() && GameManager.CanAfford(AbilityRef.UpgradeCost);
        SetEquipButtonText();

        //disable equipping when all ability slots are full
        Button_Equip.interactable = CharacterUIRef.HasFreeAbilitySlots();

        //the "Dodge" ability can only be upgraded, but must always be equipped
        if (AbilityRef.Ability == Ability.Dodge)
        {
            if (!AbilityRef.IsSelected)
                AbilityRef.IsSelected = true;
            Button_Equip.interactable = false;
        }

        RefreshUnlockConditions();
    }

    private void RefreshUnlockConditions()
    {
        Text_UnlockConditions.text = string.Empty;
        LockPanel.SetActive(true);
        UnlockConditions_Panel.SetActive(true);
        Button_Unlock.gameObject.SetActive(true);

        //special case for shield block which doesn't have a level req., but instead needs a shield
        //  to be equipped
        if (AbilityRef.Ability == Ability.ShieldBlock)
        {
            bool isShieldEquipped = GameManager.Instance.PlayerManager.Equipment.HasShieldEquipped();
            
            if (isShieldEquipped)
                LockPanel.SetActive(false);
            else
            {
                LockPanel.SetActive(true);
                Text_UnlockConditions.text = $"Only available when a shield is equipped.";
                Button_Unlock.gameObject.SetActive(false);
                
                AbilityRef.IsSelected = false;
                CharacterUIRef.UnequipAbility(AbilityRef);
            }

            return;
        }

        var unfulfilledConditions = 
            GameManager.Instance.PlayerManager.PlayerHero.LevelSystem.GetUnfulfilledConditions(AbilityRef.UnlockConditions);

        if (unfulfilledConditions.Count > 0)
        {
            //ability is definitely locked
            Button_Unlock.interactable = false;

            Text_UnlockConditions.text = GetConditionsText(unfulfilledConditions);

            return;
        }

        //ability might still be locked, but can be unlocked now
        if (AbilityRef.Level < 1)   //level 0 means the ability is locked
        {
            Button_Unlock.interactable = true;
            UnlockConditions_Panel.SetActive(false);    //to disable scrolling empty text
        }
        else
        {
            LockPanel.SetActive(false);
        }
    }

    private string GetConditionsText(List<UnlockCondition> unfulfilledConditions)
    {
        string res = string.Empty;

        foreach (var condition in unfulfilledConditions)
        {
            res += $"{condition.Skill} Lvl. {condition.Level}" + Environment.NewLine;
        }

        return res;
    }

    private void SetEquipButtonText()
    {
        var buttonText = Button_Equip.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        if (buttonText == null)
        {
            Debug.LogWarning("Couldnt find Text object in children of Button_Equip component!");
            return;
        }

        buttonText.text = AbilityRef.IsSelected ? "Unequip" : "Equip";
    }

    private string GetUpgradeCostText()
    {
        string text = string.Empty;

        if (IsMaxLevel())
        {
            return "Max level.";
        }

        text += $"Lvl {AbilityRef.Level} -> Lvl {AbilityRef.Level + 1}";
        text += Environment.NewLine;
        text += GameManager.Instance.CurrencyToDisplayString(AbilityRef.UpgradeCost);

        return text;
    }

    private bool IsMaxLevel()
    {
        if (AbilityRef == null)
            return true;

        return AbilityRef.Level == AbilityRef.MaxLevel;
    }

    public void IconButtonClicked()
    {
        //Debug.Log($"Clicked ability {AbilityRef.Name}");
        var obj = Instantiate(AbilityInfoBoxPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        obj.transform.localScale = Vector3.one;
        obj.transform.localPosition = Vector3.zero;

        obj.GetComponent<AbilityInfoBox>().Init(AbilityRef);
    }

    public void UpgradeClicked()
    {
        GameManager.Instance.Currency -= AbilityRef.UpgradeCost;
        AbilityRef.Upgrade();

        // refresh everything because we might not be able to afford some upgrades
        CharacterUIRef.RefreshAbilityPanels();
    }

    public void EquipClicked()
    {
        AbilityRef.IsSelected = !AbilityRef.IsSelected;
        
        if (AbilityRef.IsSelected)
            CharacterUIRef.EquipAbility(AbilityRef);
        else
            CharacterUIRef.UnequipAbility(AbilityRef);

        // refresh everything because we could have hit max selected abilities/we are no longer at max abilities
        CharacterUIRef.RefreshAbilityPanels();
    }

    public void UnlockClicked()
    {
        if (AbilityRef.Level < 1)
            AbilityRef.Level = 1;

        LockPanel.SetActive(false);
        UpdateUI();
    }


    #endregion METHODS
}
