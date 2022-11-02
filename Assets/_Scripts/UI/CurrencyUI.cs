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

    
    private void Start()
    {
        GameManager.Instance.OnCurrencyChanged += CurrencyChanged;
    }

    void OnEnable()
    {
        SetCurrencyUI(GameManager.Instance.Currency);
    }
    
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnCurrencyChanged -= CurrencyChanged;
    }

    #endregion


    public void SetCurrencyUI(int amount)
    {
        CurrencyTextField.text = GameManager.Instance.CurrencyToDisplayString(amount);
    }

    private void CurrencyChanged(int oldAmount, int newAmount)
    {
        SetCurrencyUI(newAmount);
    }
}
