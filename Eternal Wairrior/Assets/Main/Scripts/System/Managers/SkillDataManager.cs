using UnityEngine;

public class SkillDataManager : DataManager
{
    private const int CURRENT_DATA_VERSION = 2;
    private const string VERSION_KEY = "SkillDataVersion";

    private void MigrateDataIfNeeded()
    {
        int savedVersion = PlayerPrefs.GetInt(VERSION_KEY, 1);
        if (savedVersion < CURRENT_DATA_VERSION)
        {
            try
            {
                MigrateData(savedVersion);
                PlayerPrefs.SetInt(VERSION_KEY, CURRENT_DATA_VERSION);
                PlayerPrefs.Save();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Data migration failed: {e.Message}");
                BackupAndResetData();
            }
        }
    }

    private void MigrateData(int fromVersion)
    {
        switch (fromVersion)
        {
            case 1:
                MigrateFromVersion1To2();
                break;
                // 추가 버전 마이그레이션...
        }
    }

    private void MigrateFromVersion1To2()
    {
        foreach (var skillData in skillDatas)
        {
            if (skillData.GetStatsForLevel(1) is ProjectileSkillStat projectileStats)
            {
                projectileStats.persistenceData = new ProjectilePersistenceData
                {
                    isPersistent = false,
                    duration = 0f,
                    effectInterval = 0.5f
                };
            }
<<<<<<< HEAD
            return instance;
        }
    }

    private List<SkillData> skillDatas = new List<SkillData>();
    private const string RESOURCE_PATH = "SkillData";
    private const string SKILL_DATA_FILENAME = "SkillData.json";

    private Dictionary<SkillID, Dictionary<int, SkillStatData>> skillStatsByLevel;

    protected override void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        base.Awake();

        skillStatsByLevel = new Dictionary<SkillID, Dictionary<int, SkillStatData>>();
        LoadSkillStatsFromCSV();
        LoadAllSkillData();
    }

    public List<SkillData> GetAllSkillData()
    {
        return new List<SkillData>(skillDatas);
    }

    public void SaveAllSkillData()
    {
#if UNITY_EDITOR
        SaveAllSkillDataInEditor();
#else
        SkillDataWrapper wrapper = new SkillDataWrapper { skillDatas = skillDatas };
        SaveData(SKILL_DATA_FILENAME, wrapper);
#endif
    }

    public void LoadAllSkillData()
    {
        SkillDataWrapper wrapper = LoadData<SkillDataWrapper>(SKILL_DATA_FILENAME);
        if (wrapper != null)
        {
            skillDatas = wrapper.skillDatas ?? new List<SkillData>();
        }
        else
        {
            skillDatas = new List<SkillData>();
            CreateDefaultSkillData();
        }
    }

    private void CreateDefaultSkillData()
    {
        // 기본 스킬 데이터 생성 로직
    }

    public void UpdateSkillData(SkillData updatedSkill)
    {
        int index = skillDatas.FindIndex(x => x.metadata.ID == updatedSkill.metadata.ID);
        if (index != -1)
        {
            skillDatas[index] = updatedSkill;
            SaveAllSkillData();
        }
    }

    public SkillData GetSkillData(SkillID skillID)
    {
        if (skillID == SkillID.None)
        {
            Debug.LogWarning("Attempting to get skill data for SkillID.None");
            return null;
        }

        SkillData skillData = skillDatas.Find(x => x.metadata.ID == skillID);
        if (skillData == null)
        {
            Debug.LogError($"No skill data found for SkillID: {skillID}");
            return null;
        }

        if (skillStatsByLevel.TryGetValue(skillID, out var levelStats))
        {
            if (levelStats.TryGetValue(1, out var statData))
=======
            else if (skillData.GetStatsForLevel(1) is AreaSkillStat areaStats)
>>>>>>> 636e55d9921dee25edf69b9286cacd4495ea6e5a
            {
                areaStats.persistenceData = new AreaPersistenceData
                {
                    isPersistent = true,
                    duration = 0f,
                    effectInterval = 0f
                };
            }
        }
    }

    private void BackupAndResetData()
    {
        BackupCSVFiles();
        ClearAllData();
        LoadAllData();
    }
<<<<<<< HEAD

    private SkillStatData ParseSkillStatLine(string[] headers, string[] values)
    {
        SkillStatData statData = new SkillStatData();

        for (int i = 0; i < headers.Length && i < values.Length; i++)
        {
            string value = values[i].Trim();
            switch (headers[i].Trim().ToLower())
            {
                // 기본 스킬
                case "skillid":
                    if (System.Enum.TryParse(value, out SkillID skillID))
                        statData.skillID = skillID;
                    break;
                case "level":
                    if (int.TryParse(value, out int level))
                        statData.level = level;
                    break;
                case "damage":
                    if (float.TryParse(value, out float damage))
                        statData.damage = damage;
                    break;
                case "maxskilllevel":
                    if (int.TryParse(value, out int maxLevel))
                        statData.maxSkillLevel = maxLevel;
                    break;
                case "element":
                    if (System.Enum.TryParse(value, out ElementType element))
                        statData.element = element;
                    break;
                case "elementalpower":
                    if (float.TryParse(value, out float elementalPower))
                        statData.elementalPower = elementalPower;
                    break;

                // 투사 스킬
                case "projectilespeed":
                    if (float.TryParse(value, out float speed))
                        statData.projectileSpeed = speed;
                    break;
                case "projectilescale":
                    if (float.TryParse(value, out float scale))
                        statData.projectileScale = scale;
                    break;
                case "shotinterval":
                    if (float.TryParse(value, out float interval))
                        statData.shotInterval = interval;
                    break;
                case "piercecount":
                    if (int.TryParse(value, out int pierce))
                        statData.pierceCount = pierce;
                    break;
                case "attackrange":
                    if (float.TryParse(value, out float attackRange))
                        statData.attackRange = attackRange;
                    break;
                case "homingrange":
                    if (float.TryParse(value, out float homingRange))
                        statData.homingRange = homingRange;
                    break;
                case "ishoming":
                    if (bool.TryParse(value, out bool isHoming))
                        statData.isHoming = isHoming;
                    break;
                case "explosionrad":
                    if (float.TryParse(value, out float explosionRad))
                        statData.explosionRad = explosionRad;
                    break;
                case "projectilecount":
                    if (int.TryParse(value, out int count))
                        statData.projectileCount = count;
                    break;
                case "innerinterval":
                    if (float.TryParse(value, out float innerInterval))
                        statData.innerInterval = innerInterval;
                    break;

                // 범위 스킬
                case "radius":
                    if (float.TryParse(value, out float radius))
                        statData.radius = radius;
                    break;
                case "duration":
                    if (float.TryParse(value, out float duration))
                        statData.duration = duration;
                    break;
                case "tickrate":
                    if (float.TryParse(value, out float tickRate))
                        statData.tickRate = tickRate;
                    break;
                case "ispersistent":
                    if (bool.TryParse(value, out bool isPersistent))
                        statData.isPersistent = isPersistent;
                    break;
                case "movespeed":
                    if (float.TryParse(value, out float moveSpeed))
                        statData.moveSpeed = moveSpeed;
                    break;

                // 효과 스킬
                case "effectduration":
                    if (float.TryParse(value, out float effectDuration))
                        statData.effectDuration = effectDuration;
                    break;
                case "cooldown":
                    if (float.TryParse(value, out float cooldown))
                        statData.cooldown = cooldown;
                    break;
                case "triggerchance":
                    if (float.TryParse(value, out float triggerChance))
                        statData.triggerChance = triggerChance;
                    break;

                default:
                    Debug.LogWarning($"Unknown header: {headers[i]}");
                    break;
            }
        }

        return statData;
    }

    public ISkillStat GetSkillStatsForLevel(SkillID skillID, int level, SkillType skillType)
    {
        if (skillStatsByLevel.TryGetValue(skillID, out var levelStats) &&
            levelStats.TryGetValue(level, out var statData))
        {
            return statData.CreateSkillStat(skillType);
        }

        return null;
    }

    protected override void SaveData<T>(string fileName, T data)
    {
        try
        {
            string jsonData = JsonUtility.ToJson(data, true);
            string directory = Path.Combine(Application.dataPath, "Resources", RESOURCE_PATH);
            string path = Path.Combine(directory, fileName);

            Directory.CreateDirectory(directory);
            File.WriteAllText(path, jsonData);
            Debug.Log($"Data saved successfully to {path}");
            AssetDatabase.Refresh();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save data: {e.Message}");
        }
    }

    protected override T LoadData<T>(string fileName)
    {
        try
        {
            TextAsset jsonFile = Resources.Load<TextAsset>(Path.Combine(RESOURCE_PATH, Path.GetFileNameWithoutExtension(fileName)));
            if (jsonFile != null)
            {
                return JsonUtility.FromJson<T>(jsonFile.text);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load data: {e.Message}");
        }

        return new T();
    }

    public void UpdateSkillList(List<SkillData> newSkillList)
    {
        skillDatas = new List<SkillData>(newSkillList);
    }

#if UNITY_EDITOR
    public void SaveDataInEditor(string fileName, object data)
    {
        try
        {
            string jsonData = JsonUtility.ToJson(data, true);
            string directory = Path.Combine(Application.dataPath, "Resources", RESOURCE_PATH);
            string path = Path.Combine(directory, fileName);

            Directory.CreateDirectory(directory);
            File.WriteAllText(path, jsonData);
            Debug.Log($"Data saved successfully to {path}");
            UnityEditor.AssetDatabase.Refresh();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save data: {e.Message}");
        }
    }

    public void SaveAllSkillDataInEditor()
    {
        SkillDataWrapper wrapper = new SkillDataWrapper { skillDatas = skillDatas };
        SaveDataInEditor(SKILL_DATA_FILENAME, wrapper);
    }
#endif
}
=======
}
>>>>>>> 636e55d9921dee25edf69b9286cacd4495ea6e5a
