using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using static GameManager;
using Lean.Pool;
using Unity.Mathematics;

public class Enemy : MonoBehaviour
{
    #region Variables
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
    public float attackRange = 1.2f;
    public float preferredDistance = 1.0f;
    public ElementType elementType = ElementType.None;
    private float defenseDebuffAmount = 0f;
    private float moveSpeedDebuffAmount = 0f;
    private bool isStunned = false;
    #endregion

    #region References
    private Transform target;
    public Image hpBar;
    private Rigidbody2D rb;
    public ParticleSystem attackParticle;
    private bool isInit = false;
    private Collider2D enemyCollider;
    private SpriteRenderer spriteRenderer;
    #endregion

    #region Pathfinding
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
    private const float CORNER_CHECK_DISTANCE = 0.5f;
    private const float WALL_AVOIDANCE_DISTANCE = 1.5f;
    private const float MIN_CIRCLE_DISTANCE = 1f;
    #endregion

    #region Movement
    private Vector2 previousMoveDir;
    private bool isCirclingPlayer = false;
    private float circlingRadius = 3f;
    private float circlingAngle = 0f;
    private float previousXPosition;
    #endregion

    #region Formation Variables
    private const float FORMATION_SPACING = 1.2f;
    private const float COHESION_WEIGHT = 0.3f;
    private const float ALIGNMENT_WEIGHT = 0.5f;
    private const float SEPARATION_WEIGHT = 0.8f;
    private const float FORMATION_RADIUS = 5f;
    private Vector2 formationOffset;
    #endregion
    #endregion

    #region Unity Lifecycle
    private void Start() => enemyCollider = GetComponent<Collider2D>();

    private void OnEnable()
    {
        InitializeComponents();
        maxHp = hp;
        originalMoveSpeed = moveSpeed;

        if (GameManager.Instance != null && !GameManager.Instance.enemies.Contains(this))
        {
            GameManager.Instance.enemies.Add(this);
        }

        CalculateFormationOffset();
    }

    private void Update()
    {
        if (!isInit) Initialize();
        Move();
        UpdateVisuals();
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null && GameManager.Instance.enemies != null)
        {
            GameManager.Instance.enemies.Remove(this);
        }
    }
    #endregion

    #region Initialization
    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        previousXPosition = transform.position.x;

        // 대형 오프셋 계산
        CalculateFormationOffset();
    }

    private void Initialize()
    {
        if (GameManager.Instance?.player != null)
        {
            target = GameManager.Instance.player.transform;
            isInit = true;
        }
    }

    private void CalculateFormationOffset()
    {
        if (GameManager.Instance == null) return;

        int totalEnemies = GameManager.Instance.enemies.Count;
        if (totalEnemies == 0)
        {
            formationOffset = Vector2.zero;
            return;
        }

        int index = GameManager.Instance.enemies.IndexOf(this);
        int rowSize = Mathf.Max(1, Mathf.CeilToInt(Mathf.Sqrt(totalEnemies)));

        int row = index / rowSize;
        int col = index % rowSize;

        formationOffset = new Vector2(
            (col - rowSize / 2f) * FORMATION_SPACING,
            (row - rowSize / 2f) * FORMATION_SPACING
        );
    }
    #endregion

    #region Movement
    private Vector2 GetTargetPosition()
    {
        if (target == null) return transform.position;

        Vector2 directionToTarget = ((Vector2)target.position - (Vector2)transform.position).normalized;
        float distanceToTarget = Vector2.Distance(transform.position, target.position);

        if (distanceToTarget > attackRange)
        {
            return (Vector2)target.position - directionToTarget * preferredDistance;
        }
        else if (distanceToTarget < preferredDistance)
        {
            return (Vector2)target.position + directionToTarget * preferredDistance;
        }
        else
        {
            return (Vector2)transform.position + new Vector2(
                Mathf.Sin(Time.time * 2f),
                Mathf.Cos(Time.time * 2f)
            ) * 0.5f;
        }
    }

    public void Move()
    {
        if (isStunned) return;

        Node currentNode = PathFindingManager.Instance.GetNodeFromWorldPosition(transform.position);
        if (currentNode != null && !currentNode.walkable)
        {
            Vector2 safePosition = FindNearestSafePosition(transform.position);
            transform.position = Vector2.MoveTowards(transform.position, safePosition, moveSpeed * 2f * Time.deltaTime);
            return;
        }

        if (!PathFindingManager.Instance.IsPositionInGrid(transform.position))
        {
            Vector2 clampedPosition = PathFindingManager.Instance.ClampToGrid(transform.position);
            transform.position = clampedPosition;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);

        if (distanceToPlayer <= attackRange)
        {
            rb.velocity = Vector2.zero;
            Attack();
            return;
        }

        Vector2 moveToPosition = (Vector2)target.position - ((Vector2)target.position - (Vector2)transform.position).normalized * preferredDistance;
        MoveToPosition(moveToPosition);
    }

    private void MoveToPosition(Vector2 targetPosition)
    {
        if (ShouldUpdatePath())
        {
            List<Vector2> newPath = PathFindingManager.Instance.FindPath(transform.position, targetPosition);
            if (newPath != null && newPath.Count > 0)
            {
                bool isValidPath = true;
                foreach (Vector2 pathPoint in newPath)
                {
                    Node node = PathFindingManager.Instance.GetNodeFromWorldPosition(pathPoint);
                    if (node != null && !node.walkable)
                    {
                        isValidPath = false;
                        break;
                    }
                }

                if (isValidPath)
                {
                    currentPath = newPath;
                    lastPathUpdateTime = Time.time;
                    stuckTimer = 0f;
                }
                else
                {
                    Vector2 safePosition = FindSafePosition(targetPosition);
                    currentPath = PathFindingManager.Instance.FindPath(transform.position, safePosition);
                }
            }
        }

        FollowPath();
    }

    private Vector2 FindSafePosition(Vector2 targetPosition)
    {
        float checkRadius = 2f;
        float angleStep = 45f;

        for (float angle = 0; angle < 360; angle += angleStep)
        {
            float radian = angle * Mathf.Deg2Rad;
            Vector2 checkPosition = targetPosition + new Vector2(
                Mathf.Cos(radian) * checkRadius,
                Mathf.Sin(radian) * checkRadius
            );

            Node node = PathFindingManager.Instance.GetNodeFromWorldPosition(checkPosition);
            if (node != null && node.walkable)
            {
                return checkPosition;
            }
        }

        return transform.position;
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

    private void CircleAroundPlayer()
    {
        if (target == null) return;

        UpdateCirclingParameters();
        Vector2 targetPosition = CalculateCirclingPosition();

        Node targetNode = PathFindingManager.Instance.GetNodeFromWorldPosition(targetPosition);
        if (targetNode != null && !targetNode.walkable)
        {
            targetPosition = FindSafeCirclingPosition(targetPosition);
        }

        ApplyCirclingMovement(targetPosition);
        UpdateSpriteDirection();
    }

    private Vector2 FindSafeCirclingPosition(Vector2 originalPosition)
    {
        float[] checkAngles = { 45f, -45f, 90f, -90f, 135f, -135f, 180f };

        foreach (float angleOffset in checkAngles)
        {
            float newAngle = circlingAngle + angleOffset;
            Vector2 checkPosition = (Vector2)target.position + new Vector2(
                Mathf.Cos(newAngle * Mathf.Deg2Rad),
                Mathf.Sin(newAngle * Mathf.Deg2Rad)
            ) * circlingRadius;

            Node node = PathFindingManager.Instance.GetNodeFromWorldPosition(checkPosition);
            if (node != null && node.walkable)
            {
                return checkPosition;
            }
        }

        return originalPosition;
    }

    private void UpdateCirclingParameters()
    {
        int enemyCount = GameManager.Instance.enemies.Count;
        circlingRadius = Mathf.Max(2.0f, Mathf.Min(3.0f, enemyCount * 0.5f));

        float baseAngle = Time.time * 20f;
        int myIndex = GameManager.Instance.enemies.IndexOf(this);
        float angleStep = 360f / Mathf.Max(1, enemyCount);
        float targetAngle = baseAngle + (myIndex * angleStep);

        circlingAngle = Mathf.LerpAngle(circlingAngle, targetAngle, Time.deltaTime * 5f);
    }

    private Vector2 CalculateCirclingPosition()
    {
        Vector2 offset = new Vector2(
            Mathf.Cos(circlingAngle * Mathf.Deg2Rad),
            Mathf.Sin(circlingAngle * Mathf.Deg2Rad)
        ) * circlingRadius;

        return (Vector2)target.position + offset;
    }

    private void ApplyCirclingMovement(Vector2 targetPosition)
    {
        Vector2 moveDirection = (targetPosition - (Vector2)transform.position).normalized;
        moveDirection = CalculateAvoidanceDirection(transform.position, targetPosition);

        Vector2 separationForce = CalculateSeparationForce(transform.position);
        moveDirection = (moveDirection + separationForce * 0.1f).normalized;

        Vector2 targetVelocity = moveDirection * moveSpeed * 1.2f;
        rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, Time.deltaTime * 8f);
    }
    #endregion

    #region Pathfinding
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

    private void FollowPath()
    {
        if (currentPath == null || currentPath.Count == 0) return;

        Vector2 currentPos = transform.position;
        Vector2 nextWaypoint = currentPath[0];

        HandleStuckCheck(currentPos);
        ProcessWaypoint(currentPos, nextWaypoint);
        ApplyMovement(currentPos, nextWaypoint);
        UpdateSpriteDirection();
    }

    private void ProcessWaypoint(Vector2 currentPos, Vector2 nextWaypoint)
    {
        if (HasReachedWaypoint(currentPos, nextWaypoint))
        {
            if (currentPath != null && currentPath.Count > 0)
            {
                UpdateWaypoint();
                if (currentPath == null || currentPath.Count == 0)
                {
                    MoveDirectlyTowardsTarget();
                }
            }
        }
    }

    private Vector2 FindNearestSafePosition(Vector2 currentPosition)
    {
        float checkRadius = 1f;
        int maxAttempts = 8;
        float angleStep = 360f / maxAttempts;

        for (int i = 0; i < maxAttempts; i++)
        {
            float angle = i * angleStep;
            float radian = angle * Mathf.Deg2Rad;
            Vector2 checkPosition = currentPosition + new Vector2(
                Mathf.Cos(radian) * checkRadius,
                Mathf.Sin(radian) * checkRadius
            );

            Node node = PathFindingManager.Instance.GetNodeFromWorldPosition(checkPosition);
            if (node != null && node.walkable)
            {
                return checkPosition;
            }
        }

        return FindNearestSafePosition(currentPosition + Vector2.one * checkRadius);
    }
    #endregion

    #region Movement Helpers
    private void MoveDirectlyTowardsTarget()
    {
        if (target == null) return;

        Vector2 currentPos = transform.position;
        Vector2 targetPos = GetTargetPosition();

        // 플로킹 동작 계산
        Vector2 flockingForce = CalculateFlockingForce(currentPos);

        // 대형 위치 계산
        Vector2 formationPos = (Vector2)target.position + formationOffset;
        Vector2 formationDir = (formationPos - currentPos).normalized;

        // 최종 이동 방향 계산
        Vector2 moveDir = ((targetPos - currentPos).normalized + flockingForce + formationDir).normalized;
        moveDir = CalculateAvoidanceDirection(currentPos, currentPos + moveDir);

        Vector2 separationForce = CalculateSeparationForce(currentPos);
        moveDir = (moveDir + separationForce * 0.2f).normalized;

        ApplyVelocity(moveDir);
    }

    private Vector2 CalculateSeparationForce(Vector2 currentPos)
    {
        Vector2 separationForce = Vector2.zero;
        float separationRadius = isCirclingPlayer ? 0.8f : 1.2f;

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

    private void ApplyMovement(Vector2 currentPos, Vector2 nextWaypoint)
    {
        Vector2 moveDirection = (nextWaypoint - currentPos).normalized;
        moveDirection = CalculateAvoidanceDirection(currentPos, nextWaypoint);

        Vector2 separationForce = CalculateSeparationForce(currentPos);
        moveDirection = (moveDirection + separationForce * 0.2f).normalized;

        ApplyVelocity(moveDirection);
    }

    private void ApplyVelocity(Vector2 moveDirection)
    {
        Vector2 targetVelocity = moveDirection * moveSpeed;
        rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, Time.deltaTime * 5f);
    }

    private Vector2 CalculateAvoidanceDirection(Vector2 currentPosition, Vector2 targetPosition)
    {
        Vector2 moveDir = (targetPosition - currentPosition).normalized;
        Vector2 finalMoveDir = moveDir;

        Vector2 dirToTarget = (Vector2)target.position - currentPosition;
        bool isVerticalAligned = Mathf.Abs(dirToTarget.x) < 0.1f;
        bool isHorizontalAligned = Mathf.Abs(dirToTarget.y) < 0.1f;

        if ((isVerticalAligned || isHorizontalAligned) && Physics2D.Raycast(currentPosition, moveDir, WALL_AVOIDANCE_DISTANCE, LayerMask.GetMask("Obstacle")))
        {
            Vector2 alternativeDir = isVerticalAligned ? new Vector2(1f, 0f) : new Vector2(0f, 1f);
            if (!Physics2D.Raycast(currentPosition, alternativeDir, WALL_AVOIDANCE_DISTANCE, LayerMask.GetMask("Obstacle")))
            {
                return alternativeDir;
            }
            if (!Physics2D.Raycast(currentPosition, -alternativeDir, WALL_AVOIDANCE_DISTANCE, LayerMask.GetMask("Obstacle")))
            {
                return -alternativeDir;
            }
        }

        var obstacles = CheckObstacles(currentPosition, moveDir);
        if (HasObstacles(obstacles))
        {
            HandleObstacleAvoidance(obstacles);
            finalMoveDir = CalculateAvoidanceVector(obstacles);
        }

        return SmoothDirection(finalMoveDir);
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

    private void ResetPath()
    {
        currentPath = null;
        stuckTimer = 0f;
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

    private Vector2 CalculateFlockingForce(Vector2 currentPos)
    {
        Vector2 cohesion = Vector2.zero;
        Vector2 alignment = Vector2.zero;
        Vector2 separation = Vector2.zero;
        int neighborCount = 0;

        foreach (Enemy enemy in GameManager.Instance.enemies)
        {
            if (enemy == this) continue;

            float distance = Vector2.Distance(currentPos, enemy.transform.position);
            if (distance < FORMATION_RADIUS)
            {
                // 응집력 (Cohesion)
                cohesion += (Vector2)enemy.transform.position;

                // 정렬 (Alignment)
                alignment += enemy.rb.velocity;

                // 분리 (Separation)
                Vector2 diff = currentPos - (Vector2)enemy.transform.position;
                separation += diff.normalized / Mathf.Max(distance, 0.1f);

                neighborCount++;
            }
        }

        if (neighborCount > 0)
        {
            cohesion = (cohesion / neighborCount - currentPos) * COHESION_WEIGHT;
            alignment = (alignment / neighborCount) * ALIGNMENT_WEIGHT;
            separation = separation * SEPARATION_WEIGHT;
        }

        return (cohesion + alignment + separation).normalized;
    }
    #endregion

    #region Combat
    public void TakeDamage(float damage)
    {
        float finalDamage = damage * (1 + defenseDebuffAmount);
        hp -= finalDamage;
        if (attackParticle != null)
        {
            var particle = PoolManager.Instance.Spawn<ParticleSystem>(attackParticle.gameObject,transform.position,quaternion.identity);
            particle.Play();
            Destroy(particle.gameObject, particle.main.duration);
        }
        if (hp <= 0) Die();
    }

    public void Die()
    {

        if (GameManager.Instance?.player != null)
        {
            GameManager.Instance.player.GainExperience(mobEXP);
            GameManager.Instance.player.killCount++;
        }

        if (GameManager.Instance?.enemies != null)
        {
            GameManager.Instance.enemies.Remove(this);
        }

        PoolManager.Instance.Despawn<Enemy>(this);
    }

    private void Attack()
    {
        if (Time.time >= preDamageTime + damageInterval)
        {
            var particle = Instantiate(attackParticle, target.position, Quaternion.identity);
            particle.Play();
            Destroy(particle.gameObject, 0.3f);

            GameManager.Instance.player.TakeDamage(damage);
            preDamageTime = Time.time;
            GameManager.Instance.player.characterControl.PlayAnimation(PlayerState.DAMAGED, 0);
        }
    }

    public void ApplyDefenseDebuff(float amount, float duration)
    {
        StartCoroutine(DefenseDebuffCoroutine(amount, duration));
    }

    private IEnumerator DefenseDebuffCoroutine(float amount, float duration)
    {
        defenseDebuffAmount += amount;
        yield return new WaitForSeconds(duration);
        defenseDebuffAmount -= amount;
    }

    public void ApplySlowEffect(float amount, float duration)
    {
        StartCoroutine(SlowEffectCoroutine(amount, duration));
    }

    private IEnumerator SlowEffectCoroutine(float amount, float duration)
    {
        moveSpeedDebuffAmount += amount;
        moveSpeed = originalMoveSpeed * (1 - moveSpeedDebuffAmount);
        yield return new WaitForSeconds(duration);
        moveSpeedDebuffAmount -= amount;
        moveSpeed = originalMoveSpeed * (1 - moveSpeedDebuffAmount);
    }

    public void ApplyDotDamage(float damagePerTick, float tickInterval, float duration)
    {
        StartCoroutine(DotDamageCoroutine(damagePerTick, tickInterval, duration));
    }

    private IEnumerator DotDamageCoroutine(float damagePerTick, float tickInterval, float duration)
    {
        float endTime = Time.time + duration;
        while (Time.time < endTime)
        {
            TakeDamage(damagePerTick);
            yield return new WaitForSeconds(tickInterval);
        }
    }

    public void ApplyStun(float power, float duration)
    {
        StartCoroutine(StunCoroutine(duration));
    }

    private IEnumerator StunCoroutine(float duration)
    {
        isStunned = true;
        float originalSpeed = moveSpeed;
        moveSpeed = 0;

        yield return new WaitForSeconds(duration);

        isStunned = false;
        moveSpeed = originalSpeed * (1 - moveSpeedDebuffAmount); // 다른 디버프 효과 유지
    }
    #endregion

    #region Collision
    private void Contact()
    {
        var particle = Instantiate(attackParticle, target.position, Quaternion.identity);
        particle.Play();
        Destroy(particle.gameObject, 0.3f);
        Attack();
    }
    #endregion

    #region UI
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

    #region Utility
    public void SetCollisionState(bool isOutOfView)
    {
        if (enemyCollider != null)
        {
            enemyCollider.enabled = !isOutOfView;
        }
    }

    private void UpdateSpriteDirection()
    {
        float currentXPosition = transform.position.x;
        if (currentXPosition != previousXPosition)
        {
            spriteRenderer.flipX = (currentXPosition - previousXPosition) > 0;
            previousXPosition = currentXPosition;
        }
    }
    #endregion


}