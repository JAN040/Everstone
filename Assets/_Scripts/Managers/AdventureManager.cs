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
    [SerializeField] bool IsPaused = false;

    //[SerializeField] float Chance_Event = 0;
    [SerializeField] float Chance_Chest = 0.01f;

    private ScriptableAdventureLocation LocationRef;

    private List<ScriptableUnitBase> AlliedUnitsList;
    private List<ScriptableUnitBase> EnemyUnitsList;

    private UnitGrid UnitGrid;

    #endregion 	VARIABLES


    #region 	UNITY METHODS

    // Start is called before the first frame update
    void Start()
    {
        LocationRef = GameManager.Instance?.CurrentLocation;
        Sprite currLocationSprite = LocationRef?.background;

        if (currLocationSprite != null)
           background.sprite = GameManager.Instance?.CurrentLocation?.background;

        InitStage(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
    #endregion 	UNITY METHODS


    public void TogglePause()
    {
        IsPaused = !IsPaused;

        //DONT FORGET TO UNPAUSE AFTER ANY BUTTON CLICKS
        // (quitting the scene doesnt reset the timescale)
        //Update() still gets called even when timescale = 0
        //use time.unscaledDeltaTime to measure time even when timescale = 0
        Time.timeScale = IsPaused ? 0 : 1;
        
        TogglePausedMenu();
    }
    
    private void TogglePausedMenu()
    {
        if (PauseMenu == null)
            return;

        PauseMenu.SetActive(IsPaused);
    }

    private void InitStage(bool IsFirstInit)
    {
        if (IsFirstInit)
        {
            //Spawn allies
            SpawnAllies();
        }

        //select stage type
        StageType stageType = RollStageType();

        //spawn enemies/chests
        RollEnemies();
        SpawnEnemies();


        //notify about stage number (current progress)
    }

    /// <summary>
    /// Decide the enemies for the stage, from the ScriptableAdventureLocation enemy pool
    /// </summary>
    private void RollEnemies()
    {
        
    }

    private StageType RollStageType()
    {
        if (Helpers.DiceRoll(Chance_Chest))
        {
            return StageType.Chest;
        }

        return StageType.Battle;
    }


    #region Unit Spawning

    private void SpawnAllies()
    {

    }

    private void SpawnEnemies()
    {

    }

    #endregion Unit Spawning
}
