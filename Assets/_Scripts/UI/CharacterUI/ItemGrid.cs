using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.FilePathAttribute;

public class ItemGrid : MonoBehaviour
{
    #region VARIABLES


    #region UI References


    [Header("UI References")]

    [SerializeField] GridLayoutGroup itemContainerGrid;
    [SerializeField] GameObject itemContainerPrefab;
    [SerializeField] GameObject itemPrefab;

    private List<GameObject> ItemContainerList;

    #endregion UI References


    [Space]
    [Header("Variables")]

    [SerializeField] InventorySystem ItemSource;
    private Canvas ParentCanvas; //need this for ItemUI


    #endregion VARIABLES


    private void OnDestroy()
    {
        CleanUp();
    }


    public void Initialize(InventorySystem itemSource, Canvas canvas)
    {
        ParentCanvas = canvas;
        ItemSource = itemSource;
        ItemSource.OnInventoryChanged += RefreshInventory;
        ItemContainerList = new List<GameObject>();

        //spawn item containers and items where appropriate
        for (int i = 0; i < ItemSource.InventorySize; i++)
        {
            var currContainerPrefab = InstantiatePrefab(itemContainerPrefab, itemContainerGrid.transform);

            if (ItemSource.InventoryItems[i] != null)
            {
                var currItemPrefab = InstantiatePrefab(itemPrefab, currContainerPrefab.transform);
                currItemPrefab.GetComponent<ItemUI>().Init(
                    ParentCanvas,
                    ItemSource.InventoryItems[i],
                    currContainerPrefab.GetComponent<ItemSlotUI>()
                );
            }


            ItemContainerList.Add(currContainerPrefab);
        }
    }

    private GameObject InstantiatePrefab(GameObject prefab, Transform parentTransform)
    {
        var obj = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
        obj.transform.SetParent(parentTransform, true);
        obj.transform.localScale = new Vector3(1, 1, 1);

        return obj;
    }

    public void CleanUp()
    {
        if (ItemSource != null)
            ItemSource.OnInventoryChanged -= RefreshInventory;
        
        if (ItemContainerList != null && ItemContainerList.Count > 0)
        {
            ItemContainerList.ForEach(x => Destroy(x));
            ItemContainerList.Clear();
        }
    }

    public void RefreshInventory()
    {

    }
   
}
