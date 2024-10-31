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

    private void InitializeSkillData()
    {
        if (skillData == null)
        {
            skillData = new SkillData
            {
                _SkillID = SkillID.None,
                Name = "Multi Shot",
                Description = "Fires multiple projectiles"
            };
        }
    }

    protected override void Fire()
    {
        //Debug.Log($"MultiShot Fire() - Base Damage from currentStats: {currentStats.baseStat.damage}");
        //Debug.Log($"MultiShot Fire() - Final Damage property: {Damage}");

        foreach (var shotPoint in shotPoints)
        {
            Projectile proj = ProjectilePool.Instance.SpawnProjectile(
                skillData.projectile,
                shotPoint.position,
                transform.rotation
            );

            proj.damage = Damage;
            //Debug.Log($"Projectile created with damage: {proj.damage}");

            proj.moveSpeed = ProjectileSpeed;
            proj.isHoming = IsHoming;
            proj.transform.localScale *= ProjectileScale;
            proj.pierceCount = PierceCount;
            proj.maxTravelDistance = AttackRange;
            proj.elementType = currentStats.baseStat.element;
            proj.elementalPower = currentStats.baseStat.elementalPower;
        }
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
                    skillName = "Multi Shot",
                    skillLevel = 1,
                    maxSkillLevel = 5,
                    element = ElementType.None,
                    elementalPower = 1f
                },
                projectileSpeed = 25f,
                projectileScale = 1f,
                shotInterval = 0.5f,
                pierceCount = 1,
                attackRange = 6f,
                homingRange = 3.5f,
                isHoming = false,
                projectileCount = 3,
                innerInterval = 0.5f
            };

            currentStats = stats;
        }
    }
}

