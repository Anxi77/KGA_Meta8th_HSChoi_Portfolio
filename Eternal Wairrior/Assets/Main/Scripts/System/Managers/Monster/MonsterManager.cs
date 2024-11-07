using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Linq;

public class MonsterManager : SingletonManager<MonsterManager>
{
    #region Members

    #region Stats

    [Tooltip("ѹ ּ , Y : ִ")]
    public Vector2Int minMaxCount;

    [Tooltip("ּ ִ/ּ Ÿ.\n X : ּ , Y : ִ")]
    public Vector2 minMaxDist;

    public float spawnInterval;

    #endregion

    public Enemy enemyPrefab;

    private Coroutine spawnCoroutine;
    private bool isSpawning = false;

    [Header("Boss Settings")]
    public BossMonster bossPrefab;
    public Vector2 bossSpawnOffset = new Vector2(0, 5f);

    private bool isBossDefeated = false;
    private Vector3 lastBossPosition;

    public bool IsBossDefeated => isBossDefeated;
    public Vector3 LastBossPosition => lastBossPosition;

    #endregion

    #region Unity Message Methods

    protected override void Awake()
    {
        base.Awake();
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

            PoolManager.Instance.Spawn<Enemy>(enemyPrefab.gameObject, finalPos, quaternion.identity);
        }
    }

    public void StartSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            spawnCoroutine = StartCoroutine(SpawnCoroutine());
        }
    }

    public void StopSpawning()
    {
        if (isSpawning && spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
            isSpawning = false;
        }

        // ϴ ֮ ֮
        var enemies = FindObjectsOfType<Enemy>();
        foreach (var enemy in enemies)
        {
            PoolManager.Instance.Despawn(enemy);
        }
    }

    public void SpawnStageBoss()
    {
        // 현재 스폰된 일반 몬스터들 제거
        StopSpawning();
        ClearCurrentEnemies();

        // 보스 스폰 위치 계산
        Vector3 playerPos = GameManager.Instance.player.transform.position;
        Vector3 spawnPos = playerPos + new Vector3(bossSpawnOffset.x, bossSpawnOffset.y, 0);

        // 보스 스폰
        BossMonster boss = PoolManager.Instance.Spawn<BossMonster>(
            bossPrefab.gameObject,
            spawnPos,
            Quaternion.identity
        );

        isBossDefeated = false;
    }

    public void OnBossDefeated(Vector3 position)
    {
        isBossDefeated = true;
        lastBossPosition = position;
    }

    private void ClearCurrentEnemies()
    {
        var enemies = FindObjectsOfType<Enemy>().Where(e => !(e is BossMonster));
        foreach (var enemy in enemies)
        {
            PoolManager.Instance.Despawn(enemy);
        }
    }
    #endregion
}

