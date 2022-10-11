using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class TabGroupButton : MonoBehaviour, IPointerClickHandler
{
    public TabGroup TabGroup;
    public Image BackgroundImage;
    public RectTransform RectTransform;


    public void Init(TabGroup tabGroup)
    {
        TabGroup = tabGroup;

        BackgroundImage = transform.Find("BG").GetComponent<Image>();
        RectTransform = this.GetComponent<RectTransform>();
        RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 100f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        TabGroup.TabSelected(this);
    }


    //for hover animations... unneeded for now
    //public void OnPointerEnter(PointerEventData eventData)
    //{
    //    TabGroup.OnTabEnter(this);
    //}

    //public void OnPointerExit(PointerEventData eventData)
    //{
    //    TabGroup.OnTabExit(this);
    //}
}
