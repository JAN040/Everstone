using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;

/// <summary>
/// Used in CharacterUI/Equipment&Stats tab for the stats to update automatically
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class HeroStatText : MonoBehaviour, IPointerClickHandler
{
    #region VARIABLES

    
    [Header("Parameters")]

    [SerializeField] float TextHighlightSpeed = 1f;


    #region UI References


    //[Header("UI References")]
    private TextMeshProUGUI TextField;

    #endregion UI References


    [Space]
    [Header("Variables")]
    //which stat is the text representing
    [SerializeField] StatType StatType;
    [SerializeField] GameObject StatInfoBoxPrefab;

    private Stat StatRef;
    private float StatValue;

    private bool IsAnimating;
    private float AnimatingForValueChange;

    private float Timer = 0f;
    private Color baseColor;


    #endregion VARIABLES


    #region UNITY METHODS


    // Start is called before the first frame update
    void Start()
    {
        IsAnimating = false;

        TextField = GetComponent<TextMeshProUGUI>();
        if (TextField == null)
            return;

        StatRef = GameManager.Instance.PlayerManager.PlayerHero.Stats.GetStatFromStatType(StatType);
        if (StatRef == null)
            return;

        StatValue = StatRef.GetValue();

        StatRef.OnStatChanged += StatChanged;
        baseColor = TextField.color;

        SetText();
    }

    
    private void OnEnable()
    {
        if (TextField != null)
            TextField.color = baseColor;
        IsAnimating = false;
    }

    private void OnDestroy()
    {
        StatRef.OnStatChanged -= StatChanged;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"Clicked stat: {Stat.GetDisplayName(StatType)}");
        
        var obj = Instantiate(StatInfoBoxPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        obj.transform.localScale = Vector3.one;
        obj.transform.localPosition = Vector3.zero;

        obj.GetComponent<StatInfoBox>().Init(StatType);
    }


    #endregion UNITY METHODS


    #region METHODS


    private void SetText()
    {
        if (TextField == null)
            return;

        string icon = ResourceSystem.GetStatIconTag(StatType);
        TextField.text = $"{icon} {StatRef.GetDisplayValue()}";
    }
    
    private void StatChanged(Stat stat, bool isChangePositive)
    {
        SetText();

        float valueChange = StatRef.GetValue() - StatValue;

        //if already animating and the new change is more impactful, restart animation
        if (IsAnimating && Mathf.Abs(valueChange) > Mathf.Abs(AnimatingForValueChange))
        {
            StopAllCoroutines();
            IsAnimating = false;
            TextField.color = baseColor;
        }

        if (!IsAnimating && this.gameObject != null && this.gameObject.activeInHierarchy)
        {
            AnimatingForValueChange = valueChange;
            StartCoroutine(HighlightTextForSeconds(TextField, isChangePositive));
        }
    }

    private IEnumerator HighlightTextForSeconds(TextMeshProUGUI textComponent, bool isChangePositive)
    {
        IsAnimating = true;
        Timer = 0f;

        Debug.Log($"Started animating for stat {StatRef.Type}");

        
        textComponent.color = isChangePositive ? Color.green : Color.red;

        while (textComponent.color != baseColor)
        {
            textComponent.color = Color.Lerp(textComponent.color, baseColor, Time.deltaTime / TextHighlightSpeed);
            Timer += Time.deltaTime;

            if (Timer > 2f)
            {
                textComponent.color = baseColor;
                break;
            }

            yield return null;
        }

        IsAnimating = false;
        AnimatingForValueChange = 0;
        Debug.Log($"Stopped animating for stat {StatRef.Type}");
    }

    

    #endregion METHODS
}
