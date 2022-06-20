using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AdventureSelectManager : MonoBehaviour
{
    #region 	VARIABLES

    #region UI references

    [Space]
    [Header("UI component references")]
    [Space]

    [SerializeField] GameObject LocationScrollviewContent;
    [SerializeField] GameObject LocationPrefab;
    [SerializeField] Button StartButton; 


    #endregion UI references

    /// <summary>
    /// Stores references to the location buttons, so we can pass events around
    /// </summary>
    private List<GameObject> locationPrefabList;

    private ScriptableAdventureLocation SelectedLocation;

	#endregion 	VARIABLES


	#region 	UNITY METHODS
	
    // Start is called before the first frame update
    void Start()
    {
        //a location need be selected to start
        StartButton.interactable = false;

        List<ScriptableAdventureLocation> locations = GetAndUpdateLocationData();
        locationPrefabList = new List<GameObject>();

        foreach (var location in locations)
        {
            var currentPrefab = Instantiate(LocationPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            var a = currentPrefab.transform.localScale;
            
            currentPrefab.GetComponent<AdventureButton>()?.SetScriptableLocation(location, this);
            currentPrefab.transform.SetParent(LocationScrollviewContent.transform, true);
            currentPrefab.transform.localScale = new Vector3(1,1,1);
            
            locationPrefabList.Add(currentPrefab);
        }
    }
	
    #endregion 	UNITY METHODS


    public void OnAdventureLocationSelected(ScriptableAdventureLocation selectedLocation)
    {
        StartButton.interactable = true;
        SelectedLocation = selectedLocation;

        foreach (var location in locationPrefabList)
        {
            var adventureButtonScript = location.GetComponent<AdventureButton>();
            if (adventureButtonScript.ScriptableLocation.locationName == selectedLocation.locationName)
                continue;

            adventureButtonScript.Deselect();
        }
    }


    /// <summary>
    /// Updates GameManager AdventureLocationData list and returns a clone of it
    /// </summary>
    private List<ScriptableAdventureLocation> GetAndUpdateLocationData()
    {
        var managerLocations = GameManager.Instance.AdventureLocationData;
        var resLocations = ResourceSystem.Instance.GetAdventureLocations();

        if (managerLocations == null)
        {
            //entering adventure select for the first time this will be empty
            managerLocations = resLocations;
        }
        else
        {
            //in the case new locations were added since last visit update managerLocations
            foreach (var location in resLocations)
            {
                var match = managerLocations.FirstOrDefault(x => x.locationName == location.locationName);
                if (match == null)
                {
                    managerLocations.Add(location);
                }
            }
        }

        return new List<ScriptableAdventureLocation>(managerLocations);
    }

    public void StartAdventure()
    {
        GameManager.Instance.SetCurrentLocation(SelectedLocation);
        SceneManagementSystem.Instance.LoadScene(Scenes.Adventure);
    }
}
