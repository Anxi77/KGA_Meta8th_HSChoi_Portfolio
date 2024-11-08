using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class PlayerDataManager : DataManager<PlayerDataManager>, IInitializable
{

    private const string SAVE_PATH = "Asset/Resources/PlayerData";
    private const string DEFAULT_SAVE_SLOT = "DefaultSave";

    private PlayerStatData currentPlayerStatData;
    private InventoryData currentInventoryData;
    private LevelData currentLevelData = new LevelData { level = 1, exp = 0f };

    private JSONManager<PlayerSaveData> saveManager;
    private BackupManager backupManager;

    public new bool IsInitialized { get; private set; }
    public PlayerStatData CurrentPlayerStatData => currentPlayerStatData;
    public InventoryData CurrentInventoryData => currentInventoryData;

    [System.Serializable]
    public class PlayerSaveData
    {
        public PlayerStatData stats;
        public InventoryData inventory;
        public LevelData levelData;
    }

    protected override void Awake()
    {
        base.Awake();
    }

    public void Initialize()
    {
        try
        {
            Debug.Log("Initializing PlayerDataManager...");
            InitializeDefaultData();
            IsInitialized = true;
            Debug.Log("PlayerDataManager initialized successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error initializing PlayerDataManager: {e.Message}");
            IsInitialized = false;
        }
    }

    protected override void InitializeManagers()
    {
        saveManager = new JSONManager<PlayerSaveData>(SAVE_PATH);
        backupManager = new BackupManager();
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
            Debug.LogWarning("Default player stats not found, creating new...");
            defaultStatData = ScriptableObject.CreateInstance<PlayerStatData>();
        }
        currentPlayerStatData = Instantiate(defaultStatData);
        currentInventoryData = new InventoryData();
    }

    protected override BackupManager GetBackupManager()
    {
        return backupManager;
    }

    public override void SaveWithBackup()
    {
        if (backupManager != null)
        {
            backupManager.CreateBackup(SAVE_PATH);
        }
    }

    public override void ClearAllData()
    {
        if (saveManager != null)
        {
            saveManager.ClearAll();
        }
        currentPlayerStatData = ScriptableObject.CreateInstance<PlayerStatData>();
        currentInventoryData = new InventoryData();
        currentLevelData = new LevelData { level = 1, exp = 0f };
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

    public void SavePlayerData(string saveSlot, PlayerSaveData data)
    {
        if (!IsInitialized) Initialize();
        saveManager.SaveData(saveSlot, data);
        SaveWithBackup();
    }

    public PlayerSaveData LoadPlayerData(string saveSlot)
    {
        if (!IsInitialized) Initialize();
        var data = saveManager.LoadData(saveSlot);
        if (data != null)
        {
            LoadPlayerStatData(data.stats);
            LoadInventoryData(data.inventory);
        }
        return data;
    }

    public void LoadInventoryData(InventoryData data)
    {
        if (data != null)
        {
            currentInventoryData = data;
        }
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

    private void EnsureDirectoryExists()
    {
        string savePath = Path.Combine(Application.persistentDataPath, SAVE_PATH);
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
            Debug.Log($"Created directory: {savePath}");
        }
    }

    public bool HasSaveData(string saveSlot)
    {
        if (!IsInitialized) Initialize();
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
