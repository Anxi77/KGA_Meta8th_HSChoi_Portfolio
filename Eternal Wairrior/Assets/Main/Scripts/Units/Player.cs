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

    #region Level & Experience
    [Header("Level Related")]
    [SerializeField]
    public int level = 1;
    public float exp = 0f;

    private List<float> expList = new List<float>
    {
        100, 250, 450, 700, 1000, 1350, 1750, 2200, 2700, 3300
    };
    public List<float> _expList { get { return expList; } }

    public float HpAmount { get => playerStat.GetStat(StatType.CurrentHp) / playerStat.GetStat(StatType.MaxHp); }
    public float ExpAmount { get => (CurrentExp() / (GetExpForNextLevel() - expList[level - 1])); }
    #endregion

    #region References
    private PlayerStat playerStat;  // PlayerStat 참조 추가
    private Rigidbody2D rb;
    private float x = 0;
    private float y = 0;
    public SPUM_Prefabs characterControl;
    public List<Skill> skills;
    private Vector2 velocity;
    #endregion

    #endregion

    #region Unity Message Methods

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerStat = GameManager.Instance.playerStat;
    }

    private void Start()
    {
        GameManager.Instance.player = this;
        ResetAllStats();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Update()
    {
        GetMoveInput();
    }

    #endregion

    #region Methods

    #region Move&Skills
    public void Move()
    {
        Vector2 input = new Vector2(x, y).normalized;
        float moveSpeed = playerStat.GetStat(StatType.MoveSpeed);
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
            return expList[level - 1] - expList[level - 2];  // 현재 레벨의 요량
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

        // PlayerStat에 레벨업 정보 전달
        playerStat.UpdateStatsForLevel(level);

        // 레벨업 시 체력 회복
        float maxHp = playerStat.GetStat(StatType.MaxHp);
        playerStat.SetCurrentHp(maxHp);
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
        float currentHp = playerStat.GetStat(StatType.CurrentHp);
        float maxHp = playerStat.GetStat(StatType.MaxHp);

        currentHp = Mathf.Min(currentHp + heal, maxHp);
        playerStat.SetCurrentHp(currentHp);
    }

    public void TakeDamage(float damage)
    {
        float defense = playerStat.GetStat(StatType.Defense);
        float actualDamage = Mathf.Max(1, damage - defense);
        float currentHp = playerStat.GetStat(StatType.CurrentHp);

        currentHp -= actualDamage;
        playerStat.SetCurrentHp(currentHp);

        if (currentHp <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        playerStatus = Status.Dead;
    }

    #endregion

    #region Combat
    private Coroutine autoAttackCoroutine;
    private float attackAngle = 120f;  // 공격 각도는 상수로 유지하거나 PlayerStatData로 이동 가능

    private void StartAutoAttack()
    {
        if (autoAttackCoroutine != null)
            StopCoroutine(autoAttackCoroutine);

        autoAttackCoroutine = StartCoroutine(AutoAttackCoroutine());
    }

    private IEnumerator AutoAttackCoroutine()
    {
        while (true)
        {
            if (velocity == Vector2.zero)  // 멈춰있을 때만 공격
            {
                Enemy nearestEnemy = FindNearestEnemy();
                if (nearestEnemy != null)
                {
                    yield return PerformAttack(nearestEnemy);
                }
                else
                {
                    playerStatus = Status.Alive;
                    characterControl.PlayAnimation(PlayerState.IDLE, 0);
                }
            }

            // 공격 속도에 따른 대기
            float attackSpeed = playerStat.GetStat(StatType.AttackSpeed);
            yield return new WaitForSeconds(1f / attackSpeed);
        }
    }

    private IEnumerator PerformAttack(Enemy targetEnemy)
    {
        Vector2 directionToTarget = (targetEnemy.transform.position - transform.position).normalized;

        // 캐릭터 방향 설정
        characterControl.transform.localScale = new Vector3(
            directionToTarget.x > 0 ? -1 : 1, 1, 1);

        // 공격 상태 및 애니메이션 설정
        playerStatus = Status.Attacking;
        characterControl.PlayAnimation(PlayerState.ATTACK, 0);

        // 공격 딜레이
        yield return new WaitForSeconds(0.2f);

        // 범위 내 적 찾기 및 데미지 적용
        float attackRange = playerStat.GetStat(StatType.AttackRange);
        float damage = playerStat.GetStat(StatType.Damage);

        var enemiesInRange = GameManager.Instance.enemies
            .Where(enemy => enemy != null)
            .Where(enemy => {
                Vector2 directionToEnemy = enemy.transform.position - transform.position;
                float distanceToEnemy = directionToEnemy.magnitude;
                float angle = Vector2.Angle(directionToTarget, directionToEnemy);

                return distanceToEnemy <= attackRange && angle <= attackAngle / 2f;
            })
            .ToList();

        // 데미지 적용
        foreach (Enemy enemy in enemiesInRange)
        {
            enemy.TakeDamage(damage);
        }

        playerStatus = Status.Alive;
    }

    private Enemy FindNearestEnemy()
    {
        return GameManager.Instance.enemies?
            .Where(enemy => enemy != null)
            .OrderBy(enemy => Vector2.Distance(transform.position, enemy.transform.position))
            .FirstOrDefault();
    }

    private void OnEnable()
    {
        StartHealthRegeneration();
        StartAutoAttack();  // 자동 공격 시작
    }

    private void OnDisable()
    {
        if (healthRegenCoroutine != null)
            StopCoroutine(healthRegenCoroutine);
        if (autoAttackCoroutine != null)
            StopCoroutine(autoAttackCoroutine);
    }
    #endregion

    #region Passive Skill Effects
    public void ActivateHoming(bool activate)
    {
        playerStat.ActivateHoming(activate);
    }

    public void IncreaseAttackSpeed(float amount)
    {
        playerStat.AddStatModifier(StatType.AttackSpeed, SourceType.Passive, IncreaseType.Mul, amount / 100f);
    }

    public void IncreaseAttackRange(float amount)
    {
        playerStat.AddStatModifier(StatType.AttackRange, SourceType.Passive, IncreaseType.Mul, amount / 100f);
    }

    public void IncreaseDamage(float amount)
    {
        playerStat.AddStatModifier(StatType.Damage, SourceType.Passive, IncreaseType.Mul, amount / 100f);
    }

    public void IncreaseDefense(float amount)
    {
        playerStat.AddStatModifier(StatType.Defense, SourceType.Passive, IncreaseType.Mul, amount / 100f);
    }

    public void IncreaseExpArea(float amount)
    {
        playerStat.AddStatModifier(StatType.ExpCollectionRadius, SourceType.Passive, IncreaseType.Mul, amount / 100f);
    }

    public void IncreaseHP(float percentage)
    {
        playerStat.AddStatModifier(StatType.MaxHp, SourceType.Passive, IncreaseType.Mul, percentage / 100f);
    }

    public void IncreaseHPRegenRate(float amount)
    {
        playerStat.AddStatModifier(StatType.HpRegenRate, SourceType.Passive, IncreaseType.Mul, amount / 100f);
    }

    public void IncreaseMoveSpeed(float amount)
    {
        playerStat.AddStatModifier(StatType.MoveSpeed, SourceType.Passive, IncreaseType.Mul, amount / 100f);
    }

    public void ResetPassiveEffects()
    {
        // 모든 패시브 효과 제거
        playerStat.RemoveStatsBySource(SourceType.Passive);
        playerStat.ActivateHoming(false);
    }
    #endregion

    #region Health Regeneration
    private Coroutine healthRegenCoroutine;
    private const float REGEN_TICK_RATE = 1f;

    private void StartHealthRegeneration()
    {
        if (healthRegenCoroutine != null)
            StopCoroutine(healthRegenCoroutine);

        healthRegenCoroutine = StartCoroutine(HealthRegenCoroutine());
    }

    private IEnumerator HealthRegenCoroutine()
    {
        WaitForSeconds wait = new WaitForSeconds(REGEN_TICK_RATE);

        while (true)
        {
            float regenAmount = playerStat.GetStat(StatType.HpRegenRate);
            TakeHeal(regenAmount);
            yield return wait;
        }
    }
    #endregion

    #endregion

    #region Initializing
    public void ResetAllStats()
    {
        playerStat.ResetToBase();
        this.playerStatus = Status.Alive;
        exp = 0;
        level = 1;
    }

    #endregion

    #region Skills
    public bool AddOrUpgradeSkill(SkillData skillData)
    {
        if (skillData == null) return false;

        // SkillManager를 통해 스킬 추가/업그레이드
        SkillManager.Instance.AddOrUpgradeSkill(skillData);
        return true;
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
    #endregion
}
