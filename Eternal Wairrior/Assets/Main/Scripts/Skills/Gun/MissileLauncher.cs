using Lean.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MissileLauncher : ProjectileSkills
{
    public Projectile missilePrefab;


    protected override void Start()
    {
        StartCoroutine(FireCoroutine());
        InitializeSkillLevels();
    }

    protected override void Update()
    {
        CalcDirection();
        if (Input.GetMouseButtonDown(0) && !isHoming)
        {
            Fire();
        }
    }

    protected override void Fire()
    {
        MissileProjectile proj = LeanPool.Spawn(missilePrefab, transform.position, transform.rotation).GetComponent<MissileProjectile>();
        proj.damage = damage;
        proj.moveSpeed = projectileSpeed;
        proj.isHoming = isHoming;
        proj.maxTravelDistance = attackRange;
        proj.transform.localScale = new Vector3(1, 1, 1);
        proj.transform.localScale *= projectileScale;
        proj.explosionRad = explosionRad;

    }

    public void InitializeSkillLevels()
    {
        damageByLevel = new List<float> { 10f, 20f, 30f, 40f, 50f };
        projectileSpeedByLevel = new List<float> { 25f, 35f, 45f, 50f, 60f };
        projectileCountByLevel = new List<int> { 1, 2, 3, 4, 5 };
        attackRangeByLevel = new List<float> { 6f, 9f, 12f, 15f, 18f };
        innerIntervalByLevel = new List<float> { 0.5f, 0.4f, 0.3f, 0.2f, 0.1f };
        projectileScaleByLevel = new List<float> { 1f, 1.2f, 1.5f, 1.8f, 2.1f };
        homingRangeByLevel = new List<float> { 3.5f, 6.2f, 8.1f, 3.2f, 8.5f };
        explosionRadByLevel = new List<float> { 1.8f, 2.5f, 3.1f, 3.5f, 5.2f };
    }




}
