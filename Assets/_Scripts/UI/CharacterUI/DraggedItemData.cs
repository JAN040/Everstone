using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraggedItemData
{
    #region VARIABLES


    public float CanvasScaleFactor { get; private set; }
    public GameObject DraggedItemContainer { get; private set; }
    public ItemGrid InventoryGrid { get; set; }


    public ItemUI CurrentlyDraggedItem;

    private CharacterUI CharacterUIRef;  //fuck me couldnt do it without... only use when cant do without


    #endregion VARIABLES


    public DraggedItemData(float scaleFactor, GameObject draggedItemContainer, ItemGrid itemGrid)
    {
        CanvasScaleFactor = scaleFactor;
        DraggedItemContainer = draggedItemContainer;
        InventoryGrid = itemGrid;

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
