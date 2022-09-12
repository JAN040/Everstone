using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitStatusBar : MonoBehaviour
{

    #region VARIABLES

    #region UI References

    [Header("UI References")]

    [SerializeField] GameObject UnitStatusBarObject;

    [SerializeField] Image Portrait_Image;
    [SerializeField] Image HealthBar_Image;

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

    private ScriptableUnitBase unitRef;
    public ScriptableUnitBase UnitRef
    {
        get => unitRef;
        set
        {
            unitRef = value;
            UpdateUI();
        }
    }

    #endregion VARIABLES


    #region UNITY METHODS

    //TODO: animations
    private void Update()
    {

    } 

    #endregion UNITY METHODS

    private void UpdateUI()
    {
        var unit = UnitRef;
        var unitScript = UnitRef?.Prefab?.GetComponent<Unit>();

        if (UnitRef == null || unitScript == null)
        {
            UnitStatusBarObject.SetActive(false);

            return;
        }
        else
            UnitStatusBarObject.SetActive(true);

        Name_Text.text = unit.Name;
        UpdatePortrait();


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

    public void UpdateStats()
    {
        var unit = UnitRef?.Prefab?.GetComponent<Unit>();

        if (unit == null)
            return;

        Health_Text.text = $"{unit.Stats.HealthPoints.Round()}/{unit.Stats.MaxHP.GetValue().Round()}";
        HealthBar_Image.fillAmount = unit.Stats.GetHpNormalized();

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
        return ResourceSystem.Instance.GetIconTag(icon);
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
}
