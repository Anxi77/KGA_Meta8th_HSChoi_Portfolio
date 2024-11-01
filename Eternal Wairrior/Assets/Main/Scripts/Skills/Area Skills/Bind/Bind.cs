using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class Bind : AreaSkills
{
    public GameObject bindPrefab;
    private List<BindEffect> spawnedBindEffects = new List<BindEffect>();
    private Transform playerTransform;
    private Dictionary<Enemy, BindEffect> enemyBindEffectMap = new Dictionary<Enemy, BindEffect>();
    protected override void Awake()
    {
        base.Awake();
        if (skillData == null)
        {
            skillData = new SkillData
            {
                metadata = new SkillMetadata
                {
                    Name = "Bind",
                    Description = "Immobilizes enemies in range",
                    Type = SkillType.Area,
                    Element = ElementType.Dark,
                    Tier = 1
                }
            };
        }
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
        {
            Debug.LogError("Player not found for Bind skill!");
        }
    }

    private void InitializeSkillStats()
    {
        if (skillData.GetStatsForLevel(1) == null)
        {
            var stats = new AreaSkillStat
            {
                baseStat = new BaseSkillStat
                {
                    damage = 5f,
                    skillName = skillData.metadata.Name,
                    skillLevel = 1,
                    maxSkillLevel = 5,
                    element = skillData.metadata.Element,
                    elementalPower = 1f
                },
                radius = 3f,
                duration = 3f,
                tickRate = 0.5f,
                areaPersistent = true,
                moveSpeed = 0f
            };
            skillData.SetStatsForLevel(1, stats);
        }
    }

    private void Start()
    {
        InitializeSkillStats();
        StartCoroutine(BindingCoroutine());
    }

    private IEnumerator BindingCoroutine()
    {


        if (PoolManager.Instance == null)
        {
            Debug.LogError("PoolManager not found!");
            yield break;
        }

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

                            if (enemyBindEffectMap.ContainsKey(enemy))
                                continue;

                            Vector3 effectPosition = enemy.transform.position;

                            BindEffect bindEffect = PoolManager.Instance.Spawn<BindEffect>(
                                bindPrefab,
                                effectPosition,
                                Quaternion.identity
                            );

                            if (bindEffect != null)
                            {
                                bindEffect.gameObject.SetActive(true);
                                bindEffect.transform.SetParent(enemy.transform);
                                enemyBindEffectMap.Add(enemy, bindEffect);
                                spawnedBindEffects.Add(bindEffect);

                                enemy.OnEnemyDeath += () => RemoveBindEffect(enemy);
                            }
                            else
                            {
                                Debug.LogError("Failed to spawn BindEffect!");
                            }
                        }
                    }
                    else
                    {
                        if (enemyBindEffectMap.ContainsKey(enemy))
                        {
                            var effectToRemove = enemyBindEffectMap[enemy];
                            if (effectToRemove != null)
                            {
                                PoolManager.Instance.Despawn(effectToRemove);
                                spawnedBindEffects.Remove(effectToRemove);
                            }
                            enemyBindEffectMap.Remove(enemy);
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
                    if (enemyBindEffectMap.ContainsKey(enemy))
                    {
                        var effectToRemove = enemyBindEffectMap[enemy];
                        if (effectToRemove != null)
                        {
                            PoolManager.Instance.Despawn(effectToRemove);
                            spawnedBindEffects.Remove(effectToRemove);
                        }
                        enemyBindEffectMap.Remove(enemy);
                    }
                }
            }

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
            Gizmos.color = new Color(1, 0, 0, 1f);
            Gizmos.DrawWireSphere(playerTransform.position, Radius);
        }
    }

    public void RemoveBindEffect(Enemy enemy)
    {
        if (enemyBindEffectMap.ContainsKey(enemy))
        {
            var effectToRemove = enemyBindEffectMap[enemy];
            if (effectToRemove != null)
            {
                PoolManager.Instance.Despawn(effectToRemove);
                spawnedBindEffects.Remove(effectToRemove);
            }
            enemyBindEffectMap.Remove(enemy);
        }
    }
}
