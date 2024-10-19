using SpaceShooter;
using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    private GameObject player;
    public float moveSpeed = 22f;
    Vector3 direction;
    private void Awake()
    {
        // 태그를 사용하여 플레이어 찾기
        player = GameObject.FindGameObjectWithTag("Player");

    }

    private void Start()
    {
        direction = (player.transform.position - transform.position).normalized;
        transform.rotation = player.transform.rotation * Quaternion.Euler(-60f, 0f, 0f);
    }

    void Update()
    {
        if (player != null && direction != null)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }
}