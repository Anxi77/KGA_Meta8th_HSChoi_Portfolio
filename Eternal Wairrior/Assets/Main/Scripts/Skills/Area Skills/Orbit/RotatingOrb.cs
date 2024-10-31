using UnityEngine;
using System.Collections.Generic;
using Lean.Pool;

public class RotatingOrb : MonoBehaviour
{
    public GameObject orbPrefab;
    public float damage;
    public float radius = 2f;
    private float inOutTime = 0f;
    private float inOutSpeed = 2.5f;
    private float inOutDistance = 0.3f;
    private Vector3 originalRadius;

    private List<GameObject> orbs = new List<GameObject>();

    public void InitializeOrbs(int count)
    {
        ClearOrbs();

        float angleStep = 360f / count;
        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep;
            Vector3 orbPosition = transform.position + (Quaternion.Euler(0, 0, angle) * Vector3.right * radius);
            GameObject orb = Instantiate(orbPrefab, orbPosition, Quaternion.identity, transform);
            orbs.Add(orb);

            OrbDamage orbDamage = orb.GetComponent<OrbDamage>();
            if (orbDamage == null)
            {
                orbDamage = orb.AddComponent<OrbDamage>();
            }
            orbDamage.damage = damage;
        }
    }

    private void ClearOrbs()
    {
        foreach (var orb in orbs)
        {
            if (orb != null)
            {
                Destroy(orb);
            }
        }
        orbs.Clear();
    }

    private void OnDestroy()
    {
        ClearOrbs();
    }

    private void Start()
    {
        originalRadius = Vector3.right * radius;
    }

    private void Update()
    {
        InOut();
    }

    private void InOut() 
    {
        inOutTime += Time.deltaTime * inOutSpeed;
        float offset = Mathf.Sin(inOutTime) * inOutDistance;

        for (int i = 0; i < orbs.Count; i++)
        {
            float angle = (360f / orbs.Count) * i;
            Vector3 orbPosition = transform.localPosition + (Quaternion.Euler(0, 0, angle) * originalRadius * (1 + offset));
            orbs[i].transform.localPosition = orbPosition;
        }
    }
}

public class OrbDamage : MonoBehaviour
{
    public float damage;

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Enemy"))
        {
            Enemy enemy = other.collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
    }

    //private void OnCollisionEnter2D(Collider2D other)
    //{

    //}
}
