using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public abstract class Skill : MonoBehaviour
{
    [SerializeField] protected SkillData skillData;
    protected Vector2 fireDir;

    protected virtual void Awake()
    {
        if (skillData == null)
        {
            skillData = CreateDefaultSkillData();
        }
    }

    protected virtual SkillData CreateDefaultSkillData()
    {
        return new SkillData
        {
            metadata = new SkillMetadata
            {
                Name = "Default Skill",
                Description = "Default skill description",
                Type = SkillType.None,
                Element = ElementType.None,
                Tier = 1,
                Tags = new string[0]
            }
        };
    }

    // 기본 스탯 접근자
    public virtual float Damage => skillData?.GetCurrentTypeStat()?.baseStat?.damage ?? 0f;
    public string SkillName => skillData?.metadata?.Name ?? "Unknown";
    public int SkillLevel => skillData?.GetCurrentTypeStat()?.baseStat?.skillLevel ?? 1;
    public int MaxSkillLevel => skillData?.GetCurrentTypeStat()?.baseStat?.maxSkillLevel ?? 1;
    public SkillID SkillID => skillData?.metadata?.ID ?? SkillID.None;

    // 추상 메서드
    public abstract bool SkillLevelUpdate(int newLevel);

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
}
