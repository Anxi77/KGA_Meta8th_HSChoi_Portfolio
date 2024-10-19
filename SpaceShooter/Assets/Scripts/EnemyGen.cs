using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGen : MonoBehaviour
{
    public GameObject Player;
    public GameObject Enemy;
    public float spawnInterval = 1f;
    private float nextSpawnTime;
    public GameObject[] spawnable;

    void Start()
    {
        nextSpawnTime = Time.time + spawnInterval;
        spawnable = GameObject.FindGameObjectsWithTag("EnemySpawnPos");

    }

    void Update()
    {
        int randompos = Random.Range(0, spawnable.Length);

        // 스폰 시간이 되었고, 스폰 가능한 위치가 있는지 확인
        if (Time.time >= nextSpawnTime && spawnable.Length > 0)
        {
            // 랜덤한 Spawnable 위치 선택
            GameObject randomSpawnable = spawnable[randompos];

            // 선택된 위치에 Enemy 생성
            Instantiate(Enemy, randomSpawnable.transform.position, Quaternion.identity);

            // 다음 스폰 시간 설정
            nextSpawnTime = Time.time + spawnInterval;

            print($"적이 생성되었습니다: {randomSpawnable.transform.position} ");

            print($"스포너블 번호 : {randompos}");
        }
    }
}