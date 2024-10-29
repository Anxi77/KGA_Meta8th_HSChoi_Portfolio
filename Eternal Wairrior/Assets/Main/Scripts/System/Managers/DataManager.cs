using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using static Skill;

public class DataManager : SingletonManager<DataManager>
{
    public bool clearPrefsOnStart;

    IEnumerator Start()
    {
        if (clearPrefsOnStart) PlayerPrefs.DeleteAll();
        yield return null;
        OnLoad();
    }

    //Load
    public void OnLoad()
    {
        int totalKillCount = PlayerPrefs.GetInt("TotalKillCount", 0);

        GameManager.Instance.player.totalKillCount = totalKillCount;
    }

    //save
    public void OnSave()
    {
        int totalKillCount = GameManager.Instance.player.totalKillCount;

        PlayerPrefs.SetInt("TotalKillCount", totalKillCount);

        PlayerPrefs.Save();

    }

    private void OnApplicationQuit()
    {
        OnSave();
    }

}

#region DataManager CSV

//using UnityEngine;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;

//public class DataManager : MonoBehaviour
//{
//    private static DataManager instance;
//    public static DataManager Instance
//    {
//        get
//        {
//            if (instance == null)
//            {
//                GameObject go = new GameObject("DataManager");
//                instance = go.AddComponent<DataManager>();
//                DontDestroyOnLoad(go);
//            }
//            return instance;
//        }
//    }

//    private Dictionary<SkillID, Dictionary<string, List<float>>> skillDataByID;
//    private readonly string SKILL_DATA_FILE = "AllSkillData.csv";
//    private readonly List<string> STAT_TYPES = new List<string>
//    {
//        "SkillID", "damage", "projectileSpeed", "projectileScale",
//        "projectileCount", "pierceCount", "attackRange",
//        "innerInterval", "homingRange", "explosionRad"
//    };

//    private void Awake()
//    {
//        InitializeSingleton();
//        InitializeDataStructures();
//        LoadAllSkillData();
//    }

//    private void InitializeSingleton()
//    {
//        if (instance != null && instance != this)
//        {
//            Destroy(gameObject);
//            return;
//        }
//        instance = this;
//        DontDestroyOnLoad(gameObject);
//    }

//    private void InitializeDataStructures()
//    {
//        skillDataByID = new Dictionary<SkillID, Dictionary<string, List<float>>>();
//    }

//    public void SaveSkillData(Skill skill)
//    {
//        UpdateSkillDataDictionary(skill);
//        SaveAllSkillData();
//    }

//    private void UpdateSkillDataDictionary(Skill skill)
//    {
//        var statsData = CreateStatsDataDictionary(skill);
//        skillDataByID[skill._SkillID] = statsData;
//    }

//    private Dictionary<string, List<float>> CreateStatsDataDictionary(Skill skill)
//    {
//        return new Dictionary<string, List<float>>
//        {
//            {"damage", skill.damageByLevel},
//            {"projectileSpeed", skill.projectileSpeedByLevel},
//            {"projectileScale", skill.projectileScaleByLevel},
//            {"projectileCount", skill.projectileCountByLevel.Select(x => (float)x).ToList()},
//            {"pierceCount", skill.pierceCountByLevel.Select(x => (float)x).ToList()},
//            {"attackRange", skill.attackRangeByLevel},
//            {"innerInterval", skill.innerIntervalByLevel},
//            {"homingRange", skill.homingRangeByLevel},
//            {"explosionRad", skill.explosionRadByLevel}
//        };
//    }

//    private void SaveAllSkillData()
//    {
//        string filePath = GetSkillDataFilePath();
//        string csvContent = GenerateCSVContent();
//        File.WriteAllText(filePath, csvContent);
//    }

//    private string GetSkillDataFilePath()
//    {
//        string directory = Path.Combine(Application.dataPath, "SkillData");
//        Directory.CreateDirectory(directory);
//        return Path.Combine(directory, SKILL_DATA_FILE);
//    }

//    private string GenerateCSVContent()
//    {
//        StringBuilder sb = new StringBuilder();
//        WriteHeader(sb);
//        WriteSkillData(sb);
//        return sb.ToString();
//    }

//    private void WriteHeader(StringBuilder sb)
//    {
//        sb.AppendLine(string.Join(",", STAT_TYPES));
//    }

//    private void WriteSkillData(StringBuilder sb)
//    {
//        foreach (var skillEntry in skillDataByID)
//        {
//            WriteSkillLevelData(sb, skillEntry.Key, skillEntry.Value);
//        }
//    }

//    private void WriteSkillLevelData(StringBuilder sb, SkillID skillID, Dictionary<string, List<float>> statsData)
//    {
//        int maxLevel = GetMaxLevel(statsData);
//        for (int level = 0; level < maxLevel; level++)
//        {
//            WriteSkillLevelRow(sb, skillID, statsData, level);
//        }
//    }

//    private int GetMaxLevel(Dictionary<string, List<float>> statsData)
//    {
//        return statsData.Values.Max(list => list.Count);
//    }

//    private void WriteSkillLevelRow(StringBuilder sb, SkillID skillID, Dictionary<string, List<float>> statsData, int level)
//    {
//        List<string> row = new List<string> { skillID.ToString() };
//        foreach (var statType in STAT_TYPES.Skip(1))
//        {
//            string value = GetStatValue(statsData, statType, level);
//            row.Add(value);
//        }
//        sb.AppendLine(string.Join(",", row));
//    }

//    private string GetStatValue(Dictionary<string, List<float>> statsData, string statType, int level)
//    {
//        if (statsData.ContainsKey(statType) && level < statsData[statType].Count)
//        {
//            return statsData[statType][level].ToString();
//        }
//        return "";
//    }

//    public Dictionary<string, List<float>> LoadSkillData(SkillID skillID)
//    {
//        return skillDataByID.ContainsKey(skillID) ? skillDataByID[skillID] : null;
//    }

//    private void LoadAllSkillData()
//    {
//        string filePath = GetSkillDataFilePath();
//        if (!File.Exists(filePath))
//        {
//            Debug.Log("스킬 데이터 파일이 없습니다. 새로 생성됩니다.");
//            return;
//        }

//        string[] lines = File.ReadAllLines(filePath);
//        if (lines.Length == 0) return;

//        string[] headers = lines[0].Split(',');
//        ProcessSkillDataLines(lines.Skip(1).ToArray(), headers);
//    }

//    private void ProcessSkillDataLines(string[] dataLines, string[] headers)
//    {
//        skillDataByID.Clear();
//        foreach (string line in dataLines)
//        {
//            ProcessSkillDataLine(line, headers);
//        }
//    }

//    private void ProcessSkillDataLine(string line, string[] headers)
//    {
//        string[] values = line.Split(',');
//        if (values.Length < 2) return;

//        if (System.Enum.TryParse(values[0], out SkillID skillID))
//        {
//            InitializeSkillDataIfNeeded(skillID, headers);
//            AddSkillLevelData(skillID, values, headers);
//        }
//    }

//    private void InitializeSkillDataIfNeeded(SkillID skillID, string[] headers)
//    {
//        if (!skillDataByID.ContainsKey(skillID))
//        {
//            skillDataByID[skillID] = new Dictionary<string, List<float>>();
//            foreach (string header in headers.Skip(1))
//            {
//                skillDataByID[skillID][header] = new List<float>();
//            }
//        }
//    }

//    private void AddSkillLevelData(SkillID skillID, string[] values, string[] headers)
//    {
//        for (int i = 1; i < headers.Length && i < values.Length; i++)
//        {
//            if (!string.IsNullOrEmpty(values[i]) && float.TryParse(values[i], out float value))
//            {
//                skillDataByID[skillID][headers[i]].Add(value);
//            }
//        }
//    }
//}


#endregion
