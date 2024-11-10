using UnityEngine;
using System.Collections;
using Assets.FantasyMonsters.Common.Scripts;

public class BossMonster : Enemy
{
    [Header("Boss Specific Stats")]
    public float enrageThreshold = 0.3f; // 체력 30% 이하일 때 격노
    public float enrageDamageMultiplier = 1.5f;
    public float enrageSpeedMultiplier = 1.3f;

    public Monster monster;

    private bool isEnraged = false;
    private Vector3 startPosition;
    private Animator animator;  

    protected override void Start()
    {
        base.Start();
        startPosition = transform.position;
        animator = GetComponentInChildren<Animator>();
        InitializeBossStats();
    }

    private void InitializeBossStats()
    {
        // 보스 기본 스탯 설정
        hp *= 5f;  // 일반 몬스터보다 5배의 체력
        damage *= 2f;  // 2배의 데미지
        moveSpeed *= 0.8f;  // 80%의 이동속도
        baseDefense *= 2f;  // 2배의 방어력
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);

        // 체력이 특정 수준 이하로 떨어지면 격노 상태
        if (!isEnraged && hp <= maxHp * enrageThreshold)
        {
            EnterEnragedState();
        }
    }

    public override void Move()
    {
        base.Move();
        monster.SetState(MonsterState.Run);
    }

    private void EnterEnragedState()
    {
        isEnraged = true;
        damage *= enrageDamageMultiplier;
        moveSpeed *= enrageSpeedMultiplier;

        // 격노 이펙트 재생
        PlayEnrageEffect();
    }

    private void PlayEnrageEffect()
    {
        // 격노 상태 이펙트 재생 로직
        // 파티클 시스템 등을 사용
    }

    public override void Die()
    {
        MonsterManager.Instance.OnBossDefeated(transform.position);
        base.Die();
    }

    // 보스 전용 공격 패턴들
    //private IEnumerator SpecialAttackPattern()
    //{
    //    while (true)
    //    {
    //        // 기본 공격
    //        yield return new WaitForSeconds(3f);

    //        // 광역 공격
    //        if (hp < maxHp * 0.7f)
    //        {
    //            AreaAttack();
    //            yield return new WaitForSeconds(5f);
    //        }

    //        // 소환 공격
    //        if (hp < maxHp * 0.5f)
    //        {
    //            SummonMinions();
    //            yield return new WaitForSeconds(10f);
    //        }
    //    }
    //}

    //private void AreaAttack()
    //{
    //    // 광역 공격 구현
    //}

    //private void SummonMinions()
    //{
    //    // 하수인 소환 구현
    //}
}