using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RotateAround : MonoBehaviour
{
    public GameObject Sun;

    public float orbitPeriod;
    public float rotationPeriod;
    private float orbitSpeed;
    private float rotationSpeed;

    private void Start()
    {
        orbitSpeed = 12f;
        rotationSpeed = 12f;

    }

    void Update()
    {
        transform.RotateAround(Sun.transform.position, Vector3.up, orbitSpeed * Time.deltaTime);
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
    }
}
