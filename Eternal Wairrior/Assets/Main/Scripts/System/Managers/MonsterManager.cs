using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MonsterManager : SingletonManager<MonsterManager>
{
    #region Members

    #region Stats

    [Tooltip("한번에 스폰될 적의 수. \nX : 최소 , Y : 최대")]
    public Vector2Int minMaxCount;

    [Tooltip("스폰될 때 플레이어로부터의 최대/최소 거리.\n X : 최소 , Y : 최대")]
    public Vector2 minMaxDist;

    public float spawnInterval;

    #endregion

    public Enemy enemyPrefab;

    #endregion

    #region Unity Message Methods

    protected override void Awake()
    {
        base.Awake();
        if (MonsterPool.Instance == null)
        {
            Debug.LogError("MonsterPool이 초기화되지 않았습니다.");
            return;
        }

        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy Prefab이 할당되지 않았습니다. MonsterManager에서 Enemy Prefab을 설정해주세요.");
            return;
        }

        MonsterPool.Instance.InitializePool(enemyPrefab.gameObject);
    }

    private void Start()
    {
        StartCoroutine(SpawnCoroutine());
    }

    private IEnumerator SpawnCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            int enemyCount = Random.Range(minMaxCount.x, minMaxCount.y);
            Spawn(enemyCount);

        }
    }

    private void Spawn(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 playerPos = GameManager.Instance.player.transform.position;
            Vector2 ranPos = Random.insideUnitCircle;
            Vector2 spawnPos = (ranPos * (minMaxDist.y - minMaxDist.x)) + (ranPos.normalized * minMaxDist.x);
            Vector2 finalPos = playerPos + spawnPos;

            MonsterPool.Instance.SpawnMob(finalPos, Quaternion.identity);
        }
    }
    #endregion
}

