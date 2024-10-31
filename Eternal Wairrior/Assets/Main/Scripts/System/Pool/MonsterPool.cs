using System.Collections.Generic;
using UnityEngine;

public class MonsterPool : SingletonManager<MonsterPool>
{
    private Queue<GameObject> pooledEnemies = new Queue<GameObject>();
    private GameObject enemyPrefab;
    private int poolSize = 50;  // 초기 풀 크기

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        base.Awake();
    }

    public void InitializePool(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("Enemy Prefab이 null입니다. MonsterPool을 초기화할 수 없습니다.");
            return;
        }

        if (enemyPrefab != null)
        {
            Debug.LogWarning("MonsterPool이 이미 초기화되어 있습니다.");
            return;
        }

        enemyPrefab = prefab;
        for (int i = 0; i < poolSize; i++)
        {
            CreateNewEnemy();
        }
    }

    private void CreateNewEnemy()
    {
        GameObject enemy = Instantiate(enemyPrefab, transform);
        enemy.SetActive(false);
        pooledEnemies.Enqueue(enemy);
    }

    public GameObject SpawnMob(Vector3 position, Quaternion rotation)
    {
        if (pooledEnemies.Count == 0)
        {
            CreateNewEnemy();
        }

        GameObject enemy = pooledEnemies.Dequeue();
        enemy.transform.position = position;
        enemy.transform.rotation = rotation;
        enemy.SetActive(true);

        return enemy;
    }

    public void DespawnMob(GameObject enemy)
    {
        if (enemy != null)
        {
            enemy.SetActive(false);
            pooledEnemies.Enqueue(enemy);
        }
    }
}