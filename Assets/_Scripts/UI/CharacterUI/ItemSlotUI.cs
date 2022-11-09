using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    #region VARIABLES


    #region UI References


    [Header("UI References")]
    [SerializeField] GameObject HoverImage;
    public GameObject ItemContainer;


    #endregion UI References


    [Space]
    [Header("Variables")]
    public DraggedItemData ItemDragData;
    public InventorySystem InventoryRef;

    public List<ItemType> AcceptedItemTypes; //none means all items are accepted
    
    //further filter when AcceptedItemTypes includes ItemType.Equipment
    public List<EquipmentType> AcceptedEquipmentTypes;

    //which of the 12 equip slots this slot represents (None means its not an equip slot) 
    public EquipmentSlot EquipmentSlot = EquipmentSlot.None;


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

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ItemDragData?.CurrentlyDraggedItem == null)
            return;

        bool showEff = true;

        showEff &= ItemDragData.CurrentlyDraggedItem.SlotRef != this;
        showEff &= CanSlotItem(ItemDragData.CurrentlyDraggedItem, true);

        if (showEff)
        {
            HoverImage.gameObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HoverImage.gameObject.SetActive(false);
    }


    #endregion UNITY METHODS


    #region METHODS


    /// <param name="inventory">The inventory this item slot belongs to</param>
    public void Init(InventorySystem inventory, DraggedItemData draggedItemData)
    {
        InventoryRef = inventory;
        ItemDragData = draggedItemData;

        if (!(inventory is EquipmentSystem))
        {
            AcceptedItemTypes = new List<ItemType>() { ItemType.None };
            AcceptedEquipmentTypes = new List<EquipmentType>() { EquipmentType.None };
        }
    }

    private bool CanSlotItem(ItemUI item, bool checkSwap)
    {
        if (item == null)
            return true;

        bool canSlot = true;
        var equipData = item.ItemRef.ItemData as ItemDataEquipment;

        //ItemType.None means all items are accepted
        canSlot &= AcceptedItemTypes.Contains(ItemType.None) || AcceptedItemTypes.Contains(item.ItemRef.ItemData.ItemType);

        if (equipData != null) //when item is equipment
            canSlot &= AcceptedItemTypes.Contains(ItemType.None)
                       ||
                       AcceptedEquipmentTypes.Contains(equipData.EquipmentType);

        //check if a swap is possible
        if (checkSwap)
            canSlot &= item.SlotRef.CanSlotItem(this.GetSlottedItem(), false);

        return canSlot;
    }

    /// <returns>int index indicating where in the item grid this slot is located</returns>
    public int GetSlotPosition()
    {
        return EquipmentSlot != EquipmentSlot.None ? 
            (int)EquipmentSlot
            :
            this.gameObject.transform.GetSiblingIndex();
    }

    public void OnDrop(PointerEventData eventData)
    {
        HoverImage.gameObject.SetActive(false);

        var droppedObj = eventData.pointerDrag;

        //only snap items (anything can be dragged)
        if (droppedObj != null && droppedObj.GetComponent<ItemUI>() != null)
        {
            var droppedItemScript = droppedObj.GetComponent<ItemUI>();

            if (!CanSlotItem(droppedItemScript, true))
            {
                Debug.Log("Item drop of incorrect type detected");
                return;
            }

            Debug.Log("Item drop detected");

            HandleInventoryTransaction(droppedItemScript);
        }
    }

    //params: dropped item and its previous slot
    private void HandleInventoryTransaction(ItemUI droppedItem)
    {
        ItemSlotUI prevSlot = droppedItem.GetComponent<ItemUI>().SlotRef;

        //if the item was dropped to the same inventory slot, no changes need to be made
        if (prevSlot == this)
            return;

        int prevSlotPosition = prevSlot.GetSlotPosition();
        int thisSlotPosition = this.GetSlotPosition();

        //else notify inventory to make the appropriate movement
        var operation = prevSlot.InventoryRef.MoveItemToTarget(
            prevSlotPosition,
            this.InventoryRef,
            thisSlotPosition
        );

        //handle changes to prefabs (based on the operation that was done in the inventory)
        switch (operation)
        {
            case ItemMoveResult.Moved:
                droppedItem.SlotRef = this;
                break;

            case ItemMoveResult.Swapped:
                //need to reparent the item at targetSlot to prevSlot
                var currentlySlottedItem = this.GetSlottedItem();
                currentlySlottedItem.SlotInto(prevSlot);

                droppedItem.SlotRef = this;
                //done in ItemUI when OnDragDrop happens
                //droppedObj.transform.SetParent(this.transform, true);
                //droppedObj.GetComponent<RectTransform>().position = this.GetComponent<RectTransform>().position;
                break;

            case ItemMoveResult.StackedAll:
                //everything from the stack was consumed & InventoryItem was destroyed, we have to also destroy the prefab
                Destroy(droppedItem.gameObject);
                var a = this.GetSlottedItem();
                var b = prevSlot.InventoryRef;
                var c = this.InventoryRef;
                break;

            case ItemMoveResult.StackedWithRemainder:   //the only changes are inventory-side
            case ItemMoveResult.NoChange:               //no changes need to be made
            default:
                break;
        }
    }

    public ItemUI GetSlottedItem()
    {
        return this.transform.GetComponentInChildren<ItemUI>();
    }


    #endregion METHODS
}
