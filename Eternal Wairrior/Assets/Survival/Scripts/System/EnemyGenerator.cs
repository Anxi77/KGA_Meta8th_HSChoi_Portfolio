using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyGenerator : MonoBehaviour
{
    #region Members

    #region Stats

    [Tooltip("한번에 스폰될 적의 수. \nX : 최소 , Y : 최대")]
    public Vector2Int minMaxCount;

    [Tooltip("스폰될 때 플레이어로부터의 최대/최소 거리.\n X : 최소 , Y : 최대")]
    public Vector2 minMaxDist;

    public float spawnInterval; 

    #endregion

    #region References

    //public GameObject enemyPrefab;
    #endregion

    #endregion

    #region Unity Message Methods

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


            //플레이어 좌표에 구한 좌표를 더하여 생성.
            Enemy enemy = EnemyPool.pool.Pop();

            enemy.transform.position = playerPos + spawnPos;
            
        }
    }
    #endregion
}

