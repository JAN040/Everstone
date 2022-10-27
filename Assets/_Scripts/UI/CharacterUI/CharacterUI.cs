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
    [SerializeField] ItemGrid ItemGrid_Inventory;
    [SerializeField] ItemGrid ItemGrid_Storage;
    public Canvas ParentCanvas;
    public GameObject DraggedItemContainer;
    [SerializeField] GameObject ItemPrefab;

    [Space]
    [Header("Tabs")]
    [SerializeField] TabGroup TabGroup;
    [SerializeField] TabGroupButton InventoryTabButton;

    [Space]
    [Header("Equipment/Stats Tab")]
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
    [Header("Runes Tab")]
    [Header("Rune slots")]
    public List<ItemSlotUI> RuneSlotList;
    [SerializeField] TextMeshProUGUI  Text_RuneEffects;


    #endregion UI References


    [Space]
    [Header("Variables")]
    public ItemUI CurrentlyDraggedItem;
    public List<ItemSlotUI> EquipmentSlots;

    #endregion VARIABLES


    #region UNITY METHODS


    // Start is called before the first frame update
    void Start()
    {
        TabGroup.OnTabSelected += TabSelected;

        //TODO: remove; for testing only
        if (GameManager.Instance?.PlayerManager?.Inventory == null)
        {
            GameManager.Instance?.PlayerManager.SetInventory(
                new InventorySystem(40),
                new InventorySystem(GameManager.Instance.InitialCampStorageSpace),
                new EquipmentSystem((int)Enum.GetValues(typeof(EquipmentSlot)).Cast<EquipmentSlot>().Max() + 1, false),
                new EquipmentSystem(6, true)
            );

            //test items
            GameManager.Instance.PlayerManager.Equipment.EquipItem(new InventoryItem(ResourceSystem.Instance.Items_Equipment[0]));
            GameManager.Instance.PlayerManager.Inventory.AddItem(new InventoryItem(ResourceSystem.Instance.Items_Equipment[1]));
            GameManager.Instance.PlayerManager.Inventory.AddItem(new InventoryItem(ResourceSystem.Instance.Items_Equipment[2]));
            
            //GameManager.Instance.PlayerManager.Storage.AddItem(new InventoryItem(ResourceSystem.Instance.Items_Other[0]));
            for (int i = 0; i < GameManager.Instance.PlayerManager.Storage.InventorySize; i++)
                GameManager.Instance.PlayerManager.Storage.AddItem(new InventoryItem(ResourceSystem.Instance.Items_Other[UnityEngine.Random.Range(0, ResourceSystem.Instance.Items_Other.Count)]));
        
        }

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

        InitTab_Equipment();
        InitTab_Inventory();
        InitTab_Status();
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
        var equipment = GameManager.Instance.PlayerManager.Equipment;

        for (int i = 0; i < EquipmentSlots.Count; i++)
        {
            EquipmentSlots[i].Init(equipment, this);

            if (equipment.EquipmentItems[i] != null)
            {
                var currItemPrefab = InstantiatePrefab(ItemPrefab, EquipmentSlots[i].ItemContainer.transform);
                currItemPrefab.GetComponent<ItemUI>().Init(
                    this,
                    equipment.EquipmentItems[i],
                    EquipmentSlots[i]
                );

                equipment.EquipmentItems[i].Prefab = currItemPrefab;
            }
        }

        //stat section
        var hero = GameManager.Instance.PlayerManager.PlayerHero;

        Image_PlayerIcon.sprite  = hero.MenuSprite;
        Text_HeroName.text       = hero.Name;
        Text_HeroClass.text      = hero.ClassName;
        Text_HeroBackground.text = hero.Background;
    }

    private void InitTab_Inventory()
    {
        ItemGrid_Inventory.Initialize(GameManager.Instance.PlayerManager.Inventory, this);
        ItemGrid_Storage  .Initialize(GameManager.Instance.PlayerManager.Storage, this);
    }

    private void InitTab_Runes()
    {
        var runes = GameManager.Instance.PlayerManager.Runes;

        for (int i = 0; i < RuneSlotList.Count; i++)
        {
            RuneSlotList[i].Init(runes, this);

            if (runes.EquipmentItems[i] != null)
            {
                var currItemPrefab = InstantiatePrefab(ItemPrefab, RuneSlotList[i].ItemContainer.transform);
                currItemPrefab.GetComponent<ItemUI>().Init(
                    this,
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
                    continue;   //shouldnt happen unless new modifierTypes get added

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

    public ItemSlotUI GetFirstFreeSlotOfInventory(InventorySystem inventory)
    {
        int firstFreeIdx = inventory.FirstFreeSlotIndex();
        if (firstFreeIdx == -1)
            return null;

        if (ItemGrid_Inventory.ItemSource == inventory)
            return ItemGrid_Inventory.GetSlotAtIndex(firstFreeIdx);
        else if (ItemGrid_Storage.ItemSource == inventory)
            return ItemGrid_Storage.GetSlotAtIndex(firstFreeIdx);

        return null;
    }

    #endregion METHODS
}
