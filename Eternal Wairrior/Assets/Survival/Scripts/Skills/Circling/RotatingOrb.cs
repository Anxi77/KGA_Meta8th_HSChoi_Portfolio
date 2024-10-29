using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingOrb : MonoBehaviour
{
    private CircleCollider2D coll;
    public float damage;

    private void Start()
    {
        coll = this.GetComponent<CircleCollider2D>(); 
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy")) 
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            enemy.TakeDamage(damage);
        }
    }
}
