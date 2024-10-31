using UnityEngine;

public class SkillStatData
{
    public SkillID skillID;
    public int level;

    // 기본 스탯
    public float damage;
    public int maxSkillLevel;
    public ElementType element;
    public float elementalPower;

    // 발사체 스킬 스탯
    public float projectileSpeed;
    public float projectileScale;
    public float shotInterval;
    public int pierceCount;
    public float attackRange;
    public float homingRange;
    public bool isHoming;
    public float explosionRad;
    public int projectileCount;
    public float innerInterval;

    // 영역 스킬 스탯
    public float radius;
    public float duration;
    public float tickRate;
    public bool isPersistent;
    public float moveSpeed;

    // 패시브 스킬 스탯
    public float effectDuration;
    public float cooldown;
    public float triggerChance;

    // CSV 데이터로부터 스탯 객체 생성
    public ISkillStat CreateSkillStat(SkillType skillType)
    {
        switch (skillType)
        {
            case SkillType.Projectile:
                var projStat = new ProjectileSkillStat();
                projStat.baseStat = CreateBaseStats();

                projStat.projectileSpeed = projectileSpeed;
                projStat.projectileScale = projectileScale;
                projStat.shotInterval = shotInterval;
                projStat.pierceCount = pierceCount;
                projStat.attackRange = attackRange;
                projStat.homingRange = homingRange;
                projStat.isHoming = isHoming;
                projStat.explosionRad = explosionRad;
                projStat.projectileCount = projectileCount;
                projStat.innerInterval = innerInterval;

                return projStat;

            case SkillType.Area:
                var areaStat = new AreaSkillStat();
                areaStat.baseStat = CreateBaseStats();

                areaStat.radius = radius;
                areaStat.duration = duration;
                areaStat.tickRate = tickRate;
                areaStat.isPersistent = isPersistent;
                areaStat.moveSpeed = moveSpeed;

                return areaStat;

            case SkillType.Passive:
                var passiveStat = new PassiveSkillStat();
                passiveStat.baseStat = CreateBaseStats();

                passiveStat.effectDuration = effectDuration;
                passiveStat.cooldown = cooldown;
                passiveStat.triggerChance = triggerChance;

                return passiveStat;

            default:
                Debug.LogError($"Invalid skill type: {skillType}");
                return null;
        }
    }

    private BaseSkillStat CreateBaseStats()
    {
        return new BaseSkillStat
        {
            damage = damage,
            maxSkillLevel = maxSkillLevel,
            skillLevel = level,
            element = element,
            elementalPower = elementalPower
        };
    }

    // 기본값으로 초기화하는 생성자
    public SkillStatData()
    {
        // 기본 스탯 초기화
        damage = 10f;
        maxSkillLevel = 5;
        level = 1;
        element = ElementType.None;
        elementalPower = 1f;

        // 발사체 스킬 스탯 초기화
        projectileSpeed = 10f;
        projectileScale = 1f;
        shotInterval = 0.5f;
        pierceCount = 1;
        attackRange = 10f;
        homingRange = 5f;
        isHoming = false;
        explosionRad = 0f;
        projectileCount = 1;
        innerInterval = 0.1f;

        // 영역 스킬 스탯 초기화
        radius = 3f;
        duration = 5f;
        tickRate = 1f;
        isPersistent = false;
        moveSpeed = 0f;

        // 패시브 스킬 스탯 초기화
        effectDuration = 5f;
        cooldown = 10f;
        triggerChance = 0.5f;
    }
}