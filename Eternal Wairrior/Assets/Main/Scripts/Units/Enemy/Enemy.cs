using LaserSystem2D;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using static GameManager;

public class Enemy : MonoBehaviour, ILaserStay
{
    #region Member Variables
    #region Stats
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

    #region References
    public Transform target;
    public Image hpBar;
    private Rigidbody2D rb;
    public ParticleSystem impactParticle;
    private bool isInit = false;
    #endregion

    #region Pathfinding Variables
    public List<Vector2> currentPath { get; private set; }
    private float pathUpdateTime = 0.2f;
    private float lastPathUpdateTime;
    private float obstaclePathUpdateDelay = 0.1f;
    private float lastObstacleAvoidanceTime;
    private float stuckTimer = 0f;
    private Vector2 lastPosition;
    #endregion

    #region Constants
    private const float STUCK_THRESHOLD = 0.1f;
    private const float STUCK_CHECK_TIME = 0.5f;
    private const float CORNER_CHECK_DISTANCE = 1f;
    private const float WALL_AVOIDANCE_DISTANCE = 0.8f;
    private const float MIN_CIRCLE_DISTANCE = 1f;
    #endregion

    #region Movement Variables
    private Vector2 previousMoveDir;
    private bool isCirclingPlayer = false;
    private float circlingRadius = 2f;
    private float circlingAngle = 0f;
    private SpriteRenderer spriteRenderer;
    private float previousXPosition;
    #endregion
    #endregion

    #region Unity Lifecycle Methods
    public void Awake()
    {
        InitializeComponents();
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
        UpdateVisuals();
    }

    private void OnDisable()
    {
        GameManager.Instance?.enemies.Remove(this);
    }
    #endregion

    #region Initialization
    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        previousXPosition = transform.position.x;
    }

    private void Initialize()
    {
        if (GameManager.Instance?.player != null)
        {
            target = GameManager.Instance.player.transform;
            isInit = true;
        }
    }
    #endregion

    #region Movement and Pathfinding
    public void Move()
    {
        if (target == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);
        if (HandleCirclingBehavior(distanceToPlayer)) return;

        UpdatePath();
        FollowPath();
    }

    private bool HandleCirclingBehavior(float distanceToPlayer)
    {
        if (distanceToPlayer < MIN_CIRCLE_DISTANCE)
        {
            isCirclingPlayer = true;
            CircleAroundPlayer();
            return true;
        }
        isCirclingPlayer = false;
        return false;
    }

    private void UpdatePath()
    {
        if (ShouldUpdatePath())
        {
            List<Vector2> newPath = PathFindingManager.Instance.FindPath(transform.position, target.position);
            if (newPath != null && newPath.Count > 0)
            {
                currentPath = newPath;
                lastPathUpdateTime = Time.time;
                stuckTimer = 0f;
            }
            else
            {
                MoveDirectlyTowardsTarget();
            }
        }
    }

    private bool ShouldUpdatePath()
    {
        if (currentPath == null || currentPath.Count == 0) return true;

        if (Time.time >= lastPathUpdateTime + pathUpdateTime)
        {
            Vector2 finalDestination = currentPath[currentPath.Count - 1];
            float distanceToFinalDestination = Vector2.Distance(finalDestination, target.position);
            return distanceToFinalDestination > PathFindingManager.NODE_SIZE * 2;
        }
        return false;
    }

    private void MoveDirectlyTowardsTarget()
    {
        if (target == null) return;

        Vector2 currentPos = transform.position;
        Vector2 targetPos = target.position;
        Vector2 moveDir = (targetPos - currentPos).normalized;

        moveDir = CalculateAvoidanceDirection(currentPos, targetPos);

        Vector2 separationForce = CalculateSeparationForce(currentPos);
        moveDir = (moveDir + separationForce * 0.2f).normalized;

        Vector2 targetVelocity = moveDir * moveSpeed;
        rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, Time.deltaTime * 5f);
    }

    private Vector2 CalculateAvoidanceDirection(Vector2 currentPosition, Vector2 targetPosition)
    {
        Vector2 moveDir = (targetPosition - currentPosition).normalized;
        Vector2 finalMoveDir = moveDir;

        var obstacles = CheckObstacles(currentPosition, moveDir);
        if (HasObstacles(obstacles))
        {
            HandleObstacleAvoidance(obstacles);
            finalMoveDir = CalculateAvoidanceVector(obstacles);
        }

        return SmoothDirection(finalMoveDir);
    }

    private Vector2 CalculateSeparationForce(Vector2 currentPos)
    {
        Vector2 separationForce = Vector2.zero;
        float separationRadius = isCirclingPlayer ? 0.8f : 1.5f;

        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(currentPos, separationRadius, LayerMask.GetMask("Enemy"));

        foreach (Collider2D enemyCollider in nearbyEnemies)
        {
            if (enemyCollider.gameObject != gameObject)
            {
                Vector2 diff = currentPos - (Vector2)enemyCollider.transform.position;
                float distance = diff.magnitude;
                if (distance < separationRadius)
                {
                    float strength = isCirclingPlayer ? 0.5f : 1f;
                    separationForce += diff.normalized * (1 - distance / separationRadius) * strength;
                }
            }
        }

        return separationForce.normalized * (isCirclingPlayer ? 0.3f : 0.5f);
    }

    private void CircleAroundPlayer()
    {
        if (target == null) return;

        int enemyCount = GameManager.Instance.enemies.Count;
        circlingRadius = 1.2f;
        float angleStep = 360f / enemyCount;

        int myIndex = GameManager.Instance.enemies.IndexOf(this);
        float targetAngle = myIndex * angleStep;

        circlingAngle = Mathf.LerpAngle(circlingAngle, targetAngle, Time.deltaTime * 8f);

        Vector2 offset = new Vector2(
            Mathf.Cos(circlingAngle * Mathf.Deg2Rad),
            Mathf.Sin(circlingAngle * Mathf.Deg2Rad)
        ) * circlingRadius;

        Vector2 targetPosition = (Vector2)target.position + offset;

        Vector2 moveDirection = (targetPosition - (Vector2)transform.position).normalized;
        moveDirection = CalculateAvoidanceDirection(transform.position, targetPosition);

        Vector2 separationForce = CalculateSeparationForce(transform.position);
        moveDirection = (moveDirection + separationForce * 0.1f).normalized;

        Vector2 targetVelocity = moveDirection * moveSpeed * 1.2f;
        rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, Time.deltaTime * 8f);

        float currentXPosition = transform.position.x;
        if (currentXPosition != previousXPosition)
        {
            spriteRenderer.flipX = (currentXPosition - previousXPosition) > 0;
            previousXPosition = currentXPosition;
        }
    }

    private void FollowPath()
    {
        if (currentPath == null || currentPath.Count == 0) return;

        Vector2 currentPos = transform.position;
        Vector2 nextWaypoint = currentPath[0];

        HandleStuckCheck(currentPos);

        if (HasReachedWaypoint(currentPos, nextWaypoint))
        {
            if (currentPath != null && currentPath.Count > 0)
            {
                UpdateWaypoint();
                if (currentPath == null || currentPath.Count == 0)
                {
                    MoveDirectlyTowardsTarget();
                    return;
                }
                nextWaypoint = currentPath[0];
            }
        }

        ApplyMovement(currentPos, nextWaypoint);
        UpdateSpriteDirection();
    }

    private void HandleStuckCheck(Vector2 currentPos)
    {
        if (Vector2.Distance(currentPos, lastPosition) < STUCK_THRESHOLD)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer > STUCK_CHECK_TIME)
            {
                ResetPath();
            }
        }
        else
        {
            stuckTimer = 0f;
        }
        lastPosition = currentPos;
    }

    private void ResetPath()
    {
        currentPath = null;
        stuckTimer = 0f;
    }

    private bool HasReachedWaypoint(Vector2 currentPos, Vector2 waypoint)
    {
        return Vector2.Distance(currentPos, waypoint) < PathFindingManager.NODE_SIZE * 0.5f;
    }

    private void UpdateWaypoint()
    {
        if (currentPath != null && currentPath.Count > 0)
        {
            currentPath.RemoveAt(0);
        }
    }

    private void ApplyMovement(Vector2 currentPos, Vector2 nextWaypoint)
    {
        Vector2 moveDirection = (nextWaypoint - currentPos).normalized;
        moveDirection = CalculateAvoidanceDirection(currentPos, nextWaypoint);

        Vector2 separationForce = CalculateSeparationForce(currentPos);
        moveDirection = CalculateFinalDirection(moveDirection, separationForce);

        ApplyVelocity(moveDirection);
        VisualizeDebugLines(currentPos, nextWaypoint, moveDirection);
    }

    private Vector2 CalculateFinalDirection(Vector2 moveDirection, Vector2 separationForce)
    {
        float separationMultiplier = isCirclingPlayer ? 0.1f : 0.2f;
        return (moveDirection + separationForce * separationMultiplier).normalized;
    }

    private void ApplyVelocity(Vector2 moveDirection)
    {
        Vector2 targetVelocity = moveDirection * moveSpeed;
        rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, Time.deltaTime * 5f);
    }

    private void VisualizeDebugLines(Vector2 currentPos, Vector2 nextWaypoint, Vector2 moveDirection)
    {
        Debug.DrawLine(currentPos, nextWaypoint, Color.green);
        Debug.DrawRay(currentPos, moveDirection * 2f, Color.red);
    }

    private void UpdateSpriteDirection()
    {
        float currentXPosition = transform.position.x;
        if (currentXPosition != previousXPosition)
        {
            spriteRenderer.flipX = (currentXPosition - previousXPosition) < 0;
            previousXPosition = currentXPosition;
        }
    }

    private (RaycastHit2D front, RaycastHit2D right, RaycastHit2D left) CheckObstacles(Vector2 position, Vector2 direction)
    {
        Vector2 rightCheck = Quaternion.Euler(0, 0, 30) * direction;
        Vector2 leftCheck = Quaternion.Euler(0, 0, -30) * direction;

        return (
            Physics2D.Raycast(position, direction, WALL_AVOIDANCE_DISTANCE, LayerMask.GetMask("Obstacle")),
            Physics2D.Raycast(position, rightCheck, CORNER_CHECK_DISTANCE, LayerMask.GetMask("Obstacle")),
            Physics2D.Raycast(position, leftCheck, CORNER_CHECK_DISTANCE, LayerMask.GetMask("Obstacle"))
        );
    }

    private bool HasObstacles((RaycastHit2D front, RaycastHit2D right, RaycastHit2D left) obstacles)
    {
        return obstacles.front.collider != null ||
               obstacles.right.collider != null ||
               obstacles.left.collider != null;
    }

    private void HandleObstacleAvoidance((RaycastHit2D front, RaycastHit2D right, RaycastHit2D left) obstacles)
    {
        if (currentPath != null && Time.time >= lastObstacleAvoidanceTime + obstaclePathUpdateDelay)
        {
            ResetPathForObstacle();
        }
    }

    private void ResetPathForObstacle()
    {
        currentPath = null;
        lastPathUpdateTime = Time.time - pathUpdateTime;
        lastObstacleAvoidanceTime = Time.time;
    }

    private Vector2 CalculateAvoidanceVector((RaycastHit2D front, RaycastHit2D right, RaycastHit2D left) obstacles)
    {
        Vector2 avoidDir = Vector2.zero;

        if (obstacles.front.collider != null)
        {
            avoidDir += -obstacles.front.normal * 3f;
        }
        if (obstacles.right.collider != null)
        {
            avoidDir += Vector2.Perpendicular(obstacles.right.normal) * 2f;
        }
        if (obstacles.left.collider != null)
        {
            avoidDir += -Vector2.Perpendicular(obstacles.left.normal) * 2f;
        }

        return avoidDir != Vector2.zero ? avoidDir.normalized : (Vector2)transform.right;
    }

    private Vector2 SmoothDirection(Vector2 finalMoveDir)
    {
        if (previousMoveDir != Vector2.zero)
        {
            finalMoveDir = Vector2.Lerp(previousMoveDir, finalMoveDir, Time.deltaTime * 20f);
        }
        previousMoveDir = finalMoveDir;
        return finalMoveDir;
    }
    #endregion

    #region Combat
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

    private void Attack()
    {
        if (Time.time >= preDamageTime + damageInterval)
        {
            GameManager.Instance.player.hp -= damage;
            preDamageTime = Time.time;
            GameManager.Instance.player.characterControl.PlayAnimation(PlayerState.DAMAGED, 0);
        }
    }
    #endregion

    #region Collision Handling
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<Player>(out Player player))
        {
            isCirclingPlayer = true;
            Contact();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<Player>(out Player player))
        {
            isCirclingPlayer = false;
        }
    }

    private void Contact()
    {
        var particle = Instantiate(impactParticle, transform.position, Quaternion.identity);
        particle.Play();
        Destroy(particle.gameObject, 0.3f);
        Attack();
    }
    #endregion

    #region Utility Methods
    private bool IsNewPathBetter(List<Vector2> newPath, List<Vector2> currentPath)
    {
        if (currentPath == null) return true;

        float minDistToCurrent = float.MaxValue;
        int currentPathIndex = 0;

        for (int i = 0; i < currentPath.Count; i++)
        {
            float dist = Vector2.Distance(transform.position, currentPath[i]);
            if (dist < minDistToCurrent)
            {
                minDistToCurrent = dist;
                currentPathIndex = i;
            }
        }

        float remainingCurrentPathLength = 0;
        for (int i = currentPathIndex; i < currentPath.Count - 1; i++)
        {
            remainingCurrentPathLength += Vector2.Distance(currentPath[i], currentPath[i + 1]);
        }

        float newPathLength = 0;
        for (int i = 0; i < newPath.Count - 1; i++)
        {
            newPathLength += Vector2.Distance(newPath[i], newPath[i + 1]);
        }

        return newPathLength < remainingCurrentPathLength * 0.8f;
    }
    #endregion

    #region Properties
    public float PathUpdateInterval
    {
        get => pathUpdateTime;
        set => pathUpdateTime = Mathf.Max(0.1f, value);
    }

    public float ObstacleAvoidanceDelay
    {
        get => obstaclePathUpdateDelay;
        set => obstaclePathUpdateDelay = Mathf.Max(0.05f, value);
    }
    #endregion

    #region Laser Stay
    public void OnLaserStay(LaserBase laserBase, List<RaycastHit2D> hits)
    {
        TakeDamage(GameManager.Instance.gun.damage);
    }
    #endregion

    #region UI Methods
    private void UpdateHPBar()
    {
        if (hpBar != null)
        {
            hpBar.fillAmount = hpAmount;
        }
    }

    private void UpdateVisuals()
    {
        UpdateHPBar();
        UpdateSpriteDirection();
    }
    #endregion
}


