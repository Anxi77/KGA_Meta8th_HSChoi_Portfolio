using Lean.Pool;
using UnityEngine;

public class MonsterPool : SingletonManager<MonsterPool>
{
    private static MonsterPool instance;

    public Enemy SpawnMob(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        var enemy = LeanPool.Spawn(prefab, position, rotation).GetComponent<Enemy>();
        return enemy;
    }

    public void DespawnMob(GameObject enemy)
    {
        LeanPool.Despawn(enemy);
    }
}