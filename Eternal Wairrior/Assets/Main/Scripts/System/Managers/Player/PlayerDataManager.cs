using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class PlayerDataManager : DataManager
{
    private static PlayerDataManager instance;
    public static PlayerDataManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("PlayerDataManager");
                instance = go.AddComponent<PlayerDataManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private JSONManager<PlayerSaveData> saveManager;
    private BackupManager backupManager;
    private const string SAVE_PATH = "PlayerData";

    private PlayerStatData currentPlayerStatData;
    private InventoryData currentInventoryData;

    private LevelData currentLevelData = new LevelData { level = 1, exp = 0f };

    public PlayerStatData CurrentPlayerStatData => currentPlayerStatData;
    public InventoryData CurrentInventoryData => currentInventoryData;

    [System.Serializable]
    public class PlayerSaveData
    {
        public PlayerStatData stats;
        public InventoryData inventory;
        public LevelData levelData;
    }

    protected override void InitializeManagers()
    {
        if (!isInitialized)
        {
            saveManager = new JSONManager<PlayerSaveData>(SAVE_PATH);
            backupManager = new BackupManager();
            currentPlayerStatData = ScriptableObject.CreateInstance<PlayerStatData>();
            currentInventoryData = new InventoryData();
            isInitialized = true;
        }
    }

    public void LoadPlayerStatData(PlayerStatData data)
    {
        if (data != null)
        {
            currentPlayerStatData = data;
        }
    }

    public void SaveCurrentPlayerStatData()
    {
        currentPlayerStatData.SavePermanentStats();
    }

    public void LoadInventoryData(InventoryData data)
    {
        if (data != null)
        {
            currentInventoryData = data;
        }
    }

    public void SaveCurrentInventoryData(InventoryData data)
    {
        currentInventoryData = data;
    }

    public void SavePlayerData(string saveSlot, PlayerSaveData data)
    {
        if (!isInitialized) InitializeManagers();
        saveManager.SaveData(saveSlot, data);
        SaveWithBackup();
    }

    public PlayerSaveData LoadPlayerData(string saveSlot)
    {
        if (!isInitialized) InitializeManagers();
        var data = saveManager.LoadData(saveSlot);
        if (data != null)
        {
            LoadPlayerStatData(data.stats);
            LoadInventoryData(data.inventory);
        }
        return data;
    }

    protected override void CreateResourceFolders()
    {
        string savePath = Path.Combine(Application.persistentDataPath, SAVE_PATH);
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
            Debug.Log($"Created save directory at: {savePath}");
        }
    }

    protected override void CreateDefaultFiles()
    {
        var defaultStatData = Resources.Load<PlayerStatData>("DefaultPlayerStats");
        if (defaultStatData == null)
        {
            Debug.LogWarning("Default player stats not found in Resources folder");
            defaultStatData = ScriptableObject.CreateInstance<PlayerStatData>();
        }

        currentPlayerStatData = Instantiate(defaultStatData);
        var defaultSave = new PlayerSaveData
        {
            stats = currentPlayerStatData,
            inventory = new InventoryData(),
            levelData = new LevelData { level = 1, exp = 0f }
        };

        saveManager.SaveData("DefaultSave", defaultSave);
    }

    protected override BackupManager GetBackupManager()
    {
        return backupManager;
    }

    public override void ClearAllData()
    {
        if (saveManager != null)
        {
            saveManager.ClearAll();
        }
        base.ClearAllData();
    }

    private void EnsureDirectoryExists()
    {
        string savePath = Path.Combine(Application.persistentDataPath, SAVE_PATH);
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
            Debug.Log($"Created directory: {savePath}");
        }
    }

    public InventoryData LoadInventoryData()
    {
        return currentInventoryData;
    }

    public void SaveInventoryData(InventoryData data)
    {
        currentInventoryData = data;
        try
        {
            EnsureDirectoryExists();
            string json = JsonUtility.ToJson(data);
            string path = Path.Combine(Application.persistentDataPath, SAVE_PATH, "inventory.json");
            File.WriteAllText(path, json);
            Debug.Log($"Successfully saved inventory data to: {path}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving inventory data: {e.Message}");
        }
    }

    public bool HasSaveData(string saveSlot)
    {
        if (!isInitialized) InitializeManagers();

        string savePath = Path.Combine(Application.persistentDataPath, SAVE_PATH, $"{saveSlot}.json");
        return File.Exists(savePath);
    }
}

[System.Serializable]
public class LevelData
{
    public int level = 1;
    public float exp = 0f;
}
