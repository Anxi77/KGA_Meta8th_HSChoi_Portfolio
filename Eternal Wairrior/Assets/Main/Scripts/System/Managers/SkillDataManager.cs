using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using Unity.VisualScripting;
using System.Text;
using System.Linq;
using System;

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
    private Dictionary<SkillID, List<SkillStatData>> skillStatsList = new Dictionary<SkillID, List<SkillStatData>>();

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

    public void LoadSkillStatsFromCSV()
    {
        try
        {
            string[] fileNames = {
                "ProjectileSkillStats",
                "AreaSkillStats",
                "PassiveSkillStats"
            };

            bool anyFileLoaded = false;
            var tempStatsByLevel = new Dictionary<SkillID, Dictionary<int, SkillStatData>>();

            foreach (var fileName in fileNames)
            {
                TextAsset csvFile = Resources.Load<TextAsset>($"{RESOURCE_PATH}/{fileName}");
                if (csvFile == null)
                {
                    Debug.LogWarning($"Failed to load {fileName}.csv");
                    continue;
                }

                anyFileLoaded = true;
                ProcessCSVFile(csvFile, fileName, tempStatsByLevel);
            }

            if (anyFileLoaded)
            {
                skillStatsByLevel = tempStatsByLevel;
                Debug.Log("Successfully loaded skill stats from CSV files");
            }
            else
            {
                Debug.LogError("No skill stat CSV files were loaded!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading skill stats: {e.Message}");
        }
    }

    private void ProcessCSVFile(TextAsset csvFile, string fileName, Dictionary<SkillID, Dictionary<int, SkillStatData>> tempStats)
    {
        string[] lines = csvFile.text.Split('\n');
        string[] headers = lines[0].Trim().Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            try
            {
                string[] values = line.Split(',');
                SkillStatData statData = ParseSkillStatLine(headers, values);

                if (statData.skillID == SkillID.None)
                {
                    Debug.LogWarning($"Invalid SkillID in {fileName} at line {i}");
                    continue;
                }

                if (!tempStats.ContainsKey(statData.skillID))
                {
                    tempStats[statData.skillID] = new Dictionary<int, SkillStatData>();
                }

                if (ValidateSkillStat(statData))
                {
                    tempStats[statData.skillID][statData.level] = statData;

                    // 스프라이트 경로도 CSV에 포함시키고 로드
                    if (headers.Contains("iconPath"))
                    {
                        string iconPath = values[Array.IndexOf(headers, "iconPath")];
                        if (!string.IsNullOrEmpty(iconPath))
                        {
                            // Resources 폴더에서 스프라이트 로드
                            Sprite icon = Resources.Load<Sprite>(iconPath);
                            if (icon == null)
                            {
                                Debug.LogWarning($"Could not load sprite at path: {iconPath}");
                            }
                            else
                            {
                                // 해당 스킬의 SkillData 찾기
                                var skillData = skillDatas.Find(s => s.metadata.ID == statData.skillID);
                                if (skillData != null)
                                {
                                    skillData.icon = icon;
                                }
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError($"Invalid stat data in {fileName} at line {i}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error processing line {i} in {fileName}: {e.Message}");
            }
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

                // 패시브 스킬 누락된 스탯 추가
                case "damageincrease":
                    if (float.TryParse(value, out float damageIncrease))
                        statData.damageIncrease = damageIncrease;
                    break;
                case "defenseincrease":
                    if (float.TryParse(value, out float defenseIncrease))
                        statData.defenseIncrease = defenseIncrease;
                    break;
                case "expareaincrease":
                    if (float.TryParse(value, out float expAreaIncrease))
                        statData.expAreaIncrease = expAreaIncrease;
                    break;
                case "homingactivate":
                    if (bool.TryParse(value, out bool homingActivate))
                        statData.homingActivate = homingActivate;
                    break;
                case "hpincrease":
                    if (float.TryParse(value, out float hpIncrease))
                        statData.hpIncrease = hpIncrease;
                    break;
                case "movespeedincrease":
                    if (float.TryParse(value, out float moveSpeedInc))
                        statData.moveSpeedIncrease = moveSpeedInc;
                    break;
                case "attackspeedincrease":
                    if (float.TryParse(value, out float attackSpeedInc))
                        statData.attackSpeedIncrease = attackSpeedInc;
                    break;
                case "attackrangeincrease":
                    if (float.TryParse(value, out float attackRangeInc))
                        statData.attackRangeIncrease = attackRangeInc;
                    break;
                case "hpregenincrease":
                    if (float.TryParse(value, out float hpRegenInc))
                        statData.hpRegenIncrease = hpRegenInc;
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
        if (!ValidateSkillRequest(skillID, level, skillType, out string errorMessage))
        {
            Debug.LogError(errorMessage);
            return null;
        }

        try
        {
            if (skillStatsByLevel.TryGetValue(skillID, out var levelStats))
            {
                if (levelStats.TryGetValue(level, out var statData))
                {
                    if (statData.ValidateStats())
                    {
                        var skillStat = statData.CreateSkillStat(skillType);
                        if (skillStat != null)
                        {
                            return skillStat;
                        }
                        throw new System.Exception("Failed to create skill stat");
                    }
                    throw new System.Exception($"Invalid stat values for skill {skillID} level {level}");
                }
                throw new System.Exception($"Stats for level {level} not found");
            }
            throw new System.Exception($"No stats found for skill {skillID}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error getting skill stats: {e.Message}");
            return null;
        }
    }

    private bool ValidateSkillRequest(SkillID skillID, int level, SkillType skillType, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (skillID == SkillID.None)
        {
            errorMessage = "Invalid SkillID: None";
            return false;
        }

        if (level <= 0)
        {
            errorMessage = $"Invalid level requested for {skillID}: {level}";
            return false;
        }

        var skillData = GetSkillData(skillID);
        if (skillData == null)
        {
            errorMessage = $"Skill data not found for {skillID}";
            return false;
        }

        if (skillData.metadata.Type != skillType)
        {
            errorMessage = $"Skill type mismatch. Expected {skillType} but got {skillData.metadata.Type}";
            return false;
        }

        return true;
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

    public void UpdateSkillStatsData(Dictionary<SkillID, List<SkillStatData>> editorStats)
    {
        var backup = new Dictionary<SkillID, Dictionary<int, SkillStatData>>(skillStatsByLevel);
        try
        {
            skillStatsByLevel.Clear();
            foreach (var pair in editorStats)
            {
                skillStatsByLevel[pair.Key] = new Dictionary<int, SkillStatData>();
                foreach (var stat in pair.Value)
                {
                    if (ValidateSkillStat(stat))
                    {
                        skillStatsByLevel[pair.Key][stat.level] = stat;
                    }
                    else
                    {
                        throw new System.Exception($"Invalid stat data for skill {pair.Key} level {stat.level}");
                    }
                }
            }
            Debug.Log("Successfully updated skill stats data");
        }
        catch (System.Exception e)
        {
            skillStatsByLevel = backup;
            Debug.LogError($"Failed to update skill stats: {e.Message}. Reverting to previous data.");
        }
    }

    private bool ValidateSkillStat(SkillStatData stat)
    {
        if (stat == null) return false;
        if (stat.skillID == SkillID.None) return false;
        if (stat.level <= 0) return false;
        if (stat.maxSkillLevel <= 0) return false;
        if (stat.damage < 0) return false;

        var skillData = GetSkillData(stat.skillID);
        if (skillData == null) return false;

        switch (skillData.metadata.Type)
        {
            case SkillType.Projectile:
                return ValidateProjectileStats(stat);
            case SkillType.Area:
                return ValidateAreaStats(stat);
            case SkillType.Passive:
                return ValidatePassiveStats(stat);
            default:
                return false;
        }
    }

    private bool ValidateProjectileStats(SkillStatData stat)
    {
        if (stat.projectileSpeed <= 0) return false;
        if (stat.projectileScale <= 0) return false;
        if (stat.shotInterval <= 0) return false;
        if (stat.pierceCount < 0) return false;
        if (stat.attackRange <= 0) return false;
        if (stat.projectileCount <= 0) return false;
        return true;
    }

    private bool ValidateAreaStats(SkillStatData stat)
    {
        if (stat.radius <= 0) return false;
        if (stat.duration <= 0) return false;
        if (stat.tickRate <= 0) return false;
        return true;
    }

    private bool ValidatePassiveStats(SkillStatData stat)
    {
        if (stat.effectDuration < 0) return false;
        if (stat.cooldown < 0) return false;
        if (stat.triggerChance < 0 || stat.triggerChance > 100) return false;
        if (stat.damageIncrease < 0) return false;
        if (stat.defenseIncrease < 0) return false;
        if (stat.expAreaIncrease < 0) return false;
        if (stat.hpIncrease < 0) return false;
        if (stat.moveSpeedIncrease < 0) return false;
        if (stat.attackSpeedIncrease < 0) return false;
        if (stat.attackRangeIncrease < 0) return false;
        if (stat.hpRegenIncrease < 0) return false;
        return true;
    }

    public void SavePassiveSkillStats(string directory)
    {
        string path = Path.Combine(directory, "PassiveSkillStats.csv");
        StringBuilder csv = new StringBuilder();

        csv.AppendLine("skillid,level,damage,maxskilllevel,element,elementalpower," +
                      "effectduration,cooldown,triggerchance,damageincrease,defenseincrease," +
                      "expareaincrease,homingactivate,hpincrease," +
                      "movespeedincrease,attackspeedincrease,attackrangeincrease,hpregenincrease");

        foreach (var skillStats in skillStatsList.Values)
        {
            foreach (var stat in skillStats)
            {
                if (GetSkillType(stat.skillID) == SkillType.Passive)
                {
                    csv.AppendLine($"{stat.skillID},{stat.level},{stat.damage},{stat.maxSkillLevel}," +
                                 $"{stat.element},{stat.elementalPower},{stat.effectDuration}," +
                                 $"{stat.cooldown},{stat.triggerChance},{stat.damageIncrease}," +
                                 $"{stat.defenseIncrease},{stat.expAreaIncrease},{stat.homingActivate}," +
                                 $"{stat.hpIncrease},{stat.moveSpeedIncrease},{stat.attackSpeedIncrease}," +
                                 $"{stat.attackRangeIncrease},{stat.hpRegenIncrease}");
                }
            }
        }

        File.WriteAllText(path, csv.ToString());
        Debug.Log($"Saved passive skill stats to: {path}");
    }

    private void LoadSkillStatsFromCSV(string fileName)
    {
        TextAsset csvFile = Resources.Load<TextAsset>(fileName);
        if (csvFile != null)
        {
            string[] lines = csvFile.text.Split('\n');
            string[] headers = lines[0].Trim().Split(',');

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string[] values = line.Split(',');
                SkillStatData statData = ParseSkillStatLine(headers, values);

                if (statData.skillID == SkillID.None)
                {
                    Debug.LogWarning($"Invalid SkillID in {fileName} at line {i}");
                    continue;
                }

                if (!skillStatsList.ContainsKey(statData.skillID))
                {
                    skillStatsList[statData.skillID] = new List<SkillStatData>();
                }

                var existingStat = skillStatsList[statData.skillID].Find(s => s.level == statData.level);
                if (existingStat != null)
                {
                    int index = skillStatsList[statData.skillID].IndexOf(existingStat);
                    skillStatsList[statData.skillID][index] = statData;
                }
                else
                {
                    skillStatsList[statData.skillID].Add(statData);
                }

                Debug.Log($"Loaded stats for skill {statData.skillID} level {statData.level}");
            }
        }
        else
        {
            Debug.LogWarning($"Could not find CSV file: {fileName}");
        }
    }

    public bool ValidateSkillData(SkillData skillData)
    {
        if (skillData == null || skillData.metadata == null)
        {
            Debug.LogError("Invalid skill data: null reference");
            return false;
        }

        var stats = GetSkillStatsForLevel(skillData.metadata.ID, 1, skillData.metadata.Type);
        if (stats == null)
        {
            Debug.LogError($"No stats found for skill {skillData.metadata.Name}");
            return false;
        }

        if (skillData.metadata.Type == SkillType.Passive)
        {
            var passiveStats = stats as PassiveSkillStat;
            if (passiveStats == null)
            {
                Debug.LogError($"Invalid passive stats for skill {skillData.metadata.Name}");
                return false;
            }
        }

        return true;
    }

    private SkillType GetSkillType(SkillID skillID)
    {
        var skillData = skillDatas.Find(x => x.metadata.ID == skillID);
        return skillData?.metadata.Type ?? SkillType.None;
    }
}
