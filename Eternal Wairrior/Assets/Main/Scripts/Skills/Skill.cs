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
        currentStats = skillData.GetCurrentTypeStat();
    }

    // 기본 스탯 접근자
    public float Damage => currentStats.baseStat.damage;
    public string SkillName => currentStats.baseStat.skillName;
    public int SkillLevel => currentStats.baseStat.skillLevel;
    public int MaxSkillLevel => currentStats.baseStat.maxSkillLevel;
    public SkillID SkillID => skillData._SkillID;

    // 추상 메서드로 변경
    public abstract bool SkillLevelUpdate(int newLevel);

    // 타입별 스탯 가져오기
    protected T GetTypeStats<T>() where T : ISkillStat
    {
        if (currentStats is T typedStats)
        {
            return typedStats;
        }
        throw new System.InvalidOperationException($"Current skill is not of type {typeof(T)}");
    }
}