using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerNameInput : MonoBehaviour
{
    #region VARIABLES

    [SerializeField]
    [Tooltip("Reference to the input field the player uses to enter their name.")]
    TMP_InputField inputField;

    private TextMeshProUGUI placeholderText;

    //[SerializeField]
    //TMP_Text playerName;

    #endregion

    #region UNITY METHODS
    // Start is called before the first frame update
    void Start()
    {
        placeholderText = inputField.placeholder.GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (string.IsNullOrEmpty(inputField.text))
            placeholderText.enabled = true;
        else
            placeholderText.enabled = false;
    }

    #endregion

}
