using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("Stats references")]
    [SerializeField] Image           Image_PlayerIcon;
    [SerializeField] TextMeshProUGUI Text_HeroName;
    [SerializeField] TextMeshProUGUI Text_HeroClass;
    [SerializeField] TextMeshProUGUI Text_HeroBackground;

    //[SerializeField] TextMeshProUGUI Text_PhysDmg;
    //[SerializeField] TextMeshProUGUI Text_ArtsDmg;
    //[SerializeField] TextMeshProUGUI Text_Armor;
    //[SerializeField] TextMeshProUGUI Text_Resist;
    //[SerializeField] TextMeshProUGUI Text_Speed;
    //[SerializeField] TextMeshProUGUI Text_Dodge;

    //[SerializeField] TextMeshProUGUI Text_MaxHp;
    //[SerializeField] TextMeshProUGUI Text_MaxEnergy;
    //[SerializeField] TextMeshProUGUI Text_MaxMana;
    //[SerializeField] TextMeshProUGUI Text_HealEff;
    //[SerializeField] TextMeshProUGUI Text_EnergyRegen;
    //[SerializeField] TextMeshProUGUI Text_ManaRegen;

    //[SerializeField] TextMeshProUGUI Text_Cooldown;
    //[SerializeField] TextMeshProUGUI Text_BlockChance;



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
                new EquipmentSystem());

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
    }

    private void OnDestroy()
    {
        TabGroup.OnTabSelected -= TabSelected;
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


    private GameObject InstantiatePrefab(GameObject prefab, Transform parentTransform)
    {
        var obj = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
        obj.transform.SetParent(parentTransform, true);
        obj.transform.localScale = Vector3.one;
        obj.transform.localPosition = Vector3.zero;

        return obj;
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
