using Lean.Pool;
using UnityEngine;

public class ProjectilePool : SingletonManager<ProjectilePool>
{
    private static ProjectilePool instance;

    public Projectile SpawnProjectile(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        var proj = LeanPool.Spawn(prefab, position, rotation).GetComponent<Projectile>();
        return proj;
    }

    public void DespawnProjectile(GameObject projectile)
    {
        LeanPool.Despawn(projectile);
    }
}