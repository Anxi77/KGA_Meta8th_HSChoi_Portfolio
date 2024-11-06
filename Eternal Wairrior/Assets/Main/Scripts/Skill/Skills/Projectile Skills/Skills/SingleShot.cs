using UnityEngine;

public class SingleShot : ProjectileSkills
{
    protected override void Start()
    {
        base.Start();
        InitializeSkillStats();
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
                    skillName = "Default Gun",
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

    protected override void Fire()
    {
        Vector3 spawnPosition = transform.position + transform.up * 0.5f;

        Projectile proj = PoolManager.Instance.Spawn<Projectile>(
            skillData.projectile,
            spawnPosition,
            transform.rotation
        );

        if (proj != null)
        {
            InitializeProjectile(proj);
        }
    }

    public override string GetDetailedDescription()
    {
        string baseDesc = "Basic projectile attack that fires single shots";
        if (skillData?.GetCurrentTypeStat() != null)
        {
            baseDesc += $"\n\nCurrent Effects:" +
                       $"\nDamage: {Damage:F1}" +
                       $"\nFire Rate: {1 / ShotInterval:F1} shots/s" +
                       $"\nRange: {AttackRange:F1}" +
                       $"\nPierce: {PierceCount}";

            if (IsHoming)
            {
                baseDesc += $"\nHoming Range: {HomingRange:F1}";
            }
        }
        return baseDesc;
    }

    protected override string GetDefaultSkillName() => "Default Gun";
    protected override string GetDefaultDescription() => "Basic projectile attack that fires single shots";

    protected override SkillType GetSkillType() => SkillType.Projectile;
}
