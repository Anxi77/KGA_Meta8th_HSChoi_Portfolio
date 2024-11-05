using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using System.Linq;

public class SkillDataManager : DataManager
{
    private static SkillDataManager instance;
    public static SkillDataManager Instance => instance;

    private new ResourceManager<GameObject> prefabManager;
    private new ResourceManager<Sprite> iconManager;
    private new CSVManager<SkillStatData> statManager;
    private new JSONManager<SkillData> jsonManager;
    private new BackupManager backupManager;
    private new DataValidator dataValidator;

    private Dictionary<SkillID, SkillData> skillDatabase = new Dictionary<SkillID, SkillData>();
    private Dictionary<SkillID, Dictionary<int, SkillStatData>> statDatabase = new Dictionary<SkillID, Dictionary<int, SkillStatData>>();
    private Dictionary<SkillID, Dictionary<int, GameObject>> levelPrefabDatabase = new Dictionary<SkillID, Dictionary<int, GameObject>>();

    private SkillEditorDataContainer editorData;

    private new bool isInitialized = false;

    protected override void Awake()
    {
        if (!Application.isEditor)
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // 에디터 데이터 로드
        editorData = AssetDatabase.LoadAssetAtPath<SkillEditorDataContainer>(
            "Assets/Resources/SkillEditorData.asset"
        );

        if (editorData == null)
        {
            Debug.LogError("Could not load SkillEditorData asset");
        }

        InitializeDefaultData();
    }

    public override void SaveWithBackup()
    {
        if (TryExecute(() =>
        {
            base.SaveWithBackup();
            // 추가적인 스킬 데이터 저장 로직
        }, "SaveWithBackup"))
        {
            Debug.Log("Successfully saved skill data with backup");
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

            // JSON 데이터 저장 (수정된 부분)
            jsonManager?.SaveData($"{id}_Data", skillData);

            // 리소스 저장
            if (skillData.icon != null)
            {
                iconManager?.SaveData($"{id}_Icon", skillData.icon);
            }

            if (skillData.metadata.Prefab != null)
            {
                prefabManager?.SaveData($"{id}_Metadata", skillData.metadata.Prefab);
            }

            if (skillData.metadata.Type == SkillType.Projectile && skillData.projectile != null)
            {
                prefabManager?.SaveData($"{id}_Projectile", skillData.projectile);
            }

            // 레벨별 프리팹 저장
            if (skillData.prefabsByLevel != null)
            {
                for (int i = 0; i < skillData.prefabsByLevel.Length; i++)
                {
                    if (skillData.prefabsByLevel[i] != null)
                    {
                        prefabManager?.SaveData($"{id}_Level_{i + 1}", skillData.prefabsByLevel[i]);
                    }
                }
            }

            Debug.Log($"Successfully saved skill data for {skillData.metadata.Name}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving skill data for {skillData.metadata?.Name}: {e.Message}\n{e.StackTrace}");
        }
    }

    // 스킬 스탯 관리
    public void SaveSkillStats(SkillID id, int level, SkillStatData statData)
    {
        if (statData == null)
        {
            Debug.LogError($"Cannot save null stat data for skill {id} level {level}");
            return;
        }

        try
        {
            // 스탯 데이터베이스 초기화 확인
            if (statDatabase == null)
            {
                statDatabase = new Dictionary<SkillID, Dictionary<int, SkillStatData>>();
            }

            if (!statDatabase.ContainsKey(id))
            {
                statDatabase[id] = new Dictionary<int, SkillStatData>();
            }

            statDatabase[id][level] = statData;

            // CSV로 저장
            var allStats = new List<SkillStatData>();
            foreach (var skillStats in statDatabase.Values)
            {
                allStats.AddRange(skillStats.Values);
            }

            statManager?.SaveBulkData("SkillStats", allStats);

            Debug.Log($"Successfully saved stats for skill {id} level {level}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving skill stats for {id} level {level}: {e.Message}\n{e.StackTrace}");
        }
    }

    public void LoadSkillStats()
    {
        var stats = statManager.LoadBulkData("SkillStats").ToList();
        foreach (var stat in stats)
        {
            if (!statDatabase.ContainsKey(stat.skillID))
                statDatabase[stat.skillID] = new Dictionary<int, SkillStatData>();

            statDatabase[stat.skillID][stat.level] = stat;
        }
    }

    // 런타임 데이터 접근
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

    public SkillStatData GetSkillStats(SkillID id, int level)
    {
        if (statDatabase.TryGetValue(id, out var levelStats) &&
            levelStats.TryGetValue(level, out var statData))
        {
            return statData;
        }
        return null;
    }

    public GameObject GetLevelPrefab(SkillID id, int level)
    {
        if (levelPrefabDatabase.TryGetValue(id, out var levelPrefabs) &&
            levelPrefabs.TryGetValue(level, out var prefab))
        {
            return prefab;
        }
        return null;
    }

    // 데이터 초기화
    public override void ClearAllData()
    {
        base.ClearAllData();
        skillDatabase.Clear();
        statDatabase.Clear();
        levelPrefabDatabase.Clear();
    }

    // 기존 SkillManager와의 호환을 위한 메서드들
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

    // ���임 스킬 업그레이들을 위한 메서드
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

    // 기존 SkillManager의 AddOrUpgradeSkill 지원
    public bool ValidateSkillData(SkillData skillData)
    {
        return dataValidator.ValidateSkillData(skillData);
    }

    public void UpdateSingleSkill(SkillData skillData)
    {
        if (skillData?.metadata == null) return;
        SaveSkillData(skillData);
    }

    public List<SkillData> GetRandomSkills(int count = 3, ElementType? elementType = null)
    {
        if (skillDatabase == null || skillDatabase.Count == 0)
        {
            Debug.LogError("No skills available in SkillDataManager");
            return new List<SkillData>();
        }

        var selectedSkills = new List<SkillData>();
        var filteredSkills = skillDatabase.Values.Where(skill =>
        {
            // 스킬의 레벨 1 스탯 데이터 확인
            var stats = GetSkillStats(skill.metadata.ID, 1);
            return stats != null && (elementType == null || skill.metadata.Element == elementType);
        }).ToList();

        if (!filteredSkills.Any())
            return selectedSkills;

        if (elementType == null)
        {
            var availableElements = filteredSkills
                .Select(s => s.metadata.Element)
                .Distinct()
                .ToList();

            elementType = availableElements[Random.Range(0, availableElements.Count)];
            filteredSkills = filteredSkills.Where(s => s.metadata.Element == elementType).ToList();
        }

        while (selectedSkills.Count < count && filteredSkills.Any())
        {
            int index = Random.Range(0, filteredSkills.Count);
            selectedSkills.Add(filteredSkills[index]);
            filteredSkills.RemoveAt(index);
        }

        return selectedSkills;
    }

    public void InitializeDefaultData()
    {
        if (isInitialized) return;

        try
        {
            Debug.Log("Starting to initialize default data structure...");

            // 기본 데이터베이스 초기화
            if (skillDatabase == null)
                skillDatabase = new Dictionary<SkillID, SkillData>();

            if (statDatabase == null)
                statDatabase = new Dictionary<SkillID, Dictionary<int, SkillStatData>>();

            // 매니저 초기화
            prefabManager = new ResourceManager<GameObject>("SkillData/Prefabs");
            iconManager = new ResourceManager<Sprite>("SkillData/Icons");
            statManager = new CSVManager<SkillStatData>("SkillData/Stats");
            jsonManager = new JSONManager<SkillData>("SkillData/Json");
            backupManager = new BackupManager();
            dataValidator = new DataValidator();

            // 디렉토리 생성
            CreateDirectories();

            // 기본 CSV 파일 생성
            CreateDefaultCSVFiles();

            AssetDatabase.Refresh();
            Debug.Log("Successfully initialized default skill data structure");

            isInitialized = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error initializing default data: {e.Message}\n{e.StackTrace}");
            isInitialized = false;
        }
    }

    private void CreateDirectories()
    {
        string resourcesPath = Path.Combine(Application.dataPath, "Resources");
        string[] subFolders = new string[]
        {
            "SkillData",
            "SkillData/Prefabs",
            "SkillData/Icons",
            "SkillData/Stats",
            "SkillData/Json"
        };

        foreach (string subFolder in subFolders)
        {
            string fullPath = Path.Combine(resourcesPath, subFolder);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                Debug.Log($"Created directory: {fullPath}");
            }
        }
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

    public void UpdateSkillList(List<SkillData> newSkillList)
    {
        try
        {
            if (newSkillList == null)
            {
                Debug.LogError("Cannot update with null skill list");
                return;
            }

            // 스킬 데이터베이스 초기화
            if (skillDatabase == null)
            {
                skillDatabase = new Dictionary<SkillID, SkillData>();
            }

            // 기존 데이터 클리어
            skillDatabase.Clear();

            // 새 데이터 추가
            foreach (var skill in newSkillList)
            {
                if (skill?.metadata == null)
                {
                    Debug.LogWarning("Skipping invalid skill data");
                    continue;
                }

                skillDatabase[skill.metadata.ID] = skill;
                Debug.Log($"Added skill to database: {skill.metadata.Name}");
            }

            // 변경사항 저장
            SaveAllSkillData();

            Debug.Log($"Successfully updated skill list with {skillDatabase.Count} skills");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error updating skill list: {e.Message}\n{e.StackTrace}");
        }
    }

    public void SaveAllSkillData()
    {
        try
        {
            if (skillDatabase == null || skillDatabase.Count == 0)
            {
                Debug.LogError("No skills to save in the database");
                return;
            }

            // 스킬 타입별 스탯 데이터 수집
            var projectileStats = new List<SkillStatData>();
            var areaStats = new List<SkillStatData>();
            var passiveStats = new List<SkillStatData>();

            foreach (var skillData in skillDatabase.Values)
            {
                if (skillData?.metadata == null)
                {
                    Debug.LogWarning($"Skipping invalid skill data");
                    continue;
                }

                // JSON 데이터 저장 (수정된 부분)
                jsonManager?.SaveData($"{skillData.metadata.ID}_Data", skillData);

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

                // 나머지 코드는 동일...
            }

            // 나머지 코드는 동일...
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving all skill data: {e.Message}\n{e.StackTrace}");
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

                // JSON 데이터 저장
                jsonManager?.SaveData($"{skillData.metadata.ID}_Data", skillData);
                Debug.Log($"Saved JSON data for skill: {skillData.metadata.Name}");

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

                // 프로젝타일 프리팹 저장
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

            // 스킬 스탯 데이터 저장
            var projectileStats = new List<SkillStatData>();
            var areaStats = new List<SkillStatData>();
            var passiveStats = new List<SkillStatData>();

            foreach (var skillStatEntry in skillStats)
            {
                var skillData = skillDataList.Find(s => s.metadata.ID == skillStatEntry.skillID);
                if (skillData == null) continue;

                foreach (var stat in skillStatEntry.levelStats)
                {
                    switch (skillData.metadata.Type)
                    {
                        case SkillType.Projectile:
                            projectileStats.Add(stat);
                            break;
                        case SkillType.Area:
                            areaStats.Add(stat);
                            break;
                        case SkillType.Passive:
                            passiveStats.Add(stat);
                            break;
                    }
                }
            }

            // CSV 파일로 스탯 저장
            if (projectileStats.Count > 0)
                statManager?.SaveBulkData("ProjectileSkillStats", projectileStats);
            if (areaStats.Count > 0)
                statManager?.SaveBulkData("AreaSkillStats", areaStats);
            if (passiveStats.Count > 0)
                statManager?.SaveBulkData("PassiveSkillStats", passiveStats);

            Debug.Log("Successfully saved all skill data");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving all skill data: {e.Message}\n{e.StackTrace}");
        }
    }

    // LoadSkillData 메서드 수정
    public SkillData LoadSkillData(SkillID id)
    {
        if (skillDatabase.TryGetValue(id, out var data))
            return data;

        try
        {
            // JSON에서 데이터 로드 (수정된 부분)
            var skillData = jsonManager?.LoadData($"{id}_Data");
            if (skillData != null)
            {
                // 리소스 로드
                skillData.icon = iconManager?.LoadData($"{id}_Icon");
                skillData.metadata.Prefab = prefabManager?.LoadData($"{id}_Prefab");

                if (skillData.metadata.Type == SkillType.Projectile)
                {
                    skillData.projectile = prefabManager?.LoadData($"{id}_Projectile");
                }

                skillDatabase[id] = skillData;
                return skillData;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading skill data for {id}: {e.Message}\n{e.StackTrace}");
        }

        return null;
    }
}