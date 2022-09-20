using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Scriptable/Units/New Hero Ability", fileName = "SO_Ability_")]
public class ScriptableAbility : ScriptableObject
{
    #region PROPERTIES


    public Ability Ability;

    public Sprite MenuImage;

    public string Name;

    [TextArea(3, 5)]
    public string Description;

    //0 => locked
    //when unlocked starts at level 1
    public int Level = 0;

    /// <summary>
    /// Defines whether the ability can be toggled on & off
    /// </summary>
    public ToggleMode ToggleMode = ToggleMode.None;

    /// <summary>
    /// The amount (in seconds) it will take for the skill to become available after activation
    /// </summary>
    public float Cooldown = 0;

    private float currentCooldown = 0;
    /// <summary>
    /// Amount of time (in seconds) left till skill becomes available
    /// </summary>
    public float CurrentCooldown
    {
        get => currentCooldown;
        set
        {
            currentCooldown = value;

            if (currentCooldown < 0)
                currentCooldown = 0;
        }
    }


    [Space]
    [Header("Effect values")]

    /// <summary>
    /// The value (damage/heal multiplier, stat buff, etc.) of the skill effect.
    /// Since the skill effect is hardcoded, keeping the values separated makes it easier to modify.
    /// </summary>
    public float EffectValue;
    public float EffectValue_2;
    public float EffectValue_3;
    public float EffectValue_4;
    public float EffectValue_5;

    

    //TODO
    //[SerializeField] List<UnlockCondition> UnlockConditions;

    //Hardcoded in AdventureManager or something
    //public List<Effect> Effects;


    [Space]
    [Header("Flags")]

    /// <summary>
    /// If true, the ability will benefit from the players Cooldown reduction Stat.
    /// </summary>
    public bool IsCDRValid = true;

    /// <summary>
    /// Defines whether its effect can be applied at range (for targetable abilities)
    /// </summary>
    public bool IsRanged = false;

    /// <summary>
    /// Defines whether the ability ignores the selected enemy/ally target and chooses a random one
    /// </summary>
    public bool IsRandomTarget;

    /// <summary>
    /// Only selected abilities will be taken into battle; only valid for 'classic' abilities
    /// </summary>
    public bool IsSelected = false;


    [Space]
    [Header("Cost")]

    public float UnlockCost;


    [Space]

    public CostType CostType = CostType.Energy;
    public float EnergyCost = 0f;
    public float ManaCost = 0f;


    #endregion PROPERTIES


    public event Action<ScriptableAbility> OnAbilityActivated;


    public void Activate()
    {
        OnAbilityActivated?.Invoke(this);
        Debug.Log($"Activated ability '{this.Name}'");
    }


    public float GetCooldownNormalized()
    {
        return Cooldown <= 0 ? 0 : CurrentCooldown / Cooldown;
    }

    public string GetCooldownText()
    {
        //if more than a minute
        if (CurrentCooldown > 60f)
            return $"{ (int)CurrentCooldown / 60 }m";
        else if (CurrentCooldown > 0)
            return $"{ (int)CurrentCooldown }";
        else
            return "";
    }

    public void AddAntiSpamCooldown(float cdAmount)
    {
        if (CurrentCooldown < cdAmount)
        {
            CurrentCooldown = cdAmount;
        }
    }

    public string GetCostText()
    {
        return null;
        //return $"<color=#27A3FD>{mCost}</color> <color=#C06217>{eCost}</color>";
    }

    public void SetCostText(TextMeshProUGUI costText_Energy, TextMeshProUGUI costText_Mana)
    {
        string eCost = EnergyCost <= 0 ? "" : $"{EnergyCost}";
        string mCost =   ManaCost <= 0 ? "" : $"{ManaCost}";

        if (costText_Energy != null)
            costText_Energy.text = $"<color=#C06217>{eCost}</color>";
        if (costText_Mana != null)
            costText_Mana.text = $"<color=#27A3FD>{mCost}</color>";
    }
}

