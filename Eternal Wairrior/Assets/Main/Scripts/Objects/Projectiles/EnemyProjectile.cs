using UnityEngine;

public class EnemyProjectile : Projectile
{
    protected Transform playerTarget;

    protected override void Awake()
    {
        base.Awake();
        // Enemy 레이어와의 충돌은 무시하고, Player 레이어와의 충돌만 처리
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Enemy"), true);
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Player"), false);
    }

    public override void OnSpawnFromPool()
    {
        base.OnSpawnFromPool();
        playerTarget = GameManager.Instance.player.transform;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (other.TryGetComponent<Player>(out Player player))
        {
            player.TakeDamage(damage);

            if (impactParticle != null)
            {
                ParticleSystem particle = PoolManager.Instance.Spawn<ParticleSystem>(
                    impactParticle.gameObject,
                    transform.position,
                    Quaternion.identity
                );
                if (particle != null)
                {
                    particle.Play();
                    PoolManager.Instance.Despawn<ParticleSystem>(particle, 0.5f);
                }
            }

            if (elementType != ElementType.None && elementalPower > 0)
            {
                ElementalEffects.ApplyElementalEffect(elementType, elementalPower, other.gameObject);
            }

            PoolManager.Instance.Despawn(this);
        }
    }

    protected override void Homing()
    {
        if (playerTarget != null)
        {
            Vector2 direction = (playerTarget.position - transform.position).normalized;
            transform.up = direction;
            transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
        }
        else
        {
            Move();
        }
    }

    public override void ResetProjectile()
    {
        base.ResetProjectile();
        playerTarget = null;
    }

    public override void Move()
    {
        transform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);
    }
}
