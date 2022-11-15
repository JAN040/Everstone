using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class GameOverMenu : MonoBehaviour
{
    #region VARIABLES


    #region UI References


    [Header("UI References")]
    [SerializeField] GameObject GameOverMenu_GameObject;

    [SerializeField] TextMeshProUGUI InfoText;
    [SerializeField] Image Skull1;
    [SerializeField] Image Skull2;
    [SerializeField] Sprite SkullHardcore;


    #endregion UI References


    //[Space]
    //[Header("Variables")]


    #endregion VARIABLES


    #region UNITY METHODS


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    #endregion UNITY METHODS


    #region METHODS


    public void Init(string infoMessage, bool isHardcore)
    {
        InfoText.text = infoMessage;

        if (isHardcore)
        {
            Skull1.sprite = SkullHardcore;
            Skull2.sprite = SkullHardcore;
        }
    }
    
    public void Continue_Clicked()
    {
        //dont destroy lol
        //Destroy(GameOverMenu_GameObject.gameObject);

        SceneManagementSystem.Instance.LoadScene(Scenes.Outskirts);
    }


    #endregion METHODS
}
