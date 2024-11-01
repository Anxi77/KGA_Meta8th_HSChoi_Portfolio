using UnityEngine;

public interface IProjectileBehavior
{
    void UpdateProjectile(BaseProjectile projectile);
    void OnSpawn(BaseProjectile projectile);
    void OnDespawn(BaseProjectile projectile);
    void OnTriggerEnter2D(BaseProjectile projectile, Collider2D other);
}