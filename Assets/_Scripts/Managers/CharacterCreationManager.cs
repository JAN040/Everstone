using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCreationManager : MonoBehaviour
{
    #region VARIABLES

    private int availableSkillPoints;
    public int AvailableSkillPoints
    {
        get => availableSkillPoints;
        set
        {
            availableSkillPoints = value;
        }
    }


    #region UI Objects

    [SerializeField]
    [Tooltip("Reference to the hero class selection dropdown.")]
    TMP_Dropdown heroClassDropdown;

    [SerializeField]
    [Tooltip("Reference to the hero class selection dropdown.")]
    TMP_Dropdown heroBackgroundDropdown;

    [SerializeField]
    [Tooltip("Reference to the hero class selection dropdown.")]
    TMP_Dropdown difficultyDropdown;

    [SerializeField]
    TextMeshProUGUI SkillPointsText;



    #endregion UI Objects


    #endregion

    #region UNITY METHODS
    // Start is called before the first frame update
    void Start()
    {
        AvailableSkillPoints = 0;
        SetupCharacterCreation();
    }

    private void SetupCharacterCreation()
    {
        heroClassDropdown.ClearOptions();
        var classList = ResourceSystem.Instance.GetHeroClasses();
        heroClassDropdown.AddOptions(classList);
        heroClassDropdown.onValueChanged.AddListener(call: delegate { OnCharacterClassChanged(heroClassDropdown); });
        //heroClassDropdown.value = 1;

        difficultyDropdown.ClearOptions();
        var diffOpt = new List<string>(Enum.GetNames(typeof(Difficulty)));
        difficultyDropdown.AddOptions(diffOpt);
        difficultyDropdown.onValueChanged.AddListener(call: delegate { OnDifficultyChanged(difficultyDropdown); });
        difficultyDropdown.value = diffOpt.IndexOf(Difficulty.Normal.ToString());
        difficultyDropdown.RefreshShownValue();
    }

    public void OnCharacterClassChanged(TMP_Dropdown change)
    {

    }

    public void OnCharacterBackgroundChanged(TMP_Dropdown change)
    {

    }

    public void OnDifficultyChanged(TMP_Dropdown change)
    {
        switch ((Difficulty)change.value)
        {
            case Difficulty.Custom:
                AvailableSkillPoints = -1;
                break;
            case Difficulty.Casual:
                AvailableSkillPoints = 3;
                break;
            case Difficulty.Normal:
                AvailableSkillPoints = 1;
                break;
            case Difficulty.Hard:
                AvailableSkillPoints = 0;
                break;
            default:
                break;
        }

        UpdateUI();
    }

    private void UpdateUI()
    {

    }


    #endregion

}
