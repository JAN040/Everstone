using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


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
            if (unitRef?.GetUnit() != null)
            {
                RemoveUnitEvents(unitRef.GetUnit());
            }

            unitRef = value;
            if (value != null && value.GetUnit() != null)
            {
                AddUnitEvents(unitRef.GetUnit());
            }

            SetUpUI();
        }
    }

    //all currently spawned status effect panels are here, so we can clear them on unit switch
    private List<GameObject> StatusEffectPanels = new List<GameObject>();
    private float ColorAnimationTimer;
    private Color InitialColor;

    #endregion VARIABLES


    #region UNITY METHODS

    private void Start()
    {
        ColorAnimationTimer = 0f;
        InitialColor = Attack_Text.color;
    }

    //TODO: animations
    private void Update()
    {
        if (UnitRef == null || UnitRef.Prefab == null)
            return;

        //if (Input.GetKeyDown(KeyCode.H))
        //{
        //    Debug.Log($"Manually Healed unit {unitRef.Name} from {UnitRef.Prefab.GetComponent<Unit>().Stats.HealthPoints} to {UnitRef.Prefab.GetComponent<Unit>().Stats.HealthPoints + 20}");
        //    UnitRef.GetUnit().Heal(20);
        //}
        //if (Input.GetKeyDown(KeyCode.D))
        //{
        //    Debug.Log($"Manually Damaged unit {unitRef.Name} from {UnitRef.Prefab.GetComponent<Unit>().Stats.HealthPoints} to {UnitRef.Prefab.GetComponent<Unit>().Stats.HealthPoints - 20}");
        //    UnitRef.GetUnit().TakeDamage(new Damage(20, DamageType.True));
        //}

        UpdateHpUI();
    }

    private void OnDestroy()
    {
        if (UnitRef != null)
        {
            var unit = UnitRef.GetUnit();
            if (unit != null)
                RemoveUnitEvents(unit);
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

        #region Status Effects

        //clear current status effect panels
        StatusEffectPanels.ForEach(x => Destroy(x.gameObject));

        //spawn new effect panels
        foreach (var effect in unitScript.GetActiveEffects())
        {
            SpawnStatusEffectPanel(effect);
        } 

        #endregion Status Effects
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
        textComponent.color = Color.red;
        ColorAnimationTimer = 0f;

        while (textComponent.color != InitialColor)
        {
            textComponent.color = Color.Lerp(textComponent.color, InitialColor, Time.deltaTime / TextHighlightSpeed);
            
            ColorAnimationTimer += Time.deltaTime;

            if (ColorAnimationTimer > 2f)
            {
                textComponent.color = InitialColor;
                break;
            }

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

        //damage stat setup
        bool showArtsAtkStat = false;
        if (GameManager.Instance.PlayerManager.PlayerHero == UnitRef)
        {
            if (GameManager.Instance.PlayerManager.PlayerHero.ClassName.ToUpper().Equals("MAGE"))
                showArtsAtkStat = true; //if nothing is equipped, show arts for mage

            InventoryItem equipRightArm = GameManager.Instance.PlayerManager.Equipment.GetItemAt((int)EquipmentSlot.RightArm);
            var equipData = equipRightArm?.ItemData as ItemDataEquipment;
            if (equipRightArm != null && equipRightArm != null)
            {   //if staff is equipped show arts
                showArtsAtkStat = equipData.EquipmentType.In(EquipmentType.Staff);
            }
        }
        else
        {   //for non hero units only the highest stat matters
            showArtsAtkStat = unit.Stats.ArtsDamage.GetValue() > unit.Stats.PhysicalDamage.GetValue();
        }
        Attack_Text.text = showArtsAtkStat ?
            $"{GetIcon(Icon.Attack_Arts)} {unit.Stats.ArtsDamage.GetDisplayValue()}"
            :
            $"{GetIcon(Icon.Attack_Phys)} {unit.Stats.PhysicalDamage.GetDisplayValue()}";

        Speed_Text.text = $"{GetIcon(Icon.Speed)} {unit.Stats.Speed.GetDisplayValue()}";
        Defense_Text.text = $"{GetIcon(Icon.Defense)} {unit.Stats.Armor.GetDisplayValue()}";
        Resist_Text.text = $"{GetIcon(Icon.Arts_Resist)} {unit.Stats.ArtsResist.GetDisplayValue()}";
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

        StatusEffectPanels.Add(spawnedPrefab);
    }

    private void AddUnitEvents(Unit unit)
    {
        unit.Stats.OnHealthPointsChanged += HealthPointsChanged;
        unit.Stats.OnStatChanged += UnitStatChanged;
        unit.OnUnitStatusEffectAdded += SpawnStatusEffectPanel;
    }

    private void RemoveUnitEvents(Unit unit)
    {
        unit.Stats.OnHealthPointsChanged -= HealthPointsChanged;
        unit.Stats.OnStatChanged -= UnitStatChanged;
        unit.OnUnitStatusEffectAdded -= SpawnStatusEffectPanel;
    }
}
