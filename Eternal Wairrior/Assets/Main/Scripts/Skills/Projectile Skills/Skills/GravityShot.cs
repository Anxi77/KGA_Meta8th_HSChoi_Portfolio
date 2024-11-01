using UnityEngine;
using Lean.Pool;
using System.Collections;
using UnityEngine.EventSystems;

public class GravityProjectileSkill : ProjectileSkills
{
    [Header("Gravity Projectile Settings")]
    [SerializeField] private float _gravityForce = 10f;
    [SerializeField] private float _gravityDamageInterval = 0.5f;
    [SerializeField] private float _startSize = 1f;
    [SerializeField] private float _endSize = 3f;
    [SerializeField] private float _growthDuration = 2f;
    [SerializeField] private GravityProjectile gravityProjectilePrefab;
    [SerializeField] private KeyCode homingTriggerKey = KeyCode.LeftShift;

    private float autoFireTimer = 0f;

    protected override void Start()
    {
        base.Start();

        if (skillData == null)
        {
            skillData = new SkillData();
        }

        if (gravityProjectilePrefab != null && skillData.projectile == null)
        {
            skillData.projectile = gravityProjectilePrefab.gameObject;
        }
    }

    protected override void Fire()
    {
        if (skillData?.projectile == null)
        {
            Debug.LogError("Projectile prefab is not assigned!");
            return;
        }

        Enemy nearestEnemy = FindNearestEnemy();
        if (nearestEnemy == null) return;

        Vector3 spawnPosition = transform.position + transform.forward * 1f;

        Vector2 enemyPosition = nearestEnemy.transform.position;
        Vector2 myPosition = spawnPosition;
        Vector2 directionToEnemy = (enemyPosition - myPosition).normalized;


        var proj = PoolManager.Instance.Spawn<GravityProjectile>(gravityProjectilePrefab.gameObject,
            spawnPosition, Quaternion.identity);

        if (proj != null)
        {
            InitializeProjectile(proj);
            proj.SetDirection(directionToEnemy);

            if (_isHoming)
            {
                proj.SetTarget(nearestEnemy);
                proj.SetHoming(true);
            }
        }
        else
        {
            Debug.LogError("Failed to spawn GravityProjectile!");
        }
    }

    protected override Enemy FindNearestEnemy()
    {
        Enemy nearestEnemy = null;
        float nearestDistance = AttackRange;

        Enemy[] enemies = FindObjectsOfType<Enemy>();
        if (enemies.Length == 0) return null;

        foreach (var enemy in enemies)
        {
            //if (!enemy.gameObject.activeSelf || enemy.IsDead) continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        return nearestEnemy;
    }

    protected override void InitializeProjectile(Projectile proj)
    {
        base.InitializeProjectile(proj);

        if (proj is GravityProjectile gravityProj)
        {
            gravityProj.SetSizeParameters(_startSize, _endSize, _growthDuration);
            gravityProj.SetMaxTravelDistance(AttackRange);
            gravityProj.SetProjectileSpeed(ProjectileSpeed);

            var gravityForceField = gravityProj.GetType().GetField("_gravityForce",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var gravityDamageIntervalField = gravityProj.GetType().GetField("_gravityDamageInterval",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (gravityForceField != null) gravityForceField.SetValue(gravityProj, _gravityForce);
            if (gravityDamageIntervalField != null) gravityDamageIntervalField.SetValue(gravityProj, _gravityDamageInterval);
        }
    }

    protected override void Awake()
    {
        base.Awake();
        if (skillData == null)
        {
            skillData = new SkillData();
            skillData.metadata.Type = SkillType.Projectile;
        }
    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(homingTriggerKey))
        {
            _isHoming = !_isHoming;
            Debug.Log($"Homing mode: {_isHoming}");
        }

        if (autoFireTimer >= ShotInterval)
        {
            Enemy nearestEnemy = FindNearestEnemy();
            if (nearestEnemy != null && Vector2.Distance(transform.position, nearestEnemy.transform.position) <= AttackRange)
            {
                Fire();
                autoFireTimer = 0f;
            }
            autoFireTimer = 0f;
        }
        else
        {
            autoFireTimer += Time.deltaTime;
        }
    }

    protected override void UpdateInspectorValues(ProjectileSkillStat stats)
    {
        base.UpdateInspectorValues(stats);

        float levelMultiplier = 1f + (SkillLevel - 1) * 0.2f;
        _gravityForce *= levelMultiplier;
        _endSize *= levelMultiplier;
    }

}