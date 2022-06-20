using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdventureManager : MonoBehaviour
{
    #region 	VARIABLES

    [SerializeField] Image background;
	
	#endregion 	VARIABLES


	#region 	UNITY METHODS
	
    // Start is called before the first frame update
    void Start()
    {
        background.sprite = GameManager.Instance.CurrentLocation.background;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
    #endregion 	UNITY METHODS

}
