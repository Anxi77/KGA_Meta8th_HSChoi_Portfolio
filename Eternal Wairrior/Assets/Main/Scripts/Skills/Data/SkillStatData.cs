using UnityEngine;

[System.Serializable]
public class SkillStatData
{
    // Basic Info
    public SkillID skillID;
    public int level;

    // Base Stats
    public float damage;
    public int maxSkillLevel;
    public ElementType element;
    public float elementalPower;

    // Projectile Stats
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

    // Area Stats
    public float radius;
    public float duration;
    public float tickRate;
    public bool isPersistent;
    public float moveSpeed;

    // Passive Stats
    public float effectDuration;
    public float cooldown;
    public float triggerChance;

    public ISkillStat CreateSkillStat(SkillType skillType)
    {
        switch (skillType)
        {
            case SkillType.Projectile:
                return new ProjectileSkillStat
                {
                    baseStat = new BaseSkillStat
                    {
                        damage = damage,
                        maxSkillLevel = maxSkillLevel,
                        skillLevel = level,
                        element = element,
                        elementalPower = elementalPower
                    },
                    projectileSpeed = projectileSpeed,
                    projectileScale = projectileScale,
                    shotInterval = shotInterval,
                    pierceCount = pierceCount,
                    attackRange = attackRange,
                    homingRange = homingRange,
                    isHoming = isHoming,
                    explosionRad = explosionRad,
                    projectileCount = projectileCount,
                    innerInterval = innerInterval
                };

            case SkillType.Area:
                return new AreaSkillStat
                {
                    baseStat = new BaseSkillStat
                    {
                        damage = damage,
                        maxSkillLevel = maxSkillLevel,
                        skillLevel = level,
                        element = element,
                        elementalPower = elementalPower
                    },
                    radius = radius,
                    duration = duration,
                    tickRate = tickRate,
                    isPersistent = isPersistent,
                    moveSpeed = moveSpeed
                };

            case SkillType.Passive:
                return new PassiveSkillStat
                {
                    baseStat = new BaseSkillStat
                    {
                        damage = damage,
                        maxSkillLevel = maxSkillLevel,
                        skillLevel = level,
                        element = element,
                        elementalPower = elementalPower
                    },
                    effectDuration = effectDuration,
                    cooldown = cooldown,
                    triggerChance = triggerChance
                };

            default:
                throw new System.ArgumentException($"Invalid skill type: {skillType}");
        }
    }
}