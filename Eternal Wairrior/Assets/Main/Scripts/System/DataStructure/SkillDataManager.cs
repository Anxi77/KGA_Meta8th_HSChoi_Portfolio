using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using System.Linq;

public class SkillDataManager : DataManager
{
    private static SkillDataManager instance;
    public static SkillDataManager Instance => instance;

    private ResourceManager<GameObject> prefabManager;
    private ResourceManager<Sprite> iconManager;
    private CSVManager<SkillStatData> statManager;
    private JSONManager<SkillData> jsonManager;
    private BackupManager backupManager;
    private DataValidator dataValidator;

    // 스킬 시스템 전용 경로 상수
    private const string RESOURCE_PATH = "SkillData";
    private const string PREFAB_PATH = "SkillData/Prefabs";
    private const string ICON_PATH = "SkillData/Icons";
    private const string STAT_PATH = "SkillData/Stats";
    private const string JSON_PATH = "SkillData/Json";

    private Dictionary<SkillID, SkillData> skillDatabase = new Dictionary<SkillID, SkillData>();
    private Dictionary<SkillID, Dictionary<int, SkillStatData>> statDatabase = new Dictionary<SkillID, Dictionary<int, SkillStatData>>();
    private Dictionary<SkillID, Dictionary<int, GameObject>> levelPrefabDatabase = new Dictionary<SkillID, Dictionary<int, GameObject>>();

    private SkillEditorDataContainer editorData;

    protected override void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        // 에디터 모드와 플레이 모드 구분
        if (Application.isEditor && !Application.isPlaying)
        {
            InitializeForEditor();
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            InitializeForRuntime();
        }
    }

    private void InitializeForRuntime()
    {
        try
        {
            Debug.Log("Initializing SkillDataManager for runtime...");

            // 매니저 초기화
            InitializeManagers();

            // 모든 스킬 데이터 로드
            LoadAllSkillData();

            isInitialized = true;
            Debug.Log("SkillDataManager initialized for runtime");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error initializing SkillDataManager for runtime: {e.Message}\n{e.StackTrace}");
            isInitialized = false;
        }
    }

    private void LoadAllSkillData()
    {
        try
        {
            Debug.Log("Starting LoadAllSkillData...");

            var jsonFiles = Resources.LoadAll<TextAsset>(JSON_PATH);
            Debug.Log($"Found {jsonFiles.Length} JSON files in Resources/{JSON_PATH}");

            foreach (var jsonAsset in jsonFiles)
            {
                try
                {
                    string fileName = jsonAsset.name;
                    Debug.Log($"Processing JSON file: {fileName}");

                    string skillIdStr = fileName.Replace("_Data", "");
                    Debug.Log($"Extracted skill ID: {skillIdStr}");

                    if (System.Enum.TryParse<SkillID>(skillIdStr, out SkillID skillId))
                    {
                        var skillData = JsonUtility.FromJson<SkillData>(jsonAsset.text);
                        if (skillData != null)
                        {
                            // 기본 리소스 로드
                            string iconPath = $"{ICON_PATH}/{skillId}_Icon";
                            string prefabPath = $"{PREFAB_PATH}/{skillId}_Prefab";

                            Debug.Log($"Loading icon from: {iconPath}");
                            Debug.Log($"Loading prefab from: {prefabPath}");

                            skillData.icon = Resources.Load<Sprite>(iconPath);
                            skillData.metadata.Prefab = Resources.Load<GameObject>(prefabPath);

                            if (skillData.metadata.Type == SkillType.Projectile)
                            {
                                string projectilePath = $"{PREFAB_PATH}/{skillId}_Projectile";
                                Debug.Log($"Loading projectile from: {projectilePath}");
                                skillData.projectile = Resources.Load<GameObject>(projectilePath);
                            }

                            // 먼저 스탯 데이터 로드
                            LoadSkillStats(skillId, skillData.metadata.Type);

                            // 그 다음 레벨별 프리팹 로드
                            LoadLevelPrefabs(skillId, skillData);

                            skillDatabase[skillId] = skillData;
                            Debug.Log($"Successfully loaded skill: {skillData.metadata.Name} (ID: {skillId})");
                        }
                        else
                        {
                            Debug.LogError($"Failed to deserialize JSON for skill {skillId}");
                        }
                    }
                    else
                    {
                        Debug.LogError($"Failed to parse SkillID from filename: {fileName}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error processing JSON file {jsonAsset.name}: {e.Message}\n{e.StackTrace}");
                }
            }

            Debug.Log($"LoadAllSkillData completed. Loaded {skillDatabase.Count} skills");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in LoadAllSkillData: {e.Message}\n{e.StackTrace}");
        }
    }

    private void LoadSkillStats(SkillID skillId, SkillType type)
    {
        try
        {
            string statsFileName = type switch
            {
                SkillType.Projectile => "ProjectileSkillStats",
                SkillType.Area => "AreaSkillStats",
                SkillType.Passive => "PassiveSkillStats",
                _ => null
            };

            if (statsFileName == null)
            {
                Debug.LogError($"Invalid skill type for loading stats: {type}");
                return;
            }

            Debug.Log($"Loading stats for skill {skillId} from {statsFileName}");

            // Resources 폴더에서 CSV 파일 로드
            var textAsset = Resources.Load<TextAsset>($"{STAT_PATH}/{statsFileName}");
            if (textAsset == null)
            {
                Debug.LogError($"Failed to load stats file: {statsFileName}");
                return;
            }

            var lines = textAsset.text.Split('\n');
            if (lines.Length < 2)
            {
                Debug.LogError($"Invalid CSV format in {statsFileName}");
                return;
            }

            var headers = lines[0].Trim().Split(',');
            var stats = new Dictionary<int, SkillStatData>();

            // 이미 해당 스킬의 스탯이 로드되어 있다면 재사용
            if (statDatabase.TryGetValue(skillId, out var existingStats))
            {
                stats = existingStats;
            }

            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                var values = line.Split(',');
                if (values.Length != headers.Length) continue;

                var statData = new SkillStatData();
                for (int j = 0; j < headers.Length; j++)
                {
                    SetStatValue(statData, headers[j].ToLower(), values[j]);
                }

                // 스킬 ID와 타입이 모두 일치하는 경우에만 저장
                if (statData.skillID == skillId)
                {
                    // 스�� 타입 검증 로직 수정
                    bool isValidType = type switch
                    {
                        SkillType.Projectile => true,  // 프로젝타일 스킬은 모든 스탯이 유효
                        SkillType.Area => true,        // 에어리어 스킬은 모든 스탯이 유효
                        SkillType.Passive => true,     // 패시브 스킬은 모든 스탯이 유효
                        _ => false
                    };

                    if (isValidType)
                    {
                        stats[statData.level] = statData;
                        Debug.Log($"Loaded level {statData.level} stats for skill {skillId}");
                    }
                }
            }

            if (stats.Any())
            {
                statDatabase[skillId] = stats;
                Debug.Log($"Successfully loaded {stats.Count} stat entries for skill {skillId}");
            }
            else
            {
                Debug.LogWarning($"No valid stats found for skill {skillId} of type {type} in {statsFileName}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading stats for skill {skillId}: {e.Message}\n{e.StackTrace}");
        }
    }

    private void SetStatValue(SkillStatData statData, string fieldName, string value)
    {
        try
        {
            // 필드 이름을 소문자로 변환하여 비교
            fieldName = fieldName.ToLower();

            // 필드 이름과 프로퍼티 매핑
            string propertyName = fieldName switch
            {
                "skillid" => "SkillID",
                "level" => "Level",
                "damage" => "Damage",
                "maxskilllevel" => "MaxSkillLevel",
                "element" => "Element",
                "elementalpower" => "ElementalPower",
                "projectilespeed" => "ProjectileSpeed",
                "projectilescale" => "ProjectileScale",
                "shotinterval" => "ShotInterval",
                "piercecount" => "PierceCount",
                "attackrange" => "AttackRange",
                "homingrange" => "HomingRange",
                "ishoming" => "IsHoming",
                "explosionrad" => "ExplosionRad",
                "projectilecount" => "ProjectileCount",
                "innerinterval" => "InnerInterval",
                "radius" => "Radius",
                "duration" => "Duration",
                "tickrate" => "TickRate",
                "ispersistent" => "IsPersistent",
                "movespeed" => "MoveSpeed",
                "effectduration" => "EffectDuration",
                "cooldown" => "Cooldown",
                "triggerchance" => "TriggerChance",
                "damageincrease" => "DamageIncrease",
                "defenseincrease" => "DefenseIncrease",
                "expareaincrease" => "ExpAreaIncrease",
                "homingactivate" => "HomingActivate",
                "hpincrease" => "HpIncrease",
                "movespeedincrease" => "MoveSpeedIncrease",
                "attackspeedincrease" => "AttackSpeedIncrease",
                "attackrangeincrease" => "AttackRangeIncrease",
                "hpregenincrease" => "HpRegenIncrease",
                _ => null
            };

            if (propertyName == null)
            {
                Debug.LogWarning($"No mapping found for field: {fieldName}");
                return;
            }

            var property = typeof(SkillStatData).GetProperty(propertyName);
            if (property != null)
            {
                try
                {
                    object convertedValue;
                    if (property.PropertyType == typeof(bool))
                    {
                        convertedValue = value.ToLower() == "true" || value == "1";
                    }
                    else if (property.PropertyType == typeof(SkillID))
                    {
                        if (!System.Enum.TryParse<SkillID>(value, out var skillId))
                            throw new System.Exception($"Failed to parse SkillID: {value}");
                        convertedValue = skillId;
                    }
                    else if (property.PropertyType == typeof(ElementType))
                    {
                        if (!System.Enum.TryParse<ElementType>(value, out var elementType))
                            throw new System.Exception($"Failed to parse ElementType: {value}");
                        convertedValue = elementType;
                    }
                    else
                    {
                        convertedValue = System.Convert.ChangeType(value, property.PropertyType);
                    }

                    property.SetValue(statData, convertedValue);
                    Debug.Log($"Successfully set {propertyName} to {value}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error converting value '{value}' for field '{fieldName}': {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"Property not found: {propertyName}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in SetStatValue for field '{fieldName}': {e.Message}");
        }
    }

    protected override void InitializeManagers()
    {
        try
        {
            Debug.Log("Initializing SkillDataManager managers...");

            prefabManager = new ResourceManager<GameObject>(PREFAB_PATH);
            iconManager = new ResourceManager<Sprite>(ICON_PATH);
            statManager = new CSVManager<SkillStatData>(STAT_PATH);
            jsonManager = new JSONManager<SkillData>(JSON_PATH);
            backupManager = new BackupManager();
            dataValidator = new DataValidator();

            isInitialized = true;
            Debug.Log("SkillDataManager managers initialized successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to initialize SkillDataManager managers: {e.Message}\n{e.StackTrace}");
            isInitialized = false;
        }
    }

    protected override void CreateResourceFolders()
    {
        string resourcesPath = Path.Combine(Application.dataPath, "Resources");
        string[] paths = new string[]
        {
            RESOURCE_PATH,
            PREFAB_PATH,
            ICON_PATH,
            STAT_PATH,
            JSON_PATH
        };

        foreach (string path in paths)
        {
            string fullPath = Path.Combine(resourcesPath, path);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                Debug.Log($"Created directory: {fullPath}");
            }
        }

        AssetDatabase.Refresh();
    }

    protected override void CreateDefaultFiles()
    {
        // 스킬 타입별 CSV 파일 생성
        CreateDefaultCSVFiles();
    }

    private void CreateDefaultCSVFiles()
    {
        // 프로젝타일 스킬 CSV
        var projectileHeaders = new string[]
        {
            "skillid,level,damage,maxskilllevel,element,elementalpower," +
            "projectilespeed,projectilescale,shotinterval,piercecount,attackrange," +
            "homingrange,ishoming,explosionrad,projectilecount,innerinterval"
        };
        statManager.CreateDefaultFile("ProjectileSkillStats", projectileHeaders);

        // 에어리어 스킬 CSV
        var areaHeaders = new string[]
        {
            "skillid,level,damage,maxskilllevel,element,elementalpower," +
            "radius,duration,tickrate,ispersistent,movespeed"
        };
        statManager.CreateDefaultFile("AreaSkillStats", areaHeaders);

        // 패시브 스킬 CSV
        var passiveHeaders = new string[]
        {
            "skillid,level,damage,maxskilllevel,element,elementalpower," +
            "effectduration,cooldown,triggerchance,damageincrease,defenseincrease," +
            "expareaincrease,homingactivate,hpincrease,movespeedincrease," +
            "attackspeedincrease,attackrangeincrease,hpregenincrease"
        };
        statManager.CreateDefaultFile("PassiveSkillStats", passiveHeaders);
    }

    protected override BackupManager GetBackupManager()
    {
        return backupManager;
    }

    public override void ClearAllData()
    {
        // 에디터 모드이면서 플레이 모드 아닐  데이터 제 용
        if (Application.isEditor && !Application.isPlaying)
        {
            // 에디터에서 직접 Clear 요청했을 때만 실행
            if (EditorWindow.focusedWindow != null &&
                EditorWindow.focusedWindow.GetType().Name == "SkillDataEditorWindow")
            {
                base.ClearAllData();

                prefabManager?.ClearAll();
                iconManager?.ClearAll();
                statManager?.ClearAll();
                jsonManager?.ClearAll();

                skillDatabase?.Clear();
                statDatabase?.Clear();
                levelPrefabDatabase?.Clear();

                Debug.Log("Cleared all skill data from editor window");
            }
        }
    }

    // 스킬 데이터 관리
    public void SaveSkillData(SkillData skillData)
    {
        if (skillData?.metadata == null)
        {
            Debug.LogError("Cannot save null skill data or metadata");
            return;
        }

        try
        {
            var id = skillData.metadata.ID;

            if (skillDatabase == null)
            {
                skillDatabase = new Dictionary<SkillID, SkillData>();
            }

            skillDatabase[id] = skillData;

            // JSON 데이터 저장
            jsonManager?.SaveData($"{id}_Data", skillData);

            // 리소스 저장
            if (skillData.icon != null)
            {
                iconManager?.SaveData($"{id}_Icon", skillData.icon);
            }

            if (skillData.metadata.Prefab != null)
            {
                prefabManager?.SaveData($"{id}_Prefab", skillData.metadata.Prefab);
            }

            // 레벨별 프리팹 저장
            if (skillData.prefabsByLevel != null)
            {
                for (int i = 0; i < skillData.prefabsByLevel.Length; i++)
                {
                    if (skillData.prefabsByLevel[i] != null)
                    {
                        string prefabPath = $"{id}_Level_{i + 1}_Prefab";
                        prefabManager?.SaveData(prefabPath, skillData.prefabsByLevel[i]);
                        Debug.Log($"Saved level {i + 1} prefab for {skillData.metadata.Name}");
                    }
                }
            }

            if (skillData.metadata.Type == SkillType.Projectile && skillData.projectile != null)
            {
                prefabManager?.SaveData($"{id}_Projectile", skillData.projectile);
            }

            Debug.Log($"Successfully saved skill data for {skillData.metadata.Name}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving skill data for {skillData.metadata?.Name}: {e.Message}\n{e.StackTrace}");
        }
    }

    public void SaveAllData(List<SkillData> skillDataList, List<SkillEditorDataContainer.SkillLevelStats> skillStats)
    {
        try
        {
            if (skillDataList == null || skillDataList.Count == 0)
            {
                Debug.LogError("No skills to save in the database");
                return;
            }

            Debug.Log($"Starting to save {skillDataList.Count} skills...");

            // 스킬 데이터 저장
            foreach (var skillData in skillDataList)
            {
                if (skillData?.metadata == null) continue;

                // OnBeforeSerialize 호출하여 직렬화 준비
                if (skillData is ISerializationCallbackReceiver receiver)
                {
                    receiver.OnBeforeSerialize();
                }

                // JSON 데이터 저장 (pretty print 적용)
                string jsonData = JsonUtility.ToJson(skillData, true);
                string jsonPath = Path.Combine(Application.dataPath, "Resources", JSON_PATH, $"{skillData.metadata.ID}_Data.json");

                // 디렉토리가 없다면 생성
                Directory.CreateDirectory(Path.GetDirectoryName(jsonPath));

                // JSON 파일 저장
                File.WriteAllText(jsonPath, jsonData);
                Debug.Log($"Saved JSON data for skill: {skillData.metadata.Name} to {jsonPath}");

                // 리소스 파일들 저장
                SaveSkillResources(skillData);
            }

            // 스킬 스탯 데이터 저장
            Debug.Log($"Processing {skillStats?.Count ?? 0} skill stat entries...");

            var projectileStats = new List<SkillStatData>();
            var areaStats = new List<SkillStatData>();
            var passiveStats = new List<SkillStatData>();

            if (skillStats != null)
            {
                foreach (var skillStatEntry in skillStats)
                {
                    if (skillStatEntry == null)
                    {
                        Debug.LogError("Null skillStatEntry found");
                        continue;
                    }

                    var skillData = skillDataList.Find(s => s.metadata.ID == skillStatEntry.skillID);
                    if (skillData == null)
                    {
                        Debug.LogError($"Could not find skill data for ID: {skillStatEntry.skillID}");
                        continue;
                    }

                    Debug.Log($"Processing stats for skill: {skillData.metadata.Name} (Type: {skillData.metadata.Type})");

                    if (skillStatEntry.levelStats == null || skillStatEntry.levelStats.Count == 0)
                    {
                        Debug.LogError($"No level stats found for skill: {skillData.metadata.Name}");
                        continue;
                    }

                    foreach (var stat in skillStatEntry.levelStats)
                    {
                        if (stat == null)
                        {
                            Debug.LogError($"Null stat data found for skill {skillData.metadata.Name}");
                            continue;
                        }

                        // 스탯 데이터 복사 및 설정
                        var statCopy = stat.Clone();  // Clone 메서드를 사용하여 깊은 복사
                        statCopy.skillID = skillData.metadata.ID;
                        statCopy.element = skillData.metadata.Element;

                        // 현재 스킬의 타입별 스탯 값들을 복사
                        var currentTypeStat = skillData.GetCurrentTypeStat();
                        if (currentTypeStat != null)
                        {
                            switch (skillData.metadata.Type)
                            {
                                case SkillType.Projectile:
                                    var projStat = currentTypeStat as ProjectileSkillStat;
                                    if (projStat != null)
                                    {
                                        statCopy.projectileSpeed = projStat.projectileSpeed;
                                        statCopy.projectileScale = projStat.projectileScale;
                                        statCopy.shotInterval = projStat.shotInterval;
                                        statCopy.pierceCount = projStat.pierceCount;
                                        statCopy.attackRange = projStat.attackRange;
                                        statCopy.homingRange = projStat.homingRange;
                                        statCopy.isHoming = projStat.isHoming;
                                        statCopy.explosionRad = projStat.explosionRad;
                                        statCopy.projectileCount = projStat.projectileCount;
                                        statCopy.innerInterval = projStat.innerInterval;
                                    }
                                    projectileStats.Add(statCopy);
                                    break;

                                case SkillType.Area:
                                    var areaStat = currentTypeStat as AreaSkillStat;
                                    if (areaStat != null)
                                    {
                                        statCopy.radius = areaStat.radius;
                                        statCopy.duration = areaStat.duration;
                                        statCopy.tickRate = areaStat.tickRate;
                                        statCopy.isPersistent = areaStat.isPersistent;
                                        statCopy.moveSpeed = areaStat.moveSpeed;
                                    }
                                    areaStats.Add(statCopy);
                                    break;

                                case SkillType.Passive:
                                    var passiveStat = currentTypeStat as PassiveSkillStat;
                                    if (passiveStat != null)
                                    {
                                        statCopy.effectDuration = passiveStat.effectDuration;
                                        statCopy.cooldown = passiveStat.cooldown;
                                        statCopy.triggerChance = passiveStat.triggerChance;
                                        statCopy.damageIncrease = passiveStat.damageIncrease;
                                        statCopy.defenseIncrease = passiveStat.defenseIncrease;
                                        statCopy.expAreaIncrease = passiveStat.expAreaIncrease;
                                        statCopy.homingActivate = passiveStat.homingActivate;
                                        statCopy.hpIncrease = passiveStat.hpIncrease;
                                        statCopy.moveSpeedIncrease = passiveStat.moveSpeedIncrease;
                                        statCopy.attackSpeedIncrease = passiveStat.attackSpeedIncrease;
                                        statCopy.attackRangeIncrease = passiveStat.attackRangeIncrease;
                                        statCopy.hpRegenIncrease = passiveStat.hpRegenIncrease;
                                    }
                                    passiveStats.Add(statCopy);
                                    break;
                            }

                            Debug.Log($"Added level {statCopy.level} stats for {skillData.metadata.Name}");
                        }
                        else
                        {
                            Debug.LogError($"Failed to get current type stat for {skillData.metadata.Name}");
                        }
                    }
                }

                // CSV 파로 스탯 저장
                if (projectileStats.Count > 0)
                {
                    Debug.Log($"Saving {projectileStats.Count} projectile stats...");
                    statManager?.SaveBulkData("ProjectileSkillStats", projectileStats);
                }
                if (areaStats.Count > 0)
                {
                    Debug.Log($"Saving {areaStats.Count} area stats...");
                    statManager?.SaveBulkData("AreaSkillStats", areaStats);
                }
                if (passiveStats.Count > 0)
                {
                    Debug.Log($"Saving {passiveStats.Count} passive stats...");
                    statManager?.SaveBulkData("PassiveSkillStats", passiveStats);
                }
            }
            else
            {
                Debug.LogError("skillStats is null");
            }

            AssetDatabase.Refresh();
            Debug.Log("Successfully saved all skill data");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving all skill data: {e.Message}\n{e.StackTrace}");
        }
    }

    private void SaveSkillResources(SkillData skillData)
    {
        // 아이콘 저장
        if (skillData.icon != null)
        {
            iconManager?.SaveData($"{skillData.metadata.ID}_Icon", skillData.icon);
        }

        // 프리팹 저장
        if (skillData.metadata.Prefab != null)
        {
            prefabManager?.SaveData($"{skillData.metadata.ID}_Prefab", skillData.metadata.Prefab);
        }

        // 프로젝타일 프리 저장
        if (skillData.metadata.Type == SkillType.Projectile && skillData.projectile != null)
        {
            prefabManager?.SaveData($"{skillData.metadata.ID}_Projectile", skillData.projectile);
        }

        // 레벨별 프리팹 저장
        if (skillData.prefabsByLevel != null)
        {
            for (int i = 0; i < skillData.prefabsByLevel.Length; i++)
            {
                if (skillData.prefabsByLevel[i] != null)
                {
                    prefabManager?.SaveData($"{skillData.metadata.ID}_Level_{i + 1}", skillData.prefabsByLevel[i]);
                }
            }
        }
    }

    // 런타임 데이터 접근 메서드들
    public SkillData GetSkillData(SkillID id)
    {
        if (skillDatabase.TryGetValue(id, out var data))
            return data;

        try
        {
            // JSON에서 데이터 로드
            var skillData = jsonManager.LoadData($"{id}_Data");
            if (skillData != null)
            {
                // 리소스 로드
                skillData.icon = iconManager.LoadData($"{id}_Icon");
                skillData.metadata.Prefab = prefabManager.LoadData($"{id}_Metadata");

                if (skillData.metadata.Type == SkillType.Projectile)
                {
                    skillData.projectile = prefabManager.LoadData($"{id}_Projectile");
                }

                skillDatabase[id] = skillData;
                return skillData;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading skill data for {id}: {e.Message}");
        }

        return null;
    }

    public List<SkillData> GetAllSkillData()
    {
        return new List<SkillData>(skillDatabase.Values);
    }

    public ISkillStat GetSkillStatsForLevel(SkillID id, int level, SkillType type)
    {
        var statData = GetSkillStats(id, level);
        if (statData != null)
        {
            return statData.CreateSkillStat(type);
        }
        return null;
    }

    public SkillStatData GetSkillStats(SkillID id, int level)
    {
        if (statDatabase.TryGetValue(id, out var levelStats) &&
            levelStats.TryGetValue(level, out var statData))
        {
            return statData;
        }
        return null;
    }

    // 스킬 업그레이드 관련 메서드
    public void UpdateSkillLevel(SkillID id, int newLevel)
    {
        if (skillDatabase.TryGetValue(id, out var skillData))
        {
            var stats = GetSkillStats(id, newLevel);
            if (stats != null)
            {
                var skillStat = stats.CreateSkillStat(skillData.metadata.Type);
                skillData.SetStatsForLevel(newLevel, skillStat);
            }
        }
    }

    private void InitializeForEditor()
    {
        try
        {
            Debug.Log("Initializing SkillDataManager for Editor mode...");

            skillDatabase = new Dictionary<SkillID, SkillData>();
            statDatabase = new Dictionary<SkillID, Dictionary<int, SkillStatData>>();
            levelPrefabDatabase = new Dictionary<SkillID, Dictionary<int, GameObject>>();

            InitializeManagers();
            CreateResourceFolders();
            CreateDefaultFiles();

            isInitialized = true;
            Debug.Log("SkillDataManager initialized for Editor mode");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error initializing SkillDataManager for Editor: {e.Message}\n{e.StackTrace}");
            isInitialized = false;
        }
    }

    protected override void OnDestroy()
    {
        // 에디터 모드서 직접 삭제할 때만 데이터 정리
        if (Application.isEditor && !Application.isPlaying)
        {
            // 에디터 창이 닫힐 때만 정리
            if (EditorWindow.focusedWindow != null &&
                EditorWindow.focusedWindow.GetType().Name == "SkillDataEditorWindow")
            {
                ClearAllData();
                Debug.Log("Cleaned up skill data from editor window close");
            }
        }
        // 런타임 중에는 게임오브젝트만 제거하고 데이터는 유지
        else if (Application.isPlaying)
        {
            if (instance == this)
            {
                instance = null;
            }
            Debug.Log("SkillDataManager destroyed during runtime (data preserved)");
        }
    }

    // LoadSkillData 메서드 수정
    private SkillData LoadSkillData(SkillID id)
    {
        try
        {
            // JSON에서 데이터 로드
            string resourcePath = $"{id}_Data";
            var skillData = jsonManager?.LoadData(resourcePath);

            if (skillData != null)
            {
                Debug.Log($"Loading resources for skill {id}...");

                // 기본 리소스 로드
                skillData.icon = iconManager?.LoadData($"{id}_Icon");
                skillData.metadata.Prefab = prefabManager?.LoadData($"{id}_Prefab");

                // 레벨별 프리팹 로드
                var stats = GetSkillStats(id, 1);  // 레벨 1의 스탯으로 최대 레벨 확인
                if (stats != null)
                {
                    int maxLevel = stats.maxSkillLevel;
                    skillData.prefabsByLevel = new GameObject[maxLevel];

                    for (int i = 0; i < maxLevel; i++)
                    {
                        skillData.prefabsByLevel[i] = prefabManager?.LoadData($"{id}_Level_{i + 1}");

                        if (skillData.prefabsByLevel[i] != null)
                        {
                            Debug.Log($"Successfully loaded level {i + 1} prefab for {skillData.metadata.Name}");
                        }
                        else
                        {
                            Debug.LogWarning($"Failed to load level {i + 1} prefab");
                        }
                    }
                }

                // 프로젝타일 스킬의 경우 추가 프리팹 로드
                if (skillData.metadata.Type == SkillType.Projectile)
                {
                    skillData.projectile = prefabManager?.LoadData($"{id}_Projectile");
                }

                Debug.Log($"Successfully loaded skill data for {skillData.metadata.Name}");
                return skillData;
            }
            else
            {
                Debug.LogError($"Failed to load JSON data for skill {id}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading skill data for {id}: {e.Message}\n{e.StackTrace}");
        }

        return null;
    }

    // 레벨별 프리팹 로드를 위한 새로운 메서드 추가
    private void LoadLevelPrefabs(SkillID skillId, SkillData skillData)
    {
        try
        {
            // 스킬의 최대 레벨 정보 가져오기
            var stats = GetSkillStats(skillId, 1); // 레벨 1의 스탯으로 최대 벨 확인
            if (stats != null)
            {
                int maxLevel = stats.maxSkillLevel;
                skillData.prefabsByLevel = new GameObject[maxLevel];

                for (int i = 0; i < maxLevel; i++)
                {
                    // 파일 이름만 사용하도록 수정
                    string levelPrefabName = $"{skillId}_Level_{i + 1}";
                    var levelPrefab = Resources.Load<GameObject>($"{PREFAB_PATH}/{levelPrefabName}");

                    if (levelPrefab != null)
                    {
                        skillData.prefabsByLevel[i] = levelPrefab;
                        Debug.Log($"Loaded level {i + 1} prefab for skill {skillId}");

                        // 레벨별 프리팹 데이터베이스에도 저장
                        if (!levelPrefabDatabase.ContainsKey(skillId))
                        {
                            levelPrefabDatabase[skillId] = new Dictionary<int, GameObject>();
                        }
                        levelPrefabDatabase[skillId][i + 1] = levelPrefab;
                    }
                    else
                    {
                        Debug.LogWarning($"No prefab found for skill {skillId} at level {i + 1}");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"Could not find stats for skill {skillId}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading level prefabs for skill {skillId}: {e.Message}\n{e.StackTrace}");
        }
    }
}