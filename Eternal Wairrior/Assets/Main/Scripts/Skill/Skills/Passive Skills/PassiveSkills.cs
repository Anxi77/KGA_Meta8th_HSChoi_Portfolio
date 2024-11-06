using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public abstract class PassiveSkills : Skill
{
    protected override void Awake()
    {
        base.Awake();
    }

    public override void Initialize()
    {
        InitializePassiveSkillData();
    }

    private void InitializePassiveSkillData()
    {
        if (skillData == null) return;

        var csvStats = SkillDataManager.Instance.GetSkillStatsForLevel(
            skillData.metadata.ID,
            SkillLevel,
            SkillType.Passive
        ) as PassiveSkillStat;

        if (csvStats != null)
        {
            UpdateInspectorValues(csvStats);
            skillData.SetStatsForLevel(SkillLevel, csvStats);
        }
        else
        {
            Debug.LogWarning($"No CSV data found for {skillData.metadata.Name}, using default values");
            var defaultStats = new PassiveSkillStat
            {
                baseStat = new BaseSkillStat
                {
                    damage = _damage,
                    skillLevel = _skillLevel,
                    maxSkillLevel = 5,
                    element = ElementType.None,
                    elementalPower = _elementalPower
                },
                moveSpeedIncrease = _moveSpeedIncrease,
                effectDuration = _effectDuration,
                cooldown = _cooldown,
                triggerChance = _triggerChance,
                damageIncrease = _damageIncrease,
                defenseIncrease = _defenseIncrease,
                expAreaIncrease = _expAreaIncrease,
                homingActivate = _homingActivate,
                hpIncrease = _hpIncrease,

            };
            skillData.SetStatsForLevel(SkillLevel, defaultStats);
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

            // Base Stats Ʈ
            currentStats.baseStat.damage = _damage;
            currentStats.baseStat.skillLevel = _skillLevel;
            currentStats.baseStat.elementalPower = _elementalPower;

            // Passive Stats Ʈ
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

            //  ȭ
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
        var playerStat = player.GetComponent<PlayerStat>();

        if (_damageIncrease > 0)
            playerStat.AddStatModifier(StatType.Damage, SourceType.Passive, IncreaseType.Mul, _damageIncrease / 100f);

        if (_defenseIncrease > 0)
            playerStat.AddStatModifier(StatType.Defense, SourceType.Passive, IncreaseType.Mul, _defenseIncrease / 100f);

        if (_expAreaIncrease > 0)
            playerStat.AddStatModifier(StatType.ExpCollectionRadius, SourceType.Passive, IncreaseType.Mul, _expAreaIncrease / 100f);

        if (_homingActivate)
            player.ActivateHoming(true);

        if (_hpIncrease > 0)
            playerStat.AddStatModifier(StatType.MaxHp, SourceType.Passive, IncreaseType.Mul, _hpIncrease / 100f);

        yield return new WaitForSeconds(_effectDuration);

        if (_damageIncrease > 0 || _defenseIncrease > 0 || _expAreaIncrease > 0 || _hpIncrease > 0)
            playerStat.RemoveStatsBySource(SourceType.Passive);

        if (_homingActivate)
            player.ActivateHoming(false);
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

        _skillLevel = stats.baseStat.skillLevel;

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
            var playerStat = player.GetComponent<PlayerStat>();

            playerStat.RemoveStatsBySource(SourceType.Passive);
            if (_homingActivate)
                player.ActivateHoming(false);

            Debug.Log($"Removed all effects for {skillData?.metadata?.Name ?? "Unknown Skill"}");
        }
    }
}
