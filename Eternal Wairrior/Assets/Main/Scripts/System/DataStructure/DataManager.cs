using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;

public abstract class DataManager : MonoBehaviour
{
    protected bool isInitialized = false;
    public bool IsInitialized => isInitialized;

    protected virtual void Awake()
    {
        InitializeManagers();
    }

    protected abstract void InitializeManagers();

    public virtual void InitializeDefaultData()
    {
        try
        {
            Debug.Log($"Starting to initialize default data structure for {GetType().Name}...");

            if (!isInitialized)
            {
                InitializeManagers();
            }

            if (!isInitialized)
            {
                throw new System.Exception("Manager initialization failed");
            }

            CreateResourceFolders();
            CreateDefaultFiles();

            Debug.Log($"Successfully initialized default data structure for {GetType().Name}");
            isInitialized = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error initializing default data: {e.Message}\n{e.StackTrace}");
            isInitialized = false;
        }
    }

    protected abstract void CreateResourceFolders();
    protected abstract void CreateDefaultFiles();

    public virtual void SaveWithBackup()
    {
        string resourcePath = Path.Combine(Application.dataPath, "Resources");
        GetBackupManager()?.CreateBackup(resourcePath);
    }

    public virtual bool RestoreFromBackup(string timestamp)
    {
        return GetBackupManager()?.RestoreFromBackup(timestamp) ?? false;
    }

    public virtual void ClearAllData()
    {
        isInitialized = false;
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

    // 각 매니저에서 구현해야 할 추상 메서드들
    protected abstract BackupManager GetBackupManager();
}