using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class SuccessMenu : MonoBehaviour
{
    #region VARIABLES


    #region UI References


    [Header("UI References")]
    [SerializeField] GameObject SuccessMenu_Object;

    [SerializeField] TextMeshProUGUI StageDescText;
    [SerializeField] ItemGrid LootGrid;
    [SerializeField] ScrollRect LootScroll;


    [SerializeField] Button Button_TakeAll;
    [SerializeField] Button Button_Manage;
    [SerializeField] Button Button_Return;
    [SerializeField] Button Button_Continue;
    [SerializeField] TextMeshProUGUI Text_InventoryFull;
    [SerializeField] TextMeshProUGUI Text_FinalStageNotification;


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

    private Canvas CanvasRef;

    bool cancelledContinueOnce = false;


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

    
    public void Init(AdventureManager manager, InventorySystem lootInventory, Canvas canvas)
    {
        ManagerRef = manager;
        LootInventory = lootInventory;
        CanvasRef = canvas;

        int currProgress = ManagerRef.TemporaryProgress;
        bool gotAnyLoot = lootInventory.GetItems().Any(x => x != null);
        string lootText = gotAnyLoot ? "Obtained loot:" : "No loot obtained.";
        StageDescText.text = $"Cleared stage {currProgress}. {lootText}";

        //if the currently cleared stage is the last one
        if (currProgress >= ManagerRef.CurrentLocation.stageAmount)
        {
            Button_Continue.gameObject.SetActive(false);
        }

        RefreshLootGrid();

        //if there are no items to take it would make sense to disable the take button
        if (!gotAnyLoot)
            Button_TakeAll.interactable = false;

        //show a warning since the boss is ahead
        if (currProgress + 1 == ManagerRef.CurrentLocation.stageAmount)
        {
            //add extra exclamation points for every extra boss
            for (int i = 0; i < ManagerRef.CurrentLocation.LoopCount; i++)
                Text_FinalStageNotification.text += "!";
            
            Text_FinalStageNotification.gameObject.SetActive(true);
        }

        //manage inventory form starts hidden
        ManageInventory_Object.gameObject.SetActive(false);
    }

    private void RefreshLootGrid()
    {
        DraggedItemData itemData = new DraggedItemData(null, null, null);
        itemData.AreItemsDraggable = false; //this grid functions as display only, we dont want any dragging here

        LootInventory.MoveItemsToFront();
        LootGrid.Initialize(LootInventory, itemData);

        if (LootInventory.InventorySize > 7)
        {
            //if there are more than 7 loot items, enable scrolling (only 7 items in one line)
            LootScroll.vertical = true;
        }

        if (GameManager.Instance.PlayerManager.Inventory.HasFreeSpace())
            ShowInventoryFullNotification(false);
    }

    private void ShowInventoryFullNotification(bool isFull)
    {
        Text_InventoryFull.gameObject.SetActive(isFull);
        Button_TakeAll.interactable = !isFull;
    }


    public void TakeAll_Clicked()
    {
        bool tookAll = TryTakeAllItems();

        //if all items were taken it would make sense to disable the take button
        if (tookAll)
            Button_TakeAll.interactable = false;
    }

    /// <returns>True if all items were successfully taken</returns>
    private bool TryTakeAllItems()
    {
        var lootItems = LootInventory.GetItems();
        var playerInventory = GameManager.Instance.PlayerManager.Inventory;
        bool tookAll = true;

        for (var i = 0; i < lootItems.Count; i++)
        {
            var currItem = lootItems[i];

            //once nulls start it indicates the start of empty slots in the LootInventory
            if (currItem == null)
                continue;

            //taking money
            if (currItem.ItemData.ItemType == ItemType.Currency)
            {
                GameManager.Instance.Currency += currItem.GetSellPrice();
                continue;
            }

            if (!playerInventory.AddItem(currItem))
            {
                ShowInventoryFullNotification(true);
                tookAll = false;
                continue;
            }

            //remove the item from loot inventory
            Destroy(currItem.Prefab.gameObject);
            lootItems[i] = null;
        }

        return tookAll;
    }

    public void Manage_Clicked()
    {
        ManageInventory_Object.gameObject.SetActive(true);

        DraggedItemData itemData = new DraggedItemData(CanvasRef, DraggedItemContainer, ItemGrid_Inventory);
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

        //save player progress
        ManagerRef.CurrentLocation.PlayerProgress = ManagerRef.TemporaryProgress;

        //returning after beating the final stage: reset progress & increase loop count
        if (ManagerRef.CurrentLocation.PlayerProgress >= ManagerRef.CurrentLocation.stageAmount)
        {
            ManagerRef.CurrentLocation.LoopCount++;

            if (!GameManager.Instance.IsMultiplayer)
                ManagerRef.CurrentLocation.PlayerProgress = 0;
        }

        GameManager.Instance.EndAdventure();
    }

    public void Continue_Clicked()
    {
        if (!TryTakeAllItems() && !cancelledContinueOnce)
        {
            cancelledContinueOnce = true;
            return;
        }

        //close the menu
        Destroy(SuccessMenu_Object.gameObject); 

        ManagerRef.ContinueToNextStage();
    }


    #endregion METHODS
}
