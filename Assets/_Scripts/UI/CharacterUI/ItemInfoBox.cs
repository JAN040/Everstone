using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;


public class ItemInfoBox : MonoBehaviour
{
    #region VARIABLES


    #region UI References


    [Header("UI References")]
    [SerializeField] GameObject Object_ItemInfoBox;

    [Space]
    [SerializeField] Image Image_Icon;

    [SerializeField] TextMeshProUGUI Text_Name;
    [SerializeField] TextMeshProUGUI Text_Rarity;
    [SerializeField] TextMeshProUGUI Text_Value;

    [SerializeField] GameObject Object_Description;
    [SerializeField] TextMeshProUGUI Text_Description;

    [SerializeField] GameObject Object_Effects;
    [SerializeField] TextMeshProUGUI Text_Effects;

    [Space]
    [Header("Buttons")]
    [SerializeField] GameObject Object_ButtonArea;

    [SerializeField] GameObject Button_Equip;
    [SerializeField] GameObject Button_Unequip;
    [SerializeField] GameObject Button_Use;
    [SerializeField] GameObject Button_Split;


    #endregion UI References


    //[Space]
    //[Header("Variables")]
    private InventoryItem itemRef;
    private InventoryItem ItemRef
    {
        get => itemRef;
        set
        {
            itemRef = value;
            UpdateUI();
        }
    }

   


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


    /// <summary>
    /// Shows an info box with item details and its uses.
    /// </summary>
    public void Init(InventoryItem item)
    {
        ItemRef = item;

        Object_ItemInfoBox.SetActive(true);
    }

    private void UpdateUI()
    {
        var itemData = ItemRef.ItemData;

        Image_Icon.sprite = itemData.MenuIcon;

        Text_Name.text   = itemData.DisplayName;
        Text_Rarity.text = $"Rarity: {GetRarityText()}";
        Text_Value.text  = $"Value: {itemData.BuyPrice}"; //TODO: format with coin icons

        Text_Description.text = itemData.Description;
        Object_Description.SetActive(!string.IsNullOrEmpty(Text_Description.text));

        //TODO: effects
        Text_Effects.text = "";
        //TODO: buttons
    }

    private string GetRarityText()
    {
        var rarity = itemRef.ItemData.Rarity;
        Color color = ResourceSystem.GetRarityColor(rarity);
        
        return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{rarity}</color>";
    }

    public void CloseClicked()
    {
        Destroy(Object_ItemInfoBox);
    }


    #endregion METHODS
}
