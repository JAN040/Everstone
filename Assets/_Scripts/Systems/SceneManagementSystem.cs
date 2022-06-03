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

    public void LoadScene(int sceneNum)
    {
        //Debug.Log($"Loading scene with ID: {sceneNum}");
        StartCoroutine(LoadSceneWithTransition((Scene)sceneNum));
    }

    public void LoadScene(Scene sc)
    {
        Debug.Log($"Loading scene: {sc}");
        SceneManager.LoadScene((int)sc);
    }

    private IEnumerator LoadSceneWithTransition(Scene sc)
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

public enum Scene
{
    MainMenu = 0,
    Outskirts = 1,
    Town = 2,
    Adventure = 3
}