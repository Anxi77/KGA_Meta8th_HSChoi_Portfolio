using Lean.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEditor.PackageManager.Requests;
using Unity.VisualScripting;

public abstract class ProjectileSkills : Skill
{
    protected override void Awake()
    {
        base.Awake();
        if (skillData == null)
        {
            skillData = new SkillData();
        }
        skillData._SkillType = SkillType.Projectile;
    }

    protected ProjectileSkillStat TypedStats => GetTypeStats<ProjectileSkillStat>();
    public float ProjectileSpeed => TypedStats?.projectileSpeed ?? 0f;
    public float ProjectileScale => TypedStats?.projectileScale ?? 1f;
    public float ShotInterval => TypedStats?.shotInterval ?? 0.5f;
    public int PierceCount => TypedStats?.pierceCount ?? 0;
    public float AttackRange => TypedStats?.attackRange ?? 5f;
    public float HomingRange => TypedStats?.homingRange ?? 0f;
    public bool IsHoming => TypedStats?.isHoming ?? false;
    public float ExplosionRadius => TypedStats?.explosionRad ?? 0f;
    public int ProjectileCount => TypedStats?.projectileCount ?? 1;
    public float InnerInterval => TypedStats?.innerInterval ?? 0.1f;

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

        Projectile proj = ProjectilePool.Instance.SpawnProjectile(
            skillData.projectile,
            spawnPosition,
            transform.rotation
        );

        if (proj != null)
        {
            InitializeProjectile(proj);
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
        proj.elementType = currentStats.baseStat.element;
        proj.elementalPower = currentStats.baseStat.elementalPower;

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

    #region Skill Level Update
    public override bool SkillLevelUpdate(int newLevel)
    {
        if (newLevel <= MaxSkillLevel)
        {
            var newStats = SkillDataManager.Instance.GetSkillStatsForLevel(SkillID, newLevel, SkillType.Projectile);
            if (newStats != null)
            {
                currentStats = newStats;
                return true;
            }
        }
        return false;
    }
    #endregion
}
