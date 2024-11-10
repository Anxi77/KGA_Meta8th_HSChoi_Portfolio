using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ItemDataManager : DataManager<ItemDataManager>, IInitializable
{
    #region Constants
    private const string RESOURCE_ROOT = "Assets/Resources";
    private const string ITEM_DB_PATH = "Items/Database";
    private const string ITEM_PREFAB_PATH = "Items/Prefabs";
    private const string ITEM_ICON_PATH = "Items/Icons";
    private const string ITEM_STAT_RANGES_PATH = "Items/StatRanges";
    private const string DROP_TABLES_PATH = "Items/DropTables";
    #endregion

    #region Fields
    private Dictionary<string, ItemData> itemDatabase = new();
    private Dictionary<EnemyType, DropTableData> dropTables = new();
    private ItemGenerator itemGenerator;
    private BackupManager backupManager = new();
    #endregion

    #region Serialization Classes
    [System.Serializable]
    public class SerializableItemDatabase
    {
        public Dictionary<string, ItemData> items = new();
    }

    [System.Serializable]
    public class SerializableDropTables
    {
        public List<SerializableDropTableEntry> tables = new();
    }

    [System.Serializable]
    public class SerializableItemList
    {
        public List<ItemData> items = new();
    }

    [System.Serializable]
    public class SerializableDropTableEntry
    {
        public EnemyType enemyType;
        public float guaranteedDropRate;
        public int maxDrops;
        public List<DropTableEntry> dropEntries = new();
    }
    #endregion

    #region Properties
    public new bool IsInitialized
    {
        get => base.isInitialized;
        private set => base.isInitialized = value;
    }
    #endregion

    #region Initialization
    public void Initialize()
    {
        if (!IsInitialized)
        {
            try
            {
                Debug.Log("Initializing ItemDataManager...");
                CreateResourceFolders();
                LoadAllData();
                InitializeItemGenerator();
                IsInitialized = true;
                Debug.Log("ItemDataManager initialized successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error initializing ItemDataManager: {e.Message}");
                IsInitialized = false;
            }
        }
    }

    private void InitializeItemGenerator()
    {
        itemGenerator = new ItemGenerator(itemDatabase);
    }

    public void SaveDropTables()
    {
        try
        {
            if (dropTables == null || !dropTables.Any())
            {
                CreateDefaultDropTables();
                return;
            }

            var wrapper = new DropTablesWrapper
            {
                dropTables = dropTables.Values.ToList()
            };

            string json = JsonUtility.ToJson(wrapper, true);
            string path = Path.Combine(RESOURCE_ROOT, DROP_TABLES_PATH, "DropTables.json");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, json);

            Debug.Log($"Drop tables saved successfully to: {path}");
            Debug.Log($"Saved JSON content: {json}");

#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving drop tables: {e.Message}\n{e.StackTrace}");
        }
    }

    private void SaveDropTables(Dictionary<EnemyType, DropTableData> tables)
    {
        dropTables = new Dictionary<EnemyType, DropTableData>(tables);
        SaveDropTables();
    }
    #endregion

    #region Resource Management
    protected override void CreateResourceFolders()
    {
        try
        {
            string[] paths = new[]
            {
                ITEM_DB_PATH,
                ITEM_PREFAB_PATH,
                ITEM_ICON_PATH,
                ITEM_STAT_RANGES_PATH,
                DROP_TABLES_PATH
            };
            foreach (var path in paths)
            {
                string fullPath = Path.Combine(RESOURCE_ROOT, path);
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                    Debug.Log($"Created directory: {fullPath}");
                }
            }

#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif

        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creating resource folders: {e.Message}");
        }
    }

    private void SaveResource(Object resource, string targetPath, string guid = null)
    {
        if (resource == null) return;

#if UNITY_EDITOR
        try
        {
            string sourcePath = AssetDatabase.GetAssetPath(resource);
            if (string.IsNullOrEmpty(sourcePath)) return;

            // 대상 디렉토리가 없으면 생성
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath));

            // 파일이 이미 존재하면 삭제 후 복사
            if (File.Exists(targetPath))
            {
                AssetDatabase.DeleteAsset(targetPath);
            }
            if (AssetDatabase.CopyAsset(sourcePath, targetPath))
            {
                Debug.Log($"Successfully copied resource to: {targetPath}");
                if (!string.IsNullOrEmpty(guid))
                {
                    string newGuid = AssetDatabase.AssetPathToGUID(targetPath);
                    Debug.Log($"New GUID: {newGuid}");
                }
            }
            else
            {
                Debug.LogError($"Failed to copy resource from {sourcePath} to {targetPath}");
            }
            AssetDatabase.Refresh();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving resource: {e.Message}");
        }
#endif
    }

    private void SaveItemResources(ItemData itemData)
    {
        if (itemData == null) return;

#if UNITY_EDITOR
        try
        {
            // 아이콘만 저장
            if (itemData.icon != null)
            {
                string sourceIconPath = AssetDatabase.GetAssetPath(itemData.icon);
                if (!string.IsNullOrEmpty(sourceIconPath))
                {
                    string targetIconPath = Path.Combine(RESOURCE_ROOT, ITEM_ICON_PATH, $"{itemData.id}_Icon.png");
                    Directory.CreateDirectory(Path.GetDirectoryName(targetIconPath));

                    if (AssetDatabase.CopyAsset(sourceIconPath, targetIconPath))
                    {
                        itemData.metadata.iconPath = GetResourcePath(targetIconPath);
                        itemData.metadata.iconGuid = AssetDatabase.AssetPathToGUID(targetIconPath);
                        itemData.iconPath = itemData.metadata.iconPath;
                        itemData.iconGuid = itemData.metadata.iconGuid;
                        Debug.Log($"Icon saved to: {targetIconPath}");
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving item resources for {itemData.id}: {e.Message}");
        }
#endif
    }

    private string GetResourcePath(string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath)) return string.Empty;

        // Resources/ 이후의 경로만 추출
        int resourcesIndex = fullPath.IndexOf("Resources/");
        if (resourcesIndex == -1) return fullPath;

        string relativePath = fullPath.Substring(resourcesIndex + 10);
        return relativePath;
    }

    private void SaveJson<T>(T data, string fileName)
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);
            string path = Path.Combine(RESOURCE_ROOT, fileName);

            // 디렉토리가 없으면 생성
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            // 파일 덮어쓰기
            File.WriteAllText(path, json);

#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif

            Debug.Log($"Successfully saved JSON to: {path}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving JSON: {e.Message}");
        }
    }
    #endregion

    #region Data Management
    private void LoadAllData()
    {
        try
        {
            LoadItemDatabase();
            LoadDropTables();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading data: {e.Message}");
        }
    }

    public void SaveItemData(ItemData itemData)
    {
        if (itemData == null) return;

        try
        {
            // 리소스 저장
            SaveItemResources(itemData);
#if UNITY_EDITOR
            // 에디터 데이터 컨테이너에도 저장
            var editorData = AssetDatabase.LoadAssetAtPath<ItemEditorDataContainer>("Assets/Resources/ItemEditorData.asset");
            if (editorData != null)
            {
                editorData.SaveItemData(itemData);
            }
#endif
            // 데이터베이스에 저장
            itemDatabase[itemData.id] = itemData.Clone();

            // JSON 저장
            SaveToJson();

            Debug.Log($"Successfully saved item: {itemData.id}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving item data: {e.Message}");
        }
    }

    private void SaveToJson()
    {
        var serializableData = new SerializableItemList
        {
            items = itemDatabase.Values.ToList()
        };

        string json = JsonUtility.ToJson(serializableData, true);
        string path = Path.Combine(RESOURCE_ROOT, ITEM_DB_PATH, "ItemDatabase.json");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, json);

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }

    private void LoadItemDatabase()
    {
        try
        {
            var jsonAsset = Resources.Load<TextAsset>($"{ITEM_DB_PATH}/ItemDatabase");
            if (jsonAsset != null)
            {
                Debug.Log($"Loading item database content: {jsonAsset.text}"); // 디버그 추가
                var serializableData = JsonUtility.FromJson<SerializableItemList>(jsonAsset.text);
                if (serializableData?.items != null)
                {
                    itemDatabase = serializableData.items.ToDictionary(item => item.id);
                    foreach (var item in itemDatabase.Values)
                    {
                        Debug.Log($"Loaded item: {item.id}, Name: {item.name}, Type: {item.type}"); // 디버그 추가
                    }
                    LoadItemResources();
                }
                else
                {
                    Debug.LogError("Failed to deserialize item data or items list is null");
                }
            }
            else
            {
                Debug.LogError($"ItemDatabase.json not found at path: Resources/{ITEM_DB_PATH}/ItemDatabase");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading item database: {e.Message}\n{e.StackTrace}");
        }
    }

    private void CreateDefaultData()
    {
        Debug.Log("Creating default item data...");
        itemDatabase = new Dictionary<string, ItemData>();
        // 기본 아이템 추가
        var defaultItem = new ItemData
        {
            metadata = new ItemMetadata
            {
                ID = "default_sword",
                Name = "Default Sword",
                Description = "A basic sword",
                Type = ItemType.Weapon,
                Rarity = ItemRarity.Common
            }
        };
        itemDatabase.Add(defaultItem.id, defaultItem);
    }

    private void LoadItemResources()
    {
        foreach (var item in itemDatabase.Values)
        {
            try
            {
                if (!string.IsNullOrEmpty(item.metadata.iconPath))
                {
                    // 경로 처리를 더 명확하게
                    string iconPath = item.metadata.iconPath;

                    // Resources/ 이후의 경로만 사용
                    int resourcesIndex = iconPath.IndexOf("Resources/");
                    if (resourcesIndex != -1)
                    {
                        iconPath = iconPath.Substring(resourcesIndex + "Resources/".Length);
                    }

                    // 확장자 제거
                    iconPath = Path.ChangeExtension(iconPath, null);

                    Debug.Log($"Attempting to load icon for item {item.id} from path: {iconPath}");

                    var icon = Resources.Load<Sprite>(iconPath);
                    if (icon != null)
                    {
                        item.metadata.Icon = icon;
                        item.icon = icon; // ItemData의 icon 필드도 설정
                        Debug.Log($"Successfully loaded icon for item {item.id}");
                    }
                    else
                    {
                        // 대체 경로 시도
                        string alternativePath = $"Items/Icons/{item.id}_Icon";
                        Debug.Log($"Trying alternative path for item {item.id}: {alternativePath}");
                        icon = Resources.Load<Sprite>(alternativePath);

                        if (icon != null)
                        {
                            item.metadata.Icon = icon;
                            item.icon = icon; // ItemData의 icon 필드도 설정
                            Debug.Log($"Successfully loaded icon from alternative path for item {item.id}");
                        }
                        else
                        {
                            Debug.LogWarning($"Failed to load icon for item {item.id}. Paths tried:\n1. {iconPath}\n2. {alternativePath}");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"No icon path specified for item {item.id}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading resources for item {item.id}: {e.Message}\n{e.StackTrace}");
            }
        }
    }

    private void LoadDropTables()
    {
        try
        {
            var jsonAsset = Resources.Load<TextAsset>($"{DROP_TABLES_PATH}/DropTables");
            if (jsonAsset != null)
            {
                var wrapper = JsonUtility.FromJson<DropTablesWrapper>(jsonAsset.text);
                dropTables = wrapper.dropTables.ToDictionary(dt => dt.enemyType);
            }
            else
            {
                Debug.Log("No drop tables found. Creating default tables...");
                CreateDefaultDropTables();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading drop tables: {e.Message}");
            CreateDefaultDropTables();
        }
    }

    public void AddToDropTable(EnemyType enemyType, string itemId, float dropRate, ItemRarity rarity, int minAmount = 1, int maxAmount = 1)
    {
        if (!dropTables.TryGetValue(enemyType, out var dropTable))
        {
            dropTable = new DropTableData
            {
                enemyType = enemyType,
                dropEntries = new List<DropTableEntry>()
            };
            dropTables[enemyType] = dropTable;
        }

        // 이미 존재하는 엔트리 제거
        dropTable.dropEntries.RemoveAll(entry => entry.itemId == itemId);

        // 새 엔트리 추가
        dropTable.dropEntries.Add(new DropTableEntry
        {
            itemId = itemId,
            dropRate = dropRate,
            rarity = rarity,
            minAmount = minAmount,
            maxAmount = maxAmount
        });
        SaveDropTables();
    }

    public void RemoveFromDropTable(EnemyType enemyType, string itemId)
    {
        if (dropTables.TryGetValue(enemyType, out var dropTable))
        {
            dropTable.dropEntries.RemoveAll(entry => entry.itemId == itemId);
            SaveDropTables();
        }
    }

    public List<DropTableEntry> GetDropTableEntriesForItem(string itemId)
    {
        var entries = new List<DropTableEntry>();
        foreach (var dropTable in dropTables.Values)
        {
            entries.AddRange(dropTable.dropEntries.Where(entry => entry.itemId == itemId));
        }
        return entries;
    }

    public Dictionary<EnemyType, DropTableData> GetDropTables()
    {
        return new Dictionary<EnemyType, DropTableData>(dropTables);
    }

    private void CreateDefaultDropTables()
    {
        dropTables = new Dictionary<EnemyType, DropTableData>
        {
            {
                EnemyType.Normal,
                new DropTableData
                {
                    enemyType = EnemyType.Normal,
                    guaranteedDropRate = 0.1f,
                    maxDrops = 2,
                    dropEntries = new List<DropTableEntry>()
                }
            },
            {
                EnemyType.Elite,
                new DropTableData
                {
                    enemyType = EnemyType.Elite,
                    guaranteedDropRate = 0.3f,
                    maxDrops = 3,
                    dropEntries = new List<DropTableEntry>()
                }
            },
            {
                EnemyType.Boss,
                new DropTableData
                {
                    enemyType = EnemyType.Boss,
                    guaranteedDropRate = 1f,
                    maxDrops = 5,
                    dropEntries = new List<DropTableEntry>()
                }
            }
        };
        SaveDropTables();
    }
    #endregion

    #region DataManager Abstract Methods
    protected override void InitializeManagers()
    {
        if (!isInitialized)
        {
            try
            {
                Debug.Log("Initializing ItemDataManager managers...");
                CreateResourceFolders();
                LoadAllData();
                InitializeItemGenerator();
                isInitialized = true;
                Debug.Log("ItemDataManager managers initialized successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to initialize ItemDataManager managers: {e.Message}");
                isInitialized = false;
                throw;
            }
        }
    }

    protected override void CreateDefaultFiles()
    {
        try
        {
            // 기본 아이템 데이터 생성
            var defaultItemData = new ItemData
            {
                metadata = new ItemMetadata
                {
                    ID = "sword_01",
                    Name = "Basic Sword",
                    Description = "A simple sword",
                    Type = ItemType.Weapon,
                    Rarity = ItemRarity.Common,
                    MaxStack = 1,
                    DropRate = 0.1f
                },
                statRanges = new ItemStatRangeData
                {
                    itemId = "sword_01",
                    itemType = ItemType.Weapon,
                    minStatCount = 1,
                    maxStatCount = 3,
                    possibleStats = new List<ItemStatRange>
                    {
                        new ItemStatRange
                        {
                            statType = StatType.Damage,
                            minValue = 5,
                            maxValue = 10,
                            weight = 1f,
                            minRarity = ItemRarity.Common,
                            increaseType = IncreaseType.Add,
                            sourceType = SourceType.Equipment_Weapon
                        }
                    }
                }
            };

            // 기본 드롭테이블 생성
            var defaultDropTable = new Dictionary<EnemyType, DropTableData>
            {
                {
                    EnemyType.Normal,
                    new DropTableData
                    {
                        enemyType = EnemyType.Normal,
                        guaranteedDropRate = 0.1f,
                        maxDrops = 2,
                        dropEntries = new List<DropTableEntry>()
                    }
                }
            };

            // 데이터 저장
            SaveItemData(defaultItemData);
            SaveDropTables(defaultDropTable);
            Debug.Log("Created default files successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creating default files: {e.Message}");
            throw;
        }
    }

    protected override BackupManager GetBackupManager()
    {
        if (backupManager == null)
        {
            backupManager = new BackupManager();
        }
        return backupManager;
    }
    #endregion

    #region Data Access
    public List<ItemData> GetAllItemData()
    {
        return new List<ItemData>(itemDatabase.Values);
    }

    public ItemData GetItemData(string itemId)
    {
        if (itemDatabase.TryGetValue(itemId, out var itemData))
        {
            return itemData.Clone();
        }
        Debug.LogWarning($"Item not found: {itemId}");
        return null;
    }

    public bool HasItem(string itemId)
    {
        return itemDatabase.ContainsKey(itemId);
    }

    public Dictionary<string, ItemData> GetItemDatabase()
    {
        return new Dictionary<string, ItemData>(itemDatabase);
    }
    #endregion
}

[System.Serializable]
public class DropTablesWrapper
{
    public List<DropTableData> dropTables = new();
}
