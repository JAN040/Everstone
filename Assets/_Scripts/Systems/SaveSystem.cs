using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;

public static class SaveSystem
{
    #region VARIABLES


    //[Space]
    //[Header("Variables")]
    private static string SaveFileName_1 = "save1.json";


    #endregion VARIABLES







    #region METHODS

    public static string GetSave1Path()
    {
        return Path.Combine(Application.persistentDataPath, SaveFileName_1);
    }

    public static void SaveGame(GameData data)
    {
        string path = GetSave1Path();

        //JsonSerializer serializer = new JsonSerializer();

        //string jsonData = JsonConvert.SerializeObject(GameManager.Instance.PlayerManager.PlayerHero.Stats, Formatting.Indented);
        string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);

        File.WriteAllText(path, jsonData);
    }

    public static void DeleteCurrentSave()
    {
        string path = GetSave1Path();

        if (File.Exists(path))
            File.Delete(path);
    }

    public static bool SaveFileExists()
    {
        string path = GetSave1Path();

        return File.Exists(path);
    }

    public static GameData LoadGame()
    {
        string path = GetSave1Path();
        string jsonData = File.ReadAllText(path);

        //JsonUtility.FromJsonOverwrite(jsonData, GameManager.Instance);
        //var data = JsonUtility.FromJson<GameData>(path);

        //var data = JsonConvert.DeserializeObject<CharacterStats>(jsonData);
        GameData data = JsonConvert.DeserializeObject<GameData>(jsonData);

        if (data == null)
            Debug.LogError("LoadGame returned NULL GameData");

        //return null;
        return data;
    }

    #endregion METHODS
}
