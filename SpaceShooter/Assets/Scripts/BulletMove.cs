using SpaceShooter;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class BulletMove : MonoBehaviour
{
    public float velocity = 10f;
    public float lifeTime = 2f;
    private GameObject player;

    private Vector3 direction;

    private void Awake()
    {
        player = GameObject.FindObjectOfType<PlayerScript>()?.gameObject;

    }

    public void Start()
    {
    
        Vector3 playerDirection = player.transform.up;
        direction = playerDirection.normalized;

        // 총알의 회전을 플레이어의 회전과 일치시킴
        transform.rotation = player.transform.rotation; /** Quaternion.Euler(-90f, 0f, 0f);*/

        Destroy(gameObject, lifeTime);
    }


    private void Update()
    {
        transform.Translate(direction * velocity * Time.deltaTime, Space.World);
    }

    private void OnCollisionEnter(Collision collision)
    {
        DestroyImmediate(gameObject);
    }
}