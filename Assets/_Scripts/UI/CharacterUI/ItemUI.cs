using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class ItemUI : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    #region VARIABLES


    #region UI References


    [Header("UI References")]
    [SerializeField] Image Icon;
    [SerializeField] TextMeshProUGUI StackSizeText;
    [SerializeField] Image RarityBorder;

    [Space]
    [Header("Rarity Border Sprites")]
    [SerializeField] Sprite BorderSprite_None;
    [SerializeField] Sprite BorderSprite_Common;
    [SerializeField] Sprite BorderSprite_Uncommon;
    [SerializeField] Sprite BorderSprite_Rare;
    [SerializeField] Sprite BorderSprite_Epic;
    [SerializeField] Sprite BorderSprite_Legendary;


    #endregion UI References


    [Space]
    [Header("Variables")]

    //reference to the canvas this UI element is on
    private CharacterUI CharacterUIRef;
    [SerializeField] CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    [SerializeField] bool isBeingDragged;
    public bool IsBeingDragged
    {
        get => isBeingDragged;
        private set
        {
            isBeingDragged = value;

            canvasGroup.blocksRaycasts = !value;
            CharacterUIRef.CurrentlyDraggedItem = value ? this : null;
            RarityBorder.gameObject.SetActive(!value);
        }
    }


    public ItemSlotUI SlotRef;
    public InventoryItem ItemRef { get; set; }


    #endregion VARIABLES


    #region UNITY METHODS


    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    // Update is called once per frame
    void Update()
    {
        //if (!IsBeingDragged && SlotRef != null)
        //    rectTransform.position = SlotRef.GetComponent<RectTransform>().position;
    }

    private void OnDestroy()
    {
        ItemRef.OnStackSizeChanged -= UpdateStackSizeText;
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        //Debug.Log("BeginDrag");

        IsBeingDragged = true;
        
        MakeChildOf(CharacterUIRef.DraggedItemContainer.transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        var movement = eventData.delta;
        if (CharacterUIRef.ParentCanvas != null)
            movement /= CharacterUIRef.ParentCanvas.scaleFactor;

        rectTransform.anchoredPosition += movement;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //Debug.Log("EndDrag");

        IsBeingDragged = false;

        if (SlotRef != null)
        {
            MakeChildOf(SlotRef.ItemContainer.transform);
            MoveToSlotPosition();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Click");
    }


    #endregion UNITY METHODS


    #region METHODS


    public void Init(CharacterUI charaUI, InventoryItem item, ItemSlotUI slot = null)
    {
        CharacterUIRef = charaUI;
        SlotRef = slot;

        ItemRef = item;
        ItemRef.OnStackSizeChanged += UpdateStackSizeText;

        this.Icon.sprite = ItemRef.ItemData.MenuIcon;
        RarityBorder.gameObject.SetActive(true);

        SetRarityBorder(ItemRef.ItemData.Rarity);
        UpdateStackSizeText();
    }

    public void ButtonPressed()
    {
        Debug.Log($"Clicked item {ItemRef.ItemData.DisplayName}");
    }


    private void SetRarityBorder(ItemRarity rarity)
    {
        Sprite borderSprite;

        switch (rarity)
        {
            case ItemRarity.Common:
                borderSprite = BorderSprite_Common;
                break;

            case ItemRarity.Uncommon:
                borderSprite = BorderSprite_Uncommon;
                break;

            case ItemRarity.Rare:
                borderSprite = BorderSprite_Rare;
                break;

            case ItemRarity.Epic:
                borderSprite = BorderSprite_Epic;
                break;

            case ItemRarity.Legendary:
                borderSprite = BorderSprite_Legendary;
                break;

            case ItemRarity.None:
            default:
                borderSprite = BorderSprite_None;
                break;
        }

        RarityBorder.sprite = borderSprite;
    }

    private void UpdateStackSizeText()
    {
        //non-stackable items dont show the stack size
        this.StackSizeText.text = ItemRef.ItemData.MaxStackSize == 1 ? "" : ItemRef.StackSize.ToString();
    }


    /// <summary>
    /// Sets the parent of this gameObject
    /// </summary>
    /// <param name="parent"></param>
    public void MakeChildOf(Transform parent)
    {
        if (parent == null)
            return;

        this.transform.SetParent(parent, true);
    }

    public void MoveToSlotPosition()
    {
        this.GetComponent<RectTransform>().position = SlotRef.GetComponent<RectTransform>().position;
    }

    #endregion METHODS
}
