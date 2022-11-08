using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Scriptable/Ability/New Ability", fileName = "SO_Ability_")]
public class ScriptableAbility : ScriptableObject
{
    #region PROPERTIES


    public Ability Ability = Ability.OrdinaryAbility;

    public Sprite MenuImage;

    public string Name;

    [TextArea(4, 8)]
    public string Description;

    //0 => locked
    //when unlocked starts at level 1
    public int Level = 0;
    public int MaxLevel = 5;

    /// <summary>
    /// The amount of currency that is spent for one ability level upgrade
    /// </summary>
    public int UpgradeCost = 50;

    /// <summary>
    /// On every level-up, the UpgradeCost is multiplied by this amount
    /// </summary>
    public int CostPerLevelMultiplier = 3;

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

    /// <summary>
    /// Amount of cooldown when the ability went on cooldown.
    /// Not necessarily the same as Cooldown because of the AntiSpamCooldown mechanic
    /// </summary>
    [NonSerialized]
    public float CooldownAtStart;

    [Space]
    public List<UnlockCondition> UnlockConditions;

    /// <summary>
    /// List of effects that are activated when ability is activated
    /// or OnToggle for toggle-able abilities
    /// </summary>
    public List<StatusEffect> OnActivedEffects;

    [Space]
    public float EnergyCost = 0f;
    public float ManaCost = 0f;

    [Space]
    [Header("Flags")]

    /// <summary>
    /// If true, the ability will benefit from the players Cooldown reduction Stat.
    /// </summary>
    public bool IsCDRValid = true;

    /// <summary>
    /// Defines whether its effect can be applied at range (for targetable abilities)
    /// </summary>
    //public bool IsRanged = false;

    /// <summary>
    /// Defines whether the ability ignores the selected enemy/ally target and chooses a random one
    /// </summary>
    //public bool IsRandomTarget;

    /// <summary>
    /// Only selected abilities will be taken into battle; only valid for 'classic' abilities
    /// </summary>
    public bool IsSelected = false;


    [Space]
    [Header("TOGGLE-ABLE ABILITIES")]
    /// <summary>
    /// Defines whether the ability can be toggled on & off
    /// </summary>
    public ToggleMode ToggleMode = ToggleMode.None;

    /// <summary>
    /// List of effects that are activated when ability is untoggled (only used for toggleable abilities)
    /// </summary>
    public List<StatusEffect> OnDeactivatedEffects;

    [Space]
    [Header("Deactivation (toggle)")]
    public float EnergyCost_Deactivate = 0f;
    public float ManaCost_Deactivate = 0f;

    [Tooltip("If not equal to -1, defines after how many seconds the ability forcefully deactivates")]
    public float AutoDeactivateAfterSeconds = -1f;

    #endregion PROPERTIES


    public event Action<ScriptableAbility> OnAbilityActivated;
    public event Action<ScriptableAbility, bool> OnAbilityToggled;


    public void Activate()
    {
        if (ToggleMode != ToggleMode.None)
        {
            if (ToggleMode == ToggleMode.Toggled)
                ToggleMode = ToggleMode.UnToggled;
            else
                ToggleMode = ToggleMode.Toggled;

            Debug.Log($"Toggled ability '{this.Name}'. It is now {ToggleMode}");
            
            OnAbilityToggled?.Invoke(this, ToggleMode == ToggleMode.Toggled);

            return;
        }
        else
            Debug.Log($"Activated ability '{this.Name}'");
        
        OnAbilityActivated?.Invoke(this);
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
            CooldownAtStart = cdAmount;
            CurrentCooldown = cdAmount;
        }
    }

    public void SetCostText(TextMeshProUGUI costText_Energy, TextMeshProUGUI costText_Mana)
    {
        (float eCost, float mCost) = GetCost();

        if (costText_Energy != null)
        {
            if (eCost > 0f)
                costText_Energy.text = $"<color=#C06217>{eCost}</color>";
            else
                costText_Energy.text = string.Empty;
        }

        if (costText_Mana != null)
        {
            if (mCost > 0f)
                costText_Mana.text = $"<color=#27A3FD>{mCost}</color>";
            else
                costText_Mana.text = string.Empty;
        }
    }

    public string GetCostTextWithIcons()
    {
        (float eCost, float mCost) = GetCost();
        string res = string.Empty;

        if (eCost > 0f)
        {
            res += $"<color=#C06217>{eCost}</color>  {ResourceSystem.GetIconTag(Icon.Stamina)}";

            if (mCost > 0f)
            {
                res += $"  <color=#27A3FD>{mCost}</color> {ResourceSystem.GetIconTag(Icon.Mana)}";
                return res;
            }
        }
        
        if (mCost > 0f)
            res += $"<color=#27A3FD>{mCost}</color> {ResourceSystem.GetIconTag(Icon.Mana)}";

        return res;
    }

    public (float eCost, float mCost) GetCost()
    {
        float eCost;
        float mCost;

        if (ToggleMode.In(ToggleMode.None, ToggleMode.UnToggled))
        {
            eCost = EnergyCost;
            mCost = ManaCost;
        }
        else
        {
            //when the ability is toggled, this is the price to untoggle it
            eCost = EnergyCost_Deactivate;
            mCost = ManaCost_Deactivate;
        }

        return (eCost, mCost);
    }

    public float GetCooldownNormalized()
    {
        return CooldownAtStart <= 0 ? 0 : CurrentCooldown / CooldownAtStart;
    }

    //level up the ability
    public void Upgrade()
    {
        if (Level == MaxLevel)
            return;

        Level++;
        UpgradeCost *= CostPerLevelMultiplier;

        //increase effect values
        if (OnActivedEffects != null)
            foreach (var effect  in OnActivedEffects)
            {
                effect.EffectValue += effect.PerLevelValueChange;
                effect.Duration    += effect.PerLevelDurationChange;
            }

        if (OnDeactivatedEffects != null)
            foreach (var effect in OnDeactivatedEffects)
            {
                effect.EffectValue += effect.PerLevelValueChange;
                effect.Duration += effect.PerLevelDurationChange;
            }
    }

    public string GetDescription()
    {
        string res = string.Empty;
        res += $"Cost: {GetCostTextWithIcons()}" + Environment.NewLine;
        res += $"Cooldown: {Cooldown}" + Environment.NewLine + Environment.NewLine;

        res += "Effects:" + Environment.NewLine + Environment.NewLine;

        foreach (var effect in OnActivedEffects)
        {
            res += "- " + effect.GetEffectDescription(IsMaxed()) + Environment.NewLine;
        }

        //for toggled ability
        if (ToggleMode != ToggleMode.None)
        {
            res += Environment.NewLine;

            if (OnDeactivatedEffects.Count != 0)
                res += "When untoggled:" + Environment.NewLine + Environment.NewLine;

            foreach (var effect in OnDeactivatedEffects)
            {
                res += "- " + effect.GetEffectDescription(IsMaxed()) + Environment.NewLine;
            }

            if (AutoDeactivateAfterSeconds != -1)
                res += $"\nAuto-deactivates after {AutoDeactivateAfterSeconds}s";
        }

        return res;
    }

    public bool IsMaxed()
    {
        return Level == MaxLevel;
    }
}

