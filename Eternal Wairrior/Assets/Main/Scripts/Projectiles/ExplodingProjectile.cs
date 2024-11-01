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

            var projectile = (ExplodingProjectile)baseProjectile;
            if (!other.CompareTag("Enemy")) return;

            hasExploded = true;
            projectile.StartCoroutine(projectile.ExplodeCoroutine());
        }
    }

    // ... 나머지 코드는 동일 ...
}