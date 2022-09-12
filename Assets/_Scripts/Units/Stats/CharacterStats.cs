
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static UnityEditor.IMGUI.Controls.PrimitiveBoundsHandle;
/// <summary>
/// A struct containing the base character stats
/// </summary>
[System.Serializable]
public class CharacterStats
{
    #region NON STAT VALUES

    [Header("Raw number stats")]
    [Space]

    [SerializeField] private float _healthPoints;
    public float HealthPoints
    {
        get
        {
            return _healthPoints;
        }
        set
        {
            float oldVal = _healthPoints;

            _healthPoints = value;
            _healthPoints = Mathf.Clamp(_healthPoints, 0, this.MaxHP.GetValue());

            OnHealthPointsChanged?.Invoke(_healthPoints, oldVal);
        }
    }

    [SerializeField] private float _energy = 0;
    public float Energy
    {
        get
        {
            return _energy;
        }
        set
        {
            float oldVal = _energy;

            _energy = value;
            _energy = Mathf.Clamp(_energy, 0, this.MaxEnergy.GetValue());

            OnEnergyChanged?.Invoke(_energy, oldVal);
        }
    }

    [SerializeField] private float _mana;
    public float Mana
    {
        get
        {
            return _mana;
        }
        set
        {
            float oldVal = _mana;

            _mana = value;
            _mana = Mathf.Clamp(_mana, 0, this.MaxMana.GetValue());

            OnManaChanged?.Invoke(_mana, oldVal);
        }
    }

    [SerializeField] private int maxPets;
    public int MaxPets { get => maxPets; set => maxPets = value; }

    #endregion NON STAT VALUES

    #region STAT FIELDS

    [Header("Stat object stats")]
    [Space]

    [SerializeField] private Stat physicalDamage;
    [SerializeField] private Stat armor;
    [SerializeField] private Stat artsDamage;
    [SerializeField] private Stat artsResist;
    [SerializeField] private Stat maxHP;
    [SerializeField] private Stat maxEnergy;
    [SerializeField] private Stat energyRecovery;
    [SerializeField] private Stat speed;
    [SerializeField] private Stat dodgeChance;
    [SerializeField] private Stat healEfficiency;
    [SerializeField] private Stat baseAccuracy;
    [SerializeField] private Stat maxMana;
    [SerializeField] private Stat manaRecovery;
    [SerializeField] private Stat blockChance;
    [SerializeField] private Stat cooldownReduction;

    #endregion STAT FIELDS

    #region EVENTS

    /// <summary> Takes two float paramaters, indicating the old and new HP amount </summary>
    public event Action<float, float> OnHealthPointsChanged;
    /// <summary> Takes two float paramaters, indicating the old and new Energy amount </summary>
    public event Action<float, float> OnEnergyChanged;
    /// <summary> Takes two float paramaters, indicating the old and new Mana amount </summary>
    public event Action<float, float> OnManaChanged;
    /// <summary> Takes the changed stat as parameter </summary>
    public event Action<Stat> OnStatChanged;

    #endregion EVENTS


    #region BASE STATS

    public Stat PhysicalDamage { get => physicalDamage; private set => physicalDamage = value; }
    public Stat Armor { get => armor; private set => armor = value; }
    public Stat ArtsDamage { get => artsDamage; private set => artsDamage = value; }
    public Stat ArtsResist { get => artsResist; private set => artsResist = value; }
    public Stat MaxHP { get => maxHP; private set => maxHP = value; }
    public Stat MaxEnergy { get => maxEnergy; private set => maxEnergy = value; }
    public Stat EnergyRecovery { get => energyRecovery; private set => energyRecovery = value; }
    public Stat Speed { get => speed; private set => speed = value; }
    public Stat DodgeChance { get => dodgeChance; private set => dodgeChance = value; }
    public Stat HealEfficiency { get => healEfficiency; private set => healEfficiency = value; }

    #endregion BASE STATS


    #region HERO STATS

    public Stat BaseAccuracy { get => baseAccuracy; private set => baseAccuracy = value; }

    #endregion HERO STATS


    #region PLAYER STATS

    public Stat MaxMana { get => maxMana; private set => maxMana = value; }
    public Stat ManaRecovery { get => manaRecovery; private set => manaRecovery = value; }

    //weapon related
    public Stat BlockChance { get => blockChance; private set => blockChance = value; }

    /// <summary>
    /// Additional CDR from items/potions
    /// </summary>
    public Stat CooldownReduction { get => cooldownReduction; private set => cooldownReduction = value; }


    public Dictionary<WeaponType, WeaponProficiency> WeaponProficiencies { get; private set; }

    #endregion PLAYER STATS


    public CharacterStats(float physicalDamage, float artsDamage, float maxHP, float defense, float artsResist,
                          float maxEnergy, float speed, float dodgeChance,
                          float baseAccuracy = 0,
                          float maxMana = 0, float manaRecovery = 0, float energyRecovery = 0, float cooldownReduction = 0,
                          float blockChance = 0, float healEfficiency = 0)
    {
        PhysicalDamage = new Stat(physicalDamage, StatType.PhysicalDamage, false);
        Armor = new Stat(defense, StatType.Armor, false);
        ArtsDamage = new Stat(artsDamage, StatType.ArtsDamage, false);
        ArtsResist = new Stat(artsResist, StatType.ArtsResist, true);
        MaxHP = new Stat(maxHP, StatType.MaxHP, false);
        _healthPoints = maxHP;
        MaxEnergy = new Stat(maxEnergy, StatType.MaxEnergy, false);
        _energy = 0;
        EnergyRecovery = new Stat(energyRecovery, StatType.EnergyRecovery, true);
        Speed = new Stat(speed, StatType.Speed, false);
        DodgeChance = new Stat(dodgeChance, StatType.DodgeChance, false);
        HealEfficiency = new Stat(0, StatType.HealEfficiency, true);

        BaseAccuracy = new Stat(baseAccuracy, StatType.WeaponAccuracy, false);

        MaxMana = new Stat(maxMana, StatType.MaxMana, false);
        _mana = maxMana;
        ManaRecovery = new Stat(manaRecovery, StatType.ManaRecovery, true);
        MaxPets = 0;
        CooldownReduction = new Stat(cooldownReduction, StatType.CooldownReduction, true);
        BlockChance = new Stat(blockChance, StatType.BlockChance, false);

        //WeaponProficiencies = null;
        SetDefaultWeaponProficiencies(0f, 0f);
        SetStatEvents();
    }

    private void SetStatEvents()
    {
        PhysicalDamage.OnStatChanged += StatChanged;
        Armor.OnStatChanged += StatChanged;
        ArtsDamage.OnStatChanged += StatChanged;
        ArtsResist.OnStatChanged += StatChanged;
        MaxHP.OnStatChanged += StatChanged;
        MaxEnergy.OnStatChanged += StatChanged;
        EnergyRecovery.OnStatChanged += StatChanged;
        MaxMana.OnStatChanged += StatChanged;
        ManaRecovery.OnStatChanged += StatChanged;
        CooldownReduction.OnStatChanged += StatChanged;
        Speed.OnStatChanged += StatChanged;
        DodgeChance.OnStatChanged += StatChanged;
        HealEfficiency.OnStatChanged += StatChanged;
        BlockChance.OnStatChanged += StatChanged;
    }

    public void SetDefaultWeaponProficiencies(float baseDamageBonus, float baseAccuracyBonus)
    {
        WeaponProficiencies = new Dictionary<WeaponType, WeaponProficiency>();

        foreach (WeaponType weapon in (WeaponType[])Enum.GetValues(typeof(WeaponType)))
        {
            if (weapon == WeaponType.None)
                continue;

            WeaponProficiencies.Add(weapon, new WeaponProficiency(weapon, baseDamageBonus, baseAccuracyBonus));
        }
    }

    public float GetHpNormalized()
    {
        return HealthPoints / MaxHP.GetValue();
    }

    public float GetEnergyNormalized()
    {
        return Energy / MaxEnergy.GetValue();
    }

    public void AddModifier(StatModifier modifier)
    {
        if (modifier == null || modifier.Value == 0)
            return;

        Stat stat = GetStatFromStatType(modifier.ModifyingStatType);

        stat.AddModifier(modifier);
    }

    public Stat GetStatFromStatType(StatType type)
    {
        switch (type)
        {
            case StatType.PhysicalDamage:
                return PhysicalDamage;

            case StatType.Armor:
                return Armor;

            case StatType.ArtsDamage:
                return ArtsDamage;

            case StatType.ArtsResist:
                return ArtsResist;

            case StatType.MaxHP:
                return MaxHP;

            case StatType.MaxEnergy:
                return MaxEnergy;

            case StatType.EnergyRecovery:
                return EnergyRecovery;

            case StatType.MaxMana:
                return MaxMana;

            case StatType.ManaRecovery:
                return ManaRecovery;

            case StatType.CooldownReduction:
                return CooldownReduction;

            case StatType.Speed:
                return Speed;

            case StatType.DodgeChance:
                return DodgeChance;

            case StatType.HealEfficiency:
                return HealEfficiency;

            case StatType.BlockChance:
                return BlockChance;

            case StatType.WeaponAccuracy:
            case StatType.Proficiency:
            case StatType.WeaponProficiency:
            default:
                Debug.LogError($"GetStatFromStatType: Unexpected stat type {type}");
                return null;
        }
    }

    private void StatChanged(Stat stat)
    {
        OnStatChanged?.Invoke(stat);
    }
}
