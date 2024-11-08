using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ItemDataManager : DataManager<ItemDataManager>, IInitializable
{
    private const string RESOURCE_PATH = "Items";
    private const string PREFAB_PATH = "Items/Prefabs";
    private const string ICON_PATH = "Items/Icons";
    private const string DATA_PATH = "Items/Data";
    private const string DROPTABLE_PATH = "Items/DropTables";

    private Dictionary<string, ItemData> itemDatabase = new Dictionary<string, ItemData>();
    private Dictionary<EnemyType, DropTableData> dropTables = new Dictionary<EnemyType, DropTableData>();

    private ResourceManager<GameObject> prefabManager;
    private ResourceManager<Sprite> iconManager;
    private JSONManager<ItemData> jsonManager;
    private BackupManager backupManager;

    public new bool IsInitialized { get; private set; }

    protected override void Awake()
    {
        base.Awake();
    }

    public void Initialize()
    {
        try
        {
            Debug.Log("Initializing ItemDataManager...");
            InitializeDefaultData();
            LoadAllItemData();
            IsInitialized = true;
            Debug.Log("ItemDataManager initialized successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error initializing ItemDataManager: {e.Message}");
            IsInitialized = false;
        }
    }

    protected override void InitializeManagers()
    {
        prefabManager = new ResourceManager<GameObject>(PREFAB_PATH);
        iconManager = new ResourceManager<Sprite>(ICON_PATH);
        jsonManager = new JSONManager<ItemData>(DATA_PATH);
        backupManager = new BackupManager();
    }

    protected override void CreateResourceFolders()
    {
        string resourcesPath = Path.Combine(Application.dataPath, "Resources");
        string[] paths = new string[]
        {
            RESOURCE_PATH,
            PREFAB_PATH,
            ICON_PATH,
            DATA_PATH,
            DROPTABLE_PATH
        };

        foreach (string path in paths)
        {
            string fullPath = Path.Combine(resourcesPath, path);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
        }
    }

    protected override void CreateDefaultFiles()
    {
        // 기본 아이템 데이터 생성 로직이 필요한 경우 여기에 구현
    }

    protected override BackupManager GetBackupManager()
    {
        return backupManager;
    }

    private void LoadAllItemData()
    {
        var itemDataFiles = Resources.LoadAll<TextAsset>(DATA_PATH);
        foreach (var dataFile in itemDataFiles)
        {
            try
            {
                var itemData = JsonUtility.FromJson<ItemData>(dataFile.text);
                if (itemData != null)
                {
                    LoadItemResources(itemData);
                    itemDatabase[itemData.id] = itemData;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading item data from {dataFile.name}: {e.Message}");
            }
        }

        LoadDropTables();
    }

    private void LoadItemResources(ItemData itemData)
    {
        itemData.icon = Resources.Load<Sprite>($"{ICON_PATH}/{itemData.id}_Icon");
        itemData.prefab = Resources.Load<GameObject>($"{PREFAB_PATH}/{itemData.id}_Prefab");
    }

    private void LoadDropTables()
    {
        var dropTableFiles = Resources.LoadAll<TextAsset>(DROPTABLE_PATH);
        foreach (var tableFile in dropTableFiles)
        {
            try
            {
                var dropTable = JsonUtility.FromJson<DropTableData>(tableFile.text);
                if (dropTable != null && dropTable.enemyType != EnemyType.None)
                {
                    dropTables[dropTable.enemyType] = dropTable;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading drop table from {tableFile.name}: {e.Message}");
            }
        }
    }

    #region Public Access Methods
    public ItemData GetItemData(string itemId)
    {
        if (itemDatabase.TryGetValue(itemId, out var itemData))
        {
            return itemData.Clone();
        }
        return null;
    }

    public List<ItemData> GetAllItemData()
    {
        return itemDatabase.Values.Select(item => item.Clone()).ToList();
    }

    public DropTableData GetDropTable(EnemyType enemyType)
    {
        dropTables.TryGetValue(enemyType, out var dropTable);
        return dropTable;
    }

    public Dictionary<EnemyType, DropTableData> GetDropTables()
    {
        return new Dictionary<EnemyType, DropTableData>(dropTables);
    }
    #endregion

    public override void ClearAllData()
    {
        itemDatabase.Clear();
        dropTables.Clear();
        base.ClearAllData();
    }
}
