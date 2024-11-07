using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System;
using UnityEditor;

public class CSVManager<T> : IDataManager<T> where T : class, new()
{
    private readonly string basePath;
    private readonly Dictionary<string, T> cache;

    public CSVManager(string basePath)
    {
        this.basePath = basePath;
        this.cache = new Dictionary<string, T>();
    }

    public void SaveData(string key, T data)
    {
        string fullPath = Path.Combine(Application.dataPath, "Resources", basePath, $"{key}.csv");
        string directory = Path.GetDirectoryName(fullPath);

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        var csv = new StringBuilder();
        var properties = typeof(T).GetProperties();

        // 헤더 작성
        csv.AppendLine(string.Join(",", properties.Select(p => p.Name)));

        // 데이터 작성
        var values = properties.Select(p => p.GetValue(data)?.ToString() ?? "");
        csv.AppendLine(string.Join(",", values));

        File.WriteAllText(fullPath, csv.ToString());
        cache[key] = data;
    }

    public void SaveBulkData(string key, IEnumerable<T> dataList, bool overwrite = true)
    {
        try
        {
            if (!dataList.Any())
            {
                Debug.LogWarning($"No data to save for {key}");
                return;
            }

            string fullPath = Path.Combine(Application.dataPath, "Resources", basePath, $"{key}.csv");
            string directory = Path.GetDirectoryName(fullPath);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var csv = new StringBuilder();
            var properties = typeof(T).GetProperties(System.Reflection.BindingFlags.Public |
                                                   System.Reflection.BindingFlags.Instance);

            // 헤더 작성 (한 번만)
            var headerLine = string.Join(",", properties.Select(p => p.Name.ToLower()));
            csv.AppendLine(headerLine);

            // 데이터 작성
            int count = 0;
            foreach (var data in dataList)
            {
                if (data == null)
                {
                    Debug.LogWarning("Skipping null data entry");
                    continue;
                }

                var values = properties.Select(p =>
                {
                    var value = p.GetValue(data);
                    if (value == null) return "";

                    if (value is string strValue && strValue.Contains(","))
                        return $"\"{strValue}\"";

                    if (value is bool boolValue)
                        return boolValue ? "1" : "0";

                    return value.ToString();
                });

                var line = string.Join(",", values);
                csv.AppendLine(line);
                count++;
            }

            // 파일 저장
            File.WriteAllText(fullPath, csv.ToString());
            Debug.Log($"Successfully saved {count} entries to {fullPath}");

            AssetDatabase.Refresh();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving bulk CSV data: {e.Message}\n{e.StackTrace}");
        }
    }

    public T LoadData(string key)
    {
        if (cache.TryGetValue(key, out T cachedData))
            return cachedData;

        string fullPath = Path.Combine(Application.dataPath, "Resources", basePath, $"{key}.csv");
        if (!File.Exists(fullPath))
            return null;

        var lines = File.ReadAllLines(fullPath);
        if (lines.Length < 2) // 헤더 + 데이터 최소 2줄
            return null;

        var headers = lines[0].Split(',');
        var values = lines[1].Split(',');

        T data = new T();
        var properties = typeof(T).GetProperties();

        for (int i = 0; i < headers.Length; i++)
        {
            var prop = properties.FirstOrDefault(p => p.Name == headers[i]);
            if (prop != null && i < values.Length)
            {
                prop.SetValue(data, Convert.ChangeType(values[i], prop.PropertyType));
            }
        }

        cache[key] = data;
        return data;
    }

    public IEnumerable<T> LoadBulkData(string key)
    {
        string fullPath = Path.Combine(Application.dataPath, "Resources", basePath, $"{key}.csv");
        if (!File.Exists(fullPath))
            yield break;

        var lines = File.ReadAllLines(fullPath);
        if (lines.Length < 2)
            yield break;

        var headers = lines[0].Split(',');
        var properties = typeof(T).GetProperties();

        for (int i = 1; i < lines.Length; i++)
        {
            var values = lines[i].Split(',');
            T data = new T();

            for (int j = 0; j < headers.Length; j++)
            {
                var prop = properties.FirstOrDefault(p => p.Name == headers[j]);
                if (prop != null && j < values.Length)
                {
                    prop.SetValue(data, Convert.ChangeType(values[j], prop.PropertyType));
                }
            }

            yield return data;
        }
    }

    public bool DeleteData(string key)
    {
        string fullPath = Path.Combine(Application.dataPath, "Resources", basePath, $"{key}.csv");
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            cache.Remove(key);
            return true;
        }
        return false;
    }

    public void ClearAll()
    {
        cache.Clear();
        string directory = Path.Combine(Application.dataPath, "Resources", basePath);
        if (Directory.Exists(directory))
        {
            var files = Directory.GetFiles(directory, "*.csv");
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }
    }

    public void CreateDefaultFile(string fileName, string headers)
    {
        try
        {
            string fullPath = Path.Combine(Application.dataPath, "Resources", basePath, $"{fileName}.csv");
            string directory = Path.GetDirectoryName(fullPath);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // 이미 파일이 존재하면 덮어쓰지 않음
            if (!File.Exists(fullPath))
            {
                File.WriteAllText(fullPath, headers + "\n");  // 새 줄 문자 추가
                Debug.Log($"Created new CSV file: {fullPath}");
                AssetDatabase.Refresh();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creating default CSV file: {e.Message}\n{e.StackTrace}");
        }
    }
}