using UnityEngine;



public abstract class BaseProjectile : MonoBehaviour, IPoolable

{

    protected ProjectileStats stats;

    protected IProjectileBehavior behavior;



    public ProjectileStats Stats => stats ?? throw new System.InvalidOperationException("Projectile stats not initialized");



    public virtual void Initialize(ProjectileStats stats, IProjectileBehavior behavior)

    {

        if (stats == null)

            throw new System.ArgumentNullException(nameof(stats));

        if (behavior == null)

            throw new System.ArgumentNullException(nameof(behavior));



        this.stats = stats;

        this.behavior = behavior;

        ValidateStats();

    }



    protected virtual void Update()

    {

        behavior?.UpdateProjectile(this);

    }



    public virtual void OnSpawnFromPool()

    {

        behavior?.OnSpawn(this);

    }



    public virtual void OnReturnToPool()

    {

        behavior?.OnDespawn(this);

    }



    protected virtual void OnTriggerEnter2D(Collider2D other)

    {

        behavior?.OnTriggerEnter2D(this, other);

    }



    protected virtual void ValidateStats()

    {

        if (stats.moveSpeed <= 0)

            Debug.LogWarning($"Invalid move speed: {stats.moveSpeed}");

        if (stats.damage < 0)

            Debug.LogWarning($"Invalid damage: {stats.damage}");

        if (stats.persistenceData?.duration < 0)

            Debug.LogWarning($"Invalid duration: {stats.persistenceData.duration}");

    }

}
