using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

public class SkillDataManager : DataManager
{
    private static new SkillDataManager instance;

    public static new SkillDataManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SkillDataManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("SkillDataManager");
                    instance = go.AddComponent<SkillDataManager>();
                }
            }
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
        SkillDataWrapper wrapper = new SkillDataWrapper { skillDatas = skillDatas };
        SaveData(SKILL_DATA_FILENAME, wrapper);
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
            {
                ISkillStat skillStat = statData.CreateSkillStat(skillData.metadata.Type);
                skillData.SetStatsForLevel(1, skillStat);
            }
        }

        return skillData;
    }

    private void LoadSkillStatsFromCSV()
    {
        TextAsset csvFile = Resources.Load<TextAsset>($"{RESOURCE_PATH}/ProjectileSkillStats");
        if (csvFile == null)
        {
            Debug.LogError("스킬 CSV 로드 실패!");
            return;
        }

        string[] lines = csvFile.text.Split('\n');
        string[] headers = lines[0].Trim().Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');

            SkillStatData statData = ParseSkillStatLine(headers, values);

            if (!skillStatsByLevel.ContainsKey(statData.skillID))
            {
                skillStatsByLevel[statData.skillID] = new Dictionary<int, SkillStatData>();
            }

            skillStatsByLevel[statData.skillID][statData.level] = statData;
        }
    }

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
}
