using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BayatGames.SaveGameFree;
using BayatGames.SaveGameFree.Serializers;

public class SystemConfig
{
    static SystemConfig instance;
    public static SystemConfig Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new SystemConfig();
                instance.LoadValues();
                instance.LoadLicenceFile();
            }
            return instance;
        }
    }

    [System.Serializable]
    public class GenericData
    {
        public string key;
        public object data;
    }

    [System.Serializable]
    public class SystemData
    {
        public List<GenericData> DataMap;

        public SystemData()
        {
            DataMap = new List<GenericData>();
        }
    }

    public SystemData saveValue;
    public string identifier = "Config.ini";
    public SystemData saveLicence;
    public string Licence = "Licence.data";

    public void LoadValues()
    {
    #if UNITY_STANDALONE_WIN
        saveValue = SaveGame.Load<SystemData>(identifier, new SystemData(), SaveGamePath.DataPath);
    #else
        saveValue = SaveGame.Load<SystemData>(identifier, new SystemData(), SaveGamePath.PersistentDataPath);
    #endif
        Debug.Log("Data Loaded.");
    }

    public void SaveValues()
    {
    #if UNITY_STANDALONE_WIN
        SaveGame.Save<SystemData>(identifier, saveValue, SaveGamePath.DataPath);
    #else
        SaveGame.Save<SystemData>(identifier, saveValue, SaveGamePath.PersistentDataPath);
    #endif
        Debug.Log("Data Saved.");
    }

    public void LoadLicenceFile(){
        saveLicence = SaveGame.Load<SystemData>(Licence, new SystemData(), SaveGamePath.PersistentDataPath);
        Debug.Log("Licence Loaded.");
    }

    public void SaveLicenceFile(){
        SaveGame.Save<SystemData>(Licence, saveLicence, SaveGamePath.PersistentDataPath);
        Debug.Log("Licence Saved.");
    }

    public T GetLicence<T>(string key, T def = default(T)){
        foreach (var item in saveLicence.DataMap)
        {
            if(item.key == key){
                try {
                    T rst = (T)item.data;
                    return (T)item.data;
                }
                catch(System.Exception e){
                    Debug.LogError(e.Message.ToString());
                    return def;
                }
            }
        }
        Debug.LogWarning($"Can't find Save Key : {key}");
        return def;
    }
    public void SaveLicence(string comingKey, object comingData){
        foreach (var item in saveLicence.DataMap)
        {
            if(item.key == comingKey){
                item.data = comingData;
                return;
            }
        }
        GenericData gData = new GenericData(){
            key = comingKey,
            data = comingData,
        };
        saveLicence.DataMap.Add(gData);
    }
    
    public T GetData<T>(string key, T def = default(T)){
        foreach (var item in saveValue.DataMap)
        {
            if(item.key == key){
                try {
                    T rst = (T)item.data;
                    return (T)item.data;
                }
                catch(System.Exception e){
                    Debug.LogError(e.Message.ToString());
                    return def;
                }
            }
        }
        Debug.LogWarning($"Can't find Save Key : {key} , Use Default '{def}' and Save it");
        return def;
    }

    public void SaveData(string comingKey, object comingData){
        foreach (var item in saveValue.DataMap)
        {
            if(item.key == comingKey){
                item.data = comingData;
                return;
            }
        }
        GenericData gData = new GenericData(){
            key = comingKey,
            data = comingData,
        };
        saveValue.DataMap.Add(gData);
    }
}
