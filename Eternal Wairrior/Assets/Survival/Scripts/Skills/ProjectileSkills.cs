using Lean.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEditor.PackageManager.Requests;
using Unity.VisualScripting;

public abstract class ProjectileSkills : Skill
{


    protected virtual void Start() 
    {
        StartCoroutine(FireCoroutine());
    }

    protected virtual void Update() 
    {
        CalcDirection();
        if (Input.GetMouseButtonDown(0) && !isHoming)
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
                if (AreEnemiesInRange() && isHoming)
                {
                    for (int i = 0; i < projectileCount; i++)
                    {
                        if (AreEnemiesInRange())
                        {
                            Fire();
                            yield return new WaitForSeconds(innerInterval);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            yield return new WaitForSeconds(shotInterval);
        }
    }
    protected virtual void Fire()
    {
        Projectile proj = LeanPool.Spawn(projectile,transform.position,transform.rotation).
            GetComponent<Projectile>();
        Vector3 spawnPosition = transform.position + transform.up * 0.5f;
        proj.transform.SetPositionAndRotation(spawnPosition, transform.rotation);
        proj.damage = damage;
        proj.moveSpeed = projectileSpeed;
        proj.isHoming = isHoming;
        proj.transform.localScale *= projectileScale;
        proj.pierceCount = pierceCount;
        proj.maxTravelDistance = attackRange;
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
            if (Vector2.Distance(transform.position, enemy.transform.position) <= homingRange)
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
            if (distance < nearestDistance && distance <= homingRange)
            {
                nearestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        return nearestEnemy;
    }
    #endregion

    #region Skill Level Update
    public bool ProjectileSkillLevelUpdate(int skillLevel)
    {
        if (skillLevel < maxSkillLevel)
        {
            damage = damageByLevel[skillLevel];
            projectileSpeed = projectileSpeedByLevel[skillLevel];
            projectileCount = projectileCountByLevel[skillLevel];
            pierceCount = pierceCountByLevel[skillLevel];
            attackRange = attackRangeByLevel[skillLevel];
            innerInterval = innerIntervalByLevel[skillLevel];
            projectileScale = projectileScaleByLevel[skillLevel];
            homingRange = homingRangeByLevel[skillLevel];
            explosionRad = explosionRadByLevel[skillLevel];
            if (isHoming)
            {
                homingRange = homingRangeByLevel[skillLevel];
            }
            return true;
        }
        return false;
    }
    #endregion

}
