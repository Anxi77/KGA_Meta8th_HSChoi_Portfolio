using UnityEngine;

public class StandardProjectileBehavior : IProjectileBehavior
{
    protected Vector2 initialPosition;
    protected bool hasReachedMaxDistance;

    public virtual void UpdateProjectile(BaseProjectile projectile)
    {
        CheckTravelDistance(projectile);
        Move(projectile);
    }

    public virtual void OnSpawn(BaseProjectile projectile)
    {
        initialPosition = projectile.transform.position;
        hasReachedMaxDistance = false;
    }

    public virtual void OnDespawn(BaseProjectile projectile)
    {
        // 정리 작업
    }

    protected virtual void Move(BaseProjectile projectile)
    {
        projectile.transform.Translate(projectile.transform.up * projectile.Stats.moveSpeed * Time.deltaTime, Space.World);
    }

    protected virtual void CheckTravelDistance(BaseProjectile projectile)
    {
        if (!hasReachedMaxDistance)
        {
            float distanceTraveled = Vector2.Distance(projectile.transform.position, initialPosition);
            if (distanceTraveled >= projectile.Stats.maxTravelDistance)
            {
                hasReachedMaxDistance = true;
                PoolManager.Instance.Despawn(projectile);
            }
        }
    }
}