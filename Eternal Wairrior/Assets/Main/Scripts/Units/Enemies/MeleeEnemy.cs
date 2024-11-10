using UnityEngine;
using System.Collections;

public class MeleeEnemy : Enemy
{
    [Header("Melee Attack Settings")]
    [SerializeField] private float attackAnimationDuration = 0.5f;
    [SerializeField] private float attackPrepareTime = 0.2f;
    [SerializeField] private float attackRadius = 1.5f;
    [SerializeField] private LayerMask attackLayer;

    private bool isAttacking = false;
    private Animator animator;

    protected override void Start()
    {
        base.Start();
        animator = GetComponentInChildren<Animator>();
        attackRange = 2f;
        preferredDistance = 1.5f;
    }

    protected override void PerformMeleeAttack()
    {
        if (!isAttacking)
        {
            StartCoroutine(MeleeAttackCoroutine());
        }
    }

    private IEnumerator MeleeAttackCoroutine()
    {
        isAttacking = true;

        animator?.SetTrigger("Attack");
        yield return new WaitForSeconds(attackPrepareTime);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRadius, attackLayer);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                if (attackParticle != null)
                {
                    var particle = Instantiate(attackParticle, hit.transform.position, Quaternion.identity);
                    particle.Play();
                    Destroy(particle.gameObject, 0.3f);
                }
                hit.GetComponent<Player>()?.TakeDamage(damage);
            }
        }

        preDamageTime = Time.time;
        yield return new WaitForSeconds(attackAnimationDuration - attackPrepareTime);
        isAttacking = false;
    }

    protected override void MoveDirectlyTowardsTarget()
    {
        if (isAttacking) return;
        base.MoveDirectlyTowardsTarget();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}