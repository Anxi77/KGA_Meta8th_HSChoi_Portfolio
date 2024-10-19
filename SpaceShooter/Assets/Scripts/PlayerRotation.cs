using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRotation : MonoBehaviour
{
    public float rotationSpeed = 100f;  // ȸ�� �ӵ� (Z�� ȸ��)
    public float tiltSpeed = 50f;       // ���� �ӵ� (Y�� ����)
    public float xRotationSpeed = 80f;
    void Update()
    {

        // Z�� ȸ�� (�ٶ󺸴� ���� ����)
        float rotationInput = Input.GetAxisRaw("Horizontal");
        if (rotationInput != 0)
        {
            transform.localRotation *= Quaternion.Euler(0f, 0f, -rotationInput * rotationSpeed * Time.deltaTime);  // Z�� ���� ȸ��
        }

        // Y�� ���� (ƿƮ ǥ��)
        float tiltInput = Input.GetAxisRaw("Vertical");
        if (tiltInput != 0)
        {
            transform.localRotation *= Quaternion.Euler(tiltInput * tiltSpeed * Time.deltaTime, 0, 0);  // Y�� ���� ����
        }

        float xRotationInput = Input.GetAxisRaw("Mouse X");
        if (xRotationInput != 0)
        {
            transform.localRotation *= Quaternion.Euler(0, xRotationInput * xRotationSpeed * Time.deltaTime, 0);  // X�� ���� ȸ��
        }
    }
}
