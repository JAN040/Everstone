
using System;
using System.Collections.Generic;
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
            Mathf.Clamp(_healthPoints, 0, this.MaxHP.GetValue());

            OnHealthPointsChanged?.Invoke(_healthPoints, oldVal);
        }
    }

    [SerializeField] private float _energy;
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
            Mathf.Clamp(_energy, 0, this.MaxEnergy.GetValue());

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

            Mathf.Clamp(_mana, 0, this.MaxMana.GetValue());

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


    public CharacterStats(float physicalDamage, float defense, float artsDamage, float artsResist, float maxHP,
                          float maxEnergy, float energyRecovery, float speed, float dodgeChance,
                          float healEfficiency = 0,
                          float baseAccuracy = 0,
                          float maxMana = 0, float manaRecovery = 0, float cooldownReduction = 0,
                          float blockChance = 0)
    {
        PhysicalDamage = new Stat(physicalDamage, StatType.PhysicalDamage, false);
        Armor = new Stat(defense, StatType.Armor, false);
        ArtsDamage = new Stat(artsDamage, StatType.ArtsDamage, false);
        ArtsResist = new Stat(artsResist, StatType.ArtsResist, true);
        MaxHP = new Stat(maxHP, StatType.MaxHP, false);
        _healthPoints = maxHP;
        MaxEnergy = new Stat(maxEnergy, StatType.MaxEnergy, false);
        _energy = maxEnergy;
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
}
