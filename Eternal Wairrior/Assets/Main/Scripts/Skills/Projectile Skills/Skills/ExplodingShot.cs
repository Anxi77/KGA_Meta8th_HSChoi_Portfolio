using Lean.Pool;

using System.Collections;

using System.Collections.Generic;

using UnityEngine;



public class ExplodingProjectile : BaseProjectile

{

    [SerializeField] private ParticleSystem projectileParticle;

    [SerializeField] private ParticleSystem impactParticle;



    protected virtual void Awake()

    {

        if (projectileParticle == null)

        {

            projectileParticle = GetComponentInChildren<ParticleSystem>();

        }

    }



    public class ExplodingBehavior : StandardProjectileBehavior

    {

        private bool hasExploded = false;



        public override void UpdateProjectile(BaseProjectile baseProjectile)

        {

            if (!hasExploded)

            {

                base.UpdateProjectile(baseProjectile);

            }

        }



        public override void OnSpawn(BaseProjectile baseProjectile)

        {

            base.OnSpawn(baseProjectile);

            hasExploded = false;

        }



        public override void OnTriggerEnter2D(BaseProjectile baseProjectile, Collider2D other)

        {

            if (hasExploded) return;



            var projectile = baseProjectile as ExplodingProjectile;

            if (projectile == null || !other.CompareTag("Enemy")) return;



            hasExploded = true;

            projectile.StartCoroutine(HandleExplosion(projectile));

        }



        private IEnumerator HandleExplosion(ExplodingProjectile projectile)

        {

            projectile.stats.moveSpeed = 0;

            projectile.projectileParticle?.Stop();



            if (projectile.impactParticle != null)

            {

                ParticleSystem impactInstance = PoolManager.Instance.Spawn<ParticleSystem>(

                    projectile.impactParticle.gameObject,

                    projectile.transform.position,

                    projectile.transform.rotation

                );



                if (impactInstance != null)

                {

                    impactInstance.Play();

                    float explosionRadius = projectile.GetParticleSystemRadius(impactInstance);

                    projectile.ApplyExplosionEffects(explosionRadius);



                    yield return new WaitForSeconds(impactInstance.main.duration);

                    PoolManager.Instance.Despawn(impactInstance);

                }

            }



            if (projectile.stats.persistenceData.isPersistent)

            {

                yield return new WaitForSeconds(projectile.stats.persistenceData.duration);

            }



            PoolManager.Instance.Despawn(projectile);

        }

    }



    private void ApplyExplosionEffects(float explosionRadius)

    {

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D hitCollider in hitColliders)

        {

            if (hitCollider.TryGetComponent<Enemy>(out Enemy enemy))

            {

                enemy.TakeDamage(stats.damage);



                if (stats.elementType != ElementType.None && stats.elementalPower > 0)

                {

                    ElementalEffects.ApplyElementalEffect(

                        stats.elementType,

                        stats.elementalPower,

                        hitCollider.gameObject

                    );

                }

            }

        }

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

            return (startSize.constantMin + startSize.constantMax) / 4f;

        }

    }



    protected override void ValidateStats()

    {

        base.ValidateStats();



        if (impactParticle == null)

            Debug.LogWarning("Impact particle effect is not assigned!");

    }

}


