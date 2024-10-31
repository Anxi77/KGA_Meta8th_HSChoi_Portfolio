    using UnityEngine;

public class DefaultGun : ProjectileSkills
{
    protected override void Start()
    {
        base.Start();
        InitializeSkillStats();
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
                    skillName = "Default Gun",
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
                projectileCount = 1,
                innerInterval = 0.5f
            };
            currentStats = stats;
        }
    }
}
