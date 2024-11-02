using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
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

    #region Stats
    public enum Status
    {
        Alive = 1,
        Dead
    }

    private Status _playerStatus;

    public Status playerStatus { get { return _playerStatus; } set { _playerStatus = value; } }

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

    [Header("Experience Collection")]
    public float expCollectionRadius = 3f; // 경험치 습득 범위
    public float baseExpCollectionRadius = 3f; // 기본 경험치 습득 범위

    public float defense = 2f;  // 본 방어력
    public float baseDefense = 2f;  // 기초 방어력 값
    public float defenseIncreasePerLevel = 0.5f;  // 레벨당 증가하는 방어력

    public float attackSpeed = 1f;  // 초당 공격 횟수
    public float attackRange = 2f;  // 공격 범위
    public float attackAngle = 120f;  // 공격 각도
    private float nextAttackTime = 0f;  // 다음 공격 가능 시간





    #endregion

    #region References

    private Rigidbody2D rb;
    private float x = 0;
    private float y = 0;

    #endregion

    #region EXP & Level

    [Header("Level Related")]

    [SerializeField]
    public int level = 1;

    private List<float> expList = new List<float>
    {
        0, 100, 250, 450, 700, 1000, 1350, 1750, 2200, 2700
    };

    public List<float> _expList { get { return expList; } }

    public float hpIncreasePerLevel = 20f;

    public float damageIncreasePerLevel = 2f;

    public float speedIncreasePerLevel = 0.5f;

    public float HpAmount { get => hp / maxHp; }

    public float ExpAmount { get => (CurrentExp() / (GetExpForNextLevel() - expList[level - 1])); }

    #endregion

    #region Character

    public SPUM_Prefabs characterControl;

    #endregion

    #region Skills

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
    }

    #endregion

    #region Methods

    #region UI

    public float CurrentExp()
    {
        float currentExp = 0;
        if (level == 1)
        {
            currentExp = exp;
        }
        else
        {
            currentExp = exp - expList[level - 1];
        }
        return currentExp;
    }

    #endregion

    #region Move&Skills
    public void Move()
    {
        Vector2 input = new Vector2(x, y).normalized;
        velocity = input * moveSpeed;

        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);

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

    private void GetMoveInput()
    {
        x = Input.GetAxisRaw("Horizontal");
        y = Input.GetAxisRaw("Vertical");
    }

    #endregion

    #region Level & EXP
    public float GetExpForNextLevel()
    {
        if (level >= expList.Count)
        {
            return 99999f;
        }
        return expList[level];
    }

    public void GainExperience(float amount)
    {
        if (level < expList.Count)
        {
            exp += amount;
        }
        while (exp >= GetExpForNextLevel() && level < expList.Count)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        level++;
        maxHp += hpIncreasePerLevel;
        hp = maxHp;
        damage += damageIncreasePerLevel;
        moveSpeed += speedIncreasePerLevel;
        defense += defenseIncreasePerLevel;  // 레벨업 시 방어력 증가
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
        if (Time.time >= nextAttackTime)
        {
            Enemy nearestEnemy = FindNearestEnemy();
            if (nearestEnemy != null)
            {
                PerformAttack(nearestEnemy);
                nextAttackTime = Time.time + (1f / attackSpeed);
            }
        }
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

    private void PerformAttack(Enemy targetEnemy)
    {
        Vector2 directionToTarget = (targetEnemy.transform.position - transform.position).normalized;

        if (directionToTarget.x > 0)
        {
            characterControl.transform.localScale = new Vector3(1, 1, 1);
        }
        else if (directionToTarget.x < 0)
        {
            characterControl.transform.localScale = new Vector3(-1, 1, 1);
        }

        if (GameManager.Instance.enemies != null)
        {
            foreach (Enemy enemy in GameManager.Instance.enemies)
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
                            enemy.TakeDamage(damage);
                            characterControl.PlayAnimation(PlayerState.ATTACK, 0);
                        }
                    }
                }
            }
        }
    }

    public void IncreaseAttackSpeed(float amount)
    {
        attackSpeed += amount;
    }

    public void IncreaseAttackRange(float amount)
    {
        attackRange += amount;
    }
    #endregion
}
