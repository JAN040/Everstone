using System;
using System.Collections;
using System.Collections.Generic;
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
    public CharacterUI CharacterUIRef;
    public InventorySystem InventoryRef;

    [SerializeField] List<ItemType> AcceptedItemTypes;
    [SerializeField] List<EquipmentType> AcceptedEquipmentTypes;  //further filter when AcceptedItemTypes includes ItemType.Equipment


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
        //show hover effect when hovering over a slot with a dragged item (dont show for the items current home slot)
        if (CharacterUIRef?.CurrentlyDraggedItem != null && CharacterUIRef?.CurrentlyDraggedItem?.SlotRef != this)
            HoverImage.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HoverImage.gameObject.SetActive(false);
    }


    #endregion UNITY METHODS


    #region METHODS


    /// <param name="inventory">The inventory this item slot belongs to</param>
    public void Init(InventorySystem inventory, CharacterUI characterUI)
    {
        InventoryRef = inventory;
        CharacterUIRef = characterUI;
    }

    /// <returns>int index indicating where in the item grid this slot is located</returns>
    public int GetSlotPosition()
    {
        return this.gameObject.transform.GetSiblingIndex();
    }

    public void OnDrop(PointerEventData eventData)
    {
        HoverImage.gameObject.SetActive(false);

        var droppedObj = eventData.pointerDrag;

        //only snap items (anything can be dragged)
        if (droppedObj != null && droppedObj.GetComponent<ItemUI>() != null)
        {
            var droppedItemScript = droppedObj.GetComponent<ItemUI>();
            Debug.Log("Item drop detected");

            HandleInventoryTransaction(droppedItemScript.SlotRef, this, droppedItemScript);

            
        }
    }

    private void HandleInventoryTransaction(ItemSlotUI prevSlot, ItemSlotUI targetSlot, ItemUI item)
    {
        //if the item was dropped to the same inventory slot, no changes need to be made
        if (prevSlot == targetSlot)
            return;

        //else notify inventory to make the appropriate movement
        var operation = prevSlot.InventoryRef.MoveItemToTarget(
            prevSlot.GetSlotPosition(),
            targetSlot.InventoryRef,
            targetSlot.GetSlotPosition()
        );

        //handle changes to prefabs (basedo n the operation that was done in the inventory)
        switch (operation)
        {
            case ItemMoveResult.Moved:
                item.SlotRef = this;
                break;

            case ItemMoveResult.Swapped:
                //need to reparent the item at targetSlot to prevSlot
                var currentlySlottedItem = this.GetSlottedItem();
                currentlySlottedItem.SlotRef = prevSlot;
                currentlySlottedItem.MakeChildOf(prevSlot.ItemContainer.transform);
                currentlySlottedItem.MoveToSlotPosition();

                item.SlotRef = this;
                //done in ItemUI when OnDragDrop happens
                //droppedObj.transform.SetParent(this.transform, true);
                //droppedObj.GetComponent<RectTransform>().position = this.GetComponent<RectTransform>().position;
                break;

            case ItemMoveResult.StackedAll:
                //everything from the stack was consumed & InventoryItem was destroyed, we have to also destroy the prefab
                Destroy(item.gameObject);
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
