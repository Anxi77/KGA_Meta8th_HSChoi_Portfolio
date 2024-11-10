using UnityEngine;
using System.Collections;

public class RangedEnemy : Enemy
{
    [Header("Ranged Attack Settings")]
    [SerializeField] private EnemyProjectile projectilePrefab;
    [SerializeField] private float minAttackDistance = 5f;
    [SerializeField] private float maxAttackDistance = 15f;
    [SerializeField] private float attackAnimationDuration = 0.5f;

    private bool isAttacking = false;
    private Animator animator;

    protected override void Start()
    {
        base.Start();
        animator = GetComponentInChildren<Animator>();
        attackRange = maxAttackDistance;
        preferredDistance = minAttackDistance;
    }

    protected override void PerformRangedAttack()
    {
        if (!isAttacking)
        {
            StartCoroutine(RangedAttackCoroutine());
        }
    }

    private IEnumerator RangedAttackCoroutine()
    {
        isAttacking = true;

        animator?.SetTrigger("Attack");

        Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        EnemyProjectile projectile = PoolManager.Instance.Spawn<EnemyProjectile>(
            projectilePrefab.gameObject,
            transform.position,
            Quaternion.Euler(0, 0, angle - 90)
        );

        if (projectile != null)
        {
            projectile.damage = damage;
            projectile.moveSpeed = 10f;
            projectile.maxTravelDistance = maxAttackDistance;
            projectile.SetDirection(direction);
            projectile.gameObject.tag = "EnemyProjectile";

            if (attackParticle != null)
            {
                var particle = Instantiate(attackParticle, transform.position, Quaternion.identity);
                particle.Play();
                Destroy(particle.gameObject, 0.3f);
            }
        }

        preDamageTime = Time.time;
        yield return new WaitForSeconds(attackAnimationDuration);
        isAttacking = false;
    }

    protected override void MoveDirectlyTowardsTarget()
    {
        if (isAttacking) return;

        float distanceToTarget = Vector2.Distance(transform.position, target.position);

        if (distanceToTarget < minAttackDistance)
        {
            Vector2 fleeDirection = ((Vector2)transform.position - (Vector2)target.position).normalized;
            Vector2 fleePosition = (Vector2)transform.position + fleeDirection * moveSpeed * Time.deltaTime;
            transform.position = fleePosition;
        }
        else
        {
            base.MoveDirectlyTowardsTarget();
        }
    }
}
