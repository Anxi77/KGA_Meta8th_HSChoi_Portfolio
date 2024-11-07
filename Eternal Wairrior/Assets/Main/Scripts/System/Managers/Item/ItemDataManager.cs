using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ItemDataManager : DataManager
{
    private static ItemDataManager instance;
    public static ItemDataManager Instance => instance;

    private JSONManager<Dictionary<string, ItemData>> itemDatabase;
    private CSVManager<ItemStatData> statManager;
    private ResourceManager<GameObject> prefabManager;
    private ResourceManager<Sprite> iconManager;
    private BackupManager backupManager;
    private Dictionary<string, ItemData> availableItems = new();

    private const string ITEM_DB_PATH = "Items/Database";
    private const string ITEM_STATS_PATH = "Items/Stats";
    private const string ITEM_PREFAB_PATH = "Items/Prefabs";
    private const string ITEM_ICON_PATH = "Items/Icons";

    private Dictionary<EnemyType, DropTableData> enemyDropTables = new();

    protected override void InitializeManagers()
    {
        if (!isInitialized)
        {
            itemDatabase = new JSONManager<Dictionary<string, ItemData>>(ITEM_DB_PATH);
            statManager = new CSVManager<ItemStatData>(ITEM_STATS_PATH);
            prefabManager = new ResourceManager<GameObject>(ITEM_PREFAB_PATH);
            iconManager = new ResourceManager<Sprite>(ITEM_ICON_PATH);
            backupManager = new BackupManager();
            LoadDropTables();
            LoadItemStats();
            isInitialized = true;
        }
    }

    public List<ItemData> GetAllItemData()
    {
        return new List<ItemData>(availableItems.Values);
    }

    private void LoadItemStats()
    {
        var statsFile = Resources.Load<TextAsset>($"{ITEM_STATS_PATH}/ItemStats");
        if (statsFile != null)
        {
            var lines = statsFile.text.Split('\n');
            var headers = lines[0].Trim().Split(',');

            for (int i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Trim().Split(',');
                if (values.Length != headers.Length) continue;

                var itemData = new ItemData();
                for (int j = 0; j < headers.Length; j++)
                {
                    SetItemValue(itemData, headers[j].ToLower(), values[j]);
                }

                // 아이콘과 프리팹 로드
                itemData.icon = iconManager.LoadData($"{itemData.id}_Icon");
                itemData.prefab = prefabManager.LoadData($"{itemData.id}_Prefab");

                var database = itemDatabase.LoadData("ItemDatabase") ?? new Dictionary<string, ItemData>();
                database[itemData.id] = itemData;
                itemDatabase.SaveData("ItemDatabase", database);
            }
        }
    }

    private void SetItemValue(ItemData itemData, string fieldName, string value)
    {
        try
        {
            switch (fieldName.ToLower())
            {
                // 기본 정보
                case "id": itemData.id = value; break;
                case "name": itemData.name = value; break;
                case "description": itemData.description = value; break;
                case "type": itemData.type = ParseEnum<ItemType>(value); break;
                case "rarity": itemData.rarity = ParseEnum<ItemRarity>(value); break;
                case "maxstack": itemData.maxStack = int.Parse(value); break;
                case "droprate": itemData.dropRate = float.Parse(value); break;

                // 기본 스탯
                case "damage": itemData.stats.Add(new StatContainer(StatType.Damage, SourceType.Equipment_Weapon, IncreaseType.Add, float.Parse(value))); break;
                case "defense": itemData.stats.Add(new StatContainer(StatType.Defense, SourceType.Equipment_Armor, IncreaseType.Add, float.Parse(value))); break;
                case "hp": itemData.stats.Add(new StatContainer(StatType.MaxHp, SourceType.Equipment_Armor, IncreaseType.Add, float.Parse(value))); break;
                case "movespeed": itemData.stats.Add(new StatContainer(StatType.MoveSpeed, SourceType.Equipment_Accessory, IncreaseType.Mul, float.Parse(value))); break;
                case "attackspeed": itemData.stats.Add(new StatContainer(StatType.AttackSpeed, SourceType.Equipment_Weapon, IncreaseType.Mul, float.Parse(value))); break;
                case "attackrange": itemData.stats.Add(new StatContainer(StatType.AttackRange, SourceType.Equipment_Weapon, IncreaseType.Mul, float.Parse(value))); break;
                case "hpregen": itemData.stats.Add(new StatContainer(StatType.HpRegenRate, SourceType.Equipment_Accessory, IncreaseType.Add, float.Parse(value))); break;

                // 특수 스탯
                case "criticalchance": itemData.stats.Add(new StatContainer(StatType.CriticalChance, SourceType.Equipment_Weapon, IncreaseType.Add, float.Parse(value))); break;
                case "criticaldamage": itemData.stats.Add(new StatContainer(StatType.CriticalDamage, SourceType.Equipment_Weapon, IncreaseType.Add, float.Parse(value))); break;
                case "lifesteal": itemData.stats.Add(new StatContainer(StatType.LifeSteal, SourceType.Equipment_Weapon, IncreaseType.Add, float.Parse(value))); break;
                case "reflectdamage": itemData.stats.Add(new StatContainer(StatType.ReflectDamage, SourceType.Equipment_Armor, IncreaseType.Add, float.Parse(value))); break;
                case "dodgechance": itemData.stats.Add(new StatContainer(StatType.DodgeChance, SourceType.Equipment_Accessory, IncreaseType.Add, float.Parse(value))); break;

                // 저항 스탯
                case "fireresistance": itemData.stats.Add(new StatContainer(StatType.FireResistance, SourceType.Equipment_Armor, IncreaseType.Add, float.Parse(value))); break;
                case "iceresistance": itemData.stats.Add(new StatContainer(StatType.IceResistance, SourceType.Equipment_Armor, IncreaseType.Add, float.Parse(value))); break;
                case "lightningresistance": itemData.stats.Add(new StatContainer(StatType.LightningResistance, SourceType.Equipment_Armor, IncreaseType.Add, float.Parse(value))); break;
                case "poisonresistance": itemData.stats.Add(new StatContainer(StatType.PoisonResistance, SourceType.Equipment_Armor, IncreaseType.Add, float.Parse(value))); break;
                case "stunresistance": itemData.stats.Add(new StatContainer(StatType.StunResistance, SourceType.Equipment_Armor, IncreaseType.Add, float.Parse(value))); break;
                case "slowresistance": itemData.stats.Add(new StatContainer(StatType.SlowResistance, SourceType.Equipment_Armor, IncreaseType.Add, float.Parse(value))); break;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error setting item value for field {fieldName}: {e.Message}");
        }
    }

    private T ParseEnum<T>(string value) where T : struct
    {
        if (System.Enum.TryParse<T>(value, true, out T result))
            return result;
        return default;
    }

    public void LoadDropTables()
    {
        var dropTableJson = Resources.Load<TextAsset>($"{ITEM_DB_PATH}/DropTables");
        if (dropTableJson != null)
        {
            var wrapper = JsonUtility.FromJson<SerializableDropTables>(dropTableJson.text);
            if (wrapper?.tables != null)
            {
                enemyDropTables = wrapper.tables;
            }
        }
    }

    public void SaveDropTables()
    {
        var wrapper = new SerializableDropTables { tables = enemyDropTables };
        string json = JsonUtility.ToJson(wrapper, true);
        string path = Path.Combine(Application.dataPath, "Resources", ITEM_DB_PATH, "DropTables.json");
        File.WriteAllText(path, json);
        AssetDatabase.Refresh();
    }

    public DropTableData GetDropTable(EnemyType enemyType)
    {
        return enemyDropTables.TryGetValue(enemyType, out var dropTable) ? dropTable : null;
    }

    protected override void CreateResourceFolders()
    {
        string[] paths = new[]
        {
            ITEM_DB_PATH,
            ITEM_STATS_PATH,
            ITEM_PREFAB_PATH,
            ITEM_ICON_PATH
        };

        foreach (var path in paths)
        {
            string fullPath = Path.Combine(Application.dataPath, "Resources", path);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
        }
    }

    protected override void CreateDefaultFiles()
    {
        // 기본 아이템 스탯 CSV 파일 생성
        var headers = string.Join(",",
            "id", "name", "type", "rarity", "droprate",
            // 기본 스탯
            "damage", "defense", "hp", "movespeed", "attackspeed", "attackrange", "hpregen", "expgainrate", "goldgainrate",
            // 특수 스탯
            "criticalchance", "criticaldamage", "lifesteal", "reflectdamage", "dodgechance",
            // 저항 스탯
            "fireresistance", "iceresistance", "lightningresistance", "poisonresistance", "stunresistance", "slowresistance"
        );

        statManager.CreateDefaultFile("ItemStats", headers);

        // 기본 드롭테이블 생성
        var defaultDropTable = new Dictionary<EnemyType, DropTableData>
        {
            {
                EnemyType.Normal,
                new DropTableData
                {
                    enemyType = EnemyType.Normal,
                    guaranteedDropRate = 0.1f,
                    maxDrops = 2
                }
            }
        };

        var wrapper = new SerializableDropTables { tables = defaultDropTable };
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(
            Path.Combine(Application.dataPath, "Resources", ITEM_DB_PATH, "DropTables.json"),
            json
        );

        // 기본 아이템 생성
        var defaultItem = new ItemData
        {
            id = "default_sword",
            name = "Basic Sword",
            description = "A basic sword",
            type = ItemType.Weapon,
            rarity = ItemRarity.Common,
            stats = new List<StatContainer>
            {
                new StatContainer(StatType.Damage, SourceType.Equipment_Weapon, IncreaseType.Add, 5f)
            },
            maxStack = 1,
            dropRate = 1f
        };

        var database = new Dictionary<string, ItemData>
        {
            { defaultItem.id, defaultItem }
        };

        itemDatabase.SaveData("ItemDatabase", database);
    }

    protected override BackupManager GetBackupManager()
    {
        return backupManager;
    }

    public override void ClearAllData()
    {
        base.ClearAllData();
        itemDatabase?.ClearAll();
        statManager?.ClearAll();
        prefabManager?.ClearAll();
        iconManager?.ClearAll();
        enemyDropTables.Clear();
    }

#if UNITY_EDITOR
    public void CreateNewItem(ItemData itemData)
    {
        var database = itemDatabase.LoadData("ItemDatabase") ?? new Dictionary<string, ItemData>();
        database[itemData.id] = itemData;
        itemDatabase.SaveData("ItemDatabase", database);
    }

    public void DeleteItem(string itemId)
    {
        var database = itemDatabase.LoadData("ItemDatabase");
        if (database != null && database.ContainsKey(itemId))
        {
            database.Remove(itemId);
            itemDatabase.SaveData("ItemDatabase", database);
        }
    }

    public void UpdateItem(ItemData itemData)
    {
        var database = itemDatabase.LoadData("ItemDatabase");
        if (database != null)
        {
            database[itemData.id] = itemData;
            itemDatabase.SaveData("ItemDatabase", database);
        }
    }
#endif

    public void SaveItemDatabase()
    {
        try
        {
            var database = new Dictionary<string, ItemData>();
            foreach (var item in availableItems.Values)
            {
#if UNITY_EDITOR
                // 에디터에서는 리소스 경로와 GUID 정보 업데이트
                string iconPath = AssetDatabase.GetAssetPath(item.icon);
                string prefabPath = AssetDatabase.GetAssetPath(item.prefab);

                database[item.id] = new ItemData
                {
                    id = item.id,
                    name = item.name,
                    description = item.description,
                    type = item.type,
                    rarity = item.rarity,
                    stats = new List<StatContainer>(item.stats),
                    maxStack = item.maxStack,
                    dropRate = item.dropRate,
                    iconPath = GetResourcePath(iconPath),
                    prefabPath = GetResourcePath(prefabPath),
                    iconGuid = AssetDatabase.AssetPathToGUID(iconPath),
                    prefabGuid = AssetDatabase.AssetPathToGUID(prefabPath)
                };
#else
                database[item.id] = item.Clone();
#endif
            }

            string json = JsonUtility.ToJson(new SerializableItemDatabase { items = database }, true);
            string path = Path.Combine(Application.dataPath, "Resources", ITEM_DB_PATH, "ItemDatabase.json");
            File.WriteAllText(path, json);
            AssetDatabase.Refresh();
            Debug.Log($"Saved item database to: {path}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving item database: {e.Message}\n{e.StackTrace}");
        }
    }

    public void LoadItemDatabase()
    {
        try
        {
            string path = Path.Combine(ITEM_DB_PATH, "ItemDatabase");
            var jsonAsset = Resources.Load<TextAsset>(path);

            if (jsonAsset != null)
            {
                var database = JsonUtility.FromJson<SerializableItemDatabase>(jsonAsset.text);
                if (database?.items != null)
                {
                    foreach (var kvp in database.items)
                    {
                        var itemData = kvp.Value;
                        // 리소스 로드
                        itemData.icon = iconManager.LoadData($"{itemData.id}_Icon");
                        itemData.prefab = prefabManager.LoadData($"{itemData.id}_Prefab");

                        availableItems[kvp.Key] = itemData;
                    }
                    Debug.Log($"Loaded {availableItems.Count} items from database");
                }
            }
            else
            {
                Debug.LogWarning("Item database file not found. Creating new database.");
                CreateDefaultFiles();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading item database: {e.Message}\n{e.StackTrace}");
        }
    }

    public void SaveItemStats(ItemData itemData)
    {
        try
        {
            var stats = new ItemStatData
            {
                itemId = itemData.id,
                name = itemData.name,
                type = itemData.type,
                rarity = itemData.rarity,
                dropRate = itemData.dropRate,

                // 기본 스탯 설정
                damage = GetStatValue(itemData.stats, StatType.Damage),
                defense = GetStatValue(itemData.stats, StatType.Defense),
                hp = GetStatValue(itemData.stats, StatType.MaxHp),
                moveSpeed = GetStatValue(itemData.stats, StatType.MoveSpeed),
                attackSpeed = GetStatValue(itemData.stats, StatType.AttackSpeed),
                attackRange = GetStatValue(itemData.stats, StatType.AttackRange),
                hpRegen = GetStatValue(itemData.stats, StatType.HpRegenRate),
                expGainRate = GetStatValue(itemData.stats, StatType.ExpGainRate),
                goldGainRate = GetStatValue(itemData.stats, StatType.GoldGainRate),

                // 특수 스탯 설정
                criticalChance = GetStatValue(itemData.stats, StatType.CriticalChance),
                criticalDamage = GetStatValue(itemData.stats, StatType.CriticalDamage),
                lifeSteal = GetStatValue(itemData.stats, StatType.LifeSteal),
                reflectDamage = GetStatValue(itemData.stats, StatType.ReflectDamage),
                dodgeChance = GetStatValue(itemData.stats, StatType.DodgeChance),

                // 저항 스탯 설정
                fireResistance = GetStatValue(itemData.stats, StatType.FireResistance),
                iceResistance = GetStatValue(itemData.stats, StatType.IceResistance),
                lightningResistance = GetStatValue(itemData.stats, StatType.LightningResistance),
                poisonResistance = GetStatValue(itemData.stats, StatType.PoisonResistance),
                stunResistance = GetStatValue(itemData.stats, StatType.StunResistance),
                slowResistance = GetStatValue(itemData.stats, StatType.SlowResistance)
            };

            // CSV 파일에 저장
            statManager.SaveData(itemData.id, stats);
            Debug.Log($"Successfully saved stats for item: {itemData.id}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving item stats for {itemData.id}: {e.Message}\n{e.StackTrace}");
        }
    }

    private float GetStatValue(List<StatContainer> stats, StatType statType)
    {
        if (stats == null) return 0f;

        var stat = stats.FirstOrDefault(s => s.statType == statType);
        // StatContainer가 기본값인지 확인
        if (stat.Equals(default(StatContainer))) return 0f;

        return stat.amount;
    }

    public void SaveAllItemStats()
    {
        foreach (var item in availableItems.Values)
        {
            SaveItemStats(item);
        }
    }

    public Dictionary<EnemyType, DropTableData> GetDropTables()
    {
        return new Dictionary<EnemyType, DropTableData>(enemyDropTables);
    }
}

// 직렬화를 위한 래퍼 클래스
[System.Serializable]
public class SerializableDropTables
{
    public Dictionary<EnemyType, DropTableData> tables;
}

[System.Serializable]
public class SerializableItemDatabase
{
    public Dictionary<string, ItemData> items;
}
