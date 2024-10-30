using Lean.Pool;
using UnityEngine;

public class MissileLauncher : ProjectileSkills
{
    public Projectile missilePrefab;

    protected override void Start()
    {
        base.Start();
        InitializeSkillStats();
    }

    protected override void Fire()
    {
        Projectile proj = LeanPool.Spawn(missilePrefab, transform.position, transform.rotation)
            .GetComponent<Projectile>();

        proj.damage = Damage;
        proj.moveSpeed = ProjectileSpeed;
        proj.isHoming = IsHoming;
        proj.maxTravelDistance = AttackRange;
        proj.transform.localScale = Vector3.one * ProjectileScale;
        proj.elementType = currentStats.baseStat.element;
        proj.elementalPower = currentStats.baseStat.elementalPower;
    }

    private void InitializeSkillStats()
    {
        if (currentStats == null)
        {
            var stats = new ProjectileSkillStat
            {
                baseStat = new BaseSkillStat
                {
                    damage = 10f,
                    skillName = "Missile Launcher",
                    skillLevel = 1,
                    maxSkillLevel = 5,
                    element = ElementType.Fire,
                    elementalPower = 1.2f
                },
                projectileSpeed = 25f,
                projectileScale = 1f,
                shotInterval = 0.5f,
                pierceCount = 1,
                attackRange = 6f,
                homingRange = 3.5f,
                isHoming = true,
                explosionRad = 1.8f,
                projectileCount = 1,
                innerInterval = 0.5f
            };
            currentStats = stats;
        }
    }
}
