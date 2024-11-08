using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;

public abstract class DataManager<T> : SingletonManager<T> where T : MonoBehaviour
{
    protected bool isInitialized = false;
    public bool IsInitialized => isInitialized;

    protected override void Awake()
    {
        base.Awake();
    }

    public virtual void InitializeDefaultData()
    {
        try
        {
            Debug.Log($"Starting to initialize default data structure for {GetType().Name}...");

            InitializeManagers();

            CreateResourceFolders();

            CreateDefaultFiles();

            Debug.Log($"Successfully initialized default data structure for {GetType().Name}");
            isInitialized = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error initializing default data for {GetType().Name}: {e.Message}\n{e.StackTrace}");
            isInitialized = false;
            throw;
        }
    }

    protected abstract void InitializeManagers();
    protected abstract void CreateResourceFolders();
    protected abstract void CreateDefaultFiles();
    protected abstract BackupManager GetBackupManager();

    public virtual void SaveWithBackup()
    {
        try
        {
            var backupManager = GetBackupManager();
            if (backupManager != null)
            {
                string savePath = GetSavePath();
                if (!System.IO.Directory.Exists(savePath))
                {
                    System.IO.Directory.CreateDirectory(savePath);
                }
                backupManager.CreateBackup(savePath);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in SaveWithBackup: {e.Message}");
        }
    }

    protected virtual string GetSavePath()
    {
        return System.IO.Path.Combine(Application.persistentDataPath, "Data");
    }

    public virtual bool RestoreFromBackup(string timestamp)
    {
        return GetBackupManager()?.RestoreFromBackup(timestamp) ?? false;
    }

    public virtual void ClearAllData()
    {
        isInitialized = false;
    }
}