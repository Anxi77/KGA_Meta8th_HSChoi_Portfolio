using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour, IPoolable
{
    [Header("Base Stats")]
    [SerializeField] protected float _damage = 10f;
    [SerializeField] protected float _moveSpeed = 10f;
    [SerializeField] protected float _elementalPower = 1f;
    [SerializeField] protected ElementType _elementType = ElementType.None;

    [Header("Projectile Settings")]
    [SerializeField] protected bool _isHoming = false;
    [SerializeField] protected int _pierceCount = 1;
    [SerializeField] protected float _maxTravelDistance = 10f;
    public ParticleSystem impactParticle;

    // Properties
    public float damage { get => _damage; set => _damage = value; }
    public float moveSpeed { get => _moveSpeed; set => _moveSpeed = value; }
    public bool isHoming { get => _isHoming; set => _isHoming = value; }
    public int pierceCount { get => _pierceCount; set => _pierceCount = value; }
    public float maxTravelDistance { get => _maxTravelDistance; set => _maxTravelDistance = value; }
    public float elementalPower { get => _elementalPower; set => _elementalPower = value; }
    public ElementType elementType { get => _elementType; set => _elementType = value; }

    // Runtime variables
    public Vector2 initialPosition;
    public Vector2 direction;
    protected bool hasReachedMaxDistance = false;
    public Enemy targetEnemy;
    protected CircleCollider2D coll;
    protected List<Collider2D> contactedColls = new();
    protected ParticleSystem projectileRender;

    protected virtual void Awake()
    {
        coll = GetComponent<CircleCollider2D>();
        coll.enabled = false;
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        Physics2D.IgnoreLayerCollision(gameObject.layer, enemyLayer, false);
    }

    public virtual void OnSpawnFromPool()
    {
        if (isHoming)
        {
            FindTarget();
        }
        initialPosition = transform.position;
        hasReachedMaxDistance = false;

        InitializeCollider();
        InitializeParticleSystem();
    }

    public void OnReturnToPool()
    {
        contactedColls.Clear();
        if (projectileRender != null)
        {
            projectileRender.Stop();
        }
        ResetProjectile();
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
                PoolManager.Instance.Despawn(this);
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
            ParticleSystem particle = PoolManager.Instance.Spawn<ParticleSystem>(
                impactParticle.gameObject,
                transform.position,
                Quaternion.identity
            );
            if (particle != null)
            {
                particle.Play();
                PoolManager.Instance.Despawn<ParticleSystem>(particle,0.5f);
            }
        }

        contactedColls.Add(other);

        if (elementType != ElementType.None && elementalPower > 0)
        {
            ElementalEffects.ApplyElementalEffect(elementType, elementalPower, other.gameObject);
        }

        if (isHoming || --pierceCount <= 0)
        {
            PoolManager.Instance.Despawn(this);
        }
    }

    private IEnumerator ReturnParticleToPool(ParticleSystem particle, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (particle != null)
        {
            PoolManager.Instance.Despawn(particle);
        }
    }

    public virtual void ResetProjectile()
    {
        contactedColls.Clear();
        hasReachedMaxDistance = false;
        _pierceCount = 1;
        _damage = 10f;
        _moveSpeed = 10f;
        _isHoming = false;
        targetEnemy = null;
        _elementType = ElementType.None;
        _elementalPower = 1f;
    }
}
