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

        if (skillData.GetStatsForLevel(SkillLevel) == null)
        {
            var stats = new PassiveSkillStat
            {
                baseStat = new BaseSkillStat
                {
                    damage = _damage,
                    skillLevel = _skillLevel,
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
            skillData.SetStatsForLevel(SkillLevel, stats);
            Debug.Log($"Initialized default stats for {GetType().Name} at level {SkillLevel}");
        }
    }

    [Header("Base Stats")]
    [SerializeField] protected float _damage = 10f;
    [SerializeField] protected float _elementalPower = 1f;

    [Header("Passive Effect Stats")]
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

    public override float Damage => _damage;
    public float ElementalPower => _elementalPower;

    protected PassiveSkillStat TypedStats
    {
        get
        {
            var stats = skillData?.GetStatsForLevel(SkillLevel) as PassiveSkillStat;
            if (stats == null)
            {
                stats = new PassiveSkillStat
                {
                    baseStat = new BaseSkillStat
                    {
                        damage = _damage,
                        skillLevel = _skillLevel,
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
            var currentStats = TypedStats;

            // Base Stats 업데이트
            currentStats.baseStat.damage = _damage;
            currentStats.baseStat.skillLevel = _skillLevel;
            currentStats.baseStat.elementalPower = _elementalPower;

            // Passive Stats 업데이트
            currentStats.effectDuration = _effectDuration;
            currentStats.cooldown = _cooldown;
            currentStats.triggerChance = _triggerChance;
            currentStats.damageIncrease = _damageIncrease;
            currentStats.defenseIncrease = _defenseIncrease;
            currentStats.expAreaIncrease = _expAreaIncrease;
            currentStats.homingActivate = _homingActivate;
            currentStats.hpIncrease = _hpIncrease;
            currentStats.moveSpeedIncrease = _moveSpeedIncrease;
            currentStats.attackSpeedIncrease = _attackSpeedIncrease;
            currentStats.attackRangeIncrease = _attackRangeIncrease;
            currentStats.hpRegenIncrease = _hpRegenIncrease;

            // 값 동기화
            _damage = currentStats.baseStat.damage;
            _skillLevel = currentStats.baseStat.skillLevel;
            _elementalPower = currentStats.baseStat.elementalPower;
            _effectDuration = currentStats.effectDuration;
            _cooldown = currentStats.cooldown;
            _triggerChance = currentStats.triggerChance;
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

    protected override void UpdateSkillTypeStats(ISkillStat newStats)
    {
        if (newStats is PassiveSkillStat passiveStats)
        {
            UpdateInspectorValues(passiveStats);
        }
    }

    protected virtual void UpdateInspectorValues(PassiveSkillStat stats)
    {
        if (stats == null || stats.baseStat == null)
        {
            Debug.LogError($"Invalid stats passed to UpdateInspectorValues for {GetType().Name}");
            return;
        }

        Debug.Log($"[PassiveSkills] Before Update - Level: {_skillLevel}");

        // 레벨 업데이트
        _skillLevel = stats.baseStat.skillLevel;  // 인스펙터 값만 업데이트

        // 나머지 스탯 업데이트
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

        Debug.Log($"[PassiveSkills] After Update - Level: {_skillLevel}");
    }

    protected virtual SkillData CreateDefaultSkillData()
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
