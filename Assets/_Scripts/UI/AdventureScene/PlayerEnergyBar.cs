using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerEnergyBar : MonoBehaviour
{
    #region VARIABLES


    #region UI References

    [Header("UI References")]

    [SerializeField] Image EnergyBar;
    [SerializeField] TextMeshProUGUI EnergyBar_Max_Text;
    [SerializeField] TextMeshProUGUI EnergyBar_Current_Text;

    [Space]
    [SerializeField] Image ManaBar;
    [SerializeField] TextMeshProUGUI ManaBar_Max_Text;
    [SerializeField] TextMeshProUGUI ManaBar_Current_Text;

    [Space]
    [SerializeField] List<GameObject> AbilityPrefabList;

    #endregion UI References


    [Space]
    [Header("Variables")]

    private List<ScriptableAbility> PlayerAbilities;
    private ScriptableHero PlayerHero;

    #endregion VARIABLES



    #region UNITY METHODS

    void Update()
    {
        UpdateUI();
    }

    #endregion UNITY METHODS


    public void Initialize(ScriptableHero hero, List<ScriptableAbility> abilities)
    {
        PlayerHero = hero;
        PlayerAbilities = abilities;

        SetAbilities();
        UpdateUI();
    }

    private void SetAbilities()
    {
        ScriptableAbility ability;
        
        for (int i = 0; i < AbilityPrefabList.Count; i++)
        {
            ability = (PlayerAbilities != null && PlayerAbilities.Count > i) ? PlayerAbilities[i] : null;

            AbilityPrefabList[i].GetComponent<AbilityUI>().Initialize(PlayerHero, ability);
        }
    }

    private void UpdateUI()
    {
        if (PlayerHero == null || PlayerHero.Prefab == null)
            return;

        var playerStats = PlayerHero.Prefab.GetComponent<Unit>().Stats;

        EnergyBar.fillAmount = playerStats.GetEnergyNormalized();
        EnergyBar_Max_Text.text = playerStats.MaxEnergy.GetValue().ToKiloString();
        EnergyBar_Current_Text.text = playerStats.Energy.ToKiloString();

        ManaBar.fillAmount = playerStats.GetManaNormalized();
        ManaBar_Max_Text.text = playerStats.MaxMana.GetValue().ToKiloString();
        ManaBar_Current_Text.text = playerStats.Mana.ToKiloString();
    }
}
