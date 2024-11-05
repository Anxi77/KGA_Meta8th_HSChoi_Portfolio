using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;

public abstract class DataManager : MonoBehaviour
{
    protected ResourceManager<GameObject> prefabManager;
    protected ResourceManager<Sprite> iconManager;
    protected CSVManager<SkillStatData> statManager;
    protected JSONManager<SkillData> jsonManager;
    protected BackupManager backupManager;
    protected DataValidator dataValidator;

    protected const string RESOURCE_PATH = "SkillData";
    protected const string PREFAB_PATH = "SkillData/Prefabs";
    protected const string ICON_PATH = "SkillData/Icons";
    protected const string STAT_PATH = "SkillData/Stats";
    protected const string JSON_PATH = "SkillData/Json";

    protected bool isInitialized = false;
    public bool IsInitialized => isInitialized;

    protected virtual void Awake()
    {
        InitializeManagers();
    }

    protected virtual void InitializeManagers()
    {
        CreateResourceFolders();

        prefabManager = new ResourceManager<GameObject>(PREFAB_PATH);
        iconManager = new ResourceManager<Sprite>(ICON_PATH);
        statManager = new CSVManager<SkillStatData>(STAT_PATH);
        jsonManager = new JSONManager<SkillData>(JSON_PATH);
        backupManager = new BackupManager();
        dataValidator = new DataValidator();

        isInitialized = true;
    }

    private void CreateResourceFolders()
    {
        string resourcesPath = Path.Combine(Application.dataPath, "Resources");

        string[] folders = new string[]
        {
            Path.Combine(resourcesPath, RESOURCE_PATH),
            Path.Combine(resourcesPath, PREFAB_PATH),
            Path.Combine(resourcesPath, ICON_PATH),
            Path.Combine(resourcesPath, STAT_PATH),
            Path.Combine(resourcesPath, JSON_PATH)
        };

        foreach (string folder in folders)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
                Debug.Log($"Created directory: {folder}");
            }
        }

        AssetDatabase.Refresh();
    }

    public virtual void SaveWithBackup()
    {
        backupManager.CreateBackup(Path.Combine(Application.dataPath, "Resources"));
    }

    public virtual bool RestoreFromBackup(string timestamp)
    {
        return backupManager.RestoreFromBackup(timestamp);
    }

    public virtual void ClearAllData()
    {
        prefabManager.ClearAll();
        iconManager.ClearAll();
        statManager.ClearAll();
        jsonManager.ClearAll();
    }

    protected virtual void OnDestroy()
    {
        ClearAllData();
    }

    protected string GetResourcePath(string fullPath)
    {
        const string resourcesFolder = "Resources/";
        int resourceIndex = fullPath.IndexOf(resourcesFolder);
        if (resourceIndex != -1)
        {
            string resourcePath = fullPath.Substring(resourceIndex + resourcesFolder.Length);
            return Path.ChangeExtension(resourcePath, null);
        }
        return null;
    }

    protected bool TryExecute(System.Action action, string operationName)
    {
        try
        {
            action?.Invoke();
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error during {operationName}: {e.Message}\n{e.StackTrace}");
            return false;
        }
    }

    protected virtual async Task<bool> TryExecuteAsync(System.Func<Task> action, string operationName)
    {
        try
        {
            await action?.Invoke();
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error during async {operationName}: {e.Message}\n{e.StackTrace}");
            return false;
        }
    }
}