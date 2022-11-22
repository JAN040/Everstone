using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagementSystem : Singleton<SceneManagementSystem>
{
    #region VARIABLES

    public Animator transition;
    
    public float transitionDuration = 0.25f;

    //true while transitioning; prevents the user from spamming the transition button
    //  and starting multiple transition coroutines
    public bool IsSwitchingLocation = false;

    //public Scenes CurrentScene;

    #endregion



    /// <summary>
    /// Dictionary of scene names and their IDs
    /// </summary>



    #region UNITY METHODS
    // Start is called before the first frame update
    //void Start()
    //{

    //}

    //// Update is called once per frame
    //void Update()
    //{

    //}
    #endregion

    /// <summary>
    /// Load scene by object reference
    /// </summary>
    /// <param name="scene">Scene object</param>
    public void LoadScene(Scene scene)
    {
        if (IsSwitchingLocation)
            return;

        StartCoroutine(LoadSceneWithTransition((Scenes)scene.buildIndex));
    }

    /// <summary>
    /// Load scene by build index (not recommended)
    /// </summary>
    /// <param name="sceneNum"></param>
    public void LoadScene(int sceneNum)
    {
        if (IsSwitchingLocation)
            return;

        LoadScene((Scenes)sceneNum);
    }

    /// <summary>
    /// Load scene using the Scenes enum as reference
    /// </summary>
    /// <param name="sc"></param>
    public void LoadScene(Scenes sc)
    {
        if (IsSwitchingLocation)
            return;

        StartCoroutine(LoadSceneWithTransition(sc));
    }

    private IEnumerator LoadSceneWithTransition(Scenes sc)
    {
        IsSwitchingLocation = true;

        if (Time.timeScale == 0f)   //reset timescale cause 0 can literally hardlock the game
            Time.timeScale = 1f;

        //Debug.Log($"Loading scene: {sc}, ID: {(int)sc}");

        //play transition "Start" animation
        transition.SetTrigger("Start");

        //wait for the animation to end
        yield return new WaitForSeconds(transitionDuration);

        //allow the new scene to show
        SceneManager.LoadScene((int)sc);

        GameManager.Instance.CurrentScene = sc;

        //dont save game when switching to hero select; this would overwrite game data immediately...
        if (!sc.In(Scenes.HeroSelect, Scenes.MainMenu))
            GameManager.SaveGame();

        transition.SetTrigger("End");
        
        IsSwitchingLocation = false;
    }
}

public enum Scenes
{
    MainMenu = 0,
    Outskirts = 1,
    Town = 2,
    Adventure = 3,
    HeroSelect = 4,
    AdventureSelect = 5,
    Residence = 6,
    Shop = 7,
}