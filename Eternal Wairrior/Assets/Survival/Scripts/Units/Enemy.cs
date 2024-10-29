using LaserSystem2D;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    public float originalMoveSpeed;

    public float hpAmount { get { return hp / maxHp; } } 

    private float preDamageTime = 0;
    #endregion

    private Transform target;
    
    public Image hpBar;
    
    private Rigidbody2D rb;
   
    public ParticleSystem impactParticle;

    //public static event Action<Enemy, float> OnEnemyKilled;

    private bool isInit = false;

    #endregion

    #region Unity Message Methods

    public void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        GameManager.Instance.enemies.Add(this);
        if (GameManager.Instance.player != null)
        {
            target = GameManager.Instance.player.transform;
        }
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
    public void Move()
    {
        Vector2 moveDir = target?.position - transform.position ?? Vector2.zero;
        Vector2 movePos = rb.position + (moveDir * moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(movePos);
    }

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
