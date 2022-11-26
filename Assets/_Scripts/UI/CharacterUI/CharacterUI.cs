using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Data;

public class CharacterUI : MonoBehaviour
{
    #region VARIABLES


    #region UI References


    [Header("UI References")]
    
    public Canvas ParentCanvas;
    public GameObject DraggedItemContainer;
    [SerializeField] GameObject ItemPrefab;


    [Space]
    [Header("Tabs")]
    [SerializeField] TabGroup TabGroup;
    [SerializeField] TabGroupButton InventoryTabButton;


    [Space]
    [Header("Equipment/Stats Tab")]

    [Header("Silhoutte params")]
    [SerializeField] Image Image_Silhouette;
    [SerializeField] Sprite Silh_Mag_M;
    [SerializeField] Sprite Silh_Mag_F;
    [SerializeField] Sprite Silh_War_M;
    [SerializeField] Sprite Silh_War_F;
    [SerializeField] Sprite Silh_Rog_M;
    [SerializeField] Sprite Silh_Rog_F;

    [Header("Equipment slots")]
    public ItemSlotUI Helmet;
    public ItemSlotUI Shoulder;
    public ItemSlotUI Chestplate;
    public ItemSlotUI Pants;
    public ItemSlotUI Boots;

    public ItemSlotUI Necklace;
    public ItemSlotUI Cape;
    public ItemSlotUI Gloves;
    public ItemSlotUI Ring1;
    public ItemSlotUI Ring2;

    public ItemSlotUI RightArm;
    public ItemSlotUI LeftArm;


    [Space]
    [Header("Status Tab")]
    [SerializeField] GameObject SkillLevelUIContainer;
    [SerializeField] GameObject SkillLevelUIPrefab;

    [Header("Stats references")]
    [SerializeField] Image           Image_PlayerIcon;
    [SerializeField] TextMeshProUGUI Text_HeroName;
    [SerializeField] TextMeshProUGUI Text_HeroClass;
    [SerializeField] TextMeshProUGUI Text_HeroBackground;


    [Space]
    [Header("Inventory Tab")]
    [SerializeField] ItemGrid ItemGrid_Inventory;
    [SerializeField] ItemGrid ItemGrid_Storage;


    [Space]
    [Header("Abilities Tab")]
    [SerializeField] GameObject AbilityPanelContainer;
    [SerializeField] GameObject AbilityPanelPrefab;
    private List<AbilityPanelUI> AbilityPanelList;
    private List<ScriptableAbility> EquippedAbilities;

    [Tooltip("the list of panels showing which abilities are equipped")]
    [SerializeField] List<EquippedAbilityPanelUI> EquippedAbilityPanelList;
    [SerializeField] EquippedAbilityPanelUI EquippedAbilityPanel_Dodge;
    [SerializeField] EquippedAbilityPanelUI EquippedAbilityPanel_BasicAttack;
    
    


    [Space]
    [Header("Runes Tab")]
    [Header("Rune slots")]
    public List<ItemSlotUI> RuneSlotList;
    [SerializeField] TextMeshProUGUI Text_RuneEffects;


    #endregion UI References


    [Space]
    [Header("Variables")]

    //public ItemUI CurrentlyDraggedItem;
    public List<ItemSlotUI> EquipmentSlots;
    public DraggedItemData ItemDragData { get; private set; }


    #endregion VARIABLES


    #region UNITY METHODS


    // Start is called before the first frame update
    void Start()
    {
        TabGroup.OnTabSelected += TabSelected;

        EquipmentSlots = new List<ItemSlotUI>()
        {
            Helmet,
            Shoulder,
            Chestplate,
            Pants,
            Boots,
            Necklace,
            Cape,
            Gloves,
            Ring1,
            Ring2,
            RightArm,
            LeftArm
        };
        ItemDragData = new DraggedItemData(ParentCanvas.scaleFactor, DraggedItemContainer, ItemGrid_Inventory);
        ItemDragData.SetCharacterUI(this);

        InitTab_Equipment();
        InitTab_Inventory();
        InitTab_Status();
        InitTab_Abilities();
        InitTab_Runes();
    }

   

    private void OnDestroy()
    {
        TabGroup.OnTabSelected -= TabSelected;
        if (GameManager.Instance?.PlayerManager?.Runes != null)
            GameManager.Instance.PlayerManager.Runes.OnInventoryChanged -= UpdateRuneEffectsText;
    }

    // Update is called once per frame
    void Update()
    {

    }


    #endregion UNITY METHODS


    #region METHODS

    
    private void TabSelected(TabGroupButton tabIndex)
    {
        if (tabIndex == InventoryTabButton) //inventory tab
        {
            ItemGrid_Inventory.ItemSource.Refresh();
            ItemGrid_Storage.ItemSource.Refresh();
        }
    }

    //assign the equipment system to the equipment slots
    private void InitTab_Equipment()
    {
        var hero = GameManager.Instance.PlayerManager.PlayerHero;

        //silhoutte
        SetSilhoutte(hero.ClassName, hero.GetGender() == Gender.Male);

        //equipment items
        var equipment = GameManager.Instance.PlayerManager.Equipment;

        for (int i = 0; i < EquipmentSlots.Count; i++)
        {
            EquipmentSlots[i].Init(equipment, ItemDragData);

            if (equipment.EquipmentItems[i] != null)
            {
                var currItemPrefab = InstantiatePrefab(ItemPrefab, EquipmentSlots[i].ItemContainer.transform);
                currItemPrefab.GetComponent<ItemUI>().Init(
                    ItemDragData,
                    equipment.EquipmentItems[i],
                    EquipmentSlots[i]
                );

                equipment.EquipmentItems[i].Prefab = currItemPrefab;
            }
        }

        //stat section
        Image_PlayerIcon.sprite  = hero.MenuSprite;
        Text_HeroName.text       = hero.Name;
        Text_HeroClass.text      = hero.ClassName;

        if (hero.Background.Equals("None"))
            Text_HeroBackground.text = string.Empty;
        else
            Text_HeroBackground.text = hero.Background;
    }

    private void SetSilhoutte(string className, bool isMalePfp)
    {
        Sprite silhoutte = Silh_War_M;
        if (className.Equals("Mage"))
        {
            silhoutte = isMalePfp ? Silh_Mag_M : Silh_Mag_F;
        }
        else if (className.Equals("Warrior"))
        {
            silhoutte = isMalePfp ? Silh_War_M : Silh_War_F;
        }
        else if (className.Equals("Rogue"))
        {
            silhoutte = isMalePfp ? Silh_Rog_M : Silh_Rog_F;
        }
        else
            Debug.LogError($"Unrecognised class name {className}, cant decide on silhoutte");

        Image_Silhouette.sprite = silhoutte;
    }

    private void InitTab_Inventory()
    {
        ItemGrid_Inventory.Initialize(GameManager.Instance.PlayerManager.Inventory, ItemDragData);
        ItemGrid_Storage  .Initialize(GameManager.Instance.PlayerManager.Storage,   ItemDragData);
    }

    private void InitTab_Runes()
    {
        var runes = GameManager.Instance.PlayerManager.Runes;

        for (int i = 0; i < RuneSlotList.Count; i++)
        {
            RuneSlotList[i].Init(runes, ItemDragData);

            if (runes.EquipmentItems[i] != null)
            {
                var currItemPrefab = InstantiatePrefab(ItemPrefab, RuneSlotList[i].ItemContainer.transform);
                currItemPrefab.GetComponent<ItemUI>().Init(
                    ItemDragData,
                    runes.EquipmentItems[i],
                    RuneSlotList[i]
                );

                runes.EquipmentItems[i].Prefab = currItemPrefab;
            }
        }

        UpdateRuneEffectsText(runes);

        GameManager.Instance.PlayerManager.Runes.OnInventoryChanged += UpdateRuneEffectsText;
    }

    private void UpdateRuneEffectsText(InventorySystem invSystem)
    {
        EquipmentSystem runeSys = invSystem as EquipmentSystem;
        if (runeSys == null)
            return;

        Text_RuneEffects.text = string.Empty;
        var modList = new List<StatModifier>();
        
        foreach (var rune in runeSys.EquipmentItems)
        {
            if (rune != null && (rune.ItemData as ItemDataEquipment) != null)
            {
                modList.AddRange((rune.ItemData as ItemDataEquipment).StatModifiers);
            }
        }

        var groupedMods = modList.GroupBy(x => x.ModifyingStatType);
        foreach (var group in groupedMods)
        {
            float percMod = 0f;
            float flatMod = 0f;
            StatType currentStatType = StatType.PhysicalDamage;

            //join values of multiple modifiers of same type into one modifier then add the description of it to the text field
            foreach (var modifier in group)
            {
                if (modifier.Type == ModifierType.Flat)
                    flatMod += modifier.Value;
                else if (modifier.Type == ModifierType.Percent)
                    percMod += modifier.Value;
                else
                {
                    Debug.LogWarning($"Unexpected modifier type {modifier.Type}");
                    continue;   //shouldnt happen unless new modifierTypes get added
                }

                currentStatType = modifier.ModifyingStatType;
            }

            if (flatMod != 0f)
            {
                Text_RuneEffects.text += GetModifierDescription(new StatModifier(flatMod, currentStatType, ModifierType.Flat)); 
            }
            if (percMod != 0f)
            {
                Text_RuneEffects.text += GetModifierDescription(new StatModifier(percMod, currentStatType, ModifierType.Percent));
            }

            Text_RuneEffects.text += Environment.NewLine;
        }

        if (string.IsNullOrEmpty(Text_RuneEffects.text))
            Text_RuneEffects.text = "No active rune effects.";
    }

    private string GetModifierDescription(StatModifier modifier)
    {
        bool prfx = modifier.IsPositive();
        string icon = ResourceSystem.GetStatIconTag(modifier.ModifyingStatType);
        bool perc = modifier.Type == ModifierType.Percent;
        string val = perc ? (modifier.Value * 100).ToString("0.0") : modifier.Value.ToString("0");

        return $"{(prfx ? "+" : "-")}{val}{(perc ? "%" : "")} {icon}  ";
    }

    private GameObject InstantiatePrefab(GameObject prefab, Transform parentTransform)
    {
        var obj = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
        obj.transform.SetParent(parentTransform, true);
        obj.transform.localScale = Vector3.one;
        obj.transform.localPosition = Vector3.zero;

        return obj;
    }

    private void InitTab_Status()
    {
        LevelSystem levelSystem = GameManager.Instance.PlayerManager.PlayerHero.LevelSystem;

        foreach (var skillLevel in levelSystem.Skills.Values)
        {
            var skillLevelPrefab = InstantiatePrefab(SkillLevelUIPrefab, SkillLevelUIContainer.transform);
            skillLevelPrefab.GetComponent<SkillLevelUI>()?.Init(skillLevel);
        }
    }


    #region Ability Tab


    private void InitTab_Abilities()
    {
        AbilityPanelList = new List<AbilityPanelUI>();
        EquippedAbilities = GameManager.Instance.PlayerManager.EquippedAbilities;

        //basic attack is not upgrade-able as it is weapon-reliant
        List<ScriptableAbility> abilities = GameManager.Instance
                                                       .PlayerManager
                                                       .Abilities
                                                       .Where(x => x.Ability != Ability.BasicAttack)
                                                       .ToList();

        // initialize EquippedAbilities list (note: dodge shouldnt be added to the equipped abilities list)
        //var idx = 0;
        //foreach (var ability in abilities.Where(x => x.IsSelected && x.Ability != Ability.Dodge))
        //{
        //    EquippedAbilities[idx] = ability;
        //    idx++;
        //}

        foreach (var ability in abilities)
        {
            var abilityPanelPrefab = InstantiatePrefab(AbilityPanelPrefab, AbilityPanelContainer.transform);
            var abilityPanelUI = abilityPanelPrefab.GetComponent<AbilityPanelUI>();

            if (abilityPanelUI != null)
            {
                abilityPanelUI.Init(ability, this);
                AbilityPanelList.Add(abilityPanelUI);
            }
        }

        //update equipped abilities panels
        EquippedAbilityPanel_Dodge.SetAbility(
            GameManager.Instance.PlayerManager.Abilities.FirstOrDefault(x => x.Ability == Ability.Dodge)
        );
        EquippedAbilityPanel_BasicAttack.SetAbility(
            GameManager.Instance.PlayerManager.Abilities.FirstOrDefault(x => x.Ability == Ability.BasicAttack)
        );

        RefreshEquippedAbilitiesPanels();
    }

    /// <summary>
    /// Call UpdateUI() on every AbilityPanelUI in the ability tab
    /// </summary>
    public void RefreshAbilityPanels()
    {
        AbilityPanelList.ForEach(x => x.UpdateUI());
    }

    /// <summary>
    /// Refresh the panels which show what abilities are equipped
    /// </summary>
    public void RefreshEquippedAbilitiesPanels()
    {
        for (int i = 0; i < EquippedAbilityPanelList.Count; i++)
        {
            EquippedAbilityPanelList[i].SetAbility(EquippedAbilities[i]);
        }
    }

    public void EquipAbility(ScriptableAbility ability, int index = -1)
    {
        if (index == -1)
        {
            index = GetFirstFreeAbilitySlot();
            if (index == -1)
                return;
        }

        EquippedAbilities[index] = ability;

        RefreshEquippedAbilitiesPanels();
    }

    public void UnequipAbility(ScriptableAbility abilityToRemove, int index = -1)
    {
        if (index >= EquippedAbilities.Count)
            return;

        if (index == -1)
            index = EquippedAbilities.FindIndex(x => x == abilityToRemove);
            
        if (index != -1)
            EquippedAbilities[index] = null;

        RefreshEquippedAbilitiesPanels();
    }

    public bool HasFreeAbilitySlots()
    {
        return GetFirstFreeAbilitySlot() != -1;
    }

    private int GetFirstFreeAbilitySlot()
    {
        for (int i = 0; i < EquippedAbilities.Count; i++)
        {
            if (EquippedAbilities[i] == null)
                return i;
        }

        return -1;
    } 


    #endregion Ability Tab


    

    #endregion METHODS
}
