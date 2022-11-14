using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using static UnityEditor.Progress;

public class ItemInfoBox : MonoBehaviour
{
    #region VARIABLES


    #region UI References


    [Header("UI References")]
    [SerializeField] GameObject Object_ItemInfoBox;

    [Space]
    [SerializeField] Image Image_Icon;

    [SerializeField] TextMeshProUGUI Text_Name;
    [SerializeField] TextMeshProUGUI Text_Rarity;
    [SerializeField] TextMeshProUGUI Text_Value;

    [SerializeField] GameObject Object_Description;
    [SerializeField] TextMeshProUGUI Text_Description;

    [SerializeField] GameObject Object_Effects;
    [SerializeField] TextMeshProUGUI Text_Effects;

    [Space]
    [Header("Buttons")]
    [SerializeField] GameObject Object_ButtonArea;

    [SerializeField] GameObject Button_Equip;
    [SerializeField] GameObject Button_Unequip;
    [SerializeField] GameObject Button_Use;
    [SerializeField] GameObject Button_Split;
    [SerializeField] GameObject Button_Buy;
    [SerializeField] GameObject Button_Sell;


    #endregion UI References


    //[Space]
    //[Header("Variables")]
    private InventoryItem itemRef;
    private InventoryItem ItemRef
    {
        get => itemRef;
        set
        {
            itemRef = value;
            UpdateUI();
        }
    }

   


    #endregion VARIABLES


    #region UNITY METHODS


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    #endregion UNITY METHODS


    #region METHODS


    /// <summary>
    /// Shows an info box with item details and its uses.
    /// </summary>
    public void Init(InventoryItem item)
    {
        ItemRef = item;

        Object_ItemInfoBox.SetActive(true);
    }

    private void UpdateUI()
    {
        var itemData = ItemRef.ItemData;

        Image_Icon.sprite = itemData.MenuIcon;

        Text_Name.text   = itemData.DisplayName;
        Text_Rarity.text = $"Rarity: {GetRarityText()}";

        string buyPrice = GameManager.Instance.CurrencyToDisplayString(itemData.BuyPrice);
        int totalSell = ItemRef.GetSellPrice();
        string totalSellText = GameManager.Instance.CurrencyToDisplayString(totalSell);
        Text_Value.text  = $"{buyPrice}\n{totalSellText} ({itemRef.StackSize})";

        Text_Description.text = itemData.Description;
        Object_Description.SetActive(!string.IsNullOrEmpty(Text_Description.text));

        bool isEquipment = itemData is ItemDataEquipment;
        var equipData = itemData as ItemDataEquipment;

        //effects
        Text_Effects.text = "";
        if (isEquipment)
        {
            var groupedMods = equipData.StatModifiers.GroupBy(x => x.ModifyingStatType);
            foreach (var group in groupedMods)
            {
                foreach (var modifier in group) 
                {
                    bool prfx = modifier.IsPositive();
                    string icon = ResourceSystem.GetStatIconTag(modifier.ModifyingStatType);
                    bool perc = modifier.Type == ModifierType.Percent;
                    string val = perc ? (modifier.Value * 100).ToString("0.0") : modifier.Value.ToString("0");

                    Text_Effects.text += $"{(prfx ? "+" : "-")}{val}{(perc ? "%" : "")} {icon}  ";
                }

                Text_Effects.text += Environment.NewLine;
            }
        }

        //buttons

        bool isEquipped = ItemRef.IsRune() ? 
            GameManager.Instance.PlayerManager.Runes.IsEquipped(ItemRef)
            :
            GameManager.Instance.PlayerManager.Equipment.IsEquipped(ItemRef);
        bool playerInventoryHasFreeSpace = GameManager.Instance.PlayerManager.Inventory.FirstFreeSlotIndex() != -1;
        bool currentInventoryHasFreeSpace = ItemRef?.Prefab?.GetComponent<ItemUI>()?.SlotRef.InventoryRef.FirstFreeSlotIndex() != -1;
        bool isInResidenceScene = SceneManager.GetActiveScene().buildIndex == (int)Scenes.Residence;
        bool isInShopScene = SceneManager.GetActiveScene().buildIndex == (int)Scenes.Shop;

        //equip is available for equipment items which arent equipped
        Button_Equip.SetActive(isEquipment && 
            !isEquipped && 
            isInResidenceScene   //make sure we're in character UI
        );

        //Unequip is available for equipped equipment items when there is free space in the inventory
        Button_Unequip.SetActive(isEquipment && isEquipped && playerInventoryHasFreeSpace);

        //Use button is available for usable item types
        Button_Use.SetActive(itemData.ItemType.In(ItemType.Potion)); //add more usable item types if necessary

        //Split stack button is available for item stacks above 1
        Button_Split.SetActive(ItemRef.StackSize > 1 && (isInShopScene || isInResidenceScene));
        Button_Split.GetComponent<Button>().interactable = currentInventoryHasFreeSpace;

        //Show buy option on shop items
        Button_Buy.SetActive(ItemRef.IsShopOwned);
        Button_Buy.GetComponent<Button>().interactable = playerInventoryHasFreeSpace && ItemRef.CanAfford();

        //show sell on all owned, non equipped items (only in storage or shop menus)
        Button_Sell.SetActive(!ItemRef.IsShopOwned && !Button_Unequip.activeSelf && (isInShopScene || isInResidenceScene));
    }

    private string GetRarityText()
    {
        var rarity = itemRef.ItemData.Rarity;
        Color color = ResourceSystem.GetRarityColor(rarity);
        
        return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{rarity}</color>";
    }


    #region Buttons


    public void CloseClicked()
    {
        Destroy(Object_ItemInfoBox.gameObject);
    }

    public void EquipClicked()
    {
        ItemSlotUI currSlot = ItemRef?.Prefab?.GetComponent<ItemUI>()?.SlotRef;
        ItemUI item = ItemRef?.Prefab?.GetComponent<ItemUI>();
        bool isRune = ItemRef.IsRune();

        if (currSlot == null || item == null)
        {
            Debug.LogWarning("Tried equipping item, but couldnt fetch its slot reference.");
            return;
        }

        EquipmentSystem equipSystem;
        if (isRune)
            equipSystem = GameManager.Instance.PlayerManager.Runes;
        else
            equipSystem = GameManager.Instance.PlayerManager.Equipment;

        int equipSlotId = equipSystem.GetDefaultEquipSlotIndexFromEquipType((ItemRef.ItemData as ItemDataEquipment).EquipmentType);
        InventoryItem unequippedItem = equipSystem.GetItemAt(equipSlotId);

        //remove item from current inventory and equip it to equipmentSystem
        currSlot.InventoryRef.MoveItemToTarget(
            currSlot.GetSlotPosition(),
            equipSystem,
            equipSlotId
        );

        //need to reparent the item from current slot to equipmentSlot
        ItemSlotUI equipSlot = isRune ? 
            currSlot.ItemDragData.GetCharacterUI()?.RuneSlotList[equipSlotId]
            :
            currSlot.ItemDragData.GetCharacterUI()?.EquipmentSlots[equipSlotId];
        
        item.SlotInto(equipSlot);

        //reparent the unequiped item (if any) to current inventory
        if (unequippedItem != null)
        {
            unequippedItem.Prefab.GetComponent<ItemUI>().SlotInto(currSlot);
            //currSlot.InventoryRef.AddItem(unequippedItem); //moveItemToTarget does this
        }

        //close ItemInfoBox
        Destroy(Object_ItemInfoBox.gameObject);
    }

    public void UnequipClicked()
    {
        ItemSlotUI equipSlot = ItemRef?.Prefab?.GetComponent<ItemUI>()?.SlotRef;
        ItemUI item = ItemRef?.Prefab?.GetComponent<ItemUI>();
        bool isRune = ItemRef.IsRune();

        if (equipSlot == null || item == null)
        {
            Debug.LogWarning("Tried unequipping item, but couldnt fetch its slot reference.");
            return;
        }

        if (isRune)
            GameManager.Instance.PlayerManager.Runes.UnequipItem(equipSlot.GetSlotPosition());
        else
            GameManager.Instance.PlayerManager.Equipment.UnequipItem((int)equipSlot.EquipmentSlot);

        //need to reparent the item from the equipmentSlot to the first free inventory slot
        ItemSlotUI freeSlot = equipSlot.ItemDragData.InventoryGrid.GetFirstFreeSlotOfInventory();
        item.SlotInto(freeSlot);
        freeSlot.InventoryRef.AddItem(ItemRef);

        //close ItemInfoBox
        Destroy(Object_ItemInfoBox.gameObject);
    }

    public void UseClicked()
    {
        //TODO

        //close ItemInfoBox
        Destroy(Object_ItemInfoBox.gameObject);
    }

    public void SplitClicked()
    {
        ItemSlotUI currSlot = ItemRef?.Prefab?.GetComponent<ItemUI>()?.SlotRef;
        ItemUI item = ItemRef?.Prefab?.GetComponent<ItemUI>();
        if (currSlot == null || item == null)
        {
            Debug.LogWarning("Tried splitting the item, but couldnt fetch its slot reference.");
            return;
        }

        if (ItemRef.StackSize <= 1)
        {
            Button_Split.SetActive(false);
            return;
        }

        ItemSlotUI slotForClone = currSlot.ItemDragData.InventoryGrid.GetFirstFreeSlotOfInventory();
        
        InventoryItem itemClone = ItemRef.Clone();
        itemClone.Prefab.GetComponent<ItemUI>().Init(currSlot.ItemDragData, itemClone, null);

        int halfStack = ItemRef.StackSize / 2;

        ItemRef.RemoveFromStack(halfStack);
        itemClone.StackSize = halfStack;

        //dont use add item because it would auto stack it back to the original stack
        //  if the item is currently in the inventory
        int firstFreeSlot = currSlot.InventoryRef.FirstFreeSlotIndex();
        currSlot.InventoryRef.PlaceItemAtSlot(itemClone, firstFreeSlot);

        //place the gameobject
        itemClone.Prefab.GetComponent<ItemUI>().SlotInto(slotForClone);

        //close ItemInfoBox
        Destroy(Object_ItemInfoBox.gameObject);
    } 


    public void BuyClicked()
    {
        ItemSlotUI itemSlot = ItemRef?.Prefab?.GetComponent<ItemUI>()?.SlotRef;
        ItemUI item = ItemRef?.Prefab?.GetComponent<ItemUI>();
        ItemSlotUI freeSlot = itemSlot.ItemDragData.InventoryGrid.GetFirstFreeSlotOfInventory();

        if (itemSlot == null || item == null)
        {
            Debug.LogWarning("Tried buying the item, but couldnt fetch its slot reference.");
            return;
        }

        //move item from shop to the players inventory (also takes care of currency transaction)
        var res = itemSlot.InventoryRef.MoveItemToTarget(
            itemSlot.GetSlotPosition(),
            GameManager.Instance.PlayerManager.Inventory,
            freeSlot.GetSlotPosition()
        );

        if (res == ItemMoveResult.NoChange) return; //dont visually move the item

        //need to reparent the item from the shop slot to the first free inventory slot
        item.SlotInto(freeSlot);

        //close ItemInfoBox
        Destroy(Object_ItemInfoBox.gameObject);
    }

    public void SellClicked()
    {
        bool isInResidenceScene = SceneManager.GetActiveScene().buildIndex == (int)Scenes.Residence;
        bool isInShopScene = SceneManager.GetActiveScene().buildIndex == (int)Scenes.Shop;
        var item = ItemRef?.Prefab?.GetComponent<ItemUI>();
        var itemSlot = item?.SlotRef;
        var shopInv = GameManager.Instance.PlayerManager.ShopInventory;
        ItemSlotUI freeShopSlot = itemSlot?.ItemDragData?.ShopGrid?.GetFirstFreeSlotOfInventory();

        if (ItemRef.ItemData.ItemType == ItemType.Loot)
        {   //no point in showing loot items as a buy-back option
            GameManager.Instance.Currency += ItemRef.GetSellPrice();
            itemSlot.InventoryRef.RemoveItemAt(itemSlot.GetSlotPosition());
            Destroy(ItemRef.Prefab.gameObject);
            Destroy(Object_ItemInfoBox.gameObject);
            return;
        }

        if (shopInv.FirstFreeSlotIndex() != -1)
        {
            //move item to shop inventory
            itemSlot.InventoryRef.MoveItemToTarget(
                itemSlot.GetSlotPosition(),
                shopInv,
                shopInv.FirstFreeSlotIndex()
            );
        }
        else
        {
            //if cant move, at least remove from the inventory
            GameManager.Instance.Currency += ItemRef.GetSellPrice();
            itemSlot.InventoryRef.RemoveItemAt(itemSlot.GetSlotPosition());
        }

        if (isInShopScene && freeShopSlot != null)
        {
            //need to reparent the item from the inventory slot to the shop slot 
            item.SlotInto(freeShopSlot);
        }
        else
        {
            //get rid of the item prefab
            Destroy(ItemRef.Prefab.gameObject); 
        }
        
        //close ItemInfoBox
        Destroy(Object_ItemInfoBox.gameObject);
    }


    #endregion Buttons



    #endregion METHODS
}
