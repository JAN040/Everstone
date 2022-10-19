using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CurrencyUI : MonoBehaviour
{
    #region VARIABLES

    [SerializeField]
    private TextMeshProUGUI CurrencyTextField;

    

    #endregion

    #region UNITY METHODS

    void OnEnable()
    {
        SetCurrencyUI(GameManager.Instance.Currency);
    }
    
    #endregion

    public void SetCurrencyUI(int amount)
    {
        CurrencyTextField.text = GameManager.Instance.CurrencyToDisplayString(amount);
    }

}
