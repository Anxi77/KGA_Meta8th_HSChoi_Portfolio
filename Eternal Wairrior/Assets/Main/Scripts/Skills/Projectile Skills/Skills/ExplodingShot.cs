using UnityEngine;

public class ExplodingShot : ProjectileSkills
{
    protected override void Start()
    {
        base.Start();
        InitializeSkillStats();
    }

    protected override void Fire()
    {
        Projectile proj = PoolManager.Instance.Spawn<Projectile>(
            skillData.projectile,
            transform.position,
            transform.rotation
        );

        if (proj != null)
        {
            InitializeProjectile(proj);
            proj.transform.localScale = Vector3.one * ProjectileScale;
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
                    skillName = "Missile Launcher",
                    skillLevel = 1,
                    maxSkillLevel = 5,
                    element = ElementType.Fire,
                    elementalPower = _elementalPower
                },
                projectileSpeed = _projectileSpeed,
                projectileScale = _projectileScale,
                shotInterval = _shotInterval,
                pierceCount = _pierceCount,
                attackRange = _attackRange,
                homingRange = _homingRange,
                isHoming = _isHoming,
                explosionRad = _explosionRadius,
                projectileCount = _projectileCount,
                innerInterval = _innerInterval
            };
            skillData.SetStatsForLevel(1, stats);
        }
    }
}
