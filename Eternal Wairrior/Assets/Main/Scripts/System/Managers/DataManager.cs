using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class DataManager : SingletonManager<DataManager>
{
    protected Dictionary<string, object> dataContainers;

    protected override void Awake()
    {
        base.Awake();
        InitializeDataStructures();
    }

    protected virtual void InitializeDataStructures()
    {
        dataContainers = new Dictionary<string, object>();
    }

    #region Data Management
    protected virtual void SaveData<T>(string fileName, T data)
    {
        try
        {
            string jsonData = JsonUtility.ToJson(data, true);
            string directory = Path.Combine(Application.persistentDataPath, "Data");
            string path = Path.Combine(directory, fileName);

            Directory.CreateDirectory(directory);
            File.WriteAllText(path, jsonData);
            Debug.Log($"Data saved successfully to {path}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save data: {e.Message}");
        }
    }

    protected virtual T LoadData<T>(string fileName) where T : new()
    {
        try
        {
            string path = Path.Combine(Application.persistentDataPath, "Data", fileName);
            if (File.Exists(path))
            {
                string jsonData = File.ReadAllText(path);
                return JsonUtility.FromJson<T>(jsonData);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load data: {e.Message}");
        }

        return new T();
    }

    protected virtual void RegisterDataContainer<T>(string key, T container)
    {
        if (!dataContainers.ContainsKey(key))
        {
            dataContainers.Add(key, container);
        }
        else
        {
            Debug.LogWarning($"Data container with key {key} already exists");
        }
    }

    protected virtual T GetDataContainer<T>(string key) where T : class
    {
        if (dataContainers.TryGetValue(key, out object container))
        {
            return container as T;
        }
        return null;
    }

    public virtual void SaveAllData()
    {
        foreach (var container in dataContainers)
        {
            SaveData($"{container.Key}.json", container.Value);
        }
    }

    public virtual void LoadAllData()
    {
        // 각 데이터 매니저에서 구현
    }
    #endregion

    #region File Management
    protected virtual string GetDataPath(string fileName)
    {
        return Path.Combine(Application.persistentDataPath, "Data", fileName);
    }

    protected virtual bool DataFileExists(string fileName)
    {
        return File.Exists(GetDataPath(fileName));
    }

    protected virtual void DeleteDataFile(string fileName)
    {
        string path = GetDataPath(fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"Deleted data file: {path}");
        }
    }

    protected virtual void ClearAllData()
    {
        string directory = Path.Combine(Application.persistentDataPath, "Data");
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, true);
            Directory.CreateDirectory(directory);
            dataContainers.Clear();
            Debug.Log("All data cleared");
        }
    }
    #endregion

    #region Error Handling
    protected virtual void HandleDataError(string operation, string details)
    {
        string errorMessage = $"Data {operation} error: {details}";
        Debug.LogError(errorMessage);
        // 추가적인 에러 처리 (예: 사용자에게 알림, 로그 파일 작성 등)
    }
    #endregion

    private void OnApplicationQuit()
    {
        SaveAllData();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveAllData();
        }
    }
}