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

    [SerializeField] Button Button_TakeAll;
    [SerializeField] Button Button_Manage;
    [SerializeField] Button Button_Return;
    [SerializeField] Button Button_Continue;
    [SerializeField] TextMeshProUGUI Text_InventoryFull;



    #endregion UI References


    //[Space]
    //[Header("Variables")]
    private AdventureManager ManagerRef;
    private InventorySystem LootInventory;



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

    
    public void Init(AdventureManager manager, InventorySystem lootInventory)
    {
        ManagerRef = manager;
        LootInventory = lootInventory;

        DraggedItemData itemData = new DraggedItemData(0f, null, null);
        itemData.AreItemsDraggable = false; //this grid functions as display only, we dont want any dragging here

        LootGrid.Initialize(lootInventory, itemData);

        if (!GameManager.Instance.PlayerManager.Inventory.HasFreeSpace())
            ShowInventoryFullNotification();

        //if there are no items to take it would make sense to disable the take button
        if (lootInventory.GetItems().TrueForAll(x => x == null))
            Button_TakeAll.interactable = false;
    }


    private void ShowInventoryFullNotification()
    {
        Text_InventoryFull.gameObject.SetActive(true);
        Button_TakeAll.interactable = false;
    }


    public void TakeAll_Clicked()
    {
        var lootItems = LootInventory.GetItems();
        var playerInventory = GameManager.Instance.PlayerManager.Inventory;

        for (var i = 0; i < lootItems.Count; i++)
        {
            var currItem = lootItems[i];
            if (!playerInventory.HasFreeSpace())
            {
                ShowInventoryFullNotification();
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
        //TODO manage UI
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
