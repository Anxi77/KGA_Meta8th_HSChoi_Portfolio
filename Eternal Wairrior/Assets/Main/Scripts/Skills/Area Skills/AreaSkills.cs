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
            skillData.metadata.Type = SkillType.Area;
        }
    }

    protected AreaSkillStat TypedStats
    {
        get
        {
            var stats = skillData?.GetStatsForLevel(SkillLevel) as AreaSkillStat;
            if (stats == null)
            {
                stats = new AreaSkillStat
                {
                    baseStat = new BaseSkillStat
                    {
                        damage = _damage,
                        skillLevel = 1,
                        maxSkillLevel = 5,
                        element = skillData?.metadata.Element ?? ElementType.None,
                        elementalPower = 1f
                    },
                    radius = _radius,
                    duration = _duration,
                    tickRate = _tickRate,
                    isPersistent = _isPersistent,
                    moveSpeed = _moveSpeed
                };
                skillData?.SetStatsForLevel(1, stats);
            }
            return stats;
        }
    }

    [Header("Inspector Controls")]
    [SerializeField] protected float _damage = 10f;
    [SerializeField] protected float _radius = 5f;
    [SerializeField] protected float _duration = 5f;
    [SerializeField] protected float _tickRate = 0.1f;
    [SerializeField] protected bool _isPersistent = true;
    [SerializeField] protected float _moveSpeed = 180f;

    public override float Damage => _damage;
    public float Radius => _radius;
    public float Duration => _duration;
    public float TickRate => _tickRate;
    public bool IsPersistent => _isPersistent;
    public float MoveSpeed => _moveSpeed;

    #region Skill Level Update
    public override bool SkillLevelUpdate(int newLevel)
    {
        if (newLevel <= MaxSkillLevel)
        {
            var newStats = SkillDataManager.Instance.GetSkillStatsForLevel(skillData.metadata.ID, newLevel, SkillType.Area);
            if (newStats != null)
            {
                skillData.SetStatsForLevel(newLevel, newStats);
                UpdateInspectorValues(newStats as AreaSkillStat);
                return true;
            }
        }
        return false;
    }

    protected virtual void UpdateInspectorValues(AreaSkillStat stats)
    {
        if (stats != null)
        {
            _damage = stats.baseStat.damage;
            _radius = stats.radius;
            _duration = stats.duration;
            _tickRate = stats.tickRate;
            _isPersistent = stats.isPersistent;
            _moveSpeed = stats.moveSpeed;
        }
    }
    #endregion
}