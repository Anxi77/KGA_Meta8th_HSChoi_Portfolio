using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody rb;
    private Vector3 movement;

    void Start()
    {
        // 상위 객체의 Rigidbody 가져오기
        rb = GetComponent<Rigidbody>();

        // 상위 객체는 Z축 고정 (XY 평면에서 이동)
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionZ;
    }

    void Update()
    {
        // WASD 입력 받아 이동 처리
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement.z = 0f;
    }

    void FixedUpdate()
    {
        // 상위 객체 이동 처리
        rb.velocity = movement.normalized * moveSpeed;
    }
}
