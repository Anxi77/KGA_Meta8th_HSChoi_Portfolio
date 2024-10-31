using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class SkillData
{
    public SkillMetadata metadata;
    private Dictionary<int, ISkillStat> statsByLevel;

    // Editor and runtime references
    public Image icon;
    public GameObject projectile;
    public GameObject[] prefabsByLevel;
    public ProjectileSkillStat projectileStat;
    public AreaSkillStat areaStat;
    public PassiveSkillStat passiveStat;

    public SkillData()
    {
        metadata = new SkillMetadata();
        statsByLevel = new Dictionary<int, ISkillStat>();
        prefabsByLevel = new GameObject[0];
    }

    public ISkillStat GetStatsForLevel(int level)
    {
        if (statsByLevel.TryGetValue(level, out var stats))
            return stats;

        return CreateDefaultStats();
    }

    public void SetStatsForLevel(int level, ISkillStat stats)
    {
        statsByLevel[level] = stats;
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
}
