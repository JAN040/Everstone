using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusEffectUI : MonoBehaviour
{
    #region VARIABLES


    #region UI References


    [Header("UI References")]

    [SerializeField] Button EffectButton;
    [SerializeField] Image  EffectImage;
    [SerializeField] Image  EffectDurationIndicatorImage;
    [SerializeField] TextMeshProUGUI EffectValueText;
    [SerializeField] GameObject StatusEffectPanelPrefab;


    #endregion UI References


    [Space]
    [Header("Variables")]

    [SerializeField] ScriptableStatusEffect Effect;


    #endregion VARIABLES


    // Update is called once per frame
    void Update()
    {
        if (Effect == null)
            return;

        EffectValueText.text = Effect.DisplayValue;
        EffectDurationIndicatorImage.fillAmount = Effect.GetRemainingDurationNormalized();
    }

    public void Initialize(ScriptableStatusEffect effect)
    {
        Effect = effect;

        EffectValueText.text                = effect.DisplayValue;
        EffectImage.sprite                  = effect.MenuImage;
        EffectDurationIndicatorImage.sprite = effect.MenuImage;
    }

    public void IconButtonClicked()
    {
        Debug.Log($"Status effect: '{Effect?.Name} Duration: '{Effect.CurrentDuration:0.00}/{Effect.DurationAtStart:0.00}'");
    }
}
