using UnityEngine;

public class SlowField : MonoBehaviour
{
    [SerializeField] private float slowAmount = 0.5f;

    private float damage;
    private float radius;
    private float duration;
    private float tickRate;
    private float remainingTime;
    private float nextTickTime;

    public void Initialize(float damage, float radius, float duration, float tickRate)
    {
        this.damage = damage;
        this.radius = radius;
        this.duration = duration;
        this.remainingTime = duration;
        this.tickRate = tickRate;
        this.nextTickTime = 0f;
        transform.localScale = Vector3.one * radius;
    }

    private void Update()
    {
        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0)
        {
            Destroy(gameObject);
            return;
        }

        if (Time.time >= nextTickTime)
        {
            ApplyEffects();
            nextTickTime = Time.time + tickRate;
        }
    }

    private void ApplyEffects()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius, LayerMask.GetMask("Enemy"));

        foreach (Collider2D collider in colliders)
        {
            if (collider.TryGetComponent<Enemy>(out Enemy enemy))
            {
                enemy.TakeDamage(damage);
                enemy.ApplySlowEffect(slowAmount, tickRate * 2f);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 0, 1, 0.3f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}