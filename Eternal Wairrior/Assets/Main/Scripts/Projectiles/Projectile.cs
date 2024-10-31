using Lean.Pool;
using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    public float damage;
    public float moveSpeed;
    public bool isHoming;
    public int pierceCount;
    public Enemy targetEnemy;
    public Vector2 direction;
    public Vector2 initialPosition;
    public float maxTravelDistance;
    public ParticleSystem impactParticle;
    protected CircleCollider2D coll;
    protected bool hasReachedMaxDistance = false;
    protected List<Collider2D> contactedColls = new();
    protected ParticleSystem projectileRender;
    public ElementType elementType;
    public float elementalPower;

    protected virtual void Awake()
    {
        coll = GetComponent<CircleCollider2D>();
        coll.enabled = false;
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        Physics2D.IgnoreLayerCollision(gameObject.layer, enemyLayer, false);
    }

    protected virtual void OnEnable()
    {
        Debug.Log($"Projectile enabled with damage: {damage}");

        if (isHoming)
        {
            FindTarget();
        }
        initialPosition = transform.position;
        hasReachedMaxDistance = false;
        if (coll != null)
        {
            coll.radius = 0.2f;
            coll.isTrigger = true;
            coll.enabled = true;
        }
        projectileRender = gameObject.GetComponentInChildren<ParticleSystem>();
        if (projectileRender != null)
        {
            projectileRender.Play();
        }
    }

    protected virtual void Update()
    {
        CheckTravelDistance();
        ProjectileMove();
    }

    protected virtual void OnDisable()
    {
        contactedColls.Clear();
        if (projectileRender != null)
        {
            projectileRender.Stop();
        }
    }

    protected virtual void ProjectileMove()
    {
        if (isHoming)
        {
            Homing();
        }
        else
        {
            Move();
        }
    }

    public virtual void Move()
    {
        transform.Translate(transform.up * moveSpeed * Time.deltaTime, Space.World);
    }

    protected virtual void FindTarget()
    {
        if (GameManager.Instance.enemies.Count > 0)
        {
            float targetDistance = float.MaxValue;
            foreach (Enemy enemy in GameManager.Instance.enemies)
            {
                float distance = Vector3.Distance(enemy.transform.position, transform.position);
                if (distance < targetDistance)
                {
                    targetDistance = distance;
                    targetEnemy = enemy;
                }
            }
        }
    }

    protected virtual void Homing()
    {
        if (targetEnemy != null && targetEnemy.gameObject.activeSelf)
        {
            Vector2 direction = (targetEnemy.transform.position - transform.position).normalized;
            transform.up = direction;
            transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
        }
        else
        {
            FindTarget();
            Move();
        }
    }

    public virtual void SetInitialTarget(Enemy enemy)
    {
        targetEnemy = enemy;
    }

    public virtual void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;
        transform.up = direction;
    }

    public virtual void CheckTravelDistance()
    {
        if (!hasReachedMaxDistance)
        {
            float distanceTraveled = Vector2.Distance(transform.position, initialPosition);
            if (distanceTraveled >= maxTravelDistance)
            {
                hasReachedMaxDistance = true;
                ProjectilePool.Instance.DespawnProjectile(gameObject);
            }
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log($"Collision detected with: {other.gameObject.name} on layer: {other.gameObject.layer}");
        //Debug.Log($"Current projectile damage: {damage}");

        if (!other.TryGetComponent<Enemy>(out Enemy enemy))
        {
            Debug.Log("Enemy component not found");
            return;
        }
        float enemyHpBefore = enemy.hp;
        enemy.TakeDamage(damage);
        //Debug.Log($"Enemy HP changed from {enemyHpBefore} to {enemy.hp}");

        if (contactedColls.Contains(other))
        {
            Debug.Log("Already hit this enemy");
            return;
        }

        Debug.Log($"Dealing {damage} damage to enemy");

        if (impactParticle != null)
        {
            var particle = Instantiate(impactParticle, transform.position, Quaternion.identity);
            particle.Play();
            Destroy(particle.gameObject, 0.5f);
        }

        contactedColls.Add(other);

        if (elementType != ElementType.None)
        {
            ElementalEffects.ApplyElementalEffect(elementType, elementalPower, other.gameObject);
        }

        if (isHoming)
        {
            ProjectilePool.Instance.DespawnProjectile(gameObject);
        }
        else if (--pierceCount <= 0)
        {
            ProjectilePool.Instance.DespawnProjectile(gameObject);
        }
    }
}
