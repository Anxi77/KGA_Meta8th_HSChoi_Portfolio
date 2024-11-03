using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public abstract class PassiveSkills : Skill
{
    protected override void Awake()
    {
        base.Awake();
        InitializePassiveSkillData();
    }

    private void InitializePassiveSkillData()
    {
        if (skillData == null)
        {
            skillData = CreateDefaultSkillData();
        }

        // 기본 스탯이 없다면 생성
        if (skillData.GetStatsForLevel(1) == null)
        {
            var stats = new PassiveSkillStat
            {
                baseStat = new BaseSkillStat
                {
                    damage = _damage,
                    skillLevel = 1,
                    maxSkillLevel = 5,
                    element = ElementType.None,
                    elementalPower = _elementalPower
                },
                effectDuration = _effectDuration,
                cooldown = _cooldown,
                triggerChance = _triggerChance,
                damageIncrease = _damageIncrease,
                defenseIncrease = _defenseIncrease,
                expAreaIncrease = _expAreaIncrease,
                homingActivate = _homingActivate,
                hpIncrease = _hpIncrease,
                moveSpeedIncrease = _moveSpeedIncrease,
                attackSpeedIncrease = _attackSpeedIncrease,
                attackRangeIncrease = _attackRangeIncrease,
                hpRegenIncrease = _hpRegenIncrease
            };
            skillData.SetStatsForLevel(1, stats);
            Debug.Log($"Initialized default stats for {GetType().Name}");
        }
    }

    [Header("Base Stats")]
    [SerializeField] protected float _damage = 10f;
    [SerializeField] protected float _elementalPower = 1f;

    [Header("On Inspector Control")]
    [SerializeField] protected float _effectDuration = 5f;
    [SerializeField] protected float _cooldown = 10f;
    [SerializeField] protected float _triggerChance = 100f;
    [SerializeField] protected float _damageIncrease = 0f;
    [SerializeField] protected float _defenseIncrease = 0f;
    [SerializeField] protected float _expAreaIncrease = 0f;
    [SerializeField] protected bool _homingActivate = false;
    [SerializeField] protected float _hpIncrease = 0f;
    [SerializeField] protected float _moveSpeedIncrease = 0f;
    [SerializeField] protected float _attackSpeedIncrease = 0f;
    [SerializeField] protected float _attackRangeIncrease = 0f;
    [SerializeField] protected float _hpRegenIncrease = 0f;

    protected PassiveSkillStat TypedStats
    {
        get
        {
            var stats = skillData?.GetStatsForLevel(SkillLevel) as PassiveSkillStat;
            if (stats == null)
            {
                Debug.LogWarning($"No stats found for {GetType().Name} level {SkillLevel}, creating new stats");
                stats = new PassiveSkillStat
                {
                    baseStat = new BaseSkillStat
                    {
                        damage = _damage,
                        skillLevel = SkillLevel,
                        maxSkillLevel = 5,
                        element = skillData?.metadata.Element ?? ElementType.None,
                        elementalPower = _elementalPower
                    },
                    effectDuration = _effectDuration,
                    cooldown = _cooldown,
                    triggerChance = _triggerChance,
                    damageIncrease = _damageIncrease,
                    defenseIncrease = _defenseIncrease,
                    expAreaIncrease = _expAreaIncrease,
                    homingActivate = _homingActivate,
                    hpIncrease = _hpIncrease,
                    moveSpeedIncrease = _moveSpeedIncrease,
                    attackSpeedIncrease = _attackSpeedIncrease,
                    attackRangeIncrease = _attackRangeIncrease,
                    hpRegenIncrease = _hpRegenIncrease
                };
                skillData?.SetStatsForLevel(SkillLevel, stats);
            }
            return stats;
        }
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        if (Application.isPlaying && skillData != null)
        {
            // 인스펙터에서 값이 변경될 때 스탯 업데이트
            var currentStats = TypedStats;

            // 인스펙터 값을 TypedStats에 반영
            currentStats.damageIncrease = _damageIncrease;
            currentStats.defenseIncrease = _defenseIncrease;
            currentStats.expAreaIncrease = _expAreaIncrease;
            currentStats.homingActivate = _homingActivate;
            currentStats.hpIncrease = _hpIncrease;
            currentStats.moveSpeedIncrease = _moveSpeedIncrease;
            currentStats.attackSpeedIncrease = _attackSpeedIncrease;
            currentStats.attackRangeIncrease = _attackRangeIncrease;
            currentStats.hpRegenIncrease = _hpRegenIncrease;

            // TypedStats의 값을 인스펙터 변수에도 반영
            _damageIncrease = currentStats.damageIncrease;
            _defenseIncrease = currentStats.defenseIncrease;
            _expAreaIncrease = currentStats.expAreaIncrease;
            _homingActivate = currentStats.homingActivate;
            _hpIncrease = currentStats.hpIncrease;
            _moveSpeedIncrease = currentStats.moveSpeedIncrease;
            _attackSpeedIncrease = currentStats.attackSpeedIncrease;
            _attackRangeIncrease = currentStats.attackRangeIncrease;
            _hpRegenIncrease = currentStats.hpRegenIncrease;

            skillData.SetStatsForLevel(SkillLevel, currentStats);
            Debug.Log($"Updated stats for {GetType().Name} from inspector");
        }
    }

    protected virtual void Start()
    {
        StartCoroutine(PassiveEffectCoroutine());
    }

    protected virtual IEnumerator PassiveEffectCoroutine()
    {
        while (true)
        {
            if (Random.Range(0f, 100f) <= _triggerChance)
            {
                ApplyPassiveEffect();
            }
            yield return new WaitForSeconds(_cooldown);
        }
    }

    protected virtual void ApplyPassiveEffect()
    {
        if (GameManager.Instance.player == null) return;

        Player player = GameManager.Instance.player;

        StartCoroutine(ApplyTemporaryEffects(player));
    }

    protected virtual IEnumerator ApplyTemporaryEffects(Player player)
    {
        if (_damageIncrease > 0) player.IncreaseDamage(_damageIncrease);
        if (_defenseIncrease > 0) player.IncreaseDefense(_defenseIncrease);
        if (_expAreaIncrease > 0) player.IncreaseExpArea(_expAreaIncrease);
        if (_homingActivate) player.ActivateHoming(true);
        if (_hpIncrease > 0) player.IncreaseMaxHP(_hpIncrease);

        yield return new WaitForSeconds(_effectDuration);

        if (_damageIncrease > 0) player.IncreaseDamage(-_damageIncrease);
        if (_defenseIncrease > 0) player.IncreaseDefense(-_defenseIncrease);
        if (_expAreaIncrease > 0) player.IncreaseExpArea(-_expAreaIncrease);
        if (_homingActivate) player.ActivateHoming(false);
        if (_hpIncrease > 0) player.IncreaseMaxHP(-_hpIncrease);
    }

    #region Skill Level Update
    public override bool SkillLevelUpdate(int newLevel)
    {
        if (newLevel <= MaxSkillLevel)
        {
            var newStats = SkillDataManager.Instance.GetSkillStatsForLevel(skillData.metadata.ID, newLevel, SkillType.Passive);
            if (newStats != null)
            {
                skillData.SetStatsForLevel(newLevel, newStats);
                UpdateInspectorValues(newStats as PassiveSkillStat);
                return true;
            }
        }
        return false;
    }

    protected virtual void UpdateInspectorValues(PassiveSkillStat stats)
    {
        if (stats != null)
        {
            _damage = stats.baseStat.damage;
            _elementalPower = stats.baseStat.elementalPower;
            _effectDuration = stats.effectDuration;
            _cooldown = stats.cooldown;
            _triggerChance = stats.triggerChance;
            _damageIncrease = stats.damageIncrease;
            _defenseIncrease = stats.defenseIncrease;
            _expAreaIncrease = stats.expAreaIncrease;
            _homingActivate = stats.homingActivate;
            _hpIncrease = stats.hpIncrease;
            _moveSpeedIncrease = stats.moveSpeedIncrease;
            _attackSpeedIncrease = stats.attackSpeedIncrease;
            _attackRangeIncrease = stats.attackRangeIncrease;
            _hpRegenIncrease = stats.hpRegenIncrease;

            Debug.Log($"Updated stats for {skillData?.metadata?.Name ?? "Unknown Skill"}:" +
                      $"\nDamage Increase: {_damageIncrease}" +
                      $"\nMove Speed Increase: {_moveSpeedIncrease}" +
                      $"\nAttack Speed Increase: {_attackSpeedIncrease}" +
                      $"\nHP Regen Increase: {_hpRegenIncrease}");
        }
    }
    #endregion
    protected virtual SkillData CreateDefaultSkillData()  // virtual 키워드 추가
    {
        var data = new SkillData();
        data.metadata = new SkillMetadata
        {
            Name = GetDefaultSkillName(),
            Description = GetDefaultDescription(),
            Type = GetSkillType(),
            Element = ElementType.None,
            Tier = 1
        };
        return data;
    }

    protected override void UpdateSkillTypeStats(ISkillStat newStats)
    {
        if (newStats is PassiveSkillStat passiveStats)
        {
            UpdateInspectorValues(passiveStats);
        }
    }

    protected virtual void OnDestroy()
    {
        if (GameManager.Instance?.player != null)
        {
            StopAllCoroutines();
            Player player = GameManager.Instance.player;

            if (_damageIncrease > 0) player.IncreaseDamage(-_damageIncrease);
            if (_defenseIncrease > 0) player.IncreaseDefense(-_defenseIncrease);
            if (_expAreaIncrease > 0) player.IncreaseExpArea(-_expAreaIncrease);
            if (_homingActivate) player.ActivateHoming(false);
            if (_hpIncrease > 0) player.IncreaseMaxHP(-_hpIncrease);
            if (_moveSpeedIncrease > 0) player.IncreaseMoveSpeed(-_moveSpeedIncrease);
            if (_attackSpeedIncrease > 0) player.IncreaseAttackSpeed(-_attackSpeedIncrease);
            if (_attackRangeIncrease > 0) player.IncreaseAttackRange(-_attackRangeIncrease);
            if (_hpRegenIncrease > 0) player.IncreaseHPRegenRate(-_hpRegenIncrease);

            Debug.Log($"Removed all effects for {skillData?.metadata?.Name ?? "Unknown Skill"}");
        }
    }
}
