using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Used in CharacterUI/Equipment&Stats tab for the stats to update automatically
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class HeroStatText : MonoBehaviour
{
    #region VARIABLES

    
    [Header("Parameters")]

    [SerializeField] float TextHighlightSpeed = 1.5f;


    #region UI References


    //[Header("UI References")]
    private TextMeshProUGUI TextField;

    #endregion UI References


    [Space]
    [Header("Variables")]
    //which stat is the text representing
    [SerializeField] StatType StatType;
    private Stat StatRef;
    private bool IsAnimating;
    private float Timer = 0f;


    #endregion VARIABLES


    #region UNITY METHODS


    // Start is called before the first frame update
    void Start()
    {
        IsAnimating = false;

        TextField = GetComponent<TextMeshProUGUI>();
        StatRef = GameManager.Instance.PlayerManager.PlayerHero.BaseStats.GetStatFromStatType(StatType);
        StatRef.OnStatChanged += StatChanged;

        SetText();
    }


    #endregion UNITY METHODS


    #region METHODS


    private void SetText()
    {
        string icon = ResourceSystem.StatIconTag(StatType);
        TextField.text = $"{icon} {StatRef.GetValue().Round()}";
    }
    
    private void StatChanged(Stat stat, bool isChangePositive)
    {
        SetText();

        if (!IsAnimating && this.gameObject.activeInHierarchy)
            StartCoroutine(HighlightTextForSeconds(TextField, isChangePositive));
    }

    private IEnumerator HighlightTextForSeconds(TextMeshProUGUI textComponent, bool isChangePositive)
    {
        IsAnimating = true;
        Timer = 0f;

        Debug.Log($"Started animating for stat {StatRef.Type}");

        Color prevColor = textComponent.color;
        textComponent.color = isChangePositive ? Color.green : Color.red;

        while (textComponent.color != prevColor)
        {
            textComponent.color = Color.Lerp(textComponent.color, prevColor, Time.deltaTime / TextHighlightSpeed);
            Timer += Time.deltaTime;

            if (Timer > 2f)
            {
                textComponent.color = prevColor;
                break;
            }

            yield return null;
        }

        IsAnimating = false;
        Debug.Log($"Stopped animating for stat {StatRef.Type}");
    }

    #endregion METHODS
}
