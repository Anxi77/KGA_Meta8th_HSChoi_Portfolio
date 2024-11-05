using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using Unity.VisualScripting;
using System.Text;
using System.Linq;
using System.Collections;

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
    private const string PREFAB_PATH = "SkillData/Prefabs";
    private const string ICON_PATH = "SkillData/Icons";
    private const string SKILL_DATA_FILENAME = "SkillData.json";

    private Dictionary<SkillID, Dictionary<int, SkillStatData>> skillStatsByLevel = new Dictionary<SkillID, Dictionary<int, SkillStatData>>();
    private Dictionary<SkillID, List<SkillStatData>> skillStatsList = new Dictionary<SkillID, List<SkillStatData>>();

    private bool isInitialized = false;
    public bool IsInitialized => isInitialized;

    private Dictionary<(SkillID, int), ISkillStat> statsCache = new Dictionary<(SkillID, int), ISkillStat>();

    private float lastUpdateTime = 0f;
    private const float UPDATE_INTERVAL = 0.5f; // 업데이트 간격 (초)

    protected override void Awake()
    {
        Debug.Log($"SkillDataManager Awake - Current instance: {instance}, This: {this}");

        if (instance != null && instance != this)
        {
            Debug.Log("Destroying duplicate SkillDataManager");
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // 기본 초기화
        if (skillStatsByLevel == null)
        {
            skillStatsByLevel = new Dictionary<SkillID, Dictionary<int, SkillStatData>>();
        }
        if (skillDatas == null)
        {
            skillDatas = new List<SkillData>();
        }

        base.Awake();
        StartCoroutine(Initialize());

        Debug.Log("SkillDataManager initialization started");
    }

    private IEnumerator Initialize()
    {
        Debug.Log("Starting SkillDataManager initialization...");

        // 1. 리소스 폴더 경로 확인
        string resourcePath = Path.Combine(Application.dataPath, "Resources", RESOURCE_PATH);
        bool hasExistingData = false;

        // 2. JSON 파일 존재 여부 확인
        string jsonPath = Path.Combine(resourcePath, SKILL_DATA_FILENAME);
        if (File.Exists(jsonPath))
        {
            string jsonContent = File.ReadAllText(jsonPath);
            if (!string.IsNullOrEmpty(jsonContent))
            {
                var wrapper = JsonUtility.FromJson<SkillDataWrapper>(jsonContent);
                if (wrapper != null && wrapper.skillDatas != null && wrapper.skillDatas.Count > 0)
                {
                    skillDatas = wrapper.skillDatas;
                    hasExistingData = true;
                    Debug.Log($"Loaded {skillDatas.Count} skills from existing JSON data");
                }
            }
        }

        // 3. CSV 파일 존재 여부 확인 및 로드
        string[] csvFiles = {
            Path.Combine(resourcePath, "ProjectileSkillStats.csv"),
            Path.Combine(resourcePath, "AreaSkillStats.csv"),
            Path.Combine(resourcePath, "PassiveSkillStats.csv")
        };

        if (csvFiles.Any(File.Exists))
        {
            LoadSkillStatsFromCSV();
            hasExistingData = true;
            Debug.Log($"Loaded skill stats from existing CSV files");
        }

        // 4. 기존 데이터가 없는 경우에만 초기화
        if (!hasExistingData)
        {
            Debug.Log("No existing data found. Creating new skill data...");
            CreateDefaultSkillData();
            
            // 새로 생성된 스킬들의 리소스와 스탯 초기화
            foreach (var skill in skillDatas)
            {
                if (skill != null && skill.metadata != null)
                {
                    yield return StartCoroutine(InitializeSkill(skill));
                }
            }
        }
        else
        {
            // 기존 데이터가 있는 경우는 리소스만 로드
            foreach (var skill in skillDatas)
            {
                if (skill != null && skill.metadata != null)
                {
                    yield return StartCoroutine(LoadSkillResources(skill));
                }
            }
        }

        // 5. 초기화 완료 체크
        isInitialized = ValidateInitialization();
        Debug.Log($"SkillDataManager initialization completed: {isInitialized}");
    }

    private void CreateDefaultSkillData()
    {
        Debug.Log("Creating default skill data...");
        skillDatas = new List<SkillData>();
        skillStatsByLevel = new Dictionary<SkillID, Dictionary<int, SkillStatData>>();

        // Earth Element Skills
        CreateSkill(
            SkillID.Vine,
            "Vine Trap",
            "Creates a field of vines that can bind enemies",
            SkillType.Area,
            ElementType.Earth
        );

        CreateSkill(
            SkillID.EarthRift,
            "Earth Rift",
            "Launches earth shards that pierce through enemies",
            SkillType.Projectile,
            ElementType.Earth
        );

        CreateSkill(
            SkillID.GaiasGrace,
            "Gaia's Grace",
            "Increases defense and HP regeneration",
            SkillType.Passive,
            ElementType.Earth
        );

        // Water Element Skills
        CreateSkill(
            SkillID.ForstTide,
            "Frost Tide",
            "Creates a freezing field that slows enemies",
            SkillType.Area,
            ElementType.Water
        );

        CreateSkill(
            SkillID.FrostHunt,
            "Frost Hunt",
            "Launches ice projectiles that track enemies",
            SkillType.Projectile,
            ElementType.Water
        );

        CreateSkill(
            SkillID.TidalEssence,
            "Tidal Essence",
            "Increases movement speed and attack range",
            SkillType.Passive,
            ElementType.Water
        );

        // Dark Element Skills
        CreateSkill(
            SkillID.ShadowWaltz,
            "Shadow Waltz",
            "Creates a field of darkness that damages enemies",
            SkillType.Area,
            ElementType.Dark
        );

        CreateSkill(
            SkillID.EventHorizon,
            "Event Horizon",
            "Launches dark orbs that pull enemies in",
            SkillType.Projectile,
            ElementType.Dark
        );

        CreateSkill(
            SkillID.AbyssalExpansion,
            "Abyssal Expansion",
            "Increases damage and experience gain area",
            SkillType.Passive,
            ElementType.Dark
        );

        // Fire Element Skills
        CreateSkill(
            SkillID.Flame,
            "Flame Shot",
            "Launches fireballs that explode on impact",
            SkillType.Projectile,
            ElementType.Fire
        );

        CreateSkill(
            SkillID.FireRing,
            "Fire Ring",
            "Creates a ring of fire around the player",
            SkillType.Area,
            ElementType.Fire
        );

        CreateSkill(
            SkillID.ThermalElevation,
            "Thermal Elevation",
            "Increases attack speed and damage",
            SkillType.Passive,
            ElementType.Fire
        );

        SaveAllSkillData();
        Debug.Log($"Created {skillDatas.Count} default skills");
    }

    private void CreateSkill(SkillID id, string name, string description, SkillType type, ElementType element)
    {
        var skill = new SkillData
        {
            metadata = new SkillMetadata
            {
                ID = id,
                Name = name,
                Description = description,
                Type = type,
                Element = element,
                Tier = 1
            }
        };

        // 스킬 타입에 따른 기본 스탯 설정
        var statData = new SkillStatData
        {
            skillID = id,
            level = 1,
            maxSkillLevel = 5,
            element = element,
            elementalPower = 1f
        };

        // 스킬 타입별 특수 스탯 설정
        switch (type)
        {
            case SkillType.Projectile:
                ConfigureProjectileStats(statData, element);
                break;
            case SkillType.Area:
                ConfigureAreaStats(statData, element);
                break;
            case SkillType.Passive:
                ConfigurePassiveStats(statData, element);
                break;
        }

        // 스킬 데이터와 스탯 저장
        skillDatas.Add(skill);
        if (!skillStatsByLevel.ContainsKey(id))
        {
            skillStatsByLevel[id] = new Dictionary<int, SkillStatData>();
        }
        skillStatsByLevel[id][1] = statData;
    }

    private void ConfigureProjectileStats(SkillStatData stats, ElementType element)
    {
        stats.damage = 15f;
        stats.projectileSpeed = 12f;
        stats.projectileScale = 1f;
        stats.shotInterval = 0.8f;
        stats.pierceCount = 1;
        stats.attackRange = 12f;
        
        // 원소별 특수 설정
        switch (element)
        {
            case ElementType.Fire:
                stats.explosionRad = 2f;
                stats.damage = 18f;
                break;
            case ElementType.Water:
                stats.isHoming = true;
                stats.homingRange = 8f;
                break;
            case ElementType.Dark:
                stats.pierceCount = 2;
                break;
            case ElementType.Earth:
                stats.projectileCount = 3;
                stats.innerInterval = 0.15f;
                break;
        }
    }

    private void ConfigureAreaStats(SkillStatData stats, ElementType element)
    {
        stats.damage = 12f;
        stats.radius = 5f;
        stats.duration = 3f;
        stats.tickRate = 0.5f;
        
        // 원소별 특수 설정
        switch (element)
        {
            case ElementType.Fire:
                stats.isPersistent = true;
                stats.damage = 8f;
                stats.tickRate = 0.3f;
                break;
            case ElementType.Water:
                stats.radius = 6f;
                stats.duration = 4f;
                break;
            case ElementType.Dark:
                stats.moveSpeed = 3f;
                stats.damage = 15f;
                break;
            case ElementType.Earth:
                stats.radius = 4f;
                stats.damage = 20f;
                break;
        }
    }

    private void ConfigurePassiveStats(SkillStatData stats, ElementType element)
    {
        stats.effectDuration = 5f;
        stats.cooldown = 15f;
        stats.triggerChance = 100f;
        
        // 원소별 특수 설정
        switch (element)
        {
            case ElementType.Fire:
                stats.damageIncrease = 15f;
                stats.attackSpeedIncrease = 10f;
                break;
            case ElementType.Water:
                stats.moveSpeedIncrease = 15f;
                stats.attackRangeIncrease = 20f;
                break;
            case ElementType.Dark:
                stats.damageIncrease = 10f;
                stats.expAreaIncrease = 20f;
                break;
            case ElementType.Earth:
                stats.defenseIncrease = 15f;
                stats.hpIncrease = 20f;
                stats.hpRegenIncrease = 10f;
                break;
        }
    }

    private IEnumerator InitializeSkill(SkillData skill)
    {
        Debug.Log($"Initializing skill: {skill.metadata.Name} (ID: {skill.metadata.ID}, Type: {skill.metadata.Type})");

        // 스킬 스탯 설정
        if (skillStatsByLevel.TryGetValue(skill.metadata.ID, out var levelStats))
        {
            if (levelStats.TryGetValue(1, out var statData))
            {
                ISkillStat skillStat = null;

                try
                {
                    switch (skill.metadata.Type)
                    {
                        case SkillType.Projectile:
                            var projectileStat = new ProjectileSkillStat();
                            CopyBaseStats(skill.baseStats, projectileStat);
                            CopyProjectileStats(skill.projectileStat, projectileStat);
                            skillStat = projectileStat;
                            break;

                        case SkillType.Area:
                            var areaStat = new AreaSkillStat();
                            CopyBaseStats(skill.baseStats, areaStat);
                            CopyAreaStats(skill.areaStat, areaStat);
                            skillStat = areaStat;
                            break;

                        case SkillType.Passive:
                            var passiveStat = new PassiveSkillStat();
                            CopyBaseStats(skill.baseStats, passiveStat);
                            CopyPassiveStats(skill.passiveStat, passiveStat);
                            skillStat = passiveStat;
                            break;
                    }

                    if (skillStat != null)
                    {
                        skill.SetStatsForLevel(1, skillStat);
                        Debug.Log($"Set stats for skill: {skill.metadata.Name}, Type: {skill.metadata.Type}");
                    }
                    else
                    {
                        Debug.LogError($"Failed to create skill stat for {skill.metadata.Name}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error setting stats for {skill.metadata.Name}: {e.Message}");
                }
            }
            else
            {
                Debug.LogError($"No level 1 stats found for skill: {skill.metadata.Name}");
            }
        }
        else
        {
            Debug.LogError($"No stats found for skill: {skill.metadata.Name}");
        }

        // 리소스 로드 전에 메타데이터 검증
        if (skill.metadata == null)
        {
            Debug.LogError($"Metadata is null for skill: {skill.metadata?.Name ?? "Unknown"}");
            yield break;
        }

        // 리소스 로드
        yield return StartCoroutine(LoadSkillResources(skill));
    }

    private IEnumerator LoadSkillResources(SkillData skill)
    {
        if (skill == null || skill.metadata == null)
        {
            Debug.LogError("Cannot load resources for null skill or metadata");
            yield break;
        }

        // 메타데이터 프리팹 로드
        string metadataPrefabPath = $"{PREFAB_PATH}/{skill.metadata.ID}_Metadata";
        GameObject metadataPrefab = Resources.Load<GameObject>(metadataPrefabPath);
        if (metadataPrefab != null)
        {
            skill.metadata.Prefab = metadataPrefab;
            Debug.Log($"Loaded metadata prefab for {skill.metadata.Name}");
        }
        else
        {
            Debug.LogWarning($"Failed to load metadata prefab for {skill.metadata.Name} at path: {metadataPrefabPath}");
        }

        // 아이콘 로드
        string iconPath = $"{ICON_PATH}/{skill.metadata.ID}_Icon";
        Sprite icon = Resources.Load<Sprite>(iconPath);
        if (icon != null)
        {
            skill.icon = icon;
            skill.metadata.Icon = icon;
            Debug.Log($"Loaded icon for {skill.metadata.Name}");
        }
        else
        {
            Debug.LogWarning($"Failed to load icon for {skill.metadata.Name} at path: {iconPath}");
        }

        // 프로젝일  경우 프젝타 프리팹 로드
        if (skill.metadata.Type == SkillType.Projectile)
        {
            string projectilePath = $"{PREFAB_PATH}/{skill.metadata.ID}_Projectile";
            GameObject projectilePrefab = Resources.Load<GameObject>(projectilePath);
            if (projectilePrefab != null)
            {
                skill.projectile = projectilePrefab;
                Debug.Log($"Loaded projectile prefab for {skill.metadata.Name}");
            }
            else
            {
                Debug.LogWarning($"Failed to load projectile prefab for {skill.metadata.Name} at path: {projectilePath}");
            }
        }

        yield return null;
    }

    private bool ValidateInitialization()
    {
        if (skillDatas == null || skillDatas.Count == 0)
        {
            Debug.LogError("No skill data loaded");
            return false;
        }

        foreach (var skill in skillDatas)
        {
            if (skill == null || skill.metadata == null)
            {
                Debug.LogError("Found null skill or metadata");
                return false;
            }

            // 스킬 타입에 따른 필수 리소스 검증
            switch (skill.metadata.Type)
            {
                case SkillType.Projectile:
                    if (skill.projectile == null)
                    {
                        Debug.LogWarning($"Missing projectile prefab for {skill.metadata.Name}");
                    }
                    break;
            }

            // 스탯 데이터 검증
            if (!skillStatsByLevel.ContainsKey(skill.metadata.ID))
            {
                Debug.LogWarning($"No stats found for skill {skill.metadata.Name}");
            }
        }

        return true;
    }

    public List<SkillData> GetAllSkillData()
    {
        var copiedList = new List<SkillData>();
        foreach (var skill in skillDatas)
        {
            var copiedSkill = new SkillData
            {
                metadata = new SkillMetadata
                {
                    ID = skill.metadata.ID,
                    Name = skill.metadata.Name,
                    Description = skill.metadata.Description,
                    Type = skill.metadata.Type,
                    Element = skill.metadata.Element,
                    Tier = skill.metadata.Tier,
                    Tags = skill.metadata.Tags?.ToArray(),
                    Prefab = skill.metadata.Prefab,
                    Icon = skill.metadata.Icon
                },
                icon = skill.icon,
                projectile = skill.projectile,
                prefabsByLevel = skill.prefabsByLevel?.ToArray(),
                baseStats = new BaseSkillStat(skill.baseStats),
                projectileStat = new ProjectileSkillStat(skill.projectileStat),
                areaStat = new AreaSkillStat(skill.areaStat),
                passiveStat = new PassiveSkillStat(skill.passiveStat)
            };

            copiedList.Add(copiedSkill);
        }
        return copiedList;
    }

    public void SaveAllSkillData()
    {
        try
        {
            // 리소스 레퍼런스 데이터 생성
            var resourceReferences = new ResourceReferenceData();
            
            // 각 스킬의 리소스 참조 정보 저장
            foreach (var skill in skillDatas)
            {
                if (skill != null)
                {
                    SaveSkillResources(skill);
                    AddResourceReferences(skill, resourceReferences);
                }
            }

            // 스킬 데이터와 리소스 레퍼런스를 함께 저장
            var wrapper = new SkillDataWrapper 
            { 
                skillDatas = skillDatas,
                resourceReferences = resourceReferences
            };
            
            SaveData(SKILL_DATA_FILENAME, wrapper);

            // CSV 데이터 저장
            SaveSkillStatsToCSV(Path.Combine(Application.dataPath, "Resources", RESOURCE_PATH));

            Debug.Log("Successfully saved all skill data");
            AssetDatabase.Refresh();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving all skill data: {e.Message}\n{e.StackTrace}");
        }
    }

    private void SaveSkillResources(SkillData skill)
    {
        if (skill?.metadata == null) return;

        try
        {
            // 아이콘 저장
            if (skill.icon != null)
            {
                SaveIconToResources(skill.icon, skill.metadata.ID);
                skill.metadata.Icon = skill.icon; // 메타데이터에도 아이콘 참조 저장
            }

            // 메타데이터 프리팹 저장
            if (skill.metadata.Prefab != null)
            {
                SavePrefabToResources(skill.metadata.Prefab, skill.metadata.ID, "Metadata");
            }

            // 프로젝타일 프리팹 저장 (해당하는 경우)
            if (skill.metadata.Type == SkillType.Projectile && skill.projectile != null)
            {
                SavePrefabToResources(skill.projectile, skill.metadata.ID, "Projectile");
            }

            // 레벨별 프리팹 저장
            SaveLevelPrefabs(skill);

            // 저장 후 즉시 AssetDatabase 갱신
            AssetDatabase.Refresh();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving skill resources: {e.Message}\n{e.StackTrace}");
        }
    }

    // 리소스 참조 저장을 위한 헬퍼 메서드들
    private void SavePrefabReference(SkillData skill, ResourceReferenceData references)
    {
        if (skill.metadata.Prefab != null)
        {
            string path = AssetDatabase.GetAssetPath(skill.metadata.Prefab);
            string guid = AssetDatabase.AssetPathToGUID(path);
            references.Add($"{skill.metadata.ID}_Metadata", new AssetReference
            {
                guid = guid,
                path = path
            });
        }
    }

    private void SaveIconReference(SkillData skill, ResourceReferenceData references)
    {
        if (skill.icon != null)
        {
            string path = AssetDatabase.GetAssetPath(skill.icon);
            string guid = AssetDatabase.AssetPathToGUID(path);
            references.Add($"{skill.metadata.ID}_Icon", new AssetReference
            {
                guid = guid,
                path = path
            });
        }
    }

    private void SaveProjectileReference(SkillData skill, ResourceReferenceData references)
    {
        if (skill.metadata.Type == SkillType.Projectile && skill.projectile != null)
        {
            string path = AssetDatabase.GetAssetPath(skill.projectile);
            string guid = AssetDatabase.AssetPathToGUID(path);
            references.Add($"{skill.metadata.ID}_Projectile", new AssetReference
            {
                guid = guid,
                path = path
            });
        }
    }

    private void SaveLevelPrefabReferences(SkillData skill, ResourceReferenceData references)
    {
        if (skill.prefabsByLevel != null)
        {
            for (int i = 0; i < skill.prefabsByLevel.Length; i++)
            {
                if (skill.prefabsByLevel[i] != null)
                {
                    string path = AssetDatabase.GetAssetPath(skill.prefabsByLevel[i]);
                    string guid = AssetDatabase.AssetPathToGUID(path);
                    references.Add($"{skill.metadata.ID}_Level_{i + 1}", new AssetReference
                    {
                        guid = guid,
                        path = path
                    });
                }
            }
        }
    }

    public void LoadAllSkillData()
    {
        try
        {
            var wrapper = LoadData<SkillDataWrapper>(SKILL_DATA_FILENAME);
            if (wrapper == null || wrapper.skillDatas == null)
            {
                Debug.LogError("Failed to load skill data wrapper or skill data is null");
                skillDatas = new List<SkillData>();
                return;
            }

            skillDatas = wrapper.skillDatas;
            Debug.Log($"Loaded {skillDatas.Count} skills from JSON");

            foreach (var skill in skillDatas)
            {
                if (skill == null || skill.metadata == null)
                {
                    Debug.LogError("Found null skill or metadata while loading");
                    continue;
                }

                // 기존 스탯 데이터를 ISkillStat으로 변환하여 설정
                ISkillStat skillStat = null;
                switch (skill.metadata.Type)
                {
                    case SkillType.Projectile:
                        var projectileStat = new ProjectileSkillStat();
                        CopyBaseStats(skill.baseStats, projectileStat);
                        CopyProjectileStats(skill.projectileStat, projectileStat);
                        skillStat = projectileStat;
                        break;

                    case SkillType.Area:
                        var areaStat = new AreaSkillStat();
                        CopyBaseStats(skill.baseStats, areaStat);
                        CopyAreaStats(skill.areaStat, areaStat);
                        skillStat = areaStat;
                        break;

                    case SkillType.Passive:
                        var passiveStat = new PassiveSkillStat();
                        CopyBaseStats(skill.baseStats, passiveStat);
                        CopyPassiveStats(skill.passiveStat, passiveStat);
                        skillStat = passiveStat;
                        break;
                }

                if (skillStat != null)
                {
                    skill.SetStatsForLevel(1, skillStat);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading skill data: {e.Message}\n{e.StackTrace}");
            skillDatas = new List<SkillData>();
        }
    }

    private void CopyBaseStats(BaseSkillStat source, ISkillStat target)
    {
        target.baseStat = new BaseSkillStat
        {
            damage = source.damage,
            skillName = source.skillName,
            skillLevel = source.skillLevel,
            maxSkillLevel = source.maxSkillLevel,
            element = source.element,
            elementalPower = source.elementalPower
        };
    }

    private void CopyProjectileStats(ProjectileSkillStat source, ProjectileSkillStat target)
    {
        target.projectileSpeed = source.projectileSpeed;
        target.projectileScale = source.projectileScale;
        target.shotInterval = source.shotInterval;
        target.pierceCount = source.pierceCount;
        target.attackRange = source.attackRange;
        target.homingRange = source.homingRange;
        target.isHoming = source.isHoming;
        target.explosionRad = source.explosionRad;
        target.projectileCount = source.projectileCount;
        target.innerInterval = source.innerInterval;
    }

    private void CopyAreaStats(AreaSkillStat source, AreaSkillStat target)
    {
        target.radius = source.radius;
        target.duration = source.duration;
        target.tickRate = source.tickRate;
        target.isPersistent = source.isPersistent;
        target.moveSpeed = source.moveSpeed;
    }

    private void CopyPassiveStats(PassiveSkillStat source, PassiveSkillStat target)
    {
        target.effectDuration = source.effectDuration;
        target.cooldown = source.cooldown;
        target.triggerChance = source.triggerChance;
        target.damageIncrease = source.damageIncrease;
        target.defenseIncrease = source.defenseIncrease;
        target.expAreaIncrease = source.expAreaIncrease;
        target.homingActivate = source.homingActivate;
        target.hpIncrease = source.hpIncrease;
        target.moveSpeedIncrease = source.moveSpeedIncrease;
        target.attackSpeedIncrease = source.attackSpeedIncrease;
        target.attackRangeIncrease = source.attackRangeIncrease;
        target.hpRegenIncrease = source.hpRegenIncrease;
    }

    private string GetResourcePath(string fullPath)
    {
        // "Assets/Resources/" 이후의 경로만 추출하고 확장자 제거
        const string resourcesFolder = "Resources/";
        int resourceIndex = fullPath.IndexOf(resourcesFolder);
        if (resourceIndex != -1)
        {
            string resourcePath = fullPath.Substring(resourceIndex + resourcesFolder.Length);
            return Path.ChangeExtension(resourcePath, null);
        }
        return null;
    }

    public void UpdateSkillData(SkillData updatedSkill)
    {
        if (!ValidateSkillData(updatedSkill)) return;

        // 마지막 업데이트로부터 일정 시간이 지나지 않았다면 무시
        if (Time.realtimeSinceStartup - lastUpdateTime < UPDATE_INTERVAL)
        {
            return;
        }

        lastUpdateTime = Time.realtimeSinceStartup;

        int index = skillDatas.FindIndex(x => x.metadata.ID == updatedSkill.metadata.ID);
        if (index != -1)
        {
            skillDatas[index] = updatedSkill;
            SaveSkillResources(updatedSkill);
            SaveAllSkillData();
            Debug.Log($"Updated skill: {updatedSkill.metadata.Name}");
        }
        else
        {
            Debug.LogError($"Skill not found: {updatedSkill.metadata.Name}");
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

            skillStatsByLevel = new Dictionary<SkillID, Dictionary<int, SkillStatData>>();

            foreach (var fileName in fileNames)
            {
                TextAsset csvFile = Resources.Load<TextAsset>($"{RESOURCE_PATH}/{fileName}");
                if (csvFile == null)
                {
                    Debug.LogError($"Failed to load {fileName}.csv");
                    continue;
                }

                Debug.Log($"Loading {fileName}.csv...");
                string[] lines = csvFile.text.Split('\n');

                if (lines.Length <= 1)
                {
                    Debug.LogWarning($"{fileName}.csv is empty or contains only headers");
                    continue;
                }

                ProcessCSVFile(csvFile, fileName, skillStatsByLevel);
            }

            Debug.Log($"Loaded skill stats for {skillStatsByLevel.Count} skills");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading skill stats: {e.Message}\n{e.StackTrace}");
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
            // 줄바꿈 문자 통일
            string[] lines = csvFile.text.Replace("\r\n", "\n").Split('\n');
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
                    Debug.LogWarning($"Line {i} has incorrect number of values in {fileName}. Expected {headers.Length}, got {values.Length}");
                    Debug.LogWarning($"Line content: {line}");
                    continue;
                }

                // 빈 값이나 공백만 있는 값 필터링
                if (values.All(v => string.IsNullOrWhiteSpace(v))) continue;

                SkillStatData statData = ParseSkillStatLine(headers, values);
                if (statData != null && statData.skillID != SkillID.None)
                {
                    if (statData.level < 1)
                    {
                        Debug.LogError($"Invalid level in CSV: {statData.level}. Levels should start from 1");
                        continue;
                    }

                    if (!tempStats.ContainsKey(statData.skillID))
                    {
                        tempStats[statData.skillID] = new Dictionary<int, SkillStatData>();
                    }

                    tempStats[statData.skillID][statData.level] = statData;
                    Debug.Log($"Added stats for skill {statData.skillID} level {statData.level}");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error processing CSV file {fileName}: {e.Message}\n{e.StackTrace}");
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
        var key = (skillID, level);
        if (statsCache.TryGetValue(key, out var cachedStats))
        {
            return cachedStats;
        }

        var stats = GetSkillStatsForLevelInternal(skillID, level, skillType);
        if (stats != null)
        {
            statsCache[key] = stats;
        }
        return stats;
    }

    private ISkillStat GetSkillStatsForLevelInternal(SkillID skillID, int level, SkillType skillType)
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
                    Debug.Log($"Getting stats for {skillID} level {level}");

                    if (level > 1 && !levelStats.ContainsKey(level - 1))
                    {
                        Debug.LogError($"Missing previous level ({level - 1}) data for skill {skillID}");
                        return null;
                    }

                    if (statData.ValidateStats())
                    {
                        var skillStat = statData.CreateSkillStat(skillType);
                        if (skillStat != null)
                        {
                            // Base Stats 설정 확인
                            Debug.Log($"Base Stats for {skillID} level {level}:");
                            Debug.Log($"Damage: {statData.damage}");
                            Debug.Log($"Element: {statData.element}");
                            Debug.Log($"Elemental Power: {statData.elementalPower}");

                            // Base Stats 명시적 설정
                            skillStat.baseStat = new BaseSkillStat
                            {
                                damage = statData.damage,
                                skillLevel = level,
                                maxSkillLevel = statData.maxSkillLevel,
                                element = statData.element,
                                elementalPower = statData.elementalPower,
                                skillName = GetSkillData(skillID)?.metadata.Name ?? "Unknown"
                            };

                            return skillStat;
                        }
                    }
                }
                else
                {
                    var availableLevels = string.Join(", ", levelStats.Keys.OrderBy(k => k));
                    Debug.LogError($"Stats for level {level} not found. Available levels: {availableLevels}");
                }
            }
            return null;
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

        // 스킬 데이터 검증
        var skillData = GetSkillData(skillID);
        if (skillData == null)
        {
            errorMessage = $"Skill data not found for {skillID}";
            return false;
        }

        // 스킬 타입 검증
        if (skillData.metadata.Type != skillType)
        {
            errorMessage = $"Skill type mismatch for {skillID}. Expected {skillType} but got {skillData.metadata.Type}";
            return false;
        }

        // 레벨 데이터 존재 여부 확인
        if (skillStatsByLevel.TryGetValue(skillID, out var levelStats))
        {
            if (!levelStats.ContainsKey(level))
            {
                var availableLevels = string.Join(", ", levelStats.Keys.OrderBy(k => k));
                errorMessage = $"Level {level} not found for skill {skillID}. Available levels: {availableLevels}";
                return false;
            }
        }
        else
        {
            errorMessage = $"No level data found for skill {skillID}";
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
            string path = Path.Combine(Application.dataPath, "Resources", RESOURCE_PATH, fileName);
            if (File.Exists(path))
            {
                string jsonContent = File.ReadAllText(path);
                if (!string.IsNullOrEmpty(jsonContent))
                {
                    T data = JsonUtility.FromJson<T>(jsonContent);
                    if (data != null)
                    {
                        Debug.Log($"Successfully loaded data from {path}");
                        return data;
                    }
                }
            }
            else
            {
                Debug.Log($"No existing data file found at {path}");
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
            
            // 저장 후 즉시 리소스 레퍼런스도 업데이트
            if (data is SkillDataWrapper wrapper)
            {
                skillDatas = wrapper.skillDatas;
                foreach (var skill in skillDatas)
                {
                    if (skill != null)
                    {
                        SaveSkillResources(skill);
                    }
                }
            }
            
            Debug.Log($"Data saved successfully to {path}");
            AssetDatabase.Refresh();
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

        // 마지막 업데이트로부터 일정 시간이 지나지 않았다면 무시
        if (Time.realtimeSinceStartup - lastUpdateTime < UPDATE_INTERVAL)
        {
            return;
        }

        lastUpdateTime = Time.realtimeSinceStartup;

        try
        {
            // skillStatsByLevel null이면 초기화
            if (skillStatsByLevel == null)
            {
                skillStatsByLevel = new Dictionary<SkillID, Dictionary<int, SkillStatData>>();
            }

            // 새로 Dictionary 생성
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

                    // stat의 skillID가 None이면 pair.Key 설정
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

    public void SavePassiveSkillStats(string path)
    {
        StringBuilder csv = new StringBuilder();

        // 헤더 작성
        csv.AppendLine("skillid,level,damage,maxskilllevel,element,elementalpower," +
                      "effectduration,cooldown,triggerchance,damageincrease,defenseincrease," +
                      "expareaincrease,homingactivate,hpincrease,movespeedincrease," +
                      "attackspeedincrease,attackrangeincrease,hpregenincrease");

        foreach (var skillStats in skillStatsByLevel)
        {
            // 해당 스킬의 타입이 패시브인지 확인
            var skill = skillDatas.Find(s => s.metadata.ID == skillStats.Key);
            if (skill?.metadata.Type == SkillType.Passive)
            {
                foreach (var levelStats in skillStats.Value)
                {
                    var stat = levelStats.Value;
                    // CSV 라인 작성
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

        try
        {
            // 디렉토리가 없으면 생성
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // 파일 저장
            File.WriteAllText(path, csv.ToString());
            Debug.Log($"Successfully saved passive skill stats to: {path}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving passive skill stats: {e.Message}\n{e.StackTrace}");
        }
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

                    // 기존 레벨 탯이 있다면 업데이트, 없다면 추가
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

    public static string ResourcePath => RESOURCE_PATH;
    public static string PrefabPath => PREFAB_PATH;
    public static string IconPath => ICON_PATH;

    public Sprite LoadSkillIcon(SkillID skillId)
    {
        string iconPath = $"{ICON_PATH}/{skillId}_Icon";
        var sprite = Resources.Load<Sprite>(iconPath);
        if (sprite == null)
        {
            Debug.LogWarning($"Failed to load icon for {skillId}, using fallback");
            return Resources.Load<Sprite>("DefaultSkillIcon"); // 폴백 아이콘
        }
        return sprite;
    }

    public GameObject LoadSkillPrefab(SkillID skillId, string suffix)
    {
        string prefabPath = $"{PREFAB_PATH}/{skillId}_{suffix}";
        var prefab = Resources.Load<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogWarning($"Failed to load prefab for {skillId}_{suffix}");
            return null;
        }
        return prefab;
    }

    public List<SkillStatData> GetSkillStatsListForID(SkillID skillID)
    {
        if (skillStatsByLevel.TryGetValue(skillID, out var levelStats))
        {
            return levelStats.Values.ToList();
        }
        return null;
    }

    private bool ValidateResourcePath(string path)
    {
        if (string.IsNullOrEmpty(path)) return false;

        // Resources 폴더 내에 있는지 확인
        if (!path.Contains("/Resources/"))
        {
            Debug.LogError($"Asset path is not in Resources folder: {path}");
            return false;
        }

        // 파일이 실제로 존재하는지 확인
        string fullPath = Path.Combine(Application.dataPath, path.Replace("Assets/", ""));
        if (!File.Exists(fullPath))
        {
            Debug.LogError($"Asset file not found: {path}");
            return false;
        }

        return true;
    }

    private GameObject LoadPrefabWithFallback(string guid, string path, string resourcePath)
    {
        GameObject prefab = null;

        // 1. Resources.Load 시도
        if (!string.IsNullOrEmpty(resourcePath))
        {
            prefab = Resources.Load<GameObject>(resourcePath);
        }

        // 2. GUID로 시도
        if (prefab == null && !string.IsNullOrEmpty(guid))
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (!string.IsNullOrEmpty(assetPath))
            {
                prefab = Resources.Load<GameObject>(GetResourcePath(assetPath));
            }
        }

        // 3. 직접 경로로 시도
        if (prefab == null && !string.IsNullOrEmpty(path))
        {
            prefab = Resources.Load<GameObject>(GetResourcePath(path));
        }

        if (prefab == null)
        {
            Debug.LogWarning($"Failed to load prefab: GUID={guid}, Path={path}");
        }

        return prefab;
    }

    private void SaveSkillStatsToCSV(string directory)
    {
        try
        {
            // 프로젝타일 스킬 CSV
            string projectilePath = Path.Combine(directory, "ProjectileSkillStats.csv");
            SaveProjectileSkillStats(projectilePath);

            // 에어리어 스킬 CSV
            string areaPath = Path.Combine(directory, "AreaSkillStats.csv");
            SaveAreaSkillStats(areaPath);

            // 패시브 스킬 CSV
            string passivePath = Path.Combine(directory, "PassiveSkillStats.csv");
            SavePassiveSkillStats(passivePath);

            Debug.Log($"Successfully saved all skill stats CSV files in: {directory}");
            AssetDatabase.Refresh();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving skill stats to CSV: {e.Message}");
        }
    }

    private void SaveProjectileSkillStats(string path)
    {
        StringBuilder csv = new StringBuilder();
        csv.AppendLine("skillid,level,damage,maxskilllevel,element,elementalpower," +
                      "projectilespeed,projectilescale,shotinterval,piercecount,attackrange," +
                      "homingrange,ishoming,explosionrad,projectilecount,innerinterval");

        foreach (var skillStats in skillStatsByLevel)
        {
            var skill = skillDatas.Find(s => s.metadata.ID == skillStats.Key);
            if (skill?.metadata.Type == SkillType.Projectile)
            {
                foreach (var levelStats in skillStats.Value)
                {
                    csv.AppendLine($"{skillStats.Key},{levelStats.Key}," +
                                 $"{levelStats.Value.damage},{levelStats.Value.maxSkillLevel}," +
                                 $"{levelStats.Value.element},{levelStats.Value.elementalPower}," +
                                 $"{levelStats.Value.projectileSpeed},{levelStats.Value.projectileScale}," +
                                 $"{levelStats.Value.shotInterval},{levelStats.Value.pierceCount}," +
                                 $"{levelStats.Value.attackRange},{levelStats.Value.homingRange}," +
                                 $"{levelStats.Value.isHoming},{levelStats.Value.explosionRad}," +
                                 $"{levelStats.Value.projectileCount},{levelStats.Value.innerInterval}");
                }
            }
        }

        File.WriteAllText(path, csv.ToString());
    }

    private void SaveAreaSkillStats(string path)
    {
        StringBuilder csv = new StringBuilder();
        csv.AppendLine("skillid,level,damage,maxskilllevel,element,elementalpower," +
                      "radius,duration,tickrate,ispersistent,movespeed");

        foreach (var skillStats in skillStatsByLevel)
        {
            var skill = skillDatas.Find(s => s.metadata.ID == skillStats.Key);
            if (skill?.metadata.Type == SkillType.Area)
            {
                foreach (var levelStats in skillStats.Value)
                {
                    csv.AppendLine($"{skillStats.Key},{levelStats.Key}," +
                                 $"{levelStats.Value.damage},{levelStats.Value.maxSkillLevel}," +
                                 $"{levelStats.Value.element},{levelStats.Value.elementalPower}," +
                                 $"{levelStats.Value.radius},{levelStats.Value.duration}," +
                                 $"{levelStats.Value.tickRate},{levelStats.Value.isPersistent}," +
                                 $"{levelStats.Value.moveSpeed}");
                }
            }
        }

        File.WriteAllText(path, csv.ToString());
    }

    // 기타 필요한 데이터 관리 메서드들...

    #region Resource Management
    public void SavePrefabToResources(GameObject prefab, SkillID skillId, string suffix)
    {
        if (prefab == null) return;

        try
        {
            string directory = Path.Combine(Application.dataPath, "Resources", PREFAB_PATH);
            Directory.CreateDirectory(directory);

            string prefabPath = Path.Combine(directory, $"{skillId}_{suffix}.prefab");
            string assetPath = $"Assets/Resources/{PREFAB_PATH}/{skillId}_{suffix}.prefab";

            if (File.Exists(prefabPath))
            {
                AssetDatabase.DeleteAsset(assetPath);
            }

            GameObject prefabInstance = Instantiate(prefab);
            bool success = PrefabUtility.SaveAsPrefabAsset(prefabInstance, assetPath, out _);
            DestroyImmediate(prefabInstance);

            if (success)
            {
                Debug.Log($"Saved prefab to: {assetPath}");
                AssetDatabase.Refresh();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving prefab: {e.Message}");
        }
    }

    public void SaveIconToResources(Sprite icon, SkillID skillId)
    {
        if (icon == null) return;

        try
        {
            string directory = Path.Combine(Application.dataPath, "Resources", ICON_PATH);
            Directory.CreateDirectory(directory);

            string destinationPath = Path.Combine(directory, $"{skillId}_Icon.png");
            string assetPath = $"Assets/Resources/{ICON_PATH}/{skillId}_Icon.png";

            // 기존 아이콘 파일이 있다면 삭제
            if (File.Exists(destinationPath))
            {
                File.Delete(destinationPath);
                AssetDatabase.DeleteAsset(assetPath);
            }

            // 텍스처 읽기 가능하도록 설정
            Texture2D sourceTexture = icon.texture;
            if (!sourceTexture.isReadable)
            {
                string sourcePath = AssetDatabase.GetAssetPath(sourceTexture);
                TextureImporter importer = AssetImporter.GetAtPath(sourcePath) as TextureImporter;
                if (importer != null)
                {
                    importer.isReadable = true;
                    importer.SaveAndReimport();
                    sourceTexture = icon.texture;
                }
            }

            // 새 텍스처 생성 및 저장
            Texture2D newTexture = new Texture2D(sourceTexture.width, sourceTexture.height);
            newTexture.SetPixels(sourceTexture.GetPixels());
            newTexture.Apply();

            byte[] bytes = newTexture.EncodeToPNG();
            File.WriteAllBytes(destinationPath, bytes);

            AssetDatabase.Refresh();
            Object.DestroyImmediate(newTexture);

            Debug.Log($"Successfully saved icon for {skillId} to {destinationPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving icon: {e.Message}\n{e.StackTrace}");
        }
    }

    private void SaveLevelPrefabs(SkillData skill)
    {
        if (skill.prefabsByLevel == null) return;

        for (int i = 0; i < skill.prefabsByLevel.Length; i++)
        {
            if (skill.prefabsByLevel[i] != null)
            {
                SavePrefabToResources(skill.prefabsByLevel[i], skill.metadata.ID, $"Level_{i + 1}");
            }
        }
    }
    #endregion

    #region Skill Data Management
    public SkillData CreateNewSkill()
    {
        SkillData newSkill = new SkillData
        {
            metadata = new SkillMetadata
            {
                Name = "New Skill",
                Description = "New skill description",
                Type = SkillType.None,
                ID = SkillID.None,
                Element = ElementType.None,
                Tier = 1
            }
        };

        InitializeDefaultStats(newSkill);
        skillDatas.Add(newSkill);
        return newSkill;
    }


    private void AddResourceReferences(SkillData skill, ResourceReferenceData references)
    {
        if (skill?.metadata == null) return;

        // 메타데이터 프리팹 참조 추가
        if (skill.metadata.Prefab != null)
        {
            string path = AssetDatabase.GetAssetPath(skill.metadata.Prefab);
            string guid = AssetDatabase.AssetPathToGUID(path);
            references.Add($"{skill.metadata.ID}_Metadata", new AssetReference
            {
                guid = guid,
                path = path
            });
        }

        // 아이콘 참조 추가
        if (skill.icon != null)
        {
            string iconPath = AssetDatabase.GetAssetPath(skill.icon);
            string iconGuid = AssetDatabase.AssetPathToGUID(iconPath);
            references.Add($"{skill.metadata.ID}_Icon", new AssetReference
            {
                guid = iconGuid,
                path = iconPath
            });
        }

        // 프로젝타일 스킬인 경우 프로젝타일 프리팹 참조 추가
        if (skill.metadata.Type == SkillType.Projectile && skill.projectile != null)
        {
            string projectilePath = AssetDatabase.GetAssetPath(skill.projectile);
            string projectileGuid = AssetDatabase.AssetPathToGUID(projectilePath);
            references.Add($"{skill.metadata.ID}_Projectile", new AssetReference
            {
                guid = projectileGuid,
                path = projectilePath
            });
        }

        // 레벨별 프리팹 참조 추가
        if (skill.prefabsByLevel != null)
        {
            for (int i = 0; i < skill.prefabsByLevel.Length; i++)
            {
                if (skill.prefabsByLevel[i] != null)
                {
                    string levelPrefabPath = AssetDatabase.GetAssetPath(skill.prefabsByLevel[i]);
                    string levelPrefabGuid = AssetDatabase.AssetPathToGUID(levelPrefabPath);
                    references.Add($"{skill.metadata.ID}_Level_{i + 1}", new AssetReference
                    {
                        guid = levelPrefabGuid,
                        path = levelPrefabPath
                    });
                }
            }
        }

        Debug.Log($"Added resource references for skill: {skill.metadata.Name}");
    }
    #endregion

    #region Stats Management
    public void UpdateSkillStats(SkillData skill, Dictionary<SkillID, List<SkillStatData>> statsData)
    {
        if (!skillStatsByLevel.ContainsKey(skill.metadata.ID))
        {
            skillStatsByLevel[skill.metadata.ID] = new Dictionary<int, SkillStatData>();
        }

        var stats = statsData[skill.metadata.ID];
        foreach (var stat in stats)
        {
            skillStatsByLevel[skill.metadata.ID][stat.level] = stat;
        }
    }

    private void InitializeDefaultStats(SkillData skill)
    {
        var defaultStat = new SkillStatData
        {
            skillID = skill.metadata.ID,
            level = 1,
            damage = 10f,
            maxSkillLevel = 5,
            element = skill.metadata.Element,
            elementalPower = 1f
        };

        switch (skill.metadata.Type)
        {
            case SkillType.Projectile:
                InitializeProjectileStats(defaultStat);
                break;
            case SkillType.Area:
                InitializeAreaStats(defaultStat);
                break;
            case SkillType.Passive:
                InitializePassiveStats(defaultStat);
                break;
        }

        if (!skillStatsByLevel.ContainsKey(skill.metadata.ID))
        {
            skillStatsByLevel[skill.metadata.ID] = new Dictionary<int, SkillStatData>();
        }
        skillStatsByLevel[skill.metadata.ID][1] = defaultStat;
    }

    private void InitializeProjectileStats(SkillStatData stats)
    {
        stats.projectileSpeed = 10f;
        stats.projectileScale = 1f;
        stats.shotInterval = 1f;
        stats.pierceCount = 1;
        stats.attackRange = 10f;
        stats.homingRange = 5f;
        stats.isHoming = false;
        stats.explosionRad = 0f;
        stats.projectileCount = 1;
        stats.innerInterval = 0.1f;
    }

    private void InitializeAreaStats(SkillStatData stats)
    {
        stats.radius = 5f;
        stats.duration = 3f;
        stats.tickRate = 1f;
        stats.isPersistent = false;
        stats.moveSpeed = 0f;
    }

    private void InitializePassiveStats(SkillStatData stats)
    {
        stats.effectDuration = 5f;
        stats.cooldown = 10f;
        stats.triggerChance = 100f;
        stats.damageIncrease = 0f;
        stats.defenseIncrease = 0f;
        stats.expAreaIncrease = 0f;
        stats.homingActivate = false;
        stats.hpIncrease = 0f;
        stats.moveSpeedIncrease = 0f;
        stats.attackSpeedIncrease = 0f;
        stats.attackRangeIncrease = 0f;
        stats.hpRegenIncrease = 0f;
    }

    public void DeleteSkill(SkillData skill)
    {
        if (skill == null || skill.metadata == null) return;

        // 리소스 파일들 삭제
        DeleteSkillResources(skill.metadata.ID);

        // 스킬 데이터 제거
        skillDatas.Remove(skill);
        if (skillStatsByLevel.ContainsKey(skill.metadata.ID))
        {
            skillStatsByLevel.Remove(skill.metadata.ID);
        }

        SaveAllSkillData();
        Debug.Log($"Deleted skill: {skill.metadata.Name}");
    }

    private void DeleteSkillResources(SkillID skillId)
    {
        string[] paths = {
            $"Assets/Resources/{PREFAB_PATH}/{skillId}_Metadata.prefab",
            $"Assets/Resources/{PREFAB_PATH}/{skillId}_Projectile.prefab",
            $"Assets/Resources/{ICON_PATH}/{skillId}_Icon.png"
        };

        foreach (string path in paths)
        {
            if (File.Exists(path))
            {
                AssetDatabase.DeleteAsset(path);
            }
        }

        // 레벨 프리팹들 삭제
        int level = 1;
        while (true)
        {
            string levelPath = $"Assets/Resources/{PREFAB_PATH}/{skillId}_Level_{level}";
            if (!File.Exists(levelPath)) break;
            AssetDatabase.DeleteAsset(levelPath);
            level++;
        }

        AssetDatabase.Refresh();
    }

    public void UpdateSkillStats(SkillID skillId, List<SkillStatData> stats)
    {
        if (!skillStatsByLevel.ContainsKey(skillId))
        {
            skillStatsByLevel[skillId] = new Dictionary<int, SkillStatData>();
        }

        foreach (var stat in stats)
        {
            if (ValidateSkillStat(stat))
            {
                skillStatsByLevel[skillId][stat.level] = stat;
            }
        }

        SaveSkillStatsToCSV(Path.Combine(Application.dataPath, "Resources", RESOURCE_PATH));
    }

    public List<SkillStatData> GetSkillStats(SkillID skillId)
    {
        if (skillStatsByLevel.TryGetValue(skillId, out var stats))
        {
            return stats.Values.ToList();
        }
        return new List<SkillStatData>();
    }

    public void AddSkillLevel(SkillID skillId)
    {
        if (!skillStatsByLevel.ContainsKey(skillId)) return;

        var stats = skillStatsByLevel[skillId];
        int newLevel = stats.Keys.Max() + 1;

        // 이전 레벨의 스탯을 복사하고 수치 증가
        var prevStat = stats[newLevel - 1].Clone();
        prevStat.level = newLevel;
        prevStat.damage *= 1.2f;  // 20% 증가
        prevStat.elementalPower *= 1.1f;  // 10% 증가

        stats[newLevel] = prevStat;
        SaveSkillStatsToCSV(Path.Combine(Application.dataPath, "Resources", RESOURCE_PATH));
    }
    #endregion

    #region Resource Loading
    public GameObject LoadPrefabFromResources(SkillID skillId, string suffix)
    {
        string path = $"{PREFAB_PATH}/{skillId}_{suffix}";
        GameObject prefab = Resources.Load<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogWarning($"Failed to load prefab: {path}");
        }
        return prefab;
    }

    public Sprite LoadIconFromResources(SkillID skillId)
    {
        string path = $"{ICON_PATH}/{skillId}_Icon";
        Sprite icon = Resources.Load<Sprite>(path);
        if (icon == null)
        {
            Debug.LogWarning($"Failed to load icon: {path}");
        }
        return icon;
    }

    private void LoadLevelPrefabs(SkillData skill)
    {
        List<GameObject> levelPrefabs = new List<GameObject>();
        int level = 1;

        while (true)
        {
            string path = $"{PREFAB_PATH}/{skill.metadata.ID}_Level_{level}";
            GameObject prefab = Resources.Load<GameObject>(path);
            if (prefab == null) break;

            levelPrefabs.Add(prefab);
            level++;
        }

        skill.prefabsByLevel = levelPrefabs.ToArray();
    }
    #endregion

    public void DeleteSkillLevel(SkillID skillId, int level)
    {
        if (skillStatsByLevel.ContainsKey(skillId))
        {
            if (skillStatsByLevel[skillId].ContainsKey(level))
            {
                skillStatsByLevel[skillId].Remove(level);
                SaveSkillStatsToCSV(Path.Combine(Application.dataPath, "Resources", RESOURCE_PATH));
            }
        }
    }

    public void ExportToJson()
    {
        var wrapper = new SkillDataWrapper { skillDatas = skillDatas };
        string jsonPath = Path.Combine(Application.dataPath, "Resources", RESOURCE_PATH, "SkillData.json");
        File.WriteAllText(jsonPath, JsonUtility.ToJson(wrapper, true));
        AssetDatabase.Refresh();
    }

    public void ImportFromJson()
    {
        string jsonPath = Path.Combine(Application.dataPath, "Resources", RESOURCE_PATH, "SkillData.json");
        if (File.Exists(jsonPath))
        {
            string jsonContent = File.ReadAllText(jsonPath);
            var wrapper = JsonUtility.FromJson<SkillDataWrapper>(jsonContent);
            if (wrapper != null && wrapper.skillDatas != null)
            {
                skillDatas = wrapper.skillDatas;
                LoadSkillStatsFromCSV();
            }
        }
    }

    public new void ClearAllData()
    {
        // JSON 파일 존재 여부 확인
        string jsonPath = Path.Combine(Application.dataPath, "Resources", RESOURCE_PATH, SKILL_DATA_FILENAME);
        bool hasJsonData = File.Exists(jsonPath);

        // CSV 파일들 존재 여부 확인
        string[] csvFiles = {
            Path.Combine(Application.dataPath, "Resources", RESOURCE_PATH, "ProjectileSkillStats.csv"),
            Path.Combine(Application.dataPath, "Resources", RESOURCE_PATH, "AreaSkillStats.csv"),
            Path.Combine(Application.dataPath, "Resources", RESOURCE_PATH, "PassiveSkillStats.csv")
        };
        bool hasCsvData = csvFiles.Any(File.Exists);

        // 데이터가 존재하면 확인 메시지 표시
        if (hasJsonData || hasCsvData)
        {
            if (!EditorUtility.DisplayDialog("Clear All Data",
                "Existing skill data found. Are you sure you want to clear all data? This cannot be undone.",
                "Clear", "Cancel"))
            {
                return;
            }
        }

        skillDatas.Clear();
        skillStatsByLevel.Clear();
        SaveAllSkillData();
    }

    public void InitializeSkillStats(SkillData skill)
    {
        if (skill == null || skill.metadata == null) return;

        var defaultStat = new SkillStatData
        {
            skillID = skill.metadata.ID,
            level = 1,
            damage = 10f,
            maxSkillLevel = 5,
            element = skill.metadata.Element,
            elementalPower = 1f
        };

        InitializeStatsByType(defaultStat, skill.metadata.Type);

        if (!skillStatsByLevel.ContainsKey(skill.metadata.ID))
        {
            skillStatsByLevel[skill.metadata.ID] = new Dictionary<int, SkillStatData>();
        }
        skillStatsByLevel[skill.metadata.ID][1] = defaultStat;
    }

    private void InitializeStatsByType(SkillStatData stats, SkillType type)
    {
        switch (type)
        {
            case SkillType.Projectile:
                InitializeProjectileStats(stats);
                break;
            case SkillType.Area:
                InitializeAreaStats(stats);
                break;
            case SkillType.Passive:
                InitializePassiveStats(stats);
                break;
        }
    }

    // 스킬 ID가 변경될 때 호출될 메서드
    public void UpdateSkillID(SkillData skill, SkillID oldID, SkillID newID)
    {
        if (oldID == newID) return;

        Debug.Log($"Updating skill ID from {oldID} to {newID}");

        // 기존 스탯 데이터 백업
        Dictionary<int, SkillStatData> oldStats = null;
        if (skillStatsByLevel.ContainsKey(oldID))
        {
            oldStats = new Dictionary<int, SkillStatData>(skillStatsByLevel[oldID]);
            skillStatsByLevel.Remove(oldID);
        }

        // 새로운 ID로 스탯 데이터 이전
        if (oldStats != null)
        {
            skillStatsByLevel[newID] = new Dictionary<int, SkillStatData>();
            foreach (var levelStat in oldStats)
            {
                var newStat = levelStat.Value.Clone();
                newStat.skillID = newID;
                skillStatsByLevel[newID][levelStat.Key] = newStat;
            }
            Debug.Log($"Transferred {oldStats.Count} level stats to new ID");
        }
        else if (!skillStatsByLevel.ContainsKey(newID))
        {
            // 새로운 ID에 대한 기본 스탯 초기화
            InitializeSkillStats(skill);
            Debug.Log($"Initialized new stats for skill ID: {newID}");
        }

        // 리소스 참조 업데이트
        UpdateResourceReferences(skill, oldID, newID);
    }

    private void UpdateResourceReferences(SkillData skill, SkillID oldID, SkillID newID)
    {
        // 기존 리소스들의 이름 변경
        if (skill.metadata.Prefab != null)
        {
            RenameResource(skill.metadata.Prefab, $"{oldID}_Metadata", $"{newID}_Metadata");
        }

        if (skill.icon != null)
        {
            RenameResource(skill.icon, $"{oldID}_Icon", $"{newID}_Icon");
        }

        if (skill.metadata.Type == SkillType.Projectile && skill.projectile != null)
        {
            RenameResource(skill.projectile, $"{oldID}_Projectile", $"{newID}_Projectile");
        }

        if (skill.prefabsByLevel != null)
        {
            for (int i = 0; i < skill.prefabsByLevel.Length; i++)
            {
                if (skill.prefabsByLevel[i] != null)
                {
                    RenameResource(skill.prefabsByLevel[i], $"{oldID}_Level_{i + 1}", $"{newID}_Level_{i + 1}");
                }
            }
        }

        AssetDatabase.Refresh();
    }

    private void RenameResource(Object asset, string oldSuffix, string newSuffix)
    {
        string path = AssetDatabase.GetAssetPath(asset);
        if (!string.IsNullOrEmpty(path))
        {
            string newPath = path.Replace(oldSuffix, newSuffix);
            AssetDatabase.MoveAsset(path, newPath);
        }
    }

    private void OnEnable()
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

        // 데이터가 없을 때만 로드하도록 수정
        if (skillDatas == null || !skillDatas.Any())
        {
            LoadAllData();
            // CSV 데이터도 없을 때만 로드
            if (skillStatsByLevel == null || !skillStatsByLevel.Any())
            {
                LoadSkillStatsFromCSV();
            }
        }
    }

    private new void LoadAllData()
    {
        try
        {
            string jsonPath = Path.Combine(Application.dataPath, "Resources", RESOURCE_PATH, SKILL_DATA_FILENAME);

            // JSON 파일이 존재하고 내용이 있는지 확인
            if (File.Exists(jsonPath))
            {
                string jsonContent = File.ReadAllText(jsonPath);
                if (!string.IsNullOrEmpty(jsonContent))
                {
                    var wrapper = JsonUtility.FromJson<SkillDataWrapper>(jsonContent);
                    if (wrapper != null)
                    {
                        skillDatas = wrapper.skillDatas ?? new List<SkillData>();
                        
                        // 리소스 레퍼런스를 사용하여 리소스 복원
                        if (wrapper.resourceReferences != null)
                        {
                            foreach (var skill in skillDatas)
                            {
                                RestoreSkillResources(skill, wrapper.resourceReferences);
                            }
                        }

                        Debug.Log($"Successfully loaded {skillDatas.Count} skills from JSON");
                        return;
                    }
                }
            }

            // 데이터가 없는 경우 새로운 리스트 생성
            skillDatas = new List<SkillData>();
            Debug.Log("Created new skill data list");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading skill data: {e.Message}\n{e.StackTrace}");
            skillDatas = new List<SkillData>();
        }
    }

    private void RestoreSkillResources(SkillData skill, ResourceReferenceData references)
    {
        if (skill?.metadata == null) return;

        string metadataKey = $"{skill.metadata.ID}_Metadata";
        if (references.TryGetValue(metadataKey, out var metadataRef))
        {
            skill.metadata.Prefab = LoadPrefabWithFallback(metadataRef.guid, metadataRef.path, GetResourcePath(metadataRef.path));
        }

        string iconKey = $"{skill.metadata.ID}_Icon";
        if (references.TryGetValue(iconKey, out var iconRef))
        {
            string iconPath = GetResourcePath(iconRef.path);
            skill.icon = Resources.Load<Sprite>(iconPath);
            skill.metadata.Icon = skill.icon;
        }

        if (skill.metadata.Type == SkillType.Projectile)
        {
            string projectileKey = $"{skill.metadata.ID}_Projectile";
            if (references.TryGetValue(projectileKey, out var projectileRef))
            {
                skill.projectile = LoadPrefabWithFallback(projectileRef.guid, projectileRef.path, GetResourcePath(projectileRef.path));
            }
        }

        // 레벨별 프리팹 복원
        int level = 1;
        List<GameObject> levelPrefabs = new List<GameObject>();
        while (true)
        {
            string levelKey = $"{skill.metadata.ID}_Level_{level}";
            if (!references.TryGetValue(levelKey, out var levelRef))
                break;

            var levelPrefab = LoadPrefabWithFallback(levelRef.guid, levelRef.path, GetResourcePath(levelRef.path));
            if (levelPrefab != null)
                levelPrefabs.Add(levelPrefab);
            level++;
        }
        skill.prefabsByLevel = levelPrefabs.ToArray();
    }

}
