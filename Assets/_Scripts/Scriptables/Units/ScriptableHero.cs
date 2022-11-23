using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Newtonsoft.Json;

/// <summary>
/// Used to store hero class base data
/// </summary>
[CreateAssetMenu(menuName = "Scriptable/Units/New Hero Class", fileName = "SO_HeroClass_")]
public class ScriptableHero : ScriptableUnitBase
{
    //inherited: public UnitBase Prefab;


    [Space]
    [Header("ScriptableHero")]
    [Space]

    //pre-assigned values - GAME DATA
    //inherited: private CharacterStats _baseStats 
    public string ClassName;
    [HideInInspector]
    public string Background;

    public PointAllocationData pointAllocationData = new PointAllocationData();

    [Space]
    [Header("Runtime Data")]
    [Space]

    //data about character levels
    private LevelSystem levelSystem;
    [JsonProperty] public LevelSystem LevelSystem { get => levelSystem; private set => levelSystem = value; }

    public ScriptableHero()
    {
        //for some reason my hero portraits all face right, might change in the future
        this.FaceDirection = FacingDirection.Right;
    }


    public static ScriptableHero GetHeroFromSaveData(PlayerHeroSaveData data)
    {
        var hero = ResourceSystem.Instance.GetHeroByName(data.className);
        hero.Name = data.playerName;
        hero.Background = data.background;
        hero._stats = data.stats;
        hero.levelSystem = data.levelSystem;

        return hero;
    }

    public PlayerHeroSaveData GetSaveData()
    {
        PlayerHeroSaveData data = new PlayerHeroSaveData(
            ClassName,
            Name,
            Background,
            Stats,
            LevelSystem
        );

        return data;
    }


    public void SetLevelSystem(LevelSystem levelSystem)
    {
        LevelSystem = levelSystem;
    }
}


