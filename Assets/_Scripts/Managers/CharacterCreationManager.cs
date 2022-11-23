using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCreationManager : MonoBehaviour
{
    #region VARIABLES

    [Header("Max allocatable points")]
    [SerializeField] int maxSkillPoints;


    [Header("Currently available points")]
    [SerializeField] int _availableSkillPoints;
    public int AvailableSkillPoints
    {
        get => _availableSkillPoints;
        set
        {
            //dont update amount when -1, to preserve infinity
            if (_availableSkillPoints != -1)
            {
                _availableSkillPoints = value;
            }
            UpdateUI();
        }
    }

    [Header("Current base points (from chosen class)")]
    [SerializeField] int basePointsAmount_Constitution;
    [SerializeField] int basePointsAmount_Strength;
    [SerializeField] int basePointsAmount_Agility;
    [SerializeField] int basePointsAmount_Arts;

    [Header("Current background bonus points (from chosen background)")]
    [SerializeField] int bgPointsAmount_Constitution;
    [SerializeField] int bgPointsAmount_Strength;
    [SerializeField] int bgPointsAmount_Agility;
    [SerializeField] int bgPointsAmount_Arts;

    [Header("Currently allocated points")]
    [SerializeField] int allocatedPointsAmount_Constitution;
    [SerializeField] int allocatedPointsAmount_Strength;
    [SerializeField] int allocatedPointsAmount_Agility;
    [SerializeField] int allocatedPointsAmount_Arts;

    private int PointsAmount_Constitution => basePointsAmount_Constitution + bgPointsAmount_Constitution + allocatedPointsAmount_Constitution;
    private int PointsAmount_Strength => basePointsAmount_Strength + bgPointsAmount_Strength + allocatedPointsAmount_Strength;
    private int PointsAmount_Agility => basePointsAmount_Agility + bgPointsAmount_Agility + allocatedPointsAmount_Agility;
    private int PointsAmount_Arts => basePointsAmount_Arts + bgPointsAmount_Arts + allocatedPointsAmount_Arts;



    #region UI References

    [Space]
    [Header("UI component references")]
    [Space]

    [Header("Skill points text field")]
    [SerializeField] TextMeshProUGUI SkillPointsText;

    [Header("Start Button")]
    [SerializeField] Button startButton;

    [Header("Hero name textField")]
    [SerializeField] TMP_InputField heroName;

    [Header("Icon selector")]
    [SerializeField]
    [Tooltip("Reference to the dropdown menu the player uses to select their picture.")]
    TMP_Dropdown iconDropdown;
    [SerializeField]
    [Tooltip("List of available avatar icons")]
    public List<Sprite> playerIconList;

    [Space]
    [Header("Dropdown controls")]

    [SerializeField]
    [Tooltip("Reference to the hero class selection dropdown.")]
    TMP_Dropdown heroClassDropdown;
    [SerializeField]
    [Tooltip("Reference to the hero class selection dropdown.")]
    TMP_Dropdown heroBackgroundDropdown;
    [SerializeField]
    [Tooltip("Reference to the hero class selection dropdown.")]
    TMP_Dropdown difficultyDropdown;

    //skill point allocation controls
    [Header("Constitution")]
    [SerializeField] TextMeshProUGUI AllocatedSkillPoints_Constitution;
    [SerializeField] Button SkillPointsButton_Plus_Constitution;
    [SerializeField] Button SkillPointsButton_Minus_Constitution;

    [Header("Strength")]
    [SerializeField] TextMeshProUGUI AllocatedSkillPoints_Strength;
    [SerializeField] Button SkillPointsButton_Plus_Strength;
    [SerializeField] Button SkillPointsButton_Minus_Strength;

    [Header("Agility")]
    [SerializeField] TextMeshProUGUI AllocatedSkillPoints_Agility;
    [SerializeField] Button SkillPointsButton_Plus_Agility;
    [SerializeField] Button SkillPointsButton_Minus_Agility;

    [Header("Arts")]
    [SerializeField] TextMeshProUGUI AllocatedSkillPoints_Arts;
    [SerializeField] Button SkillPointsButton_Plus_Arts;
    [SerializeField] Button SkillPointsButton_Minus_Arts;

    [Header("Other")]
    [SerializeField] Toggle HardcoreCheckbox;
    [SerializeField] Toggle KeepInventoryCheckbox;
    

    [SerializeField] Sprite MageBasicAtkImage;


    #endregion UI References


    #endregion VARIABLES


    #region UNITY METHODS

    // Start is called before the first frame update
    void Start()
    {
        maxSkillPoints = 5;
        AvailableSkillPoints = 0;

        allocatedPointsAmount_Constitution = 0;
        allocatedPointsAmount_Strength = 0;
        allocatedPointsAmount_Agility = 0;
        allocatedPointsAmount_Arts = 0;

        SetupCharacterCreation();
    }

    #endregion


    private void SetupCharacterCreation()
    {
        //setup dropdowns

        iconDropdown.ClearOptions();
        iconDropdown.AddOptions(playerIconList);

        heroClassDropdown.ClearOptions();
        heroClassDropdown.AddOptions(ResourceSystem.Instance.GetHeroClassNames());
        heroClassDropdown.onValueChanged.AddListener(call: delegate { OnCharacterClassChanged(); });
        //setup base points
        OnCharacterClassChanged();

        heroBackgroundDropdown.ClearOptions();
        heroBackgroundDropdown.AddOptions(ResourceSystem.Instance.GetHeroBackgrounds());
        heroBackgroundDropdown.onValueChanged.AddListener(call: delegate { OnCharacterBackgroundChanged(); });
        OnCharacterBackgroundChanged();

        difficultyDropdown.ClearOptions();
        var diffOpt = new List<string>(Enum.GetNames(typeof(Difficulty)));
        difficultyDropdown.AddOptions(diffOpt);
        difficultyDropdown.onValueChanged.AddListener(call: delegate { OnDifficultyChanged(); });
        difficultyDropdown.value = diffOpt.IndexOf(Difficulty.Normal.ToString());
        difficultyDropdown.RefreshShownValue();

        UpdateUI();
    }

    public void OnCharacterClassChanged()
    {
        var chosenHero = GetSelectedHero();

        basePointsAmount_Constitution = chosenHero.pointAllocationData.GetBaseSkillPoints(Skill.Constitution);
        basePointsAmount_Strength     = chosenHero.pointAllocationData.GetBaseSkillPoints(Skill.Strength);
        basePointsAmount_Agility      = chosenHero.pointAllocationData.GetBaseSkillPoints(Skill.Agility);
        basePointsAmount_Arts         = chosenHero.pointAllocationData.GetBaseSkillPoints(Skill.Arts);

        DeallocateAllPoints();
        UpdateUI();
    }

    public void OnCharacterBackgroundChanged()
    {
        var chosenBackground = GetSelectedBackground();

        bgPointsAmount_Constitution = chosenBackground.pointAllocationData.GetBaseSkillPoints(Skill.Constitution);
        bgPointsAmount_Strength     = chosenBackground.pointAllocationData.GetBaseSkillPoints(Skill.Strength);
        bgPointsAmount_Agility      = chosenBackground.pointAllocationData.GetBaseSkillPoints(Skill.Agility);
        bgPointsAmount_Arts         = chosenBackground.pointAllocationData.GetBaseSkillPoints(Skill.Arts);

        DeallocateAllPoints();
        UpdateUI();
    }

    public void OnDifficultyChanged()
    {
        DeallocateAllPoints();

        UpdateUI();
    }

    private void ReassignSpendablePoints()
    {
        switch ((Difficulty)difficultyDropdown.value)
        {
            case Difficulty.Custom:
                _availableSkillPoints = -1;
                maxSkillPoints = 10;
                break;
            case Difficulty.Casual:
                _availableSkillPoints = 3;
                maxSkillPoints = 5;
                break;
            case Difficulty.Normal:
                _availableSkillPoints = 1;
                maxSkillPoints = 5;
                break;
            case Difficulty.Hard:
                _availableSkillPoints = 0;
                maxSkillPoints = 5;
                break;
            default:
                break;
        }
    }

    
    /// <summary>
    /// Updates stat allocation texts, available skill points and calls an update to stat allocation
    ///     buttons interactibility
    /// </summary>
    public void UpdateUI()
    {
        //-1 means infinite in this case
        if (AvailableSkillPoints == -1)
        {   //Show infinity symbol
            SkillPointsText.text = ResourceSystem.GetIconTag(Icon.Infinity);
        }
        else
        {
            SkillPointsText.text = AvailableSkillPoints.ToString();
        }

        AllocatedSkillPoints_Constitution.text  = PointsAmount_Constitution.ToString();
        AllocatedSkillPoints_Strength.text      = PointsAmount_Strength.ToString();
        AllocatedSkillPoints_Agility.text       = PointsAmount_Agility.ToString();
        AllocatedSkillPoints_Arts.text          = PointsAmount_Arts.ToString();

        //if hero name is empty disable start button
        startButton.interactable = IsCreationComplete();

        UpdateStatAllocationButtonsInteractable();
    }

    private bool IsCreationComplete()
    {
        return heroName.text.Length > 1 &&  //name must be longer than one char
               iconDropdown.value != -1;    //an icon must be selected
    }

    private void UpdateStatAllocationButtonsInteractable()
    {
        bool hasPoints = AvailableSkillPoints == -1 || AvailableSkillPoints > 0;

        //plus button: is enabled when points are available and only up to 5 points in each skill
        SkillPointsButton_Plus_Constitution.interactable = hasPoints && PointsAmount_Constitution < maxSkillPoints;
        SkillPointsButton_Plus_Strength.interactable     = hasPoints && PointsAmount_Strength     < maxSkillPoints;
        SkillPointsButton_Plus_Agility.interactable      = hasPoints && PointsAmount_Agility      < maxSkillPoints;
        SkillPointsButton_Plus_Arts.interactable         = hasPoints && PointsAmount_Arts         < maxSkillPoints;

        //minus buttons are enabled when some points are allocated in the corresponding skill
        if (allocatedPointsAmount_Constitution > 0)
            SkillPointsButton_Minus_Constitution.interactable = true;
        else
            SkillPointsButton_Minus_Constitution.interactable = false;

        if (allocatedPointsAmount_Strength > 0)
            SkillPointsButton_Minus_Strength.interactable = true;
        else
            SkillPointsButton_Minus_Strength.interactable = false;

        if (allocatedPointsAmount_Agility > 0)
            SkillPointsButton_Minus_Agility.interactable = true;
        else
            SkillPointsButton_Minus_Agility.interactable = false;

        if (allocatedPointsAmount_Arts > 0)
            SkillPointsButton_Minus_Arts.interactable = true;
        else
            SkillPointsButton_Minus_Arts.interactable = false;
    }

    private void DeallocateAllPoints()
    {
        allocatedPointsAmount_Constitution = 0;
        allocatedPointsAmount_Strength     = 0;
        allocatedPointsAmount_Agility      = 0;
        allocatedPointsAmount_Arts         = 0;

        ReassignSpendablePoints();
    }


    public void AllocatePoint_Constitution(int amount)
    {
        allocatedPointsAmount_Constitution  += amount;
        AvailableSkillPoints                -= amount;
    }

    public void AllocatePoint_Strength(int amount)
    {
        allocatedPointsAmount_Strength  += amount;
        AvailableSkillPoints            -= amount;
    }

    public void AllocatePoint_Agility(int amount)
    {
        allocatedPointsAmount_Agility   += amount;
        AvailableSkillPoints            -= amount;
    }

    public void AllocatePoint_Arts(int amount)
    {
        allocatedPointsAmount_Arts  += amount;
        AvailableSkillPoints        -= amount;
    }

    /// <summary>
    /// Create a random character
    /// </summary>
    public void Randomize()
    {
        heroName.text = "Player";
        iconDropdown.value           = UnityEngine.Random.Range(0, iconDropdown.options.Count);
        heroClassDropdown.value      = UnityEngine.Random.Range(0, heroClassDropdown.options.Count);
        heroBackgroundDropdown.value = UnityEngine.Random.Range(1, heroBackgroundDropdown.options.Count);
        difficultyDropdown.value     = UnityEngine.Random.Range(1, difficultyDropdown.options.Count);
        KeepInventoryCheckbox.isOn = Helper.DiceRoll(0.5f);
        HardcoreCheckbox     .isOn = Helper.DiceRoll(0.25f);

        iconDropdown.RefreshShownValue();
        heroClassDropdown.RefreshShownValue();
        heroBackgroundDropdown.RefreshShownValue();
        iconDropdown.RefreshShownValue();

        OnCharacterClassChanged();
        OnCharacterBackgroundChanged();
        OnDifficultyChanged();

        //randomly allocate points
        while (AvailableSkillPoints > 0)
        {
            int roll = UnityEngine.Random.Range(0, 4);
            switch (roll)
            {
                case 0:
                    AllocatePoint_Constitution(1);
                    break;
                case 1:
                    AllocatePoint_Strength(1);
                    break;
                case 2:
                    AllocatePoint_Agility(1);
                    break;
                case 3:
                    AllocatePoint_Arts(1);
                    break;
                default:
                    Debug.LogWarning($"unexpected roll: {roll}!");
                    break;
            }
        }
    }

    private ScriptableHero GetSelectedHero()
    {
        return ResourceSystem.Instance.GetHeroByName(heroClassDropdown.options[heroClassDropdown.value].text);
    }

    private ScriptableBackground GetSelectedBackground()
    {
        return ResourceSystem.Instance.GetBackground(heroBackgroundDropdown.options[heroBackgroundDropdown.value].text);
    }

    private Sprite GetSelectedCharacterPortrait()
    {
        return iconDropdown.options[iconDropdown.value].image;
    }

    public void OnFinishCreation()
    {
        var hero = GetSelectedHero();
        var stats = hero.Stats;

        hero.Name = heroName.text;
        hero.Background = GetSelectedBackground().backgroundName;

        //hero instantiation
        GameManager.Instance.PlayerManager.Init(hero);

        //inventory setup
        GameManager.Instance.PlayerManager.SetInventory(
            new InventorySystem(GameManager.Instance.PlayerInventorySpace),
            new InventorySystem(GameManager.Instance.CampStorageSpace),
            new EquipmentSystem((int)Enum.GetValues(typeof(EquipmentSlot)).Cast<EquipmentSlot>().Max() + 1, false),
            new EquipmentSystem(6, true)
        );

        //test items
        GameManager.Instance.PlayerManager.Equipment.EquipItem(new InventoryItem(ResourceSystem.Instance.Items_Equipment[0]));
        GameManager.Instance.PlayerManager.Equipment.EquipItem(new InventoryItem(ResourceSystem.Instance.Items_Equipment[2]));
        GameManager.Instance.PlayerManager.Runes.EquipItem(new InventoryItem(ResourceSystem.Instance.Items_Equipment[4]));
        GameManager.Instance.PlayerManager.Inventory.AddItem(new InventoryItem(ResourceSystem.Instance.Items_Equipment[4]));
        GameManager.Instance.PlayerManager.Inventory.AddItem(new InventoryItem(ResourceSystem.Instance.Items_Equipment[1]));
        GameManager.Instance.PlayerManager.Inventory.AddItem(new InventoryItem(ResourceSystem.Instance.Items_Equipment[3]));
        GameManager.Instance.PlayerManager.Inventory.AddItem(new InventoryItem(ResourceSystem.Instance.Items_Equipment[5]));
        GameManager.Instance.PlayerManager.Inventory.AddItem(new InventoryItem(ResourceSystem.Instance.Items_Equipment[6]));
        GameManager.Instance.PlayerManager.Inventory.AddItem(new InventoryItem(ResourceSystem.Instance.Items_Equipment[7]));
        for (int i = 0; i < GameManager.Instance.PlayerManager.Storage.InventorySize; i++)
            GameManager.Instance.PlayerManager.Storage.PlaceItemAtSlot(new InventoryItem(ResourceSystem.Instance.Items_Loot[UnityEngine.Random.Range(0, ResourceSystem.Instance.Items_Loot.Count)]), i);

        GameManager.Instance.PlayerManager.PlayerHero.MenuSprite = GetSelectedCharacterPortrait();
        GameManager.Instance.PlayerManager.PlayerHero.SetLevelSystem(new LevelSystem(GenerateSkillSystem(stats), stats));

        //player abilities setup
        GameManager.Instance.PlayerManager.SetAbilities(
            ResourceSystem.Instance.GetPlayerAbilities()
        );

        //change the default basic attack sprite for mage class
        if (hero.ClassName.ToUpper().Equals("MAGE"))
        {
            var basicAtkAbility = GameManager.Instance.PlayerManager.Abilities.FirstOrDefault(x => x.Ability == Ability.BasicAttack);

            if (basicAtkAbility != null)
                basicAtkAbility.MenuImage = MageBasicAtkImage;
        }

        GameManager.Instance.Currency = GetSelectedBackground().startingCurrencyAmount;
        GameManager.Instance.SetGameDifficulty(
            (Difficulty)difficultyDropdown.value, 
            KeepInventoryCheckbox.isOn,
            HardcoreCheckbox.isOn
        );

        //leave character creation scene
        SceneManagementSystem.Instance.LoadScene(Scenes.Outskirts);
    }

    public Dictionary<string, SkillLevel> GenerateSkillSystem(CharacterStats stats)
    {
        ScriptableHero       hero       = GetSelectedHero();
        ScriptableBackground background = GetSelectedBackground();
        var allocationData = new List<PointAllocationData>() 
        { 
            hero.pointAllocationData, 
            background.pointAllocationData 
        };

        int points_Lockpicking = PointAllocationData.GetTotalPoints(Skill.Lockpicking,  allocationData);
        int points_Taming      = PointAllocationData.GetTotalPoints(Skill.Taming,       allocationData);
        int points_Trading     = PointAllocationData.GetTotalPoints(Skill.Trading,      allocationData);

        //int points_Dagger = PointAllocationData.GetTotalWeaponPoints(WeaponType.Dagger, allocationData);
        //int points_Sword  = PointAllocationData.GetTotalWeaponPoints(WeaponType.Sword,  allocationData);
        //int points_Axe    = PointAllocationData.GetTotalWeaponPoints(WeaponType.Axe,    allocationData);
        //int points_Shield = PointAllocationData.GetTotalWeaponPoints(WeaponType.Shield, allocationData);
        //int points_Staff  = PointAllocationData.GetTotalWeaponPoints(WeaponType.Staff,  allocationData);


        return new Dictionary<string, SkillLevel>()
        {
            {
                Skill.Constitution.ToString(),
                new SkillLevel(Skill.Constitution, PointsAmount_Constitution,
                    proficiency:PointsAmount_Constitution/10f)
            },
            {
                Skill.Strength.ToString(),
                new SkillLevel(Skill.Strength, PointsAmount_Strength,
                    proficiency:PointsAmount_Strength/10f)
            },
            {
                Skill.Agility.ToString(),
                new SkillLevel(Skill.Agility, PointsAmount_Agility,
                    proficiency:PointsAmount_Agility/10f)
            },
            {
                Skill.Arts.ToString(),
                new SkillLevel(Skill.Arts, PointsAmount_Arts,
                    proficiency:PointsAmount_Arts/10f)
            },

            //Misc. skills
            {
                Skill.Lockpicking.ToString(),
                new SkillLevel(Skill.Lockpicking, points_Lockpicking,
                    proficiency:points_Lockpicking/10f)
            },
            {
                Skill.Taming.ToString(),
                new SkillLevel(Skill.Taming, points_Taming,
                    proficiency:points_Taming/10f)
            },
            {
                Skill.Trading.ToString(),
                new SkillLevel(Skill.Trading, points_Trading,
                    proficiency:points_Trading/10f)
            },

            //weapon skills
            //{
            //    WeaponType.Dagger.ToString(),
            //    new EquipmentSkillLevel(Skill.Equipment_Skill, stats, points_Dagger,
            //        0, points_Dagger/10f, WeaponType.Dagger)
            //},
            //{
            //    WeaponType.Sword.ToString(),
            //    new EquipmentSkillLevel(Skill.Equipment_Skill, stats, points_Sword,
            //        0, points_Sword/10f, WeaponType.Sword)
            //},
            //{
            //    WeaponType.Axe.ToString(),
            //    new EquipmentSkillLevel(Skill.Equipment_Skill, stats, points_Axe,
            //        0, points_Axe/10f, WeaponType.Axe)
            //},
            //{
            //    WeaponType.Shield.ToString(),
            //    new EquipmentSkillLevel(Skill.Equipment_Skill, stats, points_Shield,
            //        0, points_Shield/10f, WeaponType.Shield)
            //},
            //{
            //    WeaponType.Staff.ToString(),
            //    new EquipmentSkillLevel(Skill.Equipment_Skill, stats, points_Staff,
            //        0, points_Staff/10f, WeaponType.Staff)
            //},
        };
    }

}
