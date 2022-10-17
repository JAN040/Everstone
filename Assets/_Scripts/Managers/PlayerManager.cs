using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//holds all player related data
[System.Serializable]
public class PlayerManager
{
    #region VARIABLES


    public ScriptableHero PlayerHero { get; private set; }

    public List<ScriptableAbility> ClassicAbilities { get; private set; } = new List<ScriptableAbility>();
    public List<ScriptableAbility> SpecialAbilities { get; private set; } = new List<ScriptableAbility>();

    public InventorySystem Inventory;
    public InventorySystem Storage;
    public EquipmentSystem Equipment;


    #endregion VARIABLES


    #region METHODS

    public void SetHero(ScriptableHero hero)
    {
        PlayerHero = hero;
    }

    public void SetAbilities(List<ScriptableAbility> classicAbilities, List<ScriptableAbility> specialAbilities)
    {
        ClassicAbilities.AddRange(classicAbilities);
        SpecialAbilities.AddRange(specialAbilities);
    }

    public void SetInventory(InventorySystem inventory, InventorySystem storage, EquipmentSystem equipment)
    {
        Inventory = inventory;

        if (storage != null)
            Storage = storage;

        if (equipment != null)
            Equipment = equipment;
    }

    #endregion METHODS


}
