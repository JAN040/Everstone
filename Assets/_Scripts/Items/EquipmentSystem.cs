using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EquipmentSystem : InventorySystem
{
    #region VARIABLES

    //hardcoded equipment slots cause fuck it
    private InventoryItem EquipmentSlot_Helmet; 
    private InventoryItem EquipmentSlot_Shoulder; 
    private InventoryItem EquipmentSlot_Chestplate; 
    private InventoryItem EquipmentSlot_Pants; 
    private InventoryItem EquipmentSlot_Boots; 

    private InventoryItem EquipmentSlot_Necklace; 
    private InventoryItem EquipmentSlot_Cape; 
    private InventoryItem EquipmentSlot_Gloves; 
    private InventoryItem EquipmentSlot_Ring1; 
    private InventoryItem EquipmentSlot_Ring2;

    private InventoryItem EquipmentSlot_RightArm;
    private InventoryItem EquipmentSlot_LeftArm; 


    public Dictionary<EquipmentSlot, List<EquipmentType>> AcceptedEquipmentTypes = new Dictionary<EquipmentSlot, List<EquipmentType>>()
    {
        { EquipmentSlot.Helmet,     new List<EquipmentType>(){ EquipmentType.Helmet }       },
        { EquipmentSlot.Shoulder,   new List<EquipmentType>(){ EquipmentType.Shoulder }     },
        { EquipmentSlot.Chestplate, new List<EquipmentType>(){ EquipmentType.Chestplate }   },
        { EquipmentSlot.Pants,      new List<EquipmentType>(){ EquipmentType.Pants }        },
        { EquipmentSlot.Boots,      new List<EquipmentType>(){ EquipmentType.Boots }        },

        { EquipmentSlot.Necklace,   new List<EquipmentType>(){ EquipmentType.Necklace }     },
        { EquipmentSlot.Cape,       new List<EquipmentType>(){ EquipmentType.Cape }         },
        { EquipmentSlot.Gloves,     new List<EquipmentType>(){ EquipmentType.Gloves }       },
        { EquipmentSlot.Ring1,      new List<EquipmentType>(){ EquipmentType.Ring }         },
        { EquipmentSlot.Ring2,      new List<EquipmentType>(){ EquipmentType.Ring }         },

        { EquipmentSlot.LeftArm,    new List<EquipmentType>(){
                                        EquipmentType.Sword, 
                                        EquipmentType.Axe, 
                                        EquipmentType.Staff,
                                        EquipmentType.Dagger,
                                        EquipmentType.Shield,
                                    }
        },
        { EquipmentSlot.RightArm,   new List<EquipmentType>(){
                                        EquipmentType.Sword,
                                        EquipmentType.Axe,
                                        EquipmentType.Staff,
                                        EquipmentType.Dagger,
                                    }
        },
    };

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


    public EquipmentSystem() : base(12)
    {

    }


    #region METHODS


    /// <summary>
    /// Equips the item to the appropriate slot
    /// </summary>
    /// <returns>The previously equipped item, to enable the caller to make a swap. Or the param item on failure</returns>
    public InventoryItem EquipItem(InventoryItem item)
    {
        if (item.ItemData.ItemType != ItemType.Equipment)
            return item;

        ItemDataEquipment itemData = item.ItemData as ItemDataEquipment;
        if (itemData == null)
            return item;

        InventoryItem unequippedItem = null;

        switch (itemData.EquipmentType)
        {
            case EquipmentType.Sword:
            case EquipmentType.Dagger:
            case EquipmentType.Axe:
            case EquipmentType.Staff:
            case EquipmentType.Shield:
                if (itemData.EquipmentType == EquipmentType.Shield ||
                    EquipmentSlot_LeftArm == null && EquipmentSlot_RightArm != null)
                {   //equip to left arm
                    unequippedItem = EquipmentSlot_LeftArm;
                    EquipmentSlot_LeftArm = item;
                }
                else
                {   //equip to right arm
                    unequippedItem = EquipmentSlot_RightArm;
                    EquipmentSlot_RightArm = item;
                }
                break;

            case EquipmentType.Helmet:
                unequippedItem = EquipmentSlot_Helmet;
                EquipmentSlot_Helmet = item;
                break;

            case EquipmentType.Shoulder:
                unequippedItem = EquipmentSlot_Shoulder;
                EquipmentSlot_Shoulder = item;
                break;

            case EquipmentType.Chestplate:
                unequippedItem = EquipmentSlot_Chestplate;
                EquipmentSlot_Chestplate = item;
                break;

            case EquipmentType.Pants:
                unequippedItem = EquipmentSlot_Pants;
                EquipmentSlot_Pants = item;
                break;

            case EquipmentType.Boots:
                unequippedItem = EquipmentSlot_Boots;
                EquipmentSlot_Boots = item;
                break;

            case EquipmentType.Necklace:
                unequippedItem = EquipmentSlot_Necklace;
                EquipmentSlot_Necklace = item;
                break;

            case EquipmentType.Cape:
                unequippedItem = EquipmentSlot_Cape;
                EquipmentSlot_Cape = item;
                break;

            case EquipmentType.Gloves:
                unequippedItem = EquipmentSlot_Gloves;
                EquipmentSlot_Gloves = item;
                break;

            case EquipmentType.Ring:
                if (EquipmentSlot_Ring2 == null)
                {   //equip to ring2
                    unequippedItem = EquipmentSlot_Ring2;
                    EquipmentSlot_Ring2 = item;
                }
                else
                {   //equip to ring1
                    unequippedItem = EquipmentSlot_Ring1;
                    EquipmentSlot_Ring1 = item;
                }
                break;

            case EquipmentType.None:
            default:
                return item;
        }

        HandleModifiersOnEquipmentChange(unequippedItem, item);

        return unequippedItem;
    }

    private void HandleModifiersOnEquipmentChange(InventoryItem unequippedItem, InventoryItem equippedItem)
    {
        throw new NotImplementedException();
    }


    #endregion METHODS
}
