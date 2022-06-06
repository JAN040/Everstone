using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CurrencyUI : MonoBehaviour
{
    #region VARIABLES

    [SerializeField]
    private TextMeshProUGUI GoldCoinTextField;

    [SerializeField]
    private TextMeshProUGUI SilverCoinTextField;

    [SerializeField]
    private TextMeshProUGUI CopperCoinTextField;

    public int numOfCopperForOneSilver = 10;
    public int numOfSilverForOneGold = 100;

    #endregion

    #region UNITY METHODS
    void Awake()
    {
        SetCurrencyUI(GameManager.Instance.Currency);
    }

    
    #endregion

    public void SetCurrencyUI(int amount)
    {
        int copperAmnt = amount % numOfCopperForOneSilver;
        //convert all u can to silver
        int tempAmnt = amount / numOfCopperForOneSilver;

        int silverAmnt = tempAmnt % numOfSilverForOneGold;
        int goldAmnt = tempAmnt / numOfSilverForOneGold;

        GoldCoinTextField.text = goldAmnt.ToString();
        SilverCoinTextField.text = silverAmnt.ToString();
        CopperCoinTextField.text = copperAmnt.ToString();
    }

}
