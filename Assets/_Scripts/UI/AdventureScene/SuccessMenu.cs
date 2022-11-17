using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SuccessMenu : MonoBehaviour
{
    #region VARIABLES


    #region UI References


    [Header("UI References")]
    [SerializeField] GameObject SuccessMenu_Object;

    [SerializeField] ItemGrid LootGrid;
    [SerializeField] ScrollRect LootScroll;


    [SerializeField] Button Button_TakeAll;
    [SerializeField] Button Button_Manage;
    [SerializeField] Button Button_Return;
    [SerializeField] Button Button_Continue;
    [SerializeField] TextMeshProUGUI Text_InventoryFull;

    [Space]
    [Header("Manage Inventory")]
    [SerializeField] GameObject ManageInventory_Object;
    [SerializeField] ItemGrid ItemGrid_Inventory;
    [SerializeField] ItemGrid ItemGrid_Loot;
    [SerializeField] GameObject DraggedItemContainer;


    #endregion UI References


    //[Space]
    //[Header("Variables")]
    private AdventureManager ManagerRef;
    private InventorySystem LootInventory;

    private float CanvasScaleFactor;


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

    
    public void Init(AdventureManager manager, InventorySystem lootInventory, float canvasScaleFactor)
    {
        ManagerRef = manager;
        LootInventory = lootInventory;
        CanvasScaleFactor = canvasScaleFactor;
        
        RefreshLootGrid();

        //if there are no items to take it would make sense to disable the take button
        if (lootInventory.GetItems().TrueForAll(x => x == null))
            Button_TakeAll.interactable = false;

        //manage inventory form starts hidden
        ManageInventory_Object.gameObject.SetActive(false);
    }

    private void RefreshLootGrid()
    {
        DraggedItemData itemData = new DraggedItemData(0f, null, null);
        itemData.AreItemsDraggable = false; //this grid functions as display only, we dont want any dragging here

        LootInventory.MoveItemsToFront();
        LootGrid.Initialize(LootInventory, itemData);

        if (LootInventory.InventorySize > 7)
        {
            //if there are more than 7 loot items, enable scrolling (only 7 items in one line)
            LootScroll.vertical = true;
        }

        ShowInventoryFullNotification(!GameManager.Instance.PlayerManager.Inventory.HasFreeSpace());
    }

    private void ShowInventoryFullNotification(bool isFull)
    {
        Text_InventoryFull.gameObject.SetActive(isFull);
        Button_TakeAll.interactable = !isFull;
    }


    public void TakeAll_Clicked()
    {
        var lootItems = LootInventory.GetItems();
        var playerInventory = GameManager.Instance.PlayerManager.Inventory;

        for (var i = 0; i < lootItems.Count; i++)
        {
            var currItem = lootItems[i];

            //once nulls start it indicates the start of empty slots in the LootInventory
            if (currItem == null)
                break;

            //taking money
            if (currItem.ItemData.ItemType == ItemType.Currency)
            {
                GameManager.Instance.Currency += currItem.GetSellPrice();
                continue;
            }

            if (!playerInventory.HasFreeSpace())
            {
                ShowInventoryFullNotification(true);
                break;
            }

            playerInventory.AddItem(currItem);

            //remove the item from loot inventory
            Destroy(currItem.Prefab.gameObject);
            lootItems[i] = null;
        }

        //if all items were taken it would make sense to disable the take button
        if (lootItems.TrueForAll(x => x == null))
            Button_TakeAll.interactable = false;
    }

    public void Manage_Clicked()
    {
        ManageInventory_Object.gameObject.SetActive(true);

        DraggedItemData itemData = new DraggedItemData(CanvasScaleFactor, DraggedItemContainer, ItemGrid_Inventory);
        LootInventory.MoveItemsToFront();

        ItemGrid_Inventory.Initialize(GameManager.Instance.PlayerManager.Inventory, itemData);
        ItemGrid_Loot.Initialize(LootInventory, itemData);
    }

    public void ManageClose_Clicked()
    {
        ManageInventory_Object.gameObject.SetActive(false);

        RefreshLootGrid();
    }

    public void Return_Clicked()
    {
        //close the menu
        Destroy(SuccessMenu_Object.gameObject);

        ManagerRef.EndAdventure();
    }

    public void Continue_Clicked()
    {
        //close the menu
        Destroy(SuccessMenu_Object.gameObject); 

        ManagerRef.ContinueToNextStage();
    }


    #endregion METHODS
}
