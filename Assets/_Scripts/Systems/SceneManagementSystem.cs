using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Pun;

public class SceneManagementSystem : Singleton<SceneManagementSystem>
{
    #region VARIABLES

    public Animator transition;
    [SerializeField] TextMeshProUGUI SavingGameText;
    
    public float transitionDuration = 0.25f;

    //true while transitioning; prevents the user from spamming the transition button
    //  and starting multiple transition coroutines
    public bool IsSwitchingLocation = false;

    public Scenes ContinueTransitioningTo = Scenes.None;
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
    /// Load scene by build index (not recommended, 
    ///     only for ChangeSceneButton, because Unity editor doesnt support enums as function parameters)
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

    public void LoadSceneWithText(Scenes sc, string text)
    {
        if (IsSwitchingLocation)
            return;

        StartCoroutine(LoadSceneWithTransition(sc, text));
    }

    private IEnumerator LoadSceneWithTransition(Scenes sc, string sceneChangeText = "Saving...")
    {
        IsSwitchingLocation = true;

        if (Time.timeScale != 1f)   //reset timescale cause 0 can literally hardlock the game
            Time.timeScale = 1f;

        //Debug.Log($"Loading scene: {sc}, ID: {(int)sc}");

        //we only save the game when not in multiplayer and not in the listed scenes.
        bool saveGame = !PhotonNetwork.IsConnected &&
                        !sc.In(Scenes.HeroSelect,   //dont save game when switching to hero select; this would overwrite game data immediately...
                               Scenes.MainMenu,
                               Scenes.MultiplayerLobby,
                               Scenes.MultiplayerRoom);

        //we dont save in multiplayer mode, so dont show the text
        SavingGameText.gameObject.SetActive(saveGame);
        if (SavingGameText.gameObject.activeInHierarchy)
            SavingGameText.text = sceneChangeText;

        //play transition "Start" animation
        transition.SetTrigger("Start");

        //wait for the animation to end
        yield return new WaitForSeconds(transitionDuration);

        //allow the new scene to show
        SceneManager.LoadScene((int)sc);

        GameManager.Instance.CurrentScene = sc;

        if (saveGame)
            GameManager.SaveGame();

        transition.SetTrigger("End");
        
        IsSwitchingLocation = false;

        //if another scene is queued to be loaded after this one, continue by loading it immediately
        if (ContinueTransitioningTo != Scenes.None)
        {
            StartCoroutine(LoadSceneWithTransition(ContinueTransitioningTo));
            ContinueTransitioningTo = Scenes.None;
        }
    }
}

public enum Scenes
{
    None = -1, //for ContinueTransitioningTo

    MainMenu = 0,
    Outskirts = 1,
    Town = 2,
    Adventure = 3,
    HeroSelect = 4,
    AdventureSelect = 5,
    Residence = 6,
    Shop = 7,
    MultiplayerLobby = 8,
    MultiplayerRoom = 9,
    MultiplayerGameOver = 10,
}