using LaserSystem2D;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static GameManager;

public class Enemy : MonoBehaviour,ILaserStay
{
    #region Members

    #region Stat
    private float maxHp;
    public float hp = 10f; 
    public float damage = 5f; 
    public float moveSpeed = 3f; 
    public float mobEXP = 10f;
    public float damageInterval;
    internal float originalMoveSpeed;

    public float hpAmount { get { return hp / maxHp; } } 

    private float preDamageTime = 0;
    #endregion

    public Transform target;
    
    public Image hpBar;
    
    private Rigidbody2D rb;
   
    public ParticleSystem impactParticle;

    private float stuckTime = 0f;

    private bool isInit = false;


    #endregion

    #region Unity Message Methods

    public void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        maxHp = hp;
        originalMoveSpeed = moveSpeed;
    }

    private void Update()
    {
        if (!isInit) 
        {
            Initialize();
        }
        Move();
        hpBar.fillAmount = hpAmount;
    }

    private void Initialize()
    {
        if (GameManager.Instance?.player != null)
        {
            target = GameManager.Instance.player.transform;
            isInit = true;
        }
    }

    private void OnDisable()
    {
        GameManager.Instance?.enemies.Remove(this);
    }

    #endregion

    #region Attack & Move & Die

    #region Old Moving Method
    public void Move()
    {
        if (target == null) return;

        Vector2 startPos = transform.position;
        Vector2 endPos = target.position;

        Vector2 moveDir = CalculateBaseDirection(startPos, endPos);
        moveDir = HandleObstacleAvoidance(startPos, endPos, moveDir);
        moveDir = HandleEnemyAvoidance(startPos, moveDir);

        ApplyMovement(moveDir);

    }

    private Vector2 CalculateBaseDirection(Vector2 startPos, Vector2 endPos)
    {
        return (endPos - startPos).normalized;
    }

    private Vector2 HandleObstacleAvoidance(Vector2 startPos, Vector2 endPos, Vector2 currentDir)
    {
        int obstacleLayer = LayerMask.GetMask("Obstacle");
        float detectionDistance = 0.5f;

        Vector2 targetDir = (endPos - startPos).normalized;

        Vector2[] cardinalDirections = new Vector2[]
        {
            Vector2.up,
            Vector2.down,
            Vector2.left,
            Vector2.right
        };

        foreach (Vector2 direction in cardinalDirections)
        {
            RaycastHit2D cardinalHit = Physics2D.Raycast(startPos, direction, detectionDistance * 0.5f, obstacleLayer);
            if (cardinalHit.collider != null)
            {
                float avoidanceStrength = 1f - (cardinalHit.distance / (detectionDistance * 0.5f));
                Vector2 avoidanceDir = Vector2.Perpendicular(cardinalHit.normal);
                if (Vector2.Dot(avoidanceDir, targetDir) < 0)
                {
                    avoidanceDir = -avoidanceDir;
                }
                return Vector2.Lerp(currentDir,
                    (avoidanceDir * 7f + targetDir * 0.2f).normalized,
                    avoidanceStrength);
            }
        }

        float[] rayAngles = new float[36];
        for (int i = 0; i < 36; i++)
        {
            rayAngles[i] = i * 10f;
        }

        foreach (float angle in rayAngles)
        {
            Vector2 rayDirection = Quaternion.Euler(0, 0, angle) * targetDir;
            RaycastHit2D obstacleHit = Physics2D.Raycast(startPos, rayDirection, detectionDistance, obstacleLayer);

            if (obstacleHit.collider != null)
            {
                float avoidanceStrength = 1f - (obstacleHit.distance / detectionDistance);
                Vector2 avoidanceDir = Vector2.Perpendicular(obstacleHit.normal);
                if (Vector2.Dot(avoidanceDir, targetDir) < 0)
                {
                    avoidanceDir = -avoidanceDir;
                }
                return Vector2.Lerp(currentDir,
                    (avoidanceDir * 7f + targetDir * 0.2f).normalized,
                    avoidanceStrength);
            }
        }

        return currentDir;
    }

    private Vector2 HandleEnemyAvoidance(Vector2 startPos, Vector2 currentDir)
    {
        int enemyLayer = LayerMask.GetMask("Enemy");
        RaycastHit2D[] enemyHits = Physics2D.CircleCastAll(startPos, 3.0f, Vector2.zero, 2f, enemyLayer);

        Vector2 separationVector = CalculateSeparationVector(enemyHits);
        Vector2 playerPosition = GameManager.Instance.player.transform.position;
        Vector2 playerDirection = (playerPosition - startPos).normalized;

        if (separationVector != Vector2.zero)
        {
            Vector2 avoidanceDirection = (currentDir + separationVector.normalized * 5.0f).normalized;
            if (Vector2.Dot(avoidanceDirection, playerDirection) > 0)
            {
                avoidanceDirection = (avoidanceDirection + playerDirection).normalized;
            }
            return avoidanceDirection;
        }

        return currentDir;
    }

    private Vector2 CalculateSeparationVector(RaycastHit2D[] enemyHits)
    {
        Vector2 separationVector = Vector2.zero;
        foreach (var hit in enemyHits)
        {
            if (hit.collider.gameObject != gameObject)
            {
                Vector2 diff = (Vector2)transform.position - hit.point;
                float distance = diff.magnitude;
                if (distance < 1f)
                {
                    separationVector += diff.normalized / distance;
                }
            }
        }
        return separationVector;
    }

    private void ApplyMovement(Vector2 moveDir)
    {
        Vector2 targetVelocity = moveDir * moveSpeed;
        Vector2 smoothedVelocity = Vector2.Lerp(rb.velocity, targetVelocity, Time.fixedDeltaTime * 5f);

        if (rb.velocity.magnitude < 0.1f && moveDir.magnitude > 0.1f)
        {
            stuckTime += Time.fixedDeltaTime;
            if (stuckTime > 0.5f)
            {
                smoothedVelocity = moveDir * moveSpeed * 5.0f;
                stuckTime = 0f;
            }
        }
        else
        {
            stuckTime = 0f;
        }

        rb.velocity = smoothedVelocity;
    }
    #endregion


    public void TakeDamage(float damage)
    {
        hp -= damage;

        if (hp <= 0)
        {
            Die();
        }

    }

    public void Die()
    {       
        if (GameManager.Instance.player != null)
        {
            GameManager.Instance.player.GainExperience(mobEXP);
            GameManager.Instance.player.killCount++;
        }
        GameManager.Instance.enemies.Remove(this);
        EnemyPool.pool.Push(this);
    }
    #endregion

    #region Interactions
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<Player>(out Player player))
        {
            Contact();
        }       
    }

    private void Contact()
    {
        var particle = Instantiate(impactParticle, transform.position, Quaternion.identity);
        particle.Play();
        Destroy(particle.gameObject, 0.3f);
        Attack();
    }

    private void Attack() 
    {
        if (Time.time >= preDamageTime + damageInterval)
        {
            GameManager.Instance.player.hp -= damage;
            preDamageTime = Time.time;
            GameManager.Instance.player.characterControl.PlayAnimation(PlayerState.DAMAGED,0);
        }
    }

    public void OnLaserStay(LaserBase laserBase, List<RaycastHit2D> hits)
    {
        TakeDamage(GameManager.Instance.gun.damage);
    }
    #endregion

}
