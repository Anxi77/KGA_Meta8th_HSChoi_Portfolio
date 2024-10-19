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
        // ���� ��ü�� Rigidbody ��������
        rb = GetComponent<Rigidbody>();

        // ���� ��ü�� Z�� ���� (XY ��鿡�� �̵�)
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionZ;
    }

    void Update()
    {
        // WASD �Է� �޾� �̵� ó��
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement.z = 0f;
    }

    void FixedUpdate()
    {
        // ���� ��ü �̵� ó��
        rb.velocity = movement.normalized * moveSpeed;
    }
}
