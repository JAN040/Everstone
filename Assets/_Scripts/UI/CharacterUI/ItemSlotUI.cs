using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemSlotUI : MonoBehaviour, IDropHandler
{
    #region VARIABLES


    #region UI References


    //[Header("UI References")]


    #endregion UI References


    //[Space]
    //[Header("Variables")]


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

    public void OnDrop(PointerEventData eventData)
    {
        var droppedObj = eventData.pointerDrag;

        //only snap items (anything can be dragged)
        if (droppedObj != null && droppedObj.GetComponent<ItemUI>() != null)
        {
            Debug.Log("Item drop detected");

            droppedObj.GetComponent<ItemUI>().SlotRef = this;
            droppedObj.transform.SetParent(this.transform, true);
            
            //done in ItemUI when OnDragDrop happens
            //droppedObj.GetComponent<RectTransform>().position = this.GetComponent<RectTransform>().position;
        }
    }


    #endregion METHODS
}
