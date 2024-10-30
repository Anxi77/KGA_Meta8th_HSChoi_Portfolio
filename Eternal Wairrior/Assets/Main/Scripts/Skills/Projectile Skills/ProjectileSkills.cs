using Lean.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEditor.PackageManager.Requests;
using Unity.VisualScripting;

public abstract class ProjectileSkills : Skill
{
    protected ProjectileSkillStat TypedStats => GetTypeStats<ProjectileSkillStat>();

    // 프로젝타일 전용 스탯 접근자
    public float ProjectileSpeed => TypedStats.projectileSpeed;
    public float ProjectileScale => TypedStats.projectileScale;
    public float ShotInterval => TypedStats.shotInterval;
    public int PierceCount => TypedStats.pierceCount;
    public float AttackRange => TypedStats.attackRange;
    public float HomingRange => TypedStats.homingRange;
    public bool IsHoming => TypedStats.isHoming;
    public float ExplosionRadius => TypedStats.explosionRad;
    public int ProjectileCount => TypedStats.projectileCount;
    public float InnerInterval => TypedStats.innerInterval;

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
        Projectile proj = LeanPool.Spawn(skillData.projectile, transform.position, transform.rotation)
            .GetComponent<Projectile>();

        Vector3 spawnPosition = transform.position + transform.up * 0.5f;
        proj.transform.SetPositionAndRotation(spawnPosition, transform.rotation);

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
            // 스킬 데이터 업데이트
            var updatedSkillData = SkillDataManager.Instance.GetSkillData(SkillID);
            var projectileStats = (ProjectileSkillStat)updatedSkillData.GetCurrentTypeStat();

            // 새로운 스탯 객체 생성
            var newBaseStat = projectileStats.baseStat;
            newBaseStat.skillLevel = newLevel;

            // 새로운 ProjectileSkillStat 생성 및 할당
            var newStats = projectileStats;
            newStats.baseStat = newBaseStat;

            // 현재 스탯 업데이트
            currentStats = newStats;

            // 프리팹 업데이트 (필요한 경우)
            if (newLevel < skillData.prefabsByLevel.Length)
            {
                // 프리팹 교체 로직
            }

            return true;
        }
        return false;
    }
    #endregion

}
