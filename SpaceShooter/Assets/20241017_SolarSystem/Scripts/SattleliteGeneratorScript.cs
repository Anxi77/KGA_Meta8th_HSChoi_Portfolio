using UnityEngine;

public class SatelliteSpawner : MonoBehaviour
{
    public GameObject satellitePrefab;
    public int satelliteCount = 10;
    public float minDistance = -5f;
    public float maxDistance = 5f;

    void Start()
    {
        SpawnSatellites();
    }

    void SpawnSatellites()
    {
        for (int i = 0; i < satelliteCount; ++i)
        {
            // 랜덤한 로컬 위치 생성
            Vector3 randomLocalPosition = new Vector3(
                Random.Range(minDistance, maxDistance),
                Random.Range(minDistance, maxDistance),
                Random.Range(minDistance, maxDistance)
            );

            // 위성 생성 및 부모 설정
            GameObject satellite = Instantiate(satellitePrefab, transform);

            // 로컬 위치 설정
            satellite.transform.localPosition = randomLocalPosition;

        }
    }
}