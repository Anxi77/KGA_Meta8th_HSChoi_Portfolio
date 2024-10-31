using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public abstract class Skill : MonoBehaviour
{
    [SerializeField] protected SkillData skillData;
    protected ISkillStat currentStats;
    protected Vector2 fireDir;

    protected virtual void Awake()
    {
        if (skillData != null)
        {
            currentStats = skillData.GetCurrentTypeStat();
        }
    }

    // 기본 스탯 접근자
    public float Damage => currentStats?.baseStat?.damage ?? 0f;
    public string SkillName => currentStats?.baseStat?.skillName ?? "Unknown";
    public int SkillLevel => currentStats?.baseStat?.skillLevel ?? 1;
    public int MaxSkillLevel => currentStats?.baseStat?.maxSkillLevel ?? 1;
    public SkillID SkillID => skillData?._SkillID ?? SkillID.None;

    // 추상 메서드
    public abstract bool SkillLevelUpdate(int newLevel);

    // 타입별 스탯 가져오기
    protected T GetTypeStats<T>() where T : ISkillStat
    {
        if (currentStats == null) return default(T);

        if (currentStats is T typedStats)
        {
            return typedStats;
        }
        Debug.LogWarning($"Current skill is not of type {typeof(T)}");
        return default(T);
    }
}
