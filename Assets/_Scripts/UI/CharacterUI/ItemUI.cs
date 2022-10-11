using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemUI : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    #region VARIABLES


    #region UI References


    [Header("UI References")]
    [SerializeField] Image icon;
    [SerializeField] TextMeshProUGUI stackSizeText;


    #endregion UI References


    [Space]
    [Header("Variables")]

    //reference to the canvas this UI element is on
    [SerializeField] Canvas canvas;
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

    #endregion UNITY METHODS


    #region METHODS


    public void Init(Canvas canvas, InventoryItem item, ItemSlotUI slot = null)
    {
        this.canvas = canvas;
        SlotRef = slot;

        ItemRef = item;
        ItemRef.OnStackSizeChanged += UpdateStackSizeText;

        this.icon.sprite = ItemRef.itemData.MenuIcon;
        UpdateStackSizeText();
    }

    private void UpdateStackSizeText()
    {
        //non-stackable items dont show the stack size
        this.stackSizeText.text = ItemRef.itemData.MaxStackSize == 1 ? "" : ItemRef.stackSize.ToString();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("BeginDrag");

        IsBeingDragged = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        var movement = eventData.delta;
        if (canvas != null)
            movement /= canvas.scaleFactor;

        rectTransform.anchoredPosition += movement;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("EndDrag");

        IsBeingDragged = false;
        this.GetComponent<RectTransform>().position = SlotRef.GetComponent<RectTransform>().position;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Click");
    }



    #endregion METHODS
}
