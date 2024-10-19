using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRotation : MonoBehaviour
{
    public float rotationSpeed = 100f;  // 회전 속도 (Z축 회전)
    public float tiltSpeed = 50f;       // 기울기 속도 (Y축 기울기)
    public float xRotationSpeed = 80f;
    void Update()
    {

        // Z축 회전 (바라보는 방향 변경)
        float rotationInput = Input.GetAxisRaw("Horizontal");
        if (rotationInput != 0)
        {
            transform.localRotation *= Quaternion.Euler(0f, 0f, -rotationInput * rotationSpeed * Time.deltaTime);  // Z축 기준 회전
        }

        // Y축 기울기 (틸트 표현)
        float tiltInput = Input.GetAxisRaw("Vertical");
        if (tiltInput != 0)
        {
            transform.localRotation *= Quaternion.Euler(tiltInput * tiltSpeed * Time.deltaTime, 0, 0);  // Y축 기준 기울기
        }

        float xRotationInput = Input.GetAxisRaw("Mouse X");
        if (xRotationInput != 0)
        {
            transform.localRotation *= Quaternion.Euler(0, xRotationInput * xRotationSpeed * Time.deltaTime, 0);  // X축 기준 회전
        }
    }
}
