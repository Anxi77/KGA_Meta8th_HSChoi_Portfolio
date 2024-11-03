using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using Unity.VisualScripting;
using System.Text;

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
        if (csvFile == null)
        {
            Debug.LogError($"CSV file is null: {fileName}");
            return;
        }

        try
        {
            string[] lines = csvFile.text.Split('\n');
            if (lines.Length <= 1)
            {
                Debug.LogError($"CSV file is empty or contains only headers: {fileName}");
                return;
            }

            string[] headers = lines[0].Trim().Split(',');
            Debug.Log($"Processing {fileName} with headers: {string.Join(", ", headers)}");

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string[] values = line.Split(',');
                if (values.Length != headers.Length)
                {
                    Debug.LogWarning($"Line {i} has incorrect number of values. Expected {headers.Length}, got {values.Length}");
                    continue;
                }

                SkillStatData statData = ParseSkillStatLine(headers, values);
                if (statData == null || statData.skillID == SkillID.None)
                {
                    Debug.LogError($"Failed to parse line {i}: {line}");
                    continue;
                }

                // 스킬 타입 확인
                var skillType = GetSkillType(statData.skillID);
                if (skillType == SkillType.Passive)
                {
                    Debug.Log($"Loading passive skill stats from CSV - ID: {statData.skillID}, Level: {statData.level}\n" +
                             $"moveSpeed: {statData.moveSpeedIncrease}\n" +
                             $"attackSpeed: {statData.attackSpeedIncrease}\n" +
                             $"attackRange: {statData.attackRangeIncrease}\n" +
                             $"hpRegen: {statData.hpRegenIncrease}");
                }

                // Dictionary에 저장
                if (!tempStats.ContainsKey(statData.skillID))
                {
                    tempStats[statData.skillID] = new Dictionary<int, SkillStatData>();
                }

                tempStats[statData.skillID][statData.level] = statData;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error processing CSV file {fileName}: {e.Message}");
        }
    }

    private SkillStatData ParseSkillStatLine(string[] headers, string[] values)
    {
        SkillStatData statData = new SkillStatData();

        try
        {
            for (int i = 0; i < headers.Length && i < values.Length; i++)
            {
                string header = headers[i].Trim().ToLower();
                string value = values[i].Trim();

                if (string.IsNullOrEmpty(value)) continue;

                switch (header)
                {
                    case "skillid":
                        if (System.Enum.TryParse<SkillID>(value, true, out SkillID skillID))
                        {
                            statData.skillID = skillID;
                        }
                        else
                        {
                            Debug.LogError($"Invalid SkillID value: {value}. Must match SkillID enum values.");
                            return null;
                        }
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
                        if (System.Enum.TryParse<ElementType>(value, true, out ElementType element))
                        {
                            statData.element = element;
                        }
                        else
                        {
                            Debug.LogError($"Invalid ElementType value: {value}. Must match ElementType enum values.");
                            return null;
                        }
                        break;
                    case "elementalpower":
                        if (float.TryParse(value, out float elementalPower))
                            statData.elementalPower = elementalPower;
                        break;

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
                    case "move speed increase":
                        if (float.TryParse(value, out float moveSpeedInc))
                        {
                            statData.moveSpeedIncrease = moveSpeedInc;
                            Debug.Log($"Parsed moveSpeedIncrease: {moveSpeedInc} for skill {statData.skillID}");
                        }
                        break;

                    case "attackspeedincrease":
                    case "attack speed increase":
                        if (float.TryParse(value, out float attackSpeedInc))
                        {
                            statData.attackSpeedIncrease = attackSpeedInc;
                            Debug.Log($"Parsed attackSpeedIncrease: {attackSpeedInc} for skill {statData.skillID}");
                        }
                        break;

                    case "attackrangeincrease":
                    case "attack range increase":
                        if (float.TryParse(value, out float attackRangeInc))
                        {
                            statData.attackRangeIncrease = attackRangeInc;
                            Debug.Log($"Parsed attackRangeIncrease: {attackRangeInc} for skill {statData.skillID}");
                        }
                        break;

                    case "hpregenincrease":
                    case "hp regen increase":
                        if (float.TryParse(value, out float hpRegenInc))
                        {
                            statData.hpRegenIncrease = hpRegenInc;
                            Debug.Log($"Parsed hpRegenIncrease: {hpRegenInc} for skill {statData.skillID}");
                        }
                        break;
                }
            }

            // 패시브 스킬인 경우 값이 제대로 설정되었는지 확인
            if (GetSkillType(statData.skillID) == SkillType.Passive)
            {
                Debug.Log($"Final passive skill stats - ID: {statData.skillID}, Level: {statData.level}\n" +
                         $"moveSpeed: {statData.moveSpeedIncrease}\n" +
                         $"attackSpeed: {statData.attackSpeedIncrease}\n" +
                         $"attackRange: {statData.attackRangeIncrease}\n" +
                         $"hpRegen: {statData.hpRegenIncrease}");
            }

            return statData;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error parsing skill stat line: {e.Message}");
            return null;
        }
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
        if (editorStats == null)
        {
            Debug.LogError("Editor stats is null");
            return;
        }

        try
        {
            // skillStatsByLevel이 null이면 초기화
            if (skillStatsByLevel == null)
            {
                skillStatsByLevel = new Dictionary<SkillID, Dictionary<int, SkillStatData>>();
            }

            // 새로운 Dictionary 생성
            var newStatsByLevel = new Dictionary<SkillID, Dictionary<int, SkillStatData>>();

            foreach (var pair in editorStats)
            {
                if (pair.Key == SkillID.None || pair.Value == null)
                {
                    Debug.LogWarning($"Skipping invalid stats entry: SkillID={pair.Key}");
                    continue;
                }

                newStatsByLevel[pair.Key] = new Dictionary<int, SkillStatData>();

                foreach (var stat in pair.Value)
                {
                    if (stat == null)
                    {
                        Debug.LogWarning($"Skipping null stat for SkillID: {pair.Key}");
                        continue;
                    }

                    // stat의 skillID가 None이면 pair.Key로 설정
                    if (stat.skillID == SkillID.None)
                    {
                        stat.skillID = pair.Key;
                    }

                    try
                    {
                        if (ValidateSkillStat(stat))
                        {
                            newStatsByLevel[pair.Key][stat.level] = stat;

                            if (GetSkillType(stat.skillID) == SkillType.Passive)
                            {
                                Debug.Log($"Updating passive skill stats - ID: {stat.skillID}, Level: {stat.level}\n" +
                                        $"moveSpeed: {stat.moveSpeedIncrease}\n" +
                                        $"attackSpeed: {stat.attackSpeedIncrease}\n" +
                                        $"attackRange: {stat.attackRangeIncrease}\n" +
                                        $"hpRegen: {stat.hpRegenIncrease}");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"Failed to validate stat for SkillID: {stat.skillID}, Level: {stat.level}");
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Error processing stat for SkillID: {stat.skillID}, Level: {stat.level}\nError: {e.Message}");
                    }
                }
            }

            // 기존 데이터 백업
            var backupStats = new Dictionary<SkillID, Dictionary<int, SkillStatData>>(skillStatsByLevel);

            try
            {
                skillStatsByLevel = newStatsByLevel;
                Debug.Log($"Successfully updated skill stats data with {newStatsByLevel.Count} skills");
            }
            catch (System.Exception e)
            {
                skillStatsByLevel = backupStats;
                throw new System.Exception($"Failed to update skillStatsByLevel: {e.Message}", e);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to update skill stats: {e.Message}\nStack trace: {e.StackTrace}");
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

        // 헤더 작성 - 정확한 헤더 이름 사용
        csv.AppendLine("skillid,level,damage,maxskilllevel,element,elementalpower," +
                      "effectduration,cooldown,triggerchance,damageincrease,defenseincrease," +
                      "expareaincrease,homingactivate,hpincrease,movespeedincrease," +
                      "attackspeedincrease,attackrangeincrease,hpregenincrease");

        foreach (var skillStats in skillStatsList.Values)
        {
            foreach (var stat in skillStats)
            {
                var skill = skillDatas.Find(s => s.metadata.ID == stat.skillID);
                if (skill?.metadata.Type == SkillType.Passive)
                {
                    string line = $"{stat.skillID},{stat.level},{stat.damage},{stat.maxSkillLevel}," +
                                 $"{stat.element},{stat.elementalPower},{stat.effectDuration}," +
                                 $"{stat.cooldown},{stat.triggerChance},{stat.damageIncrease}," +
                                 $"{stat.defenseIncrease},{stat.expAreaIncrease},{stat.homingActivate}," +
                                 $"{stat.hpIncrease},{stat.moveSpeedIncrease},{stat.attackSpeedIncrease}," +
                                 $"{stat.attackRangeIncrease},{stat.hpRegenIncrease}";

                    csv.AppendLine(line);
                    Debug.Log($"Saving passive skill stats: {line}");
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

                if (statData != null)
                {
                    if (!skillStatsList.ContainsKey(statData.skillID))
                    {
                        skillStatsList[statData.skillID] = new List<SkillStatData>();
                    }

                    // 기존 레벨 스탯이 있다면 업데이트, 없다면 추가
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

                    // 패시브 스킬인 경우 추가 로그
                    var skill = skillDatas.Find(s => s.metadata.ID == statData.skillID);
                    if (skill?.metadata.Type == SkillType.Passive)
                    {
                        Debug.Log($"Loaded passive skill stats for {statData.skillID} level {statData.level}:\n" +
                                $"moveSpeed: {statData.moveSpeedIncrease}\n" +
                                $"attackSpeed: {statData.attackSpeedIncrease}\n" +
                                $"attackRange: {statData.attackRangeIncrease}\n" +
                                $"hpRegen: {statData.hpRegenIncrease}");
                    }
                }
            }
        }
    }

    // 스킬 데이터 검증 메서드 추가
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
