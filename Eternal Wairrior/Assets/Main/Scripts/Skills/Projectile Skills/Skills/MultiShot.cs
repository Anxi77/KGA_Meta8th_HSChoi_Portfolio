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

        base.InitializeSkillData();

    }



    protected override void Fire()

    {

        foreach (var shotPoint in shotPoints)

        {

            BaseProjectile proj = PoolManager.Instance.Spawn<BaseProjectile>(

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

                innerInterval = _innerInterval,

                persistenceData = new ProjectilePersistenceData

                {

                    isPersistent = _isPersistent,

                    duration = _projectileDuration,

                    effectInterval = 0.5f

                }

            };

            skillData.SetStatsForLevel(1, stats);

        }

    }



    protected override void InitializeProjectile(BaseProjectile proj)

    {

        base.InitializeProjectile(proj);

        // 추가 초기화 로직...

    }



    // ų Ʈ Ӽ ȵƮ

    public override bool SkillLevelUpdate(int newLevel)

    {

        bool result = base.SkillLevelUpdate(newLevel);

        if (result)

        {

            var stats = skillData?.GetStatsForLevel(newLevel) as ProjectileSkillStat;

            if (stats != null)

            {

                _isPersistent = stats.persistenceData.isPersistent;

                _projectileDuration = stats.persistenceData.duration;

            }

        }

        return result;

    }

}




