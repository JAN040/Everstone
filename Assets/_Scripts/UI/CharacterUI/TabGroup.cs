using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabGroup : MonoBehaviour
{
    //content that should be shown when the appropriate tabButton is pressed.
    //the right GameObject is decided by index (see OnTabSelected)
    public List<GameObject> ObjectsToSwap;

    public List<TabGroupButton> TabButtons;
    public TabGroupButton SelectedTab;

    public Sprite tabButtonIdleImage;
    public Sprite tabButtonSelectedImage;
    public float tabButtonIdleWidth;
    public float tabButtonSelectedWidthIncrease;

    public void Start()
    {
        ResetTabs();

        foreach (var tab in TabButtons)
        {
            tab.TabGroup = this;
        }

        OnTabSelected(TabButtons[0]);
    }

    public void OnTabSelected(TabGroupButton button)
    {
        SelectedTab = button;
        ResetTabs();
        button.BackgroundImage.sprite = tabButtonSelectedImage;
        button.RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, tabButtonIdleWidth + tabButtonSelectedWidthIncrease);

        int tabIndex = button.transform.GetSiblingIndex();
        for (int i = 0; i < ObjectsToSwap.Count; i++)
        {
            if (i == tabIndex)
                ObjectsToSwap[i].SetActive(true);
            else
                ObjectsToSwap[i].SetActive(false);
        }
    }

    //for hover animations... unneeded for now
    //public void OnTabEnter(TabGroupButton button)
    //{
    //    ResetTabs();
    //}

    //public void OnTabExit(TabGroupButton button)
    //{
    //    ResetTabs();
    //}


    private void ResetTabs()
    {
        foreach (var button in TabButtons)
        {
            if (button == SelectedTab)
                continue;

            button.BackgroundImage.sprite = tabButtonIdleImage;
            button.RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, tabButtonIdleWidth);
        }
    }
}
