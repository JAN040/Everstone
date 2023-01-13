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
    [SerializeField] GameObject InfoBoxPrefab;
    [SerializeField] TextMeshProUGUI PriceTagText; 


    [Space]
    [Header("Rarity Border Sprites")]
    [SerializeField] Sprite BorderSprite_None;
    [SerializeField] Sprite BorderSprite_Common;
    [SerializeField] Sprite BorderSprite_Uncommon;
    [SerializeField] Sprite BorderSprite_Rare;
    [SerializeField] Sprite BorderSprite_Epic;
    [SerializeField] Sprite BorderSprite_Legendary;

    //[Header("Round Rarity Border Sprites")]
    //[SerializeField] Sprite BorderSpriteRound_None;
    //[SerializeField] Sprite BorderSpriteRound_Common;
    //[SerializeField] Sprite BorderSpriteRound_Uncommon;
    //[SerializeField] Sprite BorderSpriteRound_Rare;
    //[SerializeField] Sprite BorderSpriteRound_Epic;
    //[SerializeField] Sprite BorderSpriteRound_Legendary;


    #endregion UI References


    [Space]
    [Header("Variables")]

    //reference to the canvas this UI element is on
    private DraggedItemData ItemDragData;
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
            ItemDragData.CurrentlyDraggedItem = value ? this : null;
            
            RefreshUIOnDragChange(isBeingDragged);
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
        if (!ItemDragData.AreItemsDraggable)
            return;

        IsBeingDragged = true;
        
        MakeChildOf(ItemDragData.DraggedItemContainer.transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!ItemDragData.AreItemsDraggable)
            return;

        var movement = eventData.delta / ItemDragData.CanvasRef.scaleFactor;

        rectTransform.anchoredPosition += movement;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!ItemDragData.AreItemsDraggable)
            return;

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


    public void Init(DraggedItemData draggedItemData, InventoryItem item, ItemSlotUI slot = null)
    {
        ItemDragData = draggedItemData;
        SlotRef = slot;

        ItemRef = item;
        ItemRef.OnStackSizeChanged += UpdateStackSizeText;

        this.Icon.sprite = ItemRef.ItemData.MenuIcon;
        RarityBorder.gameObject.SetActive(!IsEquippedRune());
        PriceTagText.gameObject.SetActive(ItemRef.IsShopOwned);
        PriceTagText.text = GameManager.Instance.CurrencyToDisplayString(ItemRef.ItemData.BuyPrice, true);

        SetRarityBorder(ItemRef.ItemData.Rarity);
        UpdateStackSizeText();
    }

    public void ButtonPressed()
    {
        Debug.Log($"Clicked item {ItemRef.ItemData.DisplayName}");
        var obj = Instantiate(InfoBoxPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        obj.GetComponent<ItemInfoBox>().Init(ItemRef);
        //obj.transform.SetParent(parentTransform, true);
        //obj.transform.localScale = new Vector3(1, 1, 1);
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
        this.StackSizeText.text = ItemRef.ItemData.MaxStackSize == 1 ? "" : ItemRef.StackSize.ToKiloString();
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
        this.GetComponent<RectTransform>().localPosition = Vector3.zero;
        this.GetComponent<RectTransform>().localScale = Vector3.one;
    }

    public void SlotInto(ItemSlotUI slot)
    {
        SlotRef = slot;
        MakeChildOf(slot.ItemContainer.transform);
        MoveToSlotPosition();
        RefreshUIOnDragChange(IsBeingDragged);
    }

    /// <returns>True, if this item is a rune that is also equipped</returns>
    private bool IsEquippedRune()
    {
        if (ItemRef == null || !ItemRef.IsRune())
            return false;

        if(GameManager.Instance.PlayerManager.Runes.IsEquipped(ItemRef))
            return true;

        return false;
    }

    public void RefreshUIOnDragChange(bool isDragged)
    {
        bool isEquippedRune = IsEquippedRune(); //equipped runes shouldnt show a border
        
        RarityBorder.gameObject.SetActive(!isDragged && !isEquippedRune);
        PriceTagText.gameObject.SetActive(!isDragged && ItemRef.IsShopOwned);
    }


    #endregion METHODS
}
