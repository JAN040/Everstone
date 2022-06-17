using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdventureButton : MonoBehaviour
{
    [SerializeField] ScriptableAdventureLocation ScriptableLocation;

    public void SetScriptableLocation(ScriptableAdventureLocation location)
    {
        ScriptableLocation = location;

        var locationIcon = this.transform.Find("Picture");
        var progressText = this.transform.Find("ProgressText");
        var locationName = locationIcon.transform.Find("NameBorder").transform.Find("Text");

        locationIcon.GetComponent<Image>().sprite = location.icon;
        progressText.GetComponent<TextMeshProUGUI>().text = $"Progress: {location.PlayerProgress}/{location.stageAmount}";
        locationName.GetComponent<TextMeshProUGUI>().text = location.locationName;
    }
}
