using UnityEngine;
using System.Collections.Generic;

public class ProjectilePool : SingletonManager<ProjectilePool>
{
    private Dictionary<string, Queue<Projectile>> pools = new Dictionary<string, Queue<Projectile>>();
    private Dictionary<string, GameObject> prefabDictionary = new Dictionary<string, GameObject>();
    private Transform poolContainer;

    protected override void Awake()
    {
        base.Awake();
        poolContainer = new GameObject("ProjectilePool").transform;
        poolContainer.parent = transform;
    }

    public Projectile SpawnProjectile(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        string prefabId = prefab.name;

        // 해당 프리팹의 풀이 없다면 새로 생성
        if (!pools.ContainsKey(prefabId))
        {
            pools[prefabId] = new Queue<Projectile>();
            prefabDictionary[prefabId] = prefab;
        }

        Projectile projectile;

        // 풀에 재사용 가능한 발사체가 있는지 확인
        if (pools[prefabId].Count > 0)
        {
            projectile = pools[prefabId].Dequeue();
            projectile.transform.position = position;
            projectile.transform.rotation = rotation;
            projectile.gameObject.SetActive(true);
        }
        else
        {
            // 풀이 비어있으면 새로 생성
            GameObject newObj = Instantiate(prefab, position, rotation, poolContainer);
            projectile = newObj.GetComponent<Projectile>();
        }

        return projectile;
    }

    public void DespawnProjectile(Projectile projectile)
    {
        string prefabId = projectile.gameObject.name.Replace("(Clone)", "");

        projectile.gameObject.SetActive(false);

        // 풀에 반환
        if (!pools.ContainsKey(prefabId))
        {
            pools[prefabId] = new Queue<Projectile>();
        }

        pools[prefabId].Enqueue(projectile);
    }
}