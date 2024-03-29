﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class AdventureButton : MonoBehaviour
{
    [SerializeField] float DeselectedPixelsPerUnitMultiplier = 1f;
    [SerializeField] float SelectedPixelsPerUnitMultiplier   = 0.5f;
    [SerializeField] float NameBorderSlideUpY = 30f;
    [SerializeField] float OriginalBorderPosY;
    [SerializeField] bool  IsSelected = false;
    public Button ButtonRef;


    [SerializeField] AdventureSelectManager AdventureSelectManagerRef;

    [SerializeField] ScriptableAdventureLocation scriptableLocation;
    public ScriptableAdventureLocation ScriptableLocation { get => scriptableLocation; private set => scriptableLocation = value; }


    public void SetScriptableLocation(ScriptableAdventureLocation location, AdventureSelectManager managerRef)
    {
        ScriptableLocation = location;
        AdventureSelectManagerRef = managerRef;
        NameBorderSlideUpY = 280f;
        IsSelected = false;

        OriginalBorderPosY = transform.Find("Picture").Find("NameBorder").GetComponent<RectTransform>().anchoredPosition.y;
        var locationIcon = this.transform.Find("Picture").GetComponent<Image>();
        var progressText = this.transform.Find("ProgressText").GetComponent<TextMeshProUGUI>();
        var locationName = locationIcon.transform.Find("NameBorder")?.transform?.Find("Text")?.GetComponent<TextMeshProUGUI>();

        locationIcon.sprite = location.icon;
        progressText.text = $"Progress: {location.PlayerProgress}/{location.stageAmount}";
        if (locationName != null)
            locationName.text = location.locationName;

        //multiplayer scenario where the stage progress isnt reset: disable the location button
        if (ScriptableLocation.PlayerProgress >= ScriptableLocation.stageAmount)
        {
            ButtonRef.interactable = false;
            progressText.text += " (Complete)";
        }
        //signify higher danger, because of the nest
        else if (ScriptableLocation.HasPlayerClearedFirstBoss)
        {
            progressText.text += $" (+{ScriptableLocation.LoopCount})";
        }
    }

    public void OnSelected()
    {
        //notify the manager about the selection
        AdventureSelectManagerRef.OnAdventureLocationSelected(this.ScriptableLocation);

        //dont update UI again if this button was already selected
        if (IsSelected)
            return;

        //update UI
        transform.Find("Picture")
                 .Find("PictureButton")
                 .GetComponent<Image>()
                 .pixelsPerUnitMultiplier = SelectedPixelsPerUnitMultiplier;

        var borderTransform = transform.Find("Picture")
                                       .Find("NameBorder")
                                       .GetComponent<RectTransform>();

        borderTransform.anchoredPosition = new Vector2(borderTransform.anchoredPosition.x, borderTransform.anchoredPosition.y + NameBorderSlideUpY);
        
        IsSelected = true;
    }

    public void Deselect()
    {
        transform.Find("Picture")
                 .Find("PictureButton")
                 .GetComponent<Image>()
                 .pixelsPerUnitMultiplier = DeselectedPixelsPerUnitMultiplier;

        var borderTransform = transform.Find("Picture")
                                       .Find("NameBorder")
                                       .GetComponent<RectTransform>();
        borderTransform.anchoredPosition = new Vector2(borderTransform.anchoredPosition.x, OriginalBorderPosY);

        IsSelected = false;
    }
}
