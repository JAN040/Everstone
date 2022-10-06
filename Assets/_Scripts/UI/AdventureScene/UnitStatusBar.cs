using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.FilePathAttribute;

public class UnitStatusBar : MonoBehaviour
{

    #region VARIABLES


    [Header("Parameters")]

    [SerializeField] float TextHighlightSpeed = 2;


    #region UI References

    [Header("UI References")]

    [SerializeField] GameObject UnitStatusBarObject;

    [SerializeField] Image Portrait_Image;
    [SerializeField] Image HealthBar_Image;
    [SerializeField] Image BackHealthBar_Image;
    [SerializeField] Sprite BackHealthBar_Damage_Image;
    [SerializeField] Sprite BackHealthBar_Heal_Image;

    [SerializeField] Image PortraitBorder_Image;
    [SerializeField] Image HealthBarBorder_Image;
    [SerializeField] Image InfoPartBorder_Image;

    [Space]
    [SerializeField] TextMeshProUGUI Name_Text;
    [SerializeField] TextMeshProUGUI Health_Text;
    [SerializeField] TextMeshProUGUI Attack_Text;
    [SerializeField] TextMeshProUGUI Speed_Text;
    [SerializeField] TextMeshProUGUI Defense_Text;
    [SerializeField] TextMeshProUGUI Resist_Text;

    [Space]
    [Header("Effect panel grid")]
    [SerializeField] GridLayoutGroup EffectPanelGrid;
    [SerializeField] GameObject EffectPanelPrefab;

    [Space]
    [Header("References for sprite swapping")]

    [SerializeField] Sprite PortraitBorder_Sprite;
    [SerializeField] Sprite HealthBarBorder_Sprite;
    [SerializeField] Sprite InfoPartBorder_Sprite;

    [SerializeField] Sprite PortraitBorder_Sprite_Elite;

    [SerializeField] Sprite PortraitBorder_Sprite_Gold;
    [SerializeField] Sprite HealthBarBorder_Sprite_Gold;
    [SerializeField] Sprite InfoPartBorder_Sprite_Gold;

    #endregion UI References


    [Space]
    [Header("Variables")]

    [SerializeField] Faction BelongingFaction;
    
    //healthbar animation
    [SerializeField] float ChipSpeed = 2f;
    [SerializeField] float LerpTimer = 0;

    [SerializeField] float Health;
    [SerializeField] float MaxHealth;


    private ScriptableUnitBase unitRef;
    public ScriptableUnitBase UnitRef
    {
        get => unitRef;
        set
        {
            if (unitRef != null && unitRef.GetUnit() != null)
            {
                unitRef.GetUnit().Stats.OnHealthPointsChanged -= HealthPointsChanged;
                unitRef.GetUnit().Stats.OnStatChanged -= UnitStatChanged;
                unitRef.GetUnit().OnUnitStatusEffectAdded -= SpawnStatusEffectPanel;
            }

            unitRef = value;
            if (value != null && value.GetUnit() != null)
            {
                unitRef.GetUnit().Stats.OnHealthPointsChanged += HealthPointsChanged;
                unitRef.GetUnit().Stats.OnStatChanged += UnitStatChanged;
                unitRef.GetUnit().OnUnitStatusEffectAdded += SpawnStatusEffectPanel;
            }

            SetUpUI();
        }
    }


    #endregion VARIABLES


    #region UNITY METHODS

    //TODO: animations
    private void Update()
    {
        if (UnitRef == null || UnitRef.Prefab == null)
            return;

        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log($"Manually Healed unit {unitRef.Name} from {UnitRef.Prefab.GetComponent<Unit>().Stats.HealthPoints} to {UnitRef.Prefab.GetComponent<Unit>().Stats.HealthPoints + 20}");
            UnitRef.GetUnit().Heal(20);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log($"Manually Damaged unit {unitRef.Name} from {UnitRef.Prefab.GetComponent<Unit>().Stats.HealthPoints} to {UnitRef.Prefab.GetComponent<Unit>().Stats.HealthPoints - 20}");
            UnitRef.GetUnit().TakeDamage(new Damage(20, DamageType.True), false);
        }

        UpdateHpUI();
    }

    private void UpdateHpUI()
    {
        float fillFront = HealthBar_Image.fillAmount;
        float fillBack = BackHealthBar_Image.fillAmount;
        float hpFraction = Health / MaxHealth;

        if (fillBack > hpFraction)
        {
            //unit took damage
            BackHealthBar_Image.sprite = BackHealthBar_Damage_Image;
            HealthBar_Image.fillAmount = hpFraction;

            LerpTimer += Time.deltaTime;
            float percentComplete = LerpTimer / ChipSpeed;
            percentComplete = percentComplete * percentComplete;

            //Debug.Log($"Animating damage taken for unit: {unitRef.Name}, percent complete: {percentComplete}");

            BackHealthBar_Image.fillAmount = Mathf.Lerp(fillBack, hpFraction, percentComplete);
        }
        
        if (fillFront < hpFraction)
        {
            //unit got healed
            BackHealthBar_Image.sprite = BackHealthBar_Heal_Image;
            BackHealthBar_Image.fillAmount = hpFraction;

            LerpTimer += Time.deltaTime;
            float percentComplete = LerpTimer / ChipSpeed;
            percentComplete = percentComplete * percentComplete;

            //Debug.Log($"Animating healing for unit: {unitRef.Name}, percent complete: {percentComplete}");

            HealthBar_Image.fillAmount = Mathf.Lerp(fillFront, hpFraction, percentComplete);
        }
    }

    #endregion UNITY METHODS

    private void SetUpUI()
    {
        var unit = UnitRef;
        var unitScript = (UnitRef == null || UnitRef.Prefab == null) ? null : UnitRef.Prefab.GetComponent<Unit>();

        if (UnitRef == null || unitScript == null)
        {
            UnitStatusBarObject.SetActive(false);

            return;
        }
        else
            UnitStatusBarObject.SetActive(true);

        Name_Text.text = unit.Name;
        UpdatePortrait();

        HealthBar_Image.fillAmount = unitScript.Stats.GetHpNormalized();
        BackHealthBar_Image.fillAmount = HealthBar_Image.fillAmount;

        Health = unitScript.Stats.HealthPoints;
        MaxHealth = unitScript.Stats.MaxHP.GetValue();

        #region Stats

        UpdateStats();

        #endregion Stats

        #region Sprite swaps

        if (unit.Type == EnemyType.Elite)
            SetEliteSprites();
        else if (unit.Type == EnemyType.Boss)
            SetBossSprites();
        else
            SetClassicSprites();

        #endregion Sprite swaps
    }

    /// <summary>
    /// The method that handles the OnStatChanged event of Unit.
    /// When any of its stats are changed, we should update the StatusBar
    ///     (if the unit is selected, and therefore shown in the StatusBar)
    /// </summary>
    private void UnitStatChanged(Stat stat)
    {
        UpdateStats();

        switch (stat.Type)
        {
            case StatType.PhysicalDamage:
            case StatType.ArtsDamage:
                StartCoroutine(HighlightTextForSeconds(Attack_Text));
                break;
            case StatType.Armor:
                StartCoroutine(HighlightTextForSeconds(Defense_Text));
                break;
            case StatType.ArtsResist:
                StartCoroutine(HighlightTextForSeconds(Resist_Text));
                break;
            case StatType.MaxHP:
                StartCoroutine(HighlightTextForSeconds(Health_Text));
                break;
            case StatType.Speed:
                StartCoroutine(HighlightTextForSeconds(Speed_Text));
                break;
            default:
                break;
        }
    }

    private IEnumerator HighlightTextForSeconds(TextMeshProUGUI textComponent)
    {
        Color prevColor = textComponent.color;
        textComponent.color = Color.red;

        while (textComponent.color != prevColor)
        {
            textComponent.color = Color.Lerp(textComponent.color, prevColor, Time.deltaTime / TextHighlightSpeed);
            yield return null;
        }
    }

    public void UpdateStats()
    {
        if (UnitRef == null || UnitRef.Prefab == null)
            return;

        var unit = UnitRef.Prefab.GetComponent<Unit>();

        Health_Text.text = $"{unit.Stats.HealthPoints.RoundHP()}/{unit.Stats.MaxHP.GetValue().RoundHP()}";
        MaxHealth = unit.Stats.MaxHP.GetValue();

        Attack_Text.text = unit.Stats.ArtsDamage.GetValue() > 0 ?
            $"{GetIcon(Icon.Attack_Arts)} {unit.Stats.ArtsDamage.GetValue().Round()}"
            :
            $"{GetIcon(Icon.Attack_Phys)} {unit.Stats.PhysicalDamage.GetValue().Round()}";

        Speed_Text.text = $"{GetIcon(Icon.Speed)} {unit.Stats.Speed.GetValue().Round()}";
        Defense_Text.text = $"{GetIcon(Icon.Defense)} {unit.Stats.Armor.GetValue().Round()}";
        Resist_Text.text = $"{GetIcon(Icon.Arts_Resist)} {unit.Stats.ArtsResist.GetValue().Round()}";
    }

    private string GetIcon(Icon icon)
    {
        return ResourceSystem.GetIconTag(icon);
    }

    private void UpdatePortrait()
    {
        Portrait_Image.sprite = UnitRef.MenuSprite;

        var scale = Portrait_Image.transform.localScale;

        if (BelongingFaction == Faction.Allies)
        {
            if (UnitRef.FaceDirection == FacingDirection.Left)
                Portrait_Image.transform.localScale = new Vector2(-1f, scale.y);
            else
                Portrait_Image.transform.localScale = new Vector2(1f, scale.y);
        }
        else
        {
            if (UnitRef.FaceDirection == FacingDirection.Right)
                Portrait_Image.transform.localScale = new Vector2(-1f, scale.y);
            else
                Portrait_Image.transform.localScale = new Vector2(1f, scale.y);
        }
    }

    private void SetClassicSprites()
    {
        PortraitBorder_Image.sprite = PortraitBorder_Sprite;
        HealthBarBorder_Image.sprite = HealthBarBorder_Sprite;
        InfoPartBorder_Image.sprite = InfoPartBorder_Sprite;
    }

    private void SetEliteSprites()
    {
        PortraitBorder_Image.sprite = PortraitBorder_Sprite_Elite;
        HealthBarBorder_Image.sprite = HealthBarBorder_Sprite;
        InfoPartBorder_Image.sprite = InfoPartBorder_Sprite;
    }

    private void SetBossSprites()
    {
        PortraitBorder_Image.sprite = PortraitBorder_Sprite_Gold;
        HealthBarBorder_Image.sprite = HealthBarBorder_Sprite_Gold;
        InfoPartBorder_Image.sprite = InfoPartBorder_Sprite_Gold;
    }

    private void HealthPointsChanged(float newHp, float oldHp)
    {
        Health = newHp;
        LerpTimer = 0;

        Health_Text.text = $"{Health.RoundHP()}/{MaxHealth.RoundHP()}";
    }

    private void SpawnStatusEffectPanel(ScriptableStatusEffect effect)
    {
        var spawnedPrefab = Instantiate(EffectPanelPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        
        spawnedPrefab.transform.SetParent(EffectPanelGrid.transform, false);
        spawnedPrefab.transform.localScale = Vector3.one;
        spawnedPrefab.GetComponent<StatusEffectUI>()?.Initialize(effect);

        effect.Prefab = spawnedPrefab;
    }
}
