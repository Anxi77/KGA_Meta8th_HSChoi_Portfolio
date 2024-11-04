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

    [Header("Base Stats")]
    [SerializeField] protected float _damage = 10f;
    [SerializeField] protected float _elementalPower = 1f;

    [Header("Area Stats")]
    [SerializeField] protected float _radius = 5f;
    [SerializeField] protected float _duration = 5f;
    [SerializeField] protected float _tickRate = 0.1f;
    [SerializeField] protected bool _isPersistent = true;
    [SerializeField] protected float _moveSpeed = 180f;

    public override float Damage => _damage;
    public float ElementalPower => _elementalPower;
    public float Radius => _radius;
    public float Duration => _duration;
    public float TickRate => _tickRate;
    public bool IsPersistent => _isPersistent;
    public float MoveSpeed => _moveSpeed;

    protected override void UpdateSkillTypeStats(ISkillStat newStats)
    {
        if (newStats is AreaSkillStat areaStats)
        {
            UpdateInspectorValues(areaStats);
        }
    }

    protected virtual void UpdateInspectorValues(AreaSkillStat stats)
    {
        if (stats == null || stats.baseStat == null)
        {
            Debug.LogError($"Invalid stats passed to UpdateInspectorValues for {GetType().Name}");
            return;
        }

        Debug.Log($"[AreaSkills] Before Update - Level: {_skillLevel}");

        // 레벨 업데이트
        _skillLevel = stats.baseStat.skillLevel;  // 인스펙터 값만 업데이트

        // 나머지 스탯 업데이트
        _damage = stats.baseStat.damage;
        _elementalPower = stats.baseStat.elementalPower;
        _radius = stats.radius;
        _duration = stats.duration;
        _tickRate = stats.tickRate;
        _isPersistent = stats.isPersistent;
        _moveSpeed = stats.moveSpeed;

        Debug.Log($"[AreaSkills] After Update - Level: {_skillLevel}");
    }

    public override string GetDetailedDescription()
    {
        string baseDesc = skillData?.metadata?.Description ?? "Area skill description";
        if (skillData?.GetCurrentTypeStat() != null)
        {
            baseDesc += $"\n\nCurrent Effects:" +
                       $"\nDamage: {Damage:F1}" +
                       $"\nRadius: {Radius:F1}" +
                       $"\nDuration: {Duration:F1}s" +
                       $"\nTick Rate: {TickRate:F1}s" +
                       $"\nMove Speed: {MoveSpeed:F1}";
        }
        return baseDesc;
    }

    protected override void OnValidate()
    {
        base.OnValidate();

        if (Application.isPlaying && skillData != null)
        {
            var currentStats = TypedStats;

            currentStats.baseStat.damage = _damage;
            currentStats.baseStat.skillLevel = _skillLevel;
            currentStats.baseStat.elementalPower = _elementalPower;
            currentStats.radius = _radius;
            currentStats.duration = _duration;
            currentStats.tickRate = _tickRate;
            currentStats.isPersistent = _isPersistent;
            currentStats.moveSpeed = _moveSpeed;

            _damage = currentStats.baseStat.damage;
            _skillLevel = currentStats.baseStat.skillLevel;
            _elementalPower = currentStats.baseStat.elementalPower;
            _radius = currentStats.radius;
            _duration = currentStats.duration;
            _tickRate = currentStats.tickRate;
            _isPersistent = currentStats.isPersistent;
            _moveSpeed = currentStats.moveSpeed;

            skillData.SetStatsForLevel(SkillLevel, currentStats);

            Debug.Log($"Updated stats for {GetType().Name} from inspector");
        }
    }
}
