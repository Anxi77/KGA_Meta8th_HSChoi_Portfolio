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

        // 기본 스탯 초기화
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
        // 기본 유효성 검사
        if (stats == null)
        {
            Debug.LogError($"Attempting to set null stats for level {level}");
            return;
        }

        if (level <= 0)
        {
            Debug.LogError($"Invalid level: {level}");
            return;
        }

        // 스킬 타입 검증
        bool isValidType = stats switch
        {
            ProjectileSkillStat _ when metadata.Type == SkillType.Projectile => true,
            AreaSkillStat _ when metadata.Type == SkillType.Area => true,
            PassiveSkillStat _ when metadata.Type == SkillType.Passive => true,
            _ => false
        };

        if (!isValidType)
        {
            Debug.LogError($"Stat type mismatch. Expected {metadata.Type} but got {stats.GetType().Name}");
            return;
        }

        // 레벨 순서 검증
        if (level > 1 && !statsByLevel.ContainsKey(level - 1))
        {
            Debug.LogWarning($"Setting stats for level {level} but level {level - 1} doesn't exist");
        }

        // 스탯 업데이트
        if (statsByLevel.ContainsKey(level))
        {
            Debug.Log($"Overwriting existing stats for level {level}");
        }

        // 스킬 타입별 참조 업데이트
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

        // Dictionary에 저장
        statsByLevel[level] = stats;

        Debug.Log($"Successfully set stats for {metadata.Name} level {level}");
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
