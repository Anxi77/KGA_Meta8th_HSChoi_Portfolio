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
        
    }

    #endregion

    #region Methods

    #region UI

    public float CurrentExp() 
    {
        float currentExp = 0;
        if(level == 1) 
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

        characterControl.PlayAnimation(PlayerState.MOVE, 0);

        if(velocity == Vector2.zero) 
        {
            characterControl.PlayAnimation(PlayerState.IDLE, 0);
        }

        

        #region Old Shit

        //float x = Input.GetAxisRaw("Horizontal");
        //float y = Input.GetAxisRaw("Vertical");
        // 입력이 없을 때 속도를 0으로 설정
        //if (input.magnitude == 0)
        //{
        //    velocity = Vector2.zero;
        //}

        //float x = Input.GetAxis("Horizontal");
        //float y = Input.GetAxis("Vertical");

        //if (x != 0 || y != 0)
        //{
        //    Vector3 targetPosition = transform.position + new Vector3(x, y, 0).normalized * moveSpeed * Time.deltaTime;
        //    transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

        //    tailfireAnimCtrl.SetBool("IsMoving", velocity.magnitude > 0.1f);
        //}
        //this.moveDir.up = moveDir;

        #endregion
        #region Notes About Vector
        /*transform.up/right/forward 에 방향 벡터를 대입할 때는 방향벡터의 magnitude 굳이 1로 제한하지 않아도 된다.
        transform.Translate(moveDir * moveSpeed * Time.deltaTime);
        print(this.moveDir.up);//normalized 되어 magnitude가 1로 고정된 방향벡터가 반환된다.
        */
        #endregion

    }

    private void GetMoveInput()
    {
        //만약 rb를 통하여 움직이는 로직이라면 인풋을 받는 타이밍과의 차이가 있기때문에 업데이트에서 인풋을 받고 실제 움직이는 로직은 fixedupdate에서
        x = Input.GetAxisRaw("Horizontal");
        y = Input.GetAxisRaw("Vertical");
    }

    private void SkillInitialize()
    {
        foreach (Skill skill in skills)
        {
            GameObject skillObj = Instantiate(skill.skillPrefabs[skill.skillLevel], transform, false);
            skillObj.name = skill.skillName; //오브젝트 이름 변경
            skillObj.transform.localPosition = Vector2.zero; //스킬 위치를 플레이어의 위치로 가져옴

            if (skillObj.TryGetComponent<ProjectileSkills>(out ProjectileSkills proj))
            {
                proj.isHoming = true;
            }
            else
            {
                proj.isHoming = false;
            }
            skill.currentSkillObject = skillObj;
            skillObj.SetActive(true);
        }
    }

    #region FireOnPlayer
    //private IEnumerator FireCoroutine()
    //{
    //    while (true)
    //    {
    //        yield return new WaitForSeconds(fireInterval);
    //        if (GameManager.Instance.enemies != null)
    //        {
    //            HomingProjectile();
    //        }
    //        else
    //        {
    //            regularFire();
    //        }

    //    }
    //}

    //private void HomingProjectile()
    //{
    //    Enemy targetEnemy = null;
    //    float targetDistance = float.MaxValue; 

    //    foreach (Enemy enemy in GameManager.Instance.enemies)
    //    {
    //        float distance = Vector3.Distance(enemy.transform.position, transform.position);
    //        if (distance < targetDistance)
    //        {
    //            targetDistance = distance;
    //            targetEnemy = enemy;
    //        }
    //    }

    //    Vector2 fireDir = Vector2.zero;

    //    if (targetEnemy != null)
    //    {
    //        fireDir = targetEnemy.transform.position - transform.position;
    //        isFiring = true;
    //    }

    //    Fire(fireDir);
    //}

    //public void regularFire()
    //{
    //    Vector2 fireDir = Vector2.zero;
    //    Vector2 mousePos = Input.mousePosition;
    //    Vector2 mouseScreenPos = Camera.main.ScreenToWorldPoint(mousePos);
    //    fireDir = mouseScreenPos - (Vector2)transform.position;
    //    Fire(fireDir);
    //}

    //public void Fire(Vector2 fireDir)
    //{
    //    LaserProjectile projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
    //    projectile.transform.up = fireDir;
    //    projectile.damage = damage;
    //}
    #endregion

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
        UIManager.Instance.levelupPanel.LevelUpPanelOpen(skills, OnSkillLevelUp);
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
        if (other.gameObject.CompareTag("Enemy"))
        {         
            rb.constraints = RigidbodyConstraints2D.None;
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

    public void Die()
    {
        if (hp <= 0)
        {
            playerStatus = Status.Dead;
        }
    }

    //파라미터로 넘어온 스킬의 레벨을 상승시키고 다음 레벨의 프리팹으로 교체
    public void OnSkillLevelUp(Skill skill)
    {

        if (skill.skillLevel >= skill.skillPrefabs.Length - 1)
        {
            print($"최대 레벨에 도달한 스킬 레벨업을 시도함 : {skill.skillName} ");
            return;
        }
        skill.skillLevel++;//스킬레벨 상승

        Destroy(skill.currentSkillObject);// 기존에 있던 스킬 오브젝트를 제거

        skill.currentSkillObject = Instantiate(skill.skillPrefabs[skill.skillLevel], transform, false);
        skill.currentSkillObject.name = skill.skillPrefabs[skill.skillLevel].name;
        skill.currentSkillObject.transform.localPosition = Vector2.zero;
    }

    #endregion

    #endregion
}
