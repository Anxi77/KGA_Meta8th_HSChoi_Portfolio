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
    public GameObject Prefab;
    public Sprite Icon;
}

[System.Serializable]
public class SkillData
{
    public SkillMetadata metadata;
    private Dictionary<int, ISkillStat> statsByLevel;

    public Sprite icon;
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

        // ⺻ ʱȭ
        projectileStat = new ProjectileSkillStat
        {
            baseStat = new BaseSkillStat()
        };

        areaStat = new AreaSkillStat
        {
            baseStat = new BaseSkillStat()
        };

        passiveStat = new PassiveSkillStat
        {
            baseStat = new BaseSkillStat(),
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

        if (stats.baseStat.skillLevel != level)
        {
            Debug.LogError($"Stat level mismatch. Expected: {level}, Got: {stats.baseStat.skillLevel}");
            stats.baseStat.skillLevel = level;
        }

        // 기존 스탯 저장
        var oldStats = statsByLevel.ContainsKey(level) ? statsByLevel[level] : null;

        try
        {
            // 새 스탯 설정
            statsByLevel[level] = stats;

            // 스킬 타입별 스탯 업데이트
            switch (stats)
            {
                case ProjectileSkillStat projectileStats:
                    projectileStat = projectileStats;
                    break;
                case AreaSkillStat areaStats:
                    areaStat = areaStats;
                    break;
                case PassiveSkillStat passiveStats:
                    passiveStat = passiveStats;
                    break;
            }

            Debug.Log($"Successfully set stats for level {level}");
        }
        catch (System.Exception e)
        {
            // 실패시 이전 스탯 복구
            if (oldStats != null)
            {
                statsByLevel[level] = oldStats;
            }
            Debug.LogError($"Error setting stats: {e.Message}");
        }
    }

    public ISkillStat GetCurrentTypeStat()
    {
        switch (metadata.Type)
        {
            case SkillType.Projectile:
                if (projectileStat == null)
                {
                    projectileStat = new ProjectileSkillStat
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
                return projectileStat;

            case SkillType.Area:
                if (areaStat == null)
                {
                    areaStat = new AreaSkillStat
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
                return areaStat;

            case SkillType.Passive:
                if (passiveStat == null)
                {
                    passiveStat = new PassiveSkillStat
                    {
                        baseStat = new BaseSkillStat
                        {
                            damage = 10f,
                            skillName = metadata.Name,
                            skillLevel = 1,
                            maxSkillLevel = 5,
                            element = metadata.Element,
                            elementalPower = 1f
                        },
                    };
                }
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
