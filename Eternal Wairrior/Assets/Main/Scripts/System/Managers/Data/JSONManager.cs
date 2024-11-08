using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

public class JSONManager<T> : IDataManager<T> where T : class
{
    private readonly string basePath;
    private readonly Dictionary<string, T> cache;

    public JSONManager(string basePath)
    {
        this.basePath = basePath;
        this.cache = new Dictionary<string, T>();
    }

    public void SaveData(string key, T data)
    {
        try
        {
            if (data == null)
            {
                Debug.LogError($"Cannot save null data for key: {key}");
                return;
            }

            string fullPath = Path.Combine(Application.dataPath, "Resources", basePath, $"{key}.json");
            string directory = Path.GetDirectoryName(fullPath);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string jsonData = JsonUtility.ToJson(data, true);
            File.WriteAllText(fullPath, jsonData);

            cache[key] = data;
            AssetDatabase.Refresh();

            Debug.Log($"JSON saved successfully: {fullPath}"); 
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving JSON data: {e.Message}\n{e.StackTrace}");
        }
    }

    public T LoadData(string key)
    {
        if (cache.TryGetValue(key, out T cachedData))
            return cachedData;

        try
        {
            string resourcePath = Path.Combine(basePath, key);
            TextAsset jsonAsset = Resources.Load<TextAsset>(resourcePath);

            if (jsonAsset != null)
            {
                T data = JsonUtility.FromJson<T>(jsonAsset.text);
                cache[key] = data;
                return data;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading JSON data: {e.Message}");
        }

        return null;
    }

    public bool DeleteData(string key)
    {
        try
        {
            string fullPath = Path.Combine(Application.dataPath, "Resources", basePath, $"{key}.json");
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                cache.Remove(key);
                AssetDatabase.Refresh();
                return true;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error deleting JSON data: {e.Message}");
        }
        return false;
    }

    public void ClearAll()
    {
        try
        {
            string directory = Path.Combine(Application.dataPath, "Resources", basePath);
            if (Directory.Exists(directory))
            {
                var files = Directory.GetFiles(directory, "*.json");
                foreach (var file in files)
                {
                    File.Delete(file);
                }
            }
            cache.Clear();
            AssetDatabase.Refresh();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error clearing JSON data: {e.Message}");
        }
    }

    // 여러 데이터를 한 번에 저장
    public void SaveBulkData(string key, IEnumerable<T> dataList)
    {
        try
        {
            var wrapper = new ListWrapper<T> { Items = dataList.ToList() };
            string jsonData = JsonUtility.ToJson(wrapper, true);
            string fullPath = Path.Combine(Application.dataPath, "Resources", basePath, $"{key}.json");
            File.WriteAllText(fullPath, jsonData);
            AssetDatabase.Refresh();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving bulk JSON data: {e.Message}");
        }
    }

    // 여러 데이터를 한 번에 로드
    public IEnumerable<T> LoadBulkData(string key)
    {
        try
        {
            var wrapper = LoadData(key) as ListWrapper<T>;
            return wrapper?.Items ?? Enumerable.Empty<T>();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading bulk JSON data: {e.Message}");
            return Enumerable.Empty<T>();
        }
    }
}

[System.Serializable]
public class ListWrapper<T>
{
    public List<T> Items = new List<T>();
}