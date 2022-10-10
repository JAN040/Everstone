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



    // Start is called before the first frame update
    void Start()
    {
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
        TabGroup.OnTabSelected(this);
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
