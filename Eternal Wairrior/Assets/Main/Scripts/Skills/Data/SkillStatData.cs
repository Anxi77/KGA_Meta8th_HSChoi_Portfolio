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
    public float effectDuration = 5f;
    public float cooldown = 10f;
    public float triggerChance = 100f;
    public float damageIncrease = 0f;
    public float defenseIncrease = 0f;
    public float expAreaIncrease = 0f;
    public bool homingActivate = false;
    public float hpIncrease = 0f;
    public float moveSpeedIncrease = 0f;
    public float attackSpeedIncrease = 0f;
    public float attackRangeIncrease = 0f;
    public float hpRegenIncrease = 0f;

    public SkillStatData()
    {
        // 기본값 초기화
        skillID = SkillID.None;
        level = 1;
        damage = 10f;
        maxSkillLevel = 5;
        element = ElementType.None;
        elementalPower = 1f;

        // Projectile 기본값
        projectileSpeed = 10f;
        projectileScale = 1f;
        shotInterval = 1f;
        pierceCount = 1;
        attackRange = 10f;
        homingRange = 5f;
        isHoming = false;
        explosionRad = 0f;
        projectileCount = 1;
        innerInterval = 0.1f;

        // Area 기본값
        radius = 5f;
        duration = 3f;
        tickRate = 1f;
        isPersistent = false;
        moveSpeed = 0f;

        // Passive 기본값
        effectDuration = 5f;
        cooldown = 10f;
        triggerChance = 100f;
        damageIncrease = 0f;
        defenseIncrease = 0f;
        expAreaIncrease = 0f;
        homingActivate = false;
        hpIncrease = 0f;
        moveSpeedIncrease = 0f;
        attackSpeedIncrease = 0f;
        attackRangeIncrease = 0f;
        hpRegenIncrease = 0f;
    }

    public SkillStatData Clone()
    {
        return new SkillStatData
        {
            skillID = this.skillID,
            level = this.level,
            damage = this.damage,
            maxSkillLevel = this.maxSkillLevel,
            element = this.element,
            elementalPower = this.elementalPower,

            projectileSpeed = this.projectileSpeed,
            projectileScale = this.projectileScale,
            shotInterval = this.shotInterval,
            pierceCount = this.pierceCount,
            attackRange = this.attackRange,
            homingRange = this.homingRange,
            isHoming = this.isHoming,
            explosionRad = this.explosionRad,
            projectileCount = this.projectileCount,
            innerInterval = this.innerInterval,

            radius = this.radius,
            duration = this.duration,
            tickRate = this.tickRate,
            isPersistent = this.isPersistent,
            moveSpeed = this.moveSpeed,

            effectDuration = this.effectDuration,
            cooldown = this.cooldown,
            triggerChance = this.triggerChance,
            damageIncrease = this.damageIncrease,
            defenseIncrease = this.defenseIncrease,
            expAreaIncrease = this.expAreaIncrease,
            homingActivate = this.homingActivate,
            hpIncrease = this.hpIncrease,
            moveSpeedIncrease = this.moveSpeedIncrease,
            attackSpeedIncrease = this.attackSpeedIncrease,
            attackRangeIncrease = this.attackRangeIncrease,
            hpRegenIncrease = this.hpRegenIncrease
        };
    }

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