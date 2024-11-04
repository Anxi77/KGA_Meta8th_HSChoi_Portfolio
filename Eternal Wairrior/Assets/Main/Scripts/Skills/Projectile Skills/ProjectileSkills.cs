using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.PackageManager.Requests;

public abstract class ProjectileSkills : Skill
{
    protected override void Awake()
    {
        base.Awake();
        if (skillData == null)
        {
            skillData = new SkillData();
            skillData.metadata.Type = SkillType.Projectile;
        }
    }

    protected ProjectileSkillStat TypedStats
    {
        get
        {
            var stats = skillData?.GetStatsForLevel(SkillLevel) as ProjectileSkillStat;
            if (stats == null)
            {
                stats = new ProjectileSkillStat
                {
                    baseStat = new BaseSkillStat
                    {
                        damage = _damage,
                        skillLevel = 1,
                        maxSkillLevel = 5,
                        element = skillData?.metadata.Element ?? ElementType.None,
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
                skillData?.SetStatsForLevel(1, stats);
            }
            return stats;
        }
    }

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

    public override float Damage => _damage;
    public float ElementalPower => _elementalPower;
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

    protected virtual void InitializeProjectile(Projectile proj)
    {
        proj.damage = Damage;
        proj.moveSpeed = ProjectileSpeed;
        proj.isHoming = IsHoming;
        proj.transform.localScale *= ProjectileScale;
        proj.pierceCount = PierceCount;
        proj.maxTravelDistance = AttackRange;
        proj.elementType = skillData.metadata.Element;
        proj.elementalPower = TypedStats.baseStat.elementalPower;

        proj.SetInitialTarget(FindNearestEnemy());
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

    public virtual void UpdateHomingState(bool activate)
    {
        _isHoming = activate;

        if (!activate)
        {
            _homingRange = 0f;
        }
        else
        {
            _homingRange = TypedStats.homingRange;
        }

        Debug.Log($"Homing state updated for {skillData.metadata.Name}: {activate}");
    }

    protected override void UpdateSkillTypeStats(ISkillStat newStats)
    {
        if (newStats is ProjectileSkillStat projectileStats)
        {
            UpdateInspectorValues(projectileStats);
        }
    }

    protected virtual void UpdateInspectorValues(ProjectileSkillStat stats)
    {
        if (stats == null || stats.baseStat == null)
        {
            Debug.LogError($"Invalid stats passed to UpdateInspectorValues for {GetType().Name}");
            return;
        }

        Debug.Log($"[ProjectileSkills] Before Update - Level: {_skillLevel}");

        _skillLevel = stats.baseStat.skillLevel;

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

        Debug.Log($"[ProjectileSkills] After Update - Level: {_skillLevel}");
    }

    public override string GetDetailedDescription()
    {
        string baseDesc = skillData?.metadata?.Description ?? "Projectile skill description";
        if (skillData?.GetCurrentTypeStat() != null)
        {
            baseDesc += $"\n\nCurrent Effects:" +
                       $"\nDamage: {Damage:F1}" +
                       $"\nProjectile Speed: {ProjectileSpeed:F1}" +
                       $"\nShot Interval: {ShotInterval:F1}s" +
                       $"\nPierce Count: {PierceCount}" +
                       $"\nAttack Range: {AttackRange:F1}" +
                       $"\nProjectile Count: {ProjectileCount}";

            if (IsHoming)
            {
                baseDesc += $"\nHoming Range: {HomingRange:F1}";
            }

            if (ExplosionRadius > 0)
            {
                baseDesc += $"\nExplosion Radius: {ExplosionRadius:F1}";
            }
        }
        return baseDesc;
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        if (Application.isPlaying && skillData != null)
        {
            var currentStats = TypedStats;

            currentStats.baseStat.damage = _damage;
            currentStats.baseStat.skillLevel = _skillLevel;
            currentStats.baseStat.elementalPower = _elementalPower;
            currentStats.projectileSpeed = _projectileSpeed;
            currentStats.projectileScale = _projectileScale;
            currentStats.shotInterval = _shotInterval;
            currentStats.pierceCount = _pierceCount;
            currentStats.attackRange = _attackRange;
            currentStats.homingRange = _homingRange;
            currentStats.isHoming = _isHoming;
            currentStats.explosionRad = _explosionRadius;
            currentStats.projectileCount = _projectileCount;
            currentStats.innerInterval = _innerInterval;

            _damage = currentStats.baseStat.damage;
            _skillLevel = currentStats.baseStat.skillLevel;
            _elementalPower = currentStats.baseStat.elementalPower;
            _projectileSpeed = currentStats.projectileSpeed;
            _projectileScale = currentStats.projectileScale;
            _shotInterval = currentStats.shotInterval;
            _pierceCount = currentStats.pierceCount;
            _attackRange = currentStats.attackRange;
            _homingRange = currentStats.homingRange;
            _isHoming = currentStats.isHoming;
            _explosionRadius = currentStats.explosionRad;
            _projectileCount = currentStats.projectileCount;
            _innerInterval = currentStats.innerInterval;

            skillData.SetStatsForLevel(SkillLevel, currentStats);
            Debug.Log($"Updated stats for {GetType().Name} from inspector");
        }
    }
}
