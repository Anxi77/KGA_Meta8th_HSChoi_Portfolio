using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public abstract class Skill : MonoBehaviour
{
    [SerializeField] protected SkillData skillData;
    protected Vector2 fireDir;

    protected virtual void Awake()
    {
        // Awake에서는 초기화를 하지 않음
    }

    // 새로운 초기화 메서드 추가
    public virtual void Initialize()
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
        // SkillDataManager와 SkillManager 모두 초기화되지 않은 경우 검증을 건너뜀
        if (SkillDataManager.Instance == null || !SkillDataManager.Instance.IsInitialized ||
            SkillManager.Instance == null || !SkillManager.Instance.IsInitialized)
        {
            return true;
        }

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
    protected int _skillLevel = 1;  // 기본 필드
    public int SkillLevel
    {
        get
        {
            var currentStats = GetSkillData()?.GetCurrentTypeStat()?.baseStat;
            if (currentStats != null)
            {
                return currentStats.skillLevel;
            }
            return _skillLevel;
        }
        protected set
        {
            _skillLevel = value;
            Debug.Log($"Setting skill level to {value} for {SkillName}");
        }
    }
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
        Debug.Log($"=== Starting SkillLevelUpdate for {SkillName} ===");
        Debug.Log($"Current Level: {SkillLevel}, Attempting to upgrade to: {newLevel}");

        // 레벨 유효성 검사
        if (newLevel <= 0)
        {
            Debug.LogError($"Invalid level: {newLevel}");
            return false;
        }

        if (newLevel > MaxSkillLevel)
        {
            Debug.LogError($"Attempted to upgrade {SkillName} beyond max level ({MaxSkillLevel})");
            return false;
        }

        // 새로운 스킬 생성 시에는 레벨 순차 검증을 건너뜀
        if (newLevel < SkillLevel)
        {
            Debug.LogError($"Cannot downgrade skill level. Current: {SkillLevel}, Attempted: {newLevel}");
            return false;
        }

        try
        {
            // 현재 스탯 로깅
            var currentStats = GetSkillData()?.GetCurrentTypeStat();
            Debug.Log($"Current stats - Level: {currentStats?.baseStat?.skillLevel}, Damage: {currentStats?.baseStat?.damage}");

            // 새로운 스탯 가져오기
            var newStats = SkillDataManager.Instance.GetSkillStatsForLevel(
                skillData.metadata.ID,
                newLevel,
                skillData.metadata.Type);

            if (newStats == null)
            {
                Debug.LogError("Failed to get new stats");
                return false;
            }

            Debug.Log($"New stats received - Level: {newStats.baseStat?.skillLevel}, Damage: {newStats.baseStat?.damage}");

            newStats.baseStat.skillLevel = newLevel;
            SkillLevel = newLevel;

            Debug.Log("Setting new stats...");
            skillData.SetStatsForLevel(newLevel, newStats);

            Debug.Log("Updating skill type stats...");
            UpdateSkillTypeStats(newStats);

            Debug.Log($"=== Successfully completed SkillLevelUpdate for {SkillName} ===");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in SkillLevelUpdate: {e.Message}\n{e.StackTrace}");
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
        // Application.isPlaying 체크를 제거하고, 초기화가 완료된 경우에만 검증하도록 수정
        if (SkillDataManager.Instance != null && SkillDataManager.Instance.IsInitialized)
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
