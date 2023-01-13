using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    #region VARIABLES


    #region UI References


    [Header("UI References")]

    public Canvas ParentCanvas;
    public GameObject DraggedItemContainer;

    [Space]
    [Header("Item Grids")]
    [SerializeField] ItemGrid ItemGrid_Inventory;
    [SerializeField] ItemGrid ItemGrid_Shop;

    #endregion UI References


    //[Space]
    //[Header("Variables")]
    private DraggedItemData ItemDragData;



    #endregion VARIABLES


    #region UNITY METHODS


    void Awake()
    {
        ItemDragData = new DraggedItemData(ParentCanvas, DraggedItemContainer, ItemGrid_Inventory);
        ItemDragData.ShopGrid = ItemGrid_Shop;
    }

    private void OnEnable()
    {
        InitUI();
    }


    // Update is called once per frame
    void Update()
    {
        
    }


    #endregion UNITY METHODS


    #region METHODS


    private void InitUI()
    {
        if (ItemGrid_Inventory == null || ItemGrid_Shop == null)
            return;

        ItemGrid_Inventory.Initialize(GameManager.Instance.PlayerManager.Inventory,     ItemDragData);
        ItemGrid_Shop     .Initialize(GameManager.Instance.PlayerManager.ShopInventory, ItemDragData);
    }


    #endregion METHODS
}
