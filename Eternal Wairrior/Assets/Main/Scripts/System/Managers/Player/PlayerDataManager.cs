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

    protected override void Awake()
    {
        base.Awake();
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
        public int level;
        public float exp;
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
            level = 1,
            exp = 0
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

    public void SaveLevelData(int level, float exp)
    {
        currentLevelData.level = level;
        currentLevelData.exp = exp;

        try
        {
            EnsureDirectoryExists();
            string json = JsonUtility.ToJson(currentLevelData);
            string path = Path.Combine(Application.persistentDataPath, SAVE_PATH, "level.json");
            File.WriteAllText(path, json);
            Debug.Log($"Successfully saved level data to: {path}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving level data: {e.Message}");
        }
    }

    public (int level, float exp) LoadLevelData()
    {
        try
        {
            string path = Path.Combine(Application.persistentDataPath, SAVE_PATH, "level.json");
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                currentLevelData = JsonUtility.FromJson<LevelData>(json);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading level data: {e.Message}");
        }
        return (currentLevelData.level, currentLevelData.exp);
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
}

[System.Serializable]
public class LevelData
{
    public int level = 1;
    public float exp = 0f;
}
