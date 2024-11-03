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
    public float damageIncrease;
    public float defenseIncrease;
    public float expAreaIncrease;
    public bool homingActivate;
    public float hpIncrease;
    public float moveSpeedIncrease;
    public float attackSpeedIncrease;
    public float attackRangeIncrease;
    public float hpRegenIncrease;

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
                    triggerChance = triggerChance,
                    damageIncrease = damageIncrease,
                    defenseIncrease = defenseIncrease,
                    expAreaIncrease = expAreaIncrease,
                    homingActivate = homingActivate,
                    hpIncrease = hpIncrease,
                    moveSpeedIncrease = moveSpeedIncrease,
                    attackSpeedIncrease = attackSpeedIncrease,
                    attackRangeIncrease = attackRangeIncrease,
                    hpRegenIncrease = hpRegenIncrease
                };

            default:
                throw new System.ArgumentException($"Invalid skill type: {skillType}");
        }
    }

    public bool ValidateStats()
    {
        // 기본 스탯 검증
        if (level <= 0)
        {
            Debug.LogError($"Invalid level value for skill {skillID}: {level}");
            return false;
        }
        if (damage < 0)
        {
            Debug.LogError($"Invalid damage value for skill {skillID}: {damage}");
            return false;
        }
        if (maxSkillLevel <= 0)
        {
            Debug.LogError($"Invalid max skill level for skill {skillID}: {maxSkillLevel}");
            return false;
        }

        // 스킬 타입별 검증
        var skillData = SkillDataManager.Instance.GetSkillData(skillID);
        if (skillData == null)
        {
            Debug.LogError($"Could not find skill data for {skillID}");
            return false;
        }

        switch (skillData.metadata.Type)
        {
            case SkillType.Projectile:
                return ValidateProjectileStats();
            case SkillType.Area:
                return ValidateAreaStats();
            case SkillType.Passive:
                return ValidatePassiveStats();
            default:
                Debug.LogError($"Unknown skill type for {skillID}");
                return false;
        }
    }

    private bool ValidateProjectileStats()
    {
        if (projectileSpeed < 0)
        {
            Debug.LogError($"Invalid projectile speed for skill {skillID}: {projectileSpeed}");
            return false;
        }
        if (projectileScale <= 0)
        {
            Debug.LogError($"Invalid projectile scale for skill {skillID}: {projectileScale}");
            return false;
        }
        if (shotInterval < 0)
        {
            Debug.LogError($"Invalid shot interval for skill {skillID}: {shotInterval}");
            return false;
        }
        if (pierceCount < 0)
        {
            Debug.LogError($"Invalid pierce count for skill {skillID}: {pierceCount}");
            return false;
        }
        if (attackRange <= 0)
        {
            Debug.LogError($"Invalid attack range for skill {skillID}: {attackRange}");
            return false;
        }
        if (homingRange < 0)
        {
            Debug.LogError($"Invalid homing range for skill {skillID}: {homingRange}");
            return false;
        }
        if (explosionRad < 0)
        {
            Debug.LogError($"Invalid explosion radius for skill {skillID}: {explosionRad}");
            return false;
        }
        if (projectileCount <= 0)
        {
            Debug.LogError($"Invalid projectile count for skill {skillID}: {projectileCount}");
            return false;
        }
        if (innerInterval < 0)
        {
            Debug.LogError($"Invalid inner interval for skill {skillID}: {innerInterval}");
            return false;
        }
        return true;
    }

    private bool ValidateAreaStats()
    {
        if (radius <= 0)
        {
            Debug.LogError($"Invalid radius for skill {skillID}: {radius}");
            return false;
        }
        if (duration <= 0)
        {
            Debug.LogError($"Invalid duration for skill {skillID}: {duration}");
            return false;
        }
        if (tickRate <= 0)
        {
            Debug.LogError($"Invalid tick rate for skill {skillID}: {tickRate}");
            return false;
        }
        if (moveSpeed < 0)
        {
            Debug.LogError($"Invalid move speed for skill {skillID}: {moveSpeed}");
            return false;
        }
        return true;
    }

    private bool ValidatePassiveStats()
    {
        if (effectDuration < 0)
        {
            Debug.LogError($"Invalid effect duration for skill {skillID}: {effectDuration}");
            return false;
        }
        if (cooldown < 0)
        {
            Debug.LogError($"Invalid cooldown for skill {skillID}: {cooldown}");
            return false;
        }
        if (triggerChance < 0 || triggerChance > 100)
        {
            Debug.LogError($"Invalid trigger chance for skill {skillID}: {triggerChance}");
            return false;
        }
        if (damageIncrease < 0)
        {
            Debug.LogError($"Invalid damage increase for skill {skillID}: {damageIncrease}");
            return false;
        }
        if (defenseIncrease < 0)
        {
            Debug.LogError($"Invalid defense increase for skill {skillID}: {defenseIncrease}");
            return false;
        }
        if (expAreaIncrease < 0)
        {
            Debug.LogError($"Invalid exp area increase for skill {skillID}: {expAreaIncrease}");
            return false;
        }
        if (hpIncrease < 0)
        {
            Debug.LogError($"Invalid HP increase for skill {skillID}: {hpIncrease}");
            return false;
        }
        if (moveSpeedIncrease < 0)
        {
            Debug.LogError($"Invalid move speed increase for skill {skillID}: {moveSpeedIncrease}");
            return false;
        }
        if (attackSpeedIncrease < 0)
        {
            Debug.LogError($"Invalid attack speed increase for skill {skillID}: {attackSpeedIncrease}");
            return false;
        }
        if (attackRangeIncrease < 0)
        {
            Debug.LogError($"Invalid attack range increase for skill {skillID}: {attackRangeIncrease}");
            return false;
        }
        if (hpRegenIncrease < 0)
        {
            Debug.LogError($"Invalid HP regen increase for skill {skillID}: {hpRegenIncrease}");
            return false;
        }

        return true;
    }
}