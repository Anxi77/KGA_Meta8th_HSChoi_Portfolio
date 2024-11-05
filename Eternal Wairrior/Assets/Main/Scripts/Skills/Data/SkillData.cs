using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

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
    public GameObject Prefab;
    public Sprite Icon;
}

[System.Serializable]
public class SkillData : ISerializationCallbackReceiver
{
    public SkillMetadata metadata;

    // Dictionary를 직렬화 가능한 형태로 변환
    [SerializeField]
    private List<SerializableSkillStat> serializedStats = new List<SerializableSkillStat>();

    [System.NonSerialized]
    private Dictionary<int, ISkillStat> statsByLevel;

    public Sprite icon;
    public GameObject projectile;
    public GameObject[] prefabsByLevel;

    // 기본 스탯들
    public BaseSkillStat baseStats = new BaseSkillStat();
    public ProjectileSkillStat projectileStat = new ProjectileSkillStat();
    public AreaSkillStat areaStat = new AreaSkillStat();
    public PassiveSkillStat passiveStat = new PassiveSkillStat();

    // 에셋 레퍼런스 정보 저장
    public ResourceReferenceData resourceReferences = new ResourceReferenceData();

    [System.Serializable]
    private class SerializableSkillStat
    {
        public int level;
        public string statJson;
        public SkillType skillType;
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        serializedStats.Clear();
        if (statsByLevel != null)
        {
            foreach (var kvp in statsByLevel)
            {
                serializedStats.Add(new SerializableSkillStat
                {
                    level = kvp.Key,
                    statJson = JsonUtility.ToJson(kvp.Value),
                    skillType = metadata.Type
                });
            }
        }

        // 리소스 레퍼런스 저장
        SaveResourceReferences();
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        statsByLevel = new Dictionary<int, ISkillStat>();
        foreach (var stat in serializedStats)
        {
            ISkillStat skillStat = null;
            switch (stat.skillType)
            {
                case SkillType.Projectile:
                    skillStat = JsonUtility.FromJson<ProjectileSkillStat>(stat.statJson);
                    break;
                case SkillType.Area:
                    skillStat = JsonUtility.FromJson<AreaSkillStat>(stat.statJson);
                    break;
                case SkillType.Passive:
                    skillStat = JsonUtility.FromJson<PassiveSkillStat>(stat.statJson);
                    break;
            }
            if (skillStat != null)
            {
                statsByLevel[stat.level] = skillStat;
            }
        }
    }

    private void SaveResourceReferences()
    {
        resourceReferences.Clear();

        // 아이콘 레퍼런스 저장
        if (icon != null)
        {
            string path = AssetDatabase.GetAssetPath(icon);
            string guid = AssetDatabase.AssetPathToGUID(path);
            resourceReferences.Add("icon", new AssetReference { guid = guid, path = path });
        }

        // 프리팹 레퍼런스 저장
        if (metadata.Prefab != null)
        {
            string path = AssetDatabase.GetAssetPath(metadata.Prefab);
            string guid = AssetDatabase.AssetPathToGUID(path);
            resourceReferences.Add("prefab", new AssetReference { guid = guid, path = path });
        }

        // 프로젝타일 프리팹 레퍼런스 저장
        if (projectile != null)
        {
            string path = AssetDatabase.GetAssetPath(projectile);
            string guid = AssetDatabase.AssetPathToGUID(path);
            resourceReferences.Add("projectile", new AssetReference { guid = guid, path = path });
        }

        // 레벨별 프리팹 레퍼런스 저장
        if (prefabsByLevel != null)
        {
            for (int i = 0; i < prefabsByLevel.Length; i++)
            {
                if (prefabsByLevel[i] != null)
                {
                    string path = AssetDatabase.GetAssetPath(prefabsByLevel[i]);
                    string guid = AssetDatabase.AssetPathToGUID(path);
                    resourceReferences.Add($"level_prefab_{i}", new AssetReference { guid = guid, path = path });
                }
            }
        }
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

    public void Clear()
    {
        keys.Clear();
        values.Clear();
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
