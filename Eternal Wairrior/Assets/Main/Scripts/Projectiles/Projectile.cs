using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage;
    public float moveSpeed;
    public bool isHoming;
    public int pierceCount;
    public float maxTravelDistance;
    public Vector2 initialPosition;
    public Vector2 direction;
    protected bool hasReachedMaxDistance = false;
    public Enemy targetEnemy;
    public ParticleSystem impactParticle;
    protected CircleCollider2D coll;
    protected List<Collider2D> contactedColls = new();
    protected ParticleSystem projectileRender;
    public float elementalPower;
    public ElementType elementType;

    protected virtual void Awake()
    {
        coll = GetComponent<CircleCollider2D>();
        coll.enabled = false;
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        Physics2D.IgnoreLayerCollision(gameObject.layer, enemyLayer, false);
    }

    protected virtual void OnEnable()
    {
        //Debug.Log($"Projectile enabled with damage: {damage}");

        if (isHoming)
        {
            FindTarget();
        }
        initialPosition = transform.position;
        hasReachedMaxDistance = false;

        InitializeCollider();
        InitializeParticleSystem();
    }

    private void InitializeCollider()
    {
        if (coll != null)
        {
            coll.radius = 0.2f;
            coll.isTrigger = true;
            coll.enabled = true;
        }
    }

    private void InitializeParticleSystem()
    {
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
                ProjectilePool.Instance.DespawnProjectile(this);
            }
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<Enemy>(out Enemy enemy))
        {
            return;
        }

        enemy.TakeDamage(damage);

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
            ProjectilePool.Instance.DespawnProjectile(this);
        }
        else if (--pierceCount <= 0)
        {
            ProjectilePool.Instance.DespawnProjectile(this);
        }
    }

    public virtual void ResetProjectile()
    {
        contactedColls.Clear();
        hasReachedMaxDistance = false;
        pierceCount = 0;
        damage = 0;
        moveSpeed = 0;
        isHoming = false;
        targetEnemy = null;
        elementType = ElementType.None;
        elementalPower = 0;
    }
}
