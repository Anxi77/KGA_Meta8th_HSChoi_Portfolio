using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static GameManager;

public class Player : MonoBehaviour
{
    #region Members

    #region Status
    public enum Status
    {
        Alive = 1,
        Dead,
        Attacking
    }
    private Status _playerStatus;
    public Status playerStatus { get { return _playerStatus; } set { _playerStatus = value; } }
    #endregion

    #region Base Stats
    public float maxHp;
    public float hp = 100f;
    public float damage = 5f;
    public float moveSpeed = 5f;
    public float exp = 0f;
    public int totalKillCount = 0;
    public int killCount = 0;
    public float fireInterval;
    public bool isFiring;
    private Vector2 velocity;
    #endregion

    #region Combat Stats
    public float defense = 2f;  //  방어력
    public float baseDefense = 2f;  // 기초 방어력 값
    public float defenseIncreasePerLevel = 0.5f;  // 레벨당 증가하는 방어력

    public float attackSpeed = 1f;  // 초당 공격 횟수
    public float attackRange = 2f;  // 공격 범위
    public float attackAngle = 120f;  // 공격 각도
    private float nextAttackTime = 0f;  // 다음 공격 가능 시간
    #endregion

    #region Collection & Regeneration
    [Header("Experience Collection")]
    public float expCollectionRadius = 3f; // 경험치 습득 범위
    public float baseExpCollectionRadius = 3f; // 기본 경험치 습득 범위

    [Header("Health Regeneration")]
    public float baseHpRegenRate = 1f;  // 기본 초당 체력 회복량
    public float hpRegenMultiplier { get; private set; } = 1f;  // 체력 회복 증가 배수
    private float lastRegenTime;
    private const float REGEN_TICK_RATE = 1f;  // 회복 주기 (1초)
    #endregion

    #region Passive Effect Multipliers
    private float damageMultiplier = 1f;
    private float defenseMultiplier = 1f;
    private float expAreaMultiplier = 1f;
    private float maxHpMultiplier = 1f;
    private float moveSpeedMultiplier = 1f;
    private bool isHomingActivated = false;
    private float attackSpeedMultiplier = 1f;  // 추가: 공격 속도 배수
    #endregion

    #region Level & Experience
    [Header("Level Related")]
    [SerializeField]
    public int level = 1;

    private List<float> expList = new List<float>
    {
        100, 250, 450, 700, 1000, 1350, 1750, 2200, 2700, 3300  // 0 제거, 각 레벨에 필요한 총 경험치
    };
    public List<float> _expList { get { return expList; } }

    public float hpIncreasePerLevel = 20f;
    public float damageIncreasePerLevel = 2f;
    public float speedIncreasePerLevel = 0.5f;

    public float HpAmount { get => hp / maxHp; }
    public float ExpAmount { get => (CurrentExp() / (GetExpForNextLevel() - expList[level - 1])); }
    #endregion

    #region References
    private Rigidbody2D rb;
    private float x = 0;
    private float y = 0;
    public SPUM_Prefabs characterControl;
    public List<Skill> skills;
    #endregion

    #endregion

    #region Unity Message Methods

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

    }

    private void Start()
    {
        GameManager.Instance.player = this;
        maxHp = hp;
        this.playerStatus = Status.Alive;
        isFiring = false;
        lastRegenTime = Time.time;
        exp = 0;
        level = 1;
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Update()
    {
        GetMoveInput();
        Die();
        AutoAttack();  // 자동 공격 실행
        UpdateHealthRegeneration();
    }

    #endregion

    #region Methods

    #region UI

    public float CurrentExp()
    {
        // 최대 레벨 체크
        if (level >= expList.Count)
        {
            return 0;
        }

        // 현재 레벨에서의 경험치 반환
        if (level == 1)
        {
            return exp;  // 1레벨일 때는 현재 경험치 그대로 반환
        }
        else
        {
            return exp - expList[level - 2];  // 현재 레벨에서의 경험치 계산
        }
    }

    #endregion

    #region Move&Skills
    public void Move()
    {
        Vector2 input = new Vector2(x, y).normalized;
        velocity = input * moveSpeed;

        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);

        if (playerStatus != Status.Attacking)
        {
            if (velocity != Vector2.zero)
            {
                if (x > 0)
                {
                    characterControl.transform.localScale = new Vector3(1, 1, 1);
                }
                else if (x < 0)
                {
                    characterControl.transform.localScale = new Vector3(-1, 1, 1);
                }

                characterControl.PlayAnimation(PlayerState.MOVE, 0);
            }
            else
            {
                characterControl.PlayAnimation(PlayerState.IDLE, 0);
            }
        }
    }

    private void GetMoveInput()
    {
        x = Input.GetAxisRaw("Horizontal");
        y = Input.GetAxisRaw("Vertical");
    }

    #endregion

    #region Level & EXP
    public float GetExpForNextLevel()
    {
        // 최대 레벨 체크
        if (level >= expList.Count)
        {
            return 99999f;
        }

        // 다음 레벨까지 필요한 경험치 계산
        if (level == 1)
        {
            return expList[0];  // 1레벨에서는 첫 번째 경험치 요구량 반환
        }
        else
        {
            return expList[level - 1] - expList[level - 2];  // 현재 레벨의 요구량
        }
    }

    public void GainExperience(float amount)
    {
        if (level >= expList.Count) return;

        exp += amount;

        // 레벨업 체크 - 한 번에 한 레벨씩만 상승하도록 수정
        if (level < expList.Count && exp >= expList[level - 1])
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        level++;

        // 기본 스탯 증가
        UpdateMaxHP();  // 레벨업 시 HP 증가 처리가 포함됨
        UpdateDamage();
        moveSpeed += speedIncreasePerLevel;
        UpdateDefense();  // 레벨업 시 방어력 증가 처리가 포함됨

        // 레벨업 시 체력 회복
        hp = maxHp;
    }
    #endregion

    #region Interactions

    public void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.TryGetComponent<IContactable>(out var contact))
        {
            contact.Contact();
        }

        if (other.gameObject.CompareTag("Enemy"))
        {
            rb.constraints = RigidbodyConstraints2D.FreezePosition;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.TryGetComponent<IContactable>(out var contact))
        {
            contact.Contact();
        }
    }


    public void TakeHeal(float heal)
    {
        hp += heal;
        if (hp > maxHp)
        {
            hp = maxHp;
        }
    }

    public void TakeDamage(float damage)
    {
        float actualDamage = Mathf.Max(1, damage - defense);  // 방어력을 적용한 실제 데미지 계산 (최소 1의 데미지는 보장)
        hp -= actualDamage;

        if (hp <= 0)
        {
            hp = 0;
            Die();
        }
    }

    public void Die()
    {
        playerStatus = Status.Dead;
    }

    #endregion

    #endregion

    public void IncreaseExpCollectionRadius(float amount)
    {
        expCollectionRadius += amount;
    }

    public void ResetExpCollectionRadius()
    {
        expCollectionRadius = baseExpCollectionRadius;
    }

    public void ResetDefense()
    {
        defense = baseDefense;
    }

    #region Combat
    private void AutoAttack()
    {
        if (velocity == Vector2.zero && Time.time >= nextAttackTime)
        {
            Enemy nearestEnemy = FindNearestEnemy();
            if (nearestEnemy != null)
            {
                Vector2 directionToTarget = (nearestEnemy.transform.position - transform.position).normalized;

                if (directionToTarget.x > 0)
                {
                    characterControl.transform.localScale = new Vector3(-1, 1, 1);
                }
                else if (directionToTarget.x < 0)
                {
                    characterControl.transform.localScale = new Vector3(1, 1, 1);
                }

                playerStatus = Status.Attacking;
                characterControl.PlayAnimation(PlayerState.ATTACK, 0);

                StartCoroutine(ApplyDamageAfterDelay(nearestEnemy, directionToTarget));
                nextAttackTime = Time.time + (1f / attackSpeed);
            }
            else
            {
                playerStatus = Status.Alive;
                characterControl.PlayAnimation(PlayerState.IDLE, 0);
            }
        }
    }

    private IEnumerator ApplyDamageAfterDelay(Enemy targetEnemy, Vector2 directionToTarget)
    {
        yield return new WaitForSeconds(0.2f);

        // 공격 범위 내의 적들 찾기
        List<Enemy> enemiesInRange = new List<Enemy>();
        foreach (Enemy enemy in GameManager.Instance.enemies.ToList())
        {
            if (enemy != null)
            {
                Vector2 directionToEnemy = (enemy.transform.position - transform.position);
                float distanceToEnemy = directionToEnemy.magnitude;

                if (distanceToEnemy <= attackRange)
                {
                    float angle = Vector2.Angle(directionToTarget, directionToEnemy);
                    if (angle <= attackAngle / 2f)
                    {
                        enemiesInRange.Add(enemy);
                    }
                }
            }
        }

        // 데미지 적용
        foreach (Enemy enemy in enemiesInRange)
        {
            enemy.TakeDamage(damage);
        }

        // 공격 애니메이션이 끝나면 상태 리셋
        playerStatus = Status.Alive;
    }

    private Enemy FindNearestEnemy()
    {
        Enemy nearestEnemy = null;
        float nearestDistance = float.MaxValue;

        if (GameManager.Instance.enemies != null)
        {
            foreach (Enemy enemy in GameManager.Instance.enemies)
            {
                if (enemy != null)
                {
                    float distance = Vector2.Distance(transform.position, enemy.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestEnemy = enemy;
                    }
                }
            }
        }

        return nearestEnemy;
    }

    // 공격 속도 증가 메서드
    public void IncreaseAttackSpeed(float percentage)
    {
        // 퍼센트를 소수점으로 변환 (예: 100% -> 1.0)
        float multiplierIncrease = percentage / 100f;
        attackSpeedMultiplier += multiplierIncrease;
        UpdateAttackSpeed();
        Debug.Log($"Attack speed multiplier increased by {percentage}% (total multiplier: {attackSpeedMultiplier:F2})");
    }

    // 공격 범위 증가 메서드
    public void IncreaseAttackRange(float amount)
    {
        attackRange += amount;
    }
    #endregion

    #region Passive Skill Effects
    public void IncreaseDamage(float amount)
    {
        damageMultiplier += amount / 100f;  // percentage to multiplier
        UpdateDamage();
        Debug.Log($"Damage multiplier: {damageMultiplier}, Current damage: {damage}");
    }

    public void IncreaseDefense(float amount)
    {
        defenseMultiplier += amount / 100f;  // percentage to multiplier
        UpdateDefense();
    }

    public void IncreaseExpArea(float amount)
    {
        expAreaMultiplier += amount / 100f;  // percentage to multiplier
        UpdateExpCollectionRadius();
    }

    public void ActivateHoming(bool activate)
    {
        isHomingActivated = activate;

        if (skills == null) return;

        foreach (var skill in skills)
        {
            // 스킬이 ProjectileSkills 타입인지 확인
            if (skill is ProjectileSkills projectileSkill)
            {
                // 스킬의 메타데이터 확인
                var skillData = projectileSkill.GetSkillData();
                if (skillData != null && skillData.metadata.Type == SkillType.Projectile)
                {
                    projectileSkill.UpdateHomingState(activate);
                    Debug.Log($"Homing {(activate ? "activated" : "deactivated")} for {skillData.metadata.Name}");
                }
            }
        }

        if (activate)
        {
            Debug.Log("Homing effect activated for all projectile skills");
        }
        else
        {
            Debug.Log("Homing effect deactivated for all projectile skills");
        }
    }

    public void IncreaseMaxHP(float amount)
    {
        maxHpMultiplier += amount / 100f;  // percentage to multiplier
        UpdateMaxHP();
    }

    public void IncreaseMoveSpeed(float percentage)
    {
        // 퍼센트를 소수점으로 변환 (예: 100% -> 1.0)
        float multiplierIncrease = percentage / 100f;
        moveSpeedMultiplier += multiplierIncrease;
        UpdateMoveSpeed();
        Debug.Log($"Move speed multiplier increased by {percentage}% (total multiplier: {moveSpeedMultiplier:F2})");
    }

    private void UpdateMoveSpeed()
    {
        float baseSpeed = 5f + (level - 1) * speedIncreasePerLevel;  // 기본 속도 + 레벨업으로 인한 증가
        moveSpeed = baseSpeed * moveSpeedMultiplier;
        Debug.Log($"Updated move speed: base={baseSpeed}, multiplier={moveSpeedMultiplier}, final={moveSpeed}");
    }

    private void UpdateDamage()
    {
        float baseDamage = 5f;  // 기본 데미지 값 추가
        float levelBaseDamage = baseDamage + (level - 1) * damageIncreasePerLevel;
        damage = levelBaseDamage * damageMultiplier;
    }

    private void UpdateDefense()
    {
        float baseDefenseValue = baseDefense * (1 + (level - 1) * defenseIncreasePerLevel);
        defense = baseDefenseValue * defenseMultiplier;
    }

    private void UpdateExpCollectionRadius()
    {
        expCollectionRadius = baseExpCollectionRadius * expAreaMultiplier;
    }

    private void UpdateMaxHP()
    {
        float baseMaxHp = 100f + (level - 1) * hpIncreasePerLevel;  // 기본 HP + 레벨업으로 인한 증가
        float newMaxHp = baseMaxHp * maxHpMultiplier;

        // HP 비율 유지하면서 최대 HP 변경
        float hpRatio = hp / maxHp;
        maxHp = newMaxHp;
        hp = maxHp * hpRatio;
    }

    // 모든 패시브 효과 초기화
    public void ResetPassiveEffects()
    {
        damageMultiplier = 1f;
        defenseMultiplier = 1f;
        expAreaMultiplier = 1f;
        isHomingActivated = false;
        maxHpMultiplier = 1f;
        moveSpeedMultiplier = 1f;
        attackSpeedMultiplier = 1f;
        hpRegenMultiplier = 1f;

        UpdateDamage();
        UpdateDefense();
        UpdateExpCollectionRadius();
        UpdateMaxHP();
        UpdateMoveSpeed();
        UpdateAttackSpeed();
        ActivateHoming(false);
    }
    #endregion

    public void ResetAllStats()
    {
        ResetPassiveEffects();
    }

    public bool AddOrUpgradeSkill(SkillData skillData)
    {
        if (skillData == null) return false;

        var existingSkill = skills.Find(s => s.SkillID == skillData.metadata.ID);

        if (existingSkill != null)
        {
            if (existingSkill.SkillLevel >= existingSkill.MaxSkillLevel)
            {
                Debug.LogWarning($"Skill {skillData.metadata.Name} is already at max level");
                return false;
            }

            return existingSkill.SkillLevelUpdate(existingSkill.SkillLevel + 1);
        }
        else
        {
            try
            {
                GameObject skillObj = Instantiate(skillData.metadata.Prefab, transform);
                if (skillObj.TryGetComponent<Skill>(out Skill newSkill))
                {
                    newSkill.SetSkillData(skillData);
                    skills.Add(newSkill);
                    return true;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error adding new skill: {e.Message}");
            }
        }

        return false;
    }

    public void RemoveSkill(SkillID skillID)
    {
        var skillToRemove = skills.Find(s => s.SkillID == skillID);
        if (skillToRemove != null)
        {
            skills.Remove(skillToRemove);
            Destroy(skillToRemove.gameObject);
        }
    }

    private void UpdateHealthRegeneration()
    {
        if (Time.time >= lastRegenTime + REGEN_TICK_RATE)
        {
            float regenAmount = baseHpRegenRate * hpRegenMultiplier;
            TakeHeal(regenAmount);
            lastRegenTime = Time.time;
        }
    }

    public void IncreaseHPRegenRate(float percentage)
    {
        hpRegenMultiplier += percentage / 100f;
        Debug.Log($"HP Regen multiplier increased to: {hpRegenMultiplier}");
    }
    private void UpdateAttackSpeed()
    {
        float baseAttackSpeed = 1f;  // 기본 초당 공격 횟수
        attackSpeed = baseAttackSpeed * attackSpeedMultiplier;
        Debug.Log($"Updated attack speed: base={baseAttackSpeed}, multiplier={attackSpeedMultiplier}, final={attackSpeed}");
    }

    public float CurrentDamage => damage;
    public float CurrentDefense => defense;
    public float CurrentMoveSpeed => moveSpeed;
    public float CurrentAttackSpeed => attackSpeed;
    public float CurrentAttackRange => attackRange;
    public float CurrentExpCollectionRadius => expCollectionRadius;

    private bool IsPlayingAttackAnimation()
    {
        return playerStatus == Status.Attacking;
    }



}
