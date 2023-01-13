using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraggedItemData
{
    #region VARIABLES


    public bool AreItemsDraggable = true;
    public Canvas CanvasRef { get; private set; }
    public GameObject DraggedItemContainer { get; private set; }
    public ItemGrid InventoryGrid { get; set; }
    public ItemGrid ShopGrid { get; set; }

    public ItemUI CurrentlyDraggedItem;

    private CharacterUI CharacterUIRef;  //fuck me couldnt do it without... only use when cant do without


    #endregion VARIABLES


    public DraggedItemData(Canvas canvas, GameObject draggedItemContainer, ItemGrid playerInventoryGridRef)
    {
        CanvasRef = canvas;
        DraggedItemContainer = draggedItemContainer;
        InventoryGrid = playerInventoryGridRef;

        CurrentlyDraggedItem = null;
    }

    public void SetCharacterUI(CharacterUI characterUI)
    {
        CharacterUIRef = characterUI;
    }

    public CharacterUI GetCharacterUI()
    {
        return CharacterUIRef;
    }
}
