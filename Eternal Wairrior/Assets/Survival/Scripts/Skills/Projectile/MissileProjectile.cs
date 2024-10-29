using Lean.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileProjectile : Projectile
{
    public float explosionRad;
    private ParticleSystem projectileParticle;

    protected override void Awake()
    {
        coll = GetComponent<CircleCollider2D>();
        projectileParticle = GetComponentInChildren<ParticleSystem>();
        coll.enabled = true;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (coll != null)
        {
            coll.radius = 0.01f;
        }
    }

    protected override void Update()
    {
        base.Update();       
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        StartCoroutine(ExplodeCoroutine());
    }

    private IEnumerator ExplodeCoroutine()
    {
        moveSpeed = 0;
        projectileParticle.Stop();

        // 임팩트 파티클 생성 및 재생
        ParticleSystem impactInstance = LeanPool.Spawn(impactParticle, transform.position, transform.rotation);
        impactInstance.Play();

        // 파티클 시스템의 실제 크기를 가져옴
        float explosionRadius = GetParticleSystemRadius(impactInstance);

        // 폭발 반경 내의 콜라이더 감지 및 데미지 적용
        Collider2D[] contactedColls = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        Explode(contactedColls);

        // 파티클 지속 시간만큼 대기
        yield return new WaitForSeconds(impactInstance.main.duration);

        // 임팩트 파티클 인스턴스 제거
        LeanPool.Despawn(impactInstance);

        // 미사일 프로젝타일 제거
        LeanPool.Despawn(gameObject);
    }

    private float GetParticleSystemRadius(ParticleSystem particleSystem)
    {
        var main = particleSystem.main;
        var startSize = main.startSize;

        if (startSize.mode == ParticleSystemCurveMode.Constant)
        {
            return startSize.constant / 2f;
        }
        else if (startSize.mode == ParticleSystemCurveMode.TwoConstants)
        {
            return Mathf.Max(startSize.constantMin, startSize.constantMax) / 2f;
        }
        else
        {
            // 다른 모드의 경우 평균값 사용
            return (startSize.constantMin + startSize.constantMax) / 4f;
        }
    }

    private void Explode(Collider2D[] contactedColls)
    {
        foreach (Collider2D contactedColl in contactedColls)
        {
            if (contactedColl.CompareTag("Enemy"))
            {
                Enemy enemy = contactedColl.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    Debug.Log($"폭발 범위 내 적 피해: {contactedColl.name}");
                }
            }
        }
    }

}
