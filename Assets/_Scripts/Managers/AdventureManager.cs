using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdventureManager : MonoBehaviour
{
    #region 	VARIABLES

    #region UI References

    [SerializeField] GameObject PauseMenu;

    #endregion UI References


    [SerializeField] Image background;
    [SerializeField] bool isPaused = false;
   


    #endregion 	VARIABLES


    #region 	UNITY METHODS

    // Start is called before the first frame update
    void Start()
    {
        Sprite currLocationSprite = GameManager.Instance?.CurrentLocation?.background;

        if (currLocationSprite != null)
           background.sprite = GameManager.Instance?.CurrentLocation?.background;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
    #endregion 	UNITY METHODS


    public void TogglePause()
    {
        isPaused = !isPaused;

        //DONT FORGET TO UNPAUSE AFTER ANY BUTTON CLICKS
        // (quitting the scene doesnt reset the timescale)
        //Update() still gets called even when timescale = 0
        //use time.unscaledDeltaTime to measure time even when timescale = 0
        Time.timeScale = isPaused ? 0 : 1;
        
        TogglePausedMenu();
    }
    
    private void TogglePausedMenu()
    {
        if (PauseMenu == null)
            return;

        PauseMenu.SetActive(isPaused);
    }

}
