using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class TabGroupButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public TabGroup TabGroup;
    public Image HoverOverlayImage;
    public RectTransform RectTransform;


    public void Init(TabGroup tabGroup)
    {
        TabGroup = tabGroup;

        if (HoverOverlayImage == null)
            HoverOverlayImage = transform.Find("HoverOvelay").GetComponent<Image>();
        if (HoverOverlayImage != null)
            HoverOverlayImage.gameObject.SetActive(false);
        
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
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (HoverOverlayImage != null)
            HoverOverlayImage.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (HoverOverlayImage != null)
            HoverOverlayImage.gameObject.SetActive(false);
    }
}
