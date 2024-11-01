using UnityEngine;

public abstract class Skill : MonoBehaviour
{
    protected SkillData skillData;

    public virtual float Damage => GetCurrentTypeStat()?.baseStat?.damage ?? 0f;
    public virtual int SkillLevel => GetCurrentTypeStat()?.baseStat?.skillLevel ?? 1;
    public virtual int MaxSkillLevel => GetCurrentTypeStat()?.baseStat?.maxSkillLevel ?? 1;

    protected virtual void Awake()
    {
        InitializeSkillData();
    }

    protected virtual void InitializeSkillData()
    {
        if (skillData == null)
        {
            skillData = new SkillData
            {
                metadata = new SkillMetadata
                {
                    Type = GetSkillType(),
                    Element = ElementType.None,
                    Tier = 1
                }
            };
        }
    }

    protected virtual SkillType GetSkillType()
    {
        return SkillType.None;
    }

    protected T GetTypeStats<T>() where T : class, ISkillStat
    {
        try
        {
            return skillData?.GetCurrentTypeStat() as T;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to get type stats for {typeof(T)}: {e.Message}");
            return null;
        }
    }

    public abstract bool SkillLevelUpdate(int newLevel);
}