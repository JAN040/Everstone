using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagementSystem : Singleton<SceneManagementSystem>
{
    #region VARIABLES

    public Animator transition;
    public float transitionDuration = 1f;

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
        StartCoroutine(LoadSceneWithTransition((Scenes)scene.buildIndex));
    }

    /// <summary>
    /// Load scene by build index (not recommended)
    /// </summary>
    /// <param name="sceneNum"></param>
    public void LoadScene(int sceneNum)
    {
        LoadScene((Scenes)sceneNum);
    }

    /// <summary>
    /// Load scene using the Scenes enum as reference
    /// </summary>
    /// <param name="sc"></param>
    public void LoadScene(Scenes sc)
    {
        //SceneManager.LoadScene((int)sc);

        StartCoroutine(LoadSceneWithTransition(sc));
    }

    private IEnumerator LoadSceneWithTransition(Scenes sc)
    {
        Debug.Log($"Loading scene: {sc}, ID: {(int)sc}");

        //play transition "Start" animation
        transition.SetTrigger("Start");

        //wait for the animation to end
        yield return new WaitForSeconds(transitionDuration);

        //allow the new scene to show
        SceneManager.LoadScene((int)sc);

        transition.SetTrigger("End");
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
}