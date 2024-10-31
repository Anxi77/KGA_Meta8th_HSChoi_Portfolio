using UnityEngine;
using System.Collections;

public abstract class AreaSkills : Skill
{
    protected override void Awake()
    {
        base.Awake();
        if (skillData == null)
        {
            skillData = new SkillData();
        }
        skillData._SkillType = SkillType.Area;
    }

    protected AreaSkillStat TypedStats => GetTypeStats<AreaSkillStat>();
    public float Radius => TypedStats?.radius ?? 5f;
    public float Duration => TypedStats?.duration ?? 5f;
    public float TickRate => TypedStats?.tickRate ?? 0.1f;
    public bool IsPersistent => TypedStats?.isPersistent ?? true;
    public float MoveSpeed => TypedStats?.moveSpeed ?? 0f;

    #region Skill Level Update
    public override bool SkillLevelUpdate(int newLevel)
    {
        if (newLevel <= MaxSkillLevel)
        {
            var newStats = SkillDataManager.Instance.GetSkillStatsForLevel(SkillID, newLevel, SkillType.Area);
            if (newStats != null)
            {
                currentStats = newStats;
                return true;
            }
        }
        return false;
    }
    #endregion
}