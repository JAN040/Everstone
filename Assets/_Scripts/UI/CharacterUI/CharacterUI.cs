using System;
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
    [SerializeField] Canvas ParentCanvas;

    [Space]
    [Header("Tabs")]
    [SerializeField] TabGroup TabGroup;
    [SerializeField] TabGroupButton InventoryTabButton;


    #endregion UI References


    //[Space]
    //[Header("Variables")]


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
            GameManager.Instance.PlayerManager.Storage.AddItem(new InventoryItem(ResourceSystem.Instance.Items_Other[0]));
        }

        ItemGrid_Inventory.Initialize(GameManager.Instance.PlayerManager.Inventory, ParentCanvas);
        ItemGrid_Storage  .Initialize(GameManager.Instance.PlayerManager.Storage,   ParentCanvas);
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
