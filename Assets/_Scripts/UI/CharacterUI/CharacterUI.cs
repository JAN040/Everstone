using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
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

    [Space]
    [Header("Tabs")]
    [SerializeField] TabGroup TabGroup;
    [SerializeField] TabGroupButton InventoryTabButton;


    #endregion UI References


    [Space]
    [Header("Variables")]
    public ItemUI CurrentlyDraggedItem;


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
                new InventorySystem(GameManager.Instance.InitialCampStorageSpace));

            //test items
            GameManager.Instance.PlayerManager.Inventory.AddItem(new InventoryItem(ResourceSystem.Instance.Items_Equipment[0]));
            GameManager.Instance.PlayerManager.Inventory.AddItem(new InventoryItem(ResourceSystem.Instance.Items_Equipment[1]));
            //GameManager.Instance.PlayerManager.Storage.AddItem(new InventoryItem(ResourceSystem.Instance.Items_Other[0]));
            for (int i = 0; i < GameManager.Instance.PlayerManager.Storage.InventorySize; i++)
                GameManager.Instance.PlayerManager.Storage.InventoryItems[i] = new InventoryItem(ResourceSystem.Instance.Items_Other[UnityEngine.Random.Range(0, ResourceSystem.Instance.Items_Other.Count)]);
        }

        ItemGrid_Inventory.Initialize(GameManager.Instance.PlayerManager.Inventory, this);
        ItemGrid_Storage  .Initialize(GameManager.Instance.PlayerManager.Storage,   this);
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
            ItemGrid_Inventory.RefreshInventory();
            ItemGrid_Storage.RefreshInventory();
        }
    }



    #endregion METHODS
}
