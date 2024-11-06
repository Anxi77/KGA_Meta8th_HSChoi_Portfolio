using UnityEngine;

public class MultiShot : ProjectileSkills
{
    public Transform[] shotPoints;

    protected override void Start()
    {
        InitializeSkillData();
        base.Start();
        InitializeSkillStats();
    }

    protected override void InitializeSkillData()
    {
        if (skillData == null)
        {
            skillData = new SkillData
            {
                metadata = new SkillMetadata
                {
                    Name = "Multi Shot",
                    Description = "Fires multiple projectiles",
                    Type = SkillType.Projectile,
                    Element = ElementType.None,
                    Tier = 1
                }
            };
        }
    }

    protected override void Fire()
    {
        foreach (var shotPoint in shotPoints)
        {
            Projectile proj = PoolManager.Instance.Spawn<Projectile>(
                skillData.projectile,
                shotPoint.position,
                transform.rotation
            );

            if (proj != null)
            {
                InitializeProjectile(proj);
            }
        }
    }

    private void InitializeSkillStats()
    {
        if (skillData.GetStatsForLevel(1) == null)
        {
            var stats = new ProjectileSkillStat
            {
                baseStat = new BaseSkillStat
                {
                    damage = _damage,
                    skillName = skillData.metadata.Name,
                    skillLevel = 1,
                    maxSkillLevel = 5,
                    element = skillData.metadata.Element,
                    elementalPower = _elementalPower
                },
                projectileSpeed = _projectileSpeed,
                projectileScale = _projectileScale,
                shotInterval = _shotInterval,
                pierceCount = _pierceCount,
                attackRange = _attackRange,
                homingRange = _homingRange,
                isHoming = _isHoming,
                projectileCount = _projectileCount,
                innerInterval = _innerInterval
            };
            skillData.SetStatsForLevel(1, stats);
        }
    }

    public override string GetDetailedDescription()
    {
        string baseDesc = "Fires multiple projectiles simultaneously";
        if (skillData?.GetCurrentTypeStat() != null)
        {
            baseDesc += $"\n\nCurrent Effects:" +
                       $"\nDamage per Shot: {Damage:F1}" +
                       $"\nNumber of Shots: {shotPoints.Length}" +
                       $"\nFire Rate: {1 / ShotInterval:F1} volleys/s" +
                       $"\nRange: {AttackRange:F1}" +
                       $"\nPierce: {PierceCount}";

            if (IsHoming)
            {
                baseDesc += $"\nHoming Range: {HomingRange:F1}";
            }
        }
        return baseDesc;
    }

    protected override string GetDefaultSkillName() => "Multi Shot";
    protected override string GetDefaultDescription() => "Fires multiple projectiles simultaneously";

    protected override SkillType GetSkillType() => SkillType.Projectile;
}
