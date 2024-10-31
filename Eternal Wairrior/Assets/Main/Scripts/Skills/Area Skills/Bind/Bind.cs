using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Pool;

public class Bind : AreaSkills
{
    public GameObject bindPrefab;
    private List<GameObject> spawnedBindEffects = new List<GameObject>();
    private Transform playerTransform;

    protected override void Awake()
    {
        base.Awake();
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
        {
            Debug.LogError("Player not found for Bind skill!");
        }
    }

    private void Start()
    {
        StartCoroutine(BindingCoroutine());
    }

    private IEnumerator BindingCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(TickRate);

            if (playerTransform == null) continue;

            List<Enemy> affectedEnemies = new List<Enemy>();

            if (GameManager.Instance.enemies != null)
            {
                foreach (Enemy enemy in GameManager.Instance.enemies)
                {
                    if (enemy != null)
                    {
                        float distanceToPlayer = Vector2.Distance(playerTransform.position, enemy.transform.position);
                        if (distanceToPlayer <= Radius)
                        {
                            affectedEnemies.Add(enemy);
                            enemy.moveSpeed = 0;
                            GameObject spawnedEffect = LeanPool.Spawn(bindPrefab, enemy.transform);
                            spawnedBindEffects.Add(spawnedEffect);
                            Debug.DrawLine(playerTransform.position, enemy.transform.position, Color.red, Duration);
                        }
                    }
                }
            }

            float elapsedTime = 0f;
            while (elapsedTime < Duration)
            {
                foreach (Enemy enemy in affectedEnemies)
                {
                    if (enemy != null)
                    {
                        enemy.TakeDamage(Damage);
                    }
                }
                yield return new WaitForSeconds(TickRate);
                elapsedTime += TickRate;
            }

            foreach (Enemy enemy in affectedEnemies)
            {
                if (enemy != null)
                {
                    enemy.moveSpeed = enemy.originalMoveSpeed;
                }
            }

            foreach (GameObject effect in spawnedBindEffects)
            {
                if (effect != null)
                {
                    LeanPool.Despawn(effect);
                }
            }
            spawnedBindEffects.Clear();

            if (!IsPersistent)
            {
                break;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (playerTransform != null)
        {
            Gizmos.color = new Color(1, 0, 0, 0.2f);
            Gizmos.DrawWireSphere(playerTransform.position, Radius);
        }
    }
}
