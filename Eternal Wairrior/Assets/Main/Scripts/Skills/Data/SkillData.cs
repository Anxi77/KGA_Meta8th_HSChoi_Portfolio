using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class SkillMetadata
{
    public SkillID ID;
    public string Name;
    public string Description;
    public SkillType Type;
    public ElementType Element;
    public int Tier;
    public string[] Tags;
    [System.NonSerialized]
    public GameObject Prefab;
    [System.NonSerialized]
    public Sprite Icon;
}

[System.Serializable]
public class SkillData
{
    public SkillMetadata metadata;
    [System.NonSerialized]
    private Dictionary<int, ISkillStat> statsByLevel;

    [System.NonSerialized]
    public Sprite icon;
    [System.NonSerialized]
    public GameObject projectile;
    [System.NonSerialized]
    public GameObject[] prefabsByLevel;

    // 기본 스탯들을 직렬화 가능하도록 수정
    public BaseSkillStat baseStats = new BaseSkillStat();
    public ProjectileSkillStat projectileStat = new ProjectileSkillStat();
    public AreaSkillStat areaStat = new AreaSkillStat();
    public PassiveSkillStat passiveStat = new PassiveSkillStat();

    public SkillData()
    {
        metadata = new SkillMetadata();
        statsByLevel = new Dictionary<int, ISkillStat>();
        prefabsByLevel = new GameObject[0];

        // 기본 스탯 초기화
        baseStats = new BaseSkillStat
        {
            damage = 10f,
            skillLevel = 1,
            maxSkillLevel = 5,
            elementalPower = 1f
        };

        projectileStat = new ProjectileSkillStat
        {
            baseStat = baseStats
        };

        areaStat = new AreaSkillStat
        {
            baseStat = baseStats
        };

        passiveStat = new PassiveSkillStat
        {
            baseStat = baseStats,
            moveSpeedIncrease = 0f,
            attackSpeedIncrease = 0f,
            attackRangeIncrease = 0f,
            hpRegenIncrease = 0f
        };
    }

    public ISkillStat GetStatsForLevel(int level)
    {
        if (statsByLevel.TryGetValue(level, out var stats))
            return stats;

        return CreateDefaultStats();
    }

    public void SetStatsForLevel(int level, ISkillStat stats)
    {
        if (stats?.baseStat == null)
        {
            Debug.LogError("Attempting to set null stats");
            return;
        }

        try
        {
            // 기본 스탯 업데이트
            baseStats = new BaseSkillStat(stats.baseStat);

            // 스킬 타입별 스탯 업데이트
            switch (stats)
            {
                case ProjectileSkillStat projectileStats:
                    projectileStat = new ProjectileSkillStat(projectileStats);
                    break;
                case AreaSkillStat areaStats:
                    areaStat = new AreaSkillStat(areaStats);
                    break;
                case PassiveSkillStat passiveStats:
                    passiveStat = new PassiveSkillStat(passiveStats);
                    break;
            }

            // 레벨별 스탯 저장
            if (statsByLevel == null) statsByLevel = new Dictionary<int, ISkillStat>();
            statsByLevel[level] = stats;

            Debug.Log($"Successfully set stats for level {level}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error setting stats: {e.Message}");
        }
    }

    public ISkillStat GetCurrentTypeStat()
    {
        switch (metadata.Type)
        {
            case SkillType.Projectile:
                return projectileStat;
            case SkillType.Area:
                return areaStat;
            case SkillType.Passive:
                return passiveStat;
            default:
                return null;
        }
    }

    private ISkillStat CreateDefaultStats()
    {
        // Always return ProjectileSkillStat as default if type is None
        if (metadata.Type == SkillType.None)
        {
            return new ProjectileSkillStat
            {
                baseStat = new BaseSkillStat
                {
                    damage = 10f,
                    skillName = metadata.Name,
                    skillLevel = 1,
                    maxSkillLevel = 5,
                    element = metadata.Element,
                    elementalPower = 1f
                }
            };
        }

        switch (metadata.Type)
        {
            case SkillType.Projectile:
                return new ProjectileSkillStat();
            case SkillType.Area:
                return new AreaSkillStat();
            case SkillType.Passive:
                return new PassiveSkillStat();
            default:
                Debug.LogWarning($"Creating default ProjectileSkillStat for unknown type: {metadata.Type}");
                return new ProjectileSkillStat();
        }
    }

    public int GetMaxLevel()
    {
        return statsByLevel.Keys.Count > 0 ? statsByLevel.Keys.Max() : 1;
    }

    public void RemoveLevel(int level)
    {
        if (statsByLevel.ContainsKey(level))
        {
            statsByLevel.Remove(level);
        }
    }

    public void ClearAllStats()
    {
        statsByLevel.Clear();
    }
}

[System.Serializable]
public class SkillDataWrapper
{
    public List<SkillData> skillDatas;
    public ResourceReferenceData resourceReferences;

    public SkillDataWrapper()
    {
        skillDatas = new List<SkillData>();
        resourceReferences = new ResourceReferenceData();
    }
}

[System.Serializable]
public class ResourceReferenceData
{
    public List<string> keys = new List<string>();
    public List<AssetReference> values = new List<AssetReference>();

    public void Add(string key, AssetReference value)
    {
        keys.Add(key);
        values.Add(value);
    }

    public bool TryGetValue(string key, out AssetReference value)
    {
        int index = keys.IndexOf(key);
        if (index != -1)
        {
            value = values[index];
            return true;
        }
        value = null;
        return false;
    }

    public bool ContainsKey(string key)
    {
        return keys.Contains(key);
    }
}

[System.Serializable]
public class AssetReference
{
    public string guid;
    public string path;
}
