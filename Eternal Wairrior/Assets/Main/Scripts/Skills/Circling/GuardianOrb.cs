using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardianOrb : MonoBehaviour 
{ 
    public float rotateSpeed;
    public float damage;
    public RotatingOrb orbs;
    private void Start()
    {
        orbs = GetComponentInChildren<RotatingOrb>();
        orbs.damage = damage;
    }
    public void Update()
    {
        transform.Rotate(Vector3.forward, 1f * rotateSpeed * Time.deltaTime);
    }


}
