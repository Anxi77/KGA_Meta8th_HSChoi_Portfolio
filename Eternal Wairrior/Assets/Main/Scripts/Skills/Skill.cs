using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public abstract class Skill : MonoBehaviour
{
    [SerializeField] protected SkillData skillData;
    protected Vector2 fireDir;

    protected virtual void Awake()
    {
        InitializeSkillData();
    }

    protected virtual void InitializeSkillData()
    {
        if (skillData == null || !IsValidSkillData(skillData))
        {
            skillData = new SkillData
            {
                metadata = new SkillMetadata
                {
                    Type = GetSkillType(),
                    Name = GetDefaultSkillName(),
                    Description = GetDefaultDescription(),
                    Element = GetDefaultElement(),
                    Tier = 1
                }
            };
            Debug.Log($"Created default skill data for {gameObject.name}");
        }
    }

    protected abstract SkillType GetSkillType();
    protected abstract string GetDefaultSkillName();
    protected abstract string GetDefaultDescription();
    protected virtual ElementType GetDefaultElement() => ElementType.None;

    protected virtual void OnDisable()
    {
        CleanupSkill();
    }

    protected virtual void CleanupSkill()
    {
        // 자식 클래스에서 구현
    }

    protected bool IsValidSkillData(SkillData data)
    {
        if (data.metadata == null) return false;
        if (data.metadata.Type == SkillType.None) return false;
        if (string.IsNullOrEmpty(data.metadata.Name)) return false;
        if (data.metadata.ID == SkillID.None) return false;

        // 스킬 타입별 필수 데이터 검증
        var currentStats = data.GetCurrentTypeStat();
        if (currentStats == null) return false;
        if (currentStats.baseStat == null) return false;

        return true;
    }

    // 기본 스탯 접근자
    public virtual float Damage => skillData?.GetCurrentTypeStat()?.baseStat?.damage ?? 0f;
    public string SkillName => skillData?.metadata?.Name ?? "Unknown";
    public int SkillLevel => skillData?.GetCurrentTypeStat()?.baseStat?.skillLevel ?? 1;
    public int MaxSkillLevel => skillData?.GetCurrentTypeStat()?.baseStat?.maxSkillLevel ?? 1;
    public SkillID SkillID => skillData?.metadata?.ID ?? SkillID.None;

    // 타입별 스탯 가져오기
    protected T GetTypeStats<T>() where T : ISkillStat
    {
        if (skillData == null) return default(T);

        var currentStats = skillData.GetCurrentTypeStat();
        if (currentStats == null) return default(T);

        if (currentStats is T typedStats)
        {
            return typedStats;
        }
        Debug.LogWarning($"Current skill is not of type {typeof(T)}");
        return default(T);
    }

    // Unity 인스펙터에서 값을 수정할 수 있도록 하는 메서드
    public virtual void SetSkillData(SkillData data)
    {
        skillData = data;
    }

    // 현재 스킬 데이터 가져오기
    public virtual SkillData GetSkillData()
    {
        return skillData;
    }

    public virtual bool SkillLevelUpdate(int newLevel)
    {
        if (newLevel > MaxSkillLevel)
        {
            Debug.LogError($"Attempted to upgrade {SkillName} beyond max level");
            return false;
        }

        try
        {
            var newStats = SkillDataManager.Instance.GetSkillStatsForLevel(
                skillData.metadata.ID,
                newLevel,
                skillData.metadata.Type);

            if (newStats == null)
            {
                Debug.LogError($"Failed to get stats for {SkillName} level {newLevel}");
                return false;
            }

            skillData.SetStatsForLevel(newLevel, newStats);

            // 스킬 타입별 업데이트 처리
            UpdateSkillTypeStats(newStats);

            Debug.Log($"Successfully upgraded {SkillName} to level {newLevel}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error upgrading skill {SkillName}: {e.Message}");
            return false;
        }
    }

    protected virtual void UpdateSkillTypeStats(ISkillStat newStats)
    {
    }

    public virtual string GetDetailedDescription()
    {
        return skillData?.metadata?.Description ?? "No description available";
    }

    protected virtual void OnValidate()
    {
        if (Application.isPlaying)
        {
            if (skillData == null)
            {
                Debug.LogWarning($"SkillData is null for {GetType().Name}");
                return;
            }

            if (!IsValidSkillData(skillData))
            {
                Debug.LogError($"Invalid skill data for {GetType().Name}");
                return;
            }

            Debug.Log($"Validated skill data for {skillData.metadata.Name}");
        }
    }
}
