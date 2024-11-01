using UnityEngine;

public abstract class AreaSkills : Skill
{
    protected AreaSkillStat TypedStats => GetTypeStats<AreaSkillStat>();

    private AreaStats cachedStats;
    private float lastStatsUpdateTime;
    private const float STATS_UPDATE_INTERVAL = 0.5f;

    [Header("Base Stats")]
    [SerializeField] protected float _damage = 10f;
    [SerializeField] protected float _elementalPower = 1f;

    [Header("Area Stats")]
    [SerializeField] protected float _radius = 3f;
    [SerializeField] protected float _tickRate = 0.5f;
    [SerializeField] protected float _moveSpeed = 0f;
    [SerializeField] protected bool _isPersistent = true;
    [SerializeField] protected float _duration = 3f;

    public float Radius => TypedStats?.radius ?? _radius;
    public float TickRate => TypedStats?.tickRate ?? _tickRate;
    public float MoveSpeed => TypedStats?.moveSpeed ?? _moveSpeed;
    public bool IsPersistent => TypedStats?.persistenceData.isPersistent ?? _isPersistent;
    public float Duration => TypedStats?.persistenceData.duration ?? _duration;

    protected virtual void UpdateAreaStats()
    {
        if (Time.time - lastStatsUpdateTime < STATS_UPDATE_INTERVAL) return;

        cachedStats = new AreaStats
        {
            damage = Damage,
            radius = Radius,
            tickRate = TickRate,
            moveSpeed = MoveSpeed,
            elementType = skillData.metadata.Element,
            elementalPower = TypedStats.baseStat.elementalPower,
            persistenceData = TypedStats.persistenceData
        };

        lastStatsUpdateTime = Time.time;
    }

    public override bool SkillLevelUpdate(int newLevel)
    {
        if (newLevel <= MaxSkillLevel)
        {
            var newStats = SkillDataManager.Instance.GetSkillStatsForLevel(
                skillData.metadata.ID, newLevel, SkillType.Area);

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
            _elementalPower = stats.baseStat.elementalPower;
            _radius = stats.radius;
            _tickRate = stats.tickRate;
            _moveSpeed = stats.moveSpeed;
            _isPersistent = stats.persistenceData.isPersistent;
            _duration = stats.persistenceData.duration;
        }
    }
}