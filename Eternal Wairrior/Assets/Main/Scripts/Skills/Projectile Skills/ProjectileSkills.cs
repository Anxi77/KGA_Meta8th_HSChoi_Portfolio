using Lean.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEditor.PackageManager.Requests;
using Unity.VisualScripting;

public abstract class ProjectileSkills : Skill
{
    private ProjectileStats cachedStats;
    private float lastStatsUpdateTime;
    private const float STATS_UPDATE_INTERVAL = 0.5f; // 스탯 업데이트 간격

    protected override void Awake()
    {
        base.Awake();
        if (skillData == null)
        {
            skillData = new SkillData();
            skillData.metadata.Type = SkillType.Projectile;
        }
    }

    protected ProjectileSkillStat TypedStats => GetTypeStats<ProjectileSkillStat>();

    // Inspector-editable fields
    [Header("Base Stats")]
    [SerializeField] protected float _damage = 10f;
    [SerializeField] protected float _elementalPower = 1f;

    [Header("Projectile Stats")]
    [SerializeField] protected float _projectileSpeed = 25f;
    [SerializeField] protected float _projectileScale = 1f;
    [SerializeField] protected float _shotInterval = 0.5f;
    [SerializeField] protected int _pierceCount = 1;
    [SerializeField] protected float _attackRange = 6f;
    [SerializeField] protected float _homingRange = 3.5f;
    [SerializeField] protected bool _isHoming = false;
    [SerializeField] protected float _explosionRadius = 0f;
    [SerializeField] protected int _projectileCount = 1;
    [SerializeField] protected float _innerInterval = 0.1f;
    [SerializeField] protected bool _isPersistent;
    [SerializeField] protected float _projectileDuration;

    // Properties using inspector values
    public override float Damage => _damage;
    public float ProjectileSpeed => _projectileSpeed;
    public float ProjectileScale => _projectileScale;
    public float ShotInterval => _shotInterval;
    public int PierceCount => _pierceCount;
    public float AttackRange => _attackRange;
    public float HomingRange => _homingRange;
    public bool IsHoming => _isHoming;
    public float ExplosionRadius => _explosionRadius;
    public int ProjectileCount => _projectileCount;
    public float InnerInterval => _innerInterval;
    public bool IsPersistent => _isPersistent;
    public float ProjectileDuration => _projectileDuration;

    protected virtual void Start()
    {
        StartCoroutine(FireCoroutine());
    }

    protected virtual void Update()
    {
        CalcDirection();
        if (Input.GetMouseButtonDown(0) && !IsHoming)
        {
            Fire();
        }
    }

    #region Fire
    protected virtual IEnumerator FireCoroutine()
    {
        while (true)
        {
            if (GameManager.Instance.enemies != null)
            {
                if (AreEnemiesInRange() && IsHoming)
                {
                    for (int i = 0; i < ProjectileCount; i++)
                    {
                        if (AreEnemiesInRange())
                        {
                            Fire();
                            yield return new WaitForSeconds(InnerInterval);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            yield return new WaitForSeconds(ShotInterval);
        }
    }

    protected virtual void Fire()
    {
        Vector3 spawnPosition = transform.position + transform.up * 0.5f;

        var pool = GetComponent<ObjectPool>();
        if (pool != null)
        {
            Projectile proj = pool.Spawn<Projectile>(
                skillData.projectile,
                spawnPosition,
                transform.rotation
            );

            if (proj != null)
            {
                InitializeProjectile(proj);
            }
        }
    }

    protected virtual void InitializeProjectile(BaseProjectile proj)
    {
        UpdateProjectileStats();
        proj.Initialize(cachedStats, CreateProjectileBehavior());
    }

    protected virtual void UpdateProjectileStats()
    {
        if (Time.time - lastStatsUpdateTime < STATS_UPDATE_INTERVAL) return;

        cachedStats = new ProjectileStats
        {
            damage = Damage,
            moveSpeed = TypedStats.projectileSpeed,
            scale = TypedStats.projectileScale,
            elementType = skillData.metadata.Element,
            elementalPower = TypedStats.baseStat.elementalPower,
            pierceCount = TypedStats.pierceCount,
            maxTravelDistance = TypedStats.attackRange,
            persistenceData = TypedStats.persistenceData
        };

        lastStatsUpdateTime = Time.time;
    }

    // 객체 풀링 최적화
    private readonly Dictionary<ProjectileBehaviorType, IProjectileBehavior> behaviorPool
        = new Dictionary<ProjectileBehaviorType, IProjectileBehavior>();

    protected virtual IProjectileBehavior CreateProjectileBehavior()
    {
        var behaviorType = TypedStats.isHoming ?
            ProjectileBehaviorType.Homing :
            ProjectileBehaviorType.Standard;

        if (!behaviorPool.TryGetValue(behaviorType, out var behavior))
        {
            behavior = behaviorType == ProjectileBehaviorType.Homing ?
                new HomingProjectileBehavior() :
                new StandardProjectileBehavior();
            behaviorPool[behaviorType] = behavior;
        }

        return behavior;
    }
    #endregion

    #region Enemy Searching Methods
    protected virtual void CalcDirection()
    {
        Vector2 mousePos = Input.mousePosition;
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
        fireDir = (mouseWorldPos - (Vector2)transform.position).normalized;
        transform.up = fireDir;
    }

    protected virtual bool AreEnemiesInRange()
    {
        foreach (Enemy enemy in GameManager.Instance.enemies)
        {
            if (Vector2.Distance(transform.position, enemy.transform.position) <= HomingRange)
            {
                return true;
            }
        }
        return false;
    }

    protected virtual Enemy FindNearestEnemy()
    {
        Enemy nearestEnemy = null;
        float nearestDistance = float.MaxValue;

        foreach (Enemy enemy in GameManager.Instance.enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < nearestDistance && distance <= HomingRange)
            {
                nearestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        return nearestEnemy;
    }
    #endregion

    #region Skill Level Update
    public override bool SkillLevelUpdate(int newLevel)
    {
        if (newLevel <= MaxSkillLevel)
        {
            var newStats = SkillDataManager.Instance.GetSkillStatsForLevel(skillData.metadata.ID, newLevel, SkillType.Projectile);
            if (newStats != null)
            {
                skillData.SetStatsForLevel(newLevel, newStats);
                UpdateInspectorValues(newStats as ProjectileSkillStat);
                return true;
            }
        }
        return false;
    }

    protected virtual void UpdateInspectorValues(ProjectileSkillStat stats)
    {
        if (stats != null)
        {
            _damage = stats.baseStat.damage;
            _elementalPower = stats.baseStat.elementalPower;
            _projectileSpeed = stats.projectileSpeed;
            _projectileScale = stats.projectileScale;
            _shotInterval = stats.shotInterval;
            _pierceCount = stats.pierceCount;
            _attackRange = stats.attackRange;
            _homingRange = stats.homingRange;
            _isHoming = stats.isHoming;
            _explosionRadius = stats.explosionRad;
            _projectileCount = stats.projectileCount;
            _innerInterval = stats.innerInterval;
        }
    }
    #endregion
}
