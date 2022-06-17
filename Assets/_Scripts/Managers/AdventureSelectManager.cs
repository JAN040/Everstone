using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdventureSelectManager : MonoBehaviour
{
    #region 	VARIABLES


    [Space]
    [Header("UI component references")]
    [Space]

    [SerializeField] GameObject LocationScrollviewContent;
    [SerializeField] GameObject LocationPrefab;

	#endregion 	VARIABLES


	#region 	UNITY METHODS
	
    // Start is called before the first frame update
    void Start()
    {
        var locations = ResourceSystem.Instance.GetAdventureLocations();

        foreach (var location in locations)
        {
            var currentPrefab = Instantiate(LocationPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            var a = currentPrefab.transform.localScale;
            
            currentPrefab.GetComponent<AdventureButton>()?.SetScriptableLocation(location);
            currentPrefab.transform.SetParent(LocationScrollviewContent.transform, true);
            currentPrefab.transform.localScale = new Vector3(1,1,1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
    #endregion 	UNITY METHODS

}
