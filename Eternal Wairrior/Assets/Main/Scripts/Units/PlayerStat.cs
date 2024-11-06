using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum StatType
{
    // 기본 스탯
    MaxHp,              // 최대 체력
    CurrentHp,          // 현재 체력
    Damage,             // 공격력
    Defense,            // 방어력
    MoveSpeed,          // 이동 속도
    AttackSpeed,        // 공격 속도
    AttackRange,        // 공격 범위
    AttackAngle,        // 공격 각도

    // 특수 스탯
    ExpCollectionRadius,// 경험치 획득 범위
    HpRegenRate,       // HP 재생률
    ExpGainRate,       // 경험치 획득량
    GoldGainRate,      // 골드 획득량
    CriticalChance,    // 치명타 확률
    CriticalDamage,    // 치명타 데미지

    // 원소 저항
    FireResistance,    // 화염 저항
    IceResistance,     // 빙결 저항
    LightningResistance,// 번개 저항
    PoisonResistance,  // 독 저항

    // 상태이상 저항
    StunResistance,    // 기절 저항
    SlowResistance,    // 슬로우 저항

    // 기타
    Luck,              // 행운 (아이템 드랍률 등)
    DodgeChance,       // 회피율
    ReflectDamage,     // 데미지 반사
    LifeSteal,         // 생명력 흡수
}

public enum SourceType
{
    Base,       // 기본 스탯
    Level,      // 레벨업으로 인한 증가
    Passive,    // 패시브 스킬 효과
    Active,     // 액티브 스킬 효과
    Equipment_Weapon,    // 무기
    Equipment_Armor,     // 방어구
    Equipment_Accessory, // 장신구
    Equipment_Special,   // 특수 장비
    Consumable, // 소비 아이템
    Buff,       // 일시적 버프
    Debuff      // 디버프
}

public enum IncreaseType
{
    Add,    // 더하기
    Mul     // 곱하기
}

public enum EquipmentSlot
{
    None,
    Weapon,
    Armor,
    Accessory1,
    Accessory2,
    Special
}

[System.Serializable]
public struct StatContainer
{
    public StatType statType;      // 어떤 스탯인지
    public SourceType buffType;    // 어디서 온 효과인지
    public IncreaseType incType;   // 더하기인지 곱하기인지
    public float amount;           // 수치
    public EquipmentSlot equipSlot;  // 장비 슬롯 정보 추가

    public StatContainer(StatType statType, SourceType buffType, IncreaseType incType, float amount, EquipmentSlot slot = EquipmentSlot.None)
    {
        this.statType = statType;
        this.buffType = buffType;
        this.incType = incType;
        this.amount = amount;
        this.equipSlot = slot;
    }

    public override string ToString()
    {
        return $"[{buffType}] {statType} {(incType == IncreaseType.Add ? "+" : "x")} {amount}";
    }
}

public class PlayerStat
{
    private PlayerStatData baseData;  // 기본 스탯 데이터
    private Dictionary<StatType, float> currentStats = new();
    private Dictionary<SourceType, List<StatContainer>> activeEffects = new();
    private Player player;

    // 슬롯별 현재 장착된 장비의 효과 추적
    private Dictionary<EquipmentSlot, List<StatContainer>> equippedItems = new();

    public PlayerStat(Player player, PlayerStatData baseData)
    {
        this.player = player;
        this.baseData = baseData;
        InitializeStats();
    }

    private void InitializeStats()
    {
        // 기본 스탯 초기화
        currentStats[StatType.MaxHp] = baseData.baseHp;
        currentStats[StatType.CurrentHp] = baseData.baseHp;
        currentStats[StatType.Damage] = baseData.baseDamage;
        currentStats[StatType.MoveSpeed] = baseData.baseSpeed;
        currentStats[StatType.Defense] = baseData.baseDefense;
        currentStats[StatType.AttackSpeed] = baseData.baseAttackSpeed;
        currentStats[StatType.AttackRange] = baseData.baseAttackRange;
        currentStats[StatType.AttackAngle] = baseData.baseAttackAngle;
        currentStats[StatType.ExpCollectionRadius] = baseData.baseExpCollectionRadius;
        currentStats[StatType.HpRegenRate] = baseData.baseHpRegenRate;

        // 다른 스탯들도 PlayerStatData의 base 값으로 초기화
        currentStats[StatType.ExpGainRate] = baseData.baseExpGainRate;
        currentStats[StatType.GoldGainRate] = baseData.baseGoldGainRate;
        currentStats[StatType.CriticalChance] = baseData.baseCriticalChance;
        currentStats[StatType.CriticalDamage] = baseData.baseCriticalDamage;
        currentStats[StatType.FireResistance] = baseData.baseFireResistance;
        currentStats[StatType.IceResistance] = baseData.baseIceResistance;
        currentStats[StatType.LightningResistance] = baseData.baseLightningResistance;
        currentStats[StatType.PoisonResistance] = baseData.basePoisonResistance;
        currentStats[StatType.StunResistance] = baseData.baseStunResistance;
        currentStats[StatType.SlowResistance] = baseData.baseSlowResistance;
        currentStats[StatType.Luck] = baseData.baseLuck;
        currentStats[StatType.DodgeChance] = baseData.baseDodgeChance;
        currentStats[StatType.ReflectDamage] = baseData.baseReflectDamage;
        currentStats[StatType.LifeSteal] = baseData.baseLifeSteal;

        // 영구적인 효과들 적용
        foreach (var effect in baseData.GetAllPermanentStats())
        {
            if (!activeEffects.ContainsKey(effect.buffType))
                activeEffects[effect.buffType] = new List<StatContainer>();

            activeEffects[effect.buffType].Add(effect);
        }

        RecalculateStats();
    }

    public void AddStatModifier(StatType statType, SourceType source, IncreaseType incType, float amount)
    {
        var container = new StatContainer(statType, source, incType, amount);

        if (!activeEffects.ContainsKey(source))
            activeEffects[source] = new List<StatContainer>();

        activeEffects[source].Add(container);

        // 영구적인 효과라면 저장
        if (IsPermanentSource(source))
        {
            baseData.AddPermanentStat(container);
        }

        RecalculateStats();
    }

    private bool IsPermanentSource(SourceType source)
    {
        return source == SourceType.Equipment_Weapon ||
               source == SourceType.Equipment_Armor ||
               source == SourceType.Equipment_Accessory ||
               source == SourceType.Equipment_Special;
    }

    public float GetStat(StatType type) => currentStats[type];

    private void RecalculateStats()
    {
        // 스탯 재계산 로직
        foreach (StatType statType in System.Enum.GetValues(typeof(StatType)))
        {
            float baseValue = GetBaseValue(statType);
            float addValue = 0;
            float mulValue = 1f;

            foreach (var effectList in activeEffects.Values)
            {
                foreach (var effect in effectList)
                {
                    if (effect.statType != statType) continue;

                    if (effect.incType == IncreaseType.Add)
                        addValue += effect.amount;
                    else
                        mulValue *= (1 + effect.amount);
                }
            }

            currentStats[statType] = (baseValue + addValue) * mulValue;
        }
    }

    public void RemoveStatsBySource(SourceType source)
    {
        if (activeEffects.ContainsKey(source))
        {
            activeEffects[source].Clear();

            // 영구적인 효과인 경우 permanentStats에서도 제거
            if (IsPermanentSource(source))
            {
                baseData.RemovePermanentStatsBySource(source);
            }
        }

        RecalculateStats();
    }

    private bool isHomingActivated = false;

    public void ActivateHoming(bool activate)
    {
        isHomingActivated = activate;

        if (player.skills == null) return;

        foreach (var skill in player.skills)
        {
            if (skill is ProjectileSkills projectileSkill)
            {
                var skillData = projectileSkill.GetSkillData();
                if (skillData != null && skillData.metadata.Type == SkillType.Projectile)
                {
                    projectileSkill.UpdateHomingState(activate);
                }
            }
        }
    }

    public bool IsHomingActivated() => isHomingActivated;

    public void UpdateStatsForLevel(int level)
    {
        // 레벨에 따른 스탯 증가
        AddStatModifier(StatType.MaxHp, SourceType.Level, IncreaseType.Add,
            baseData.hpIncreasePerLevel * (level - 1));

        AddStatModifier(StatType.Damage, SourceType.Level, IncreaseType.Add,
            baseData.damageIncreasePerLevel * (level - 1));

        AddStatModifier(StatType.MoveSpeed, SourceType.Level, IncreaseType.Add,
            baseData.speedIncreasePerLevel * (level - 1));

        AddStatModifier(StatType.Defense, SourceType.Level, IncreaseType.Add,
            baseData.defenseIncreasePerLevel * (level - 1));
    }

    private float GetBaseValue(StatType statType)
    {
        switch (statType)
        {
            case StatType.MaxHp:
                return baseData.baseHp;
            case StatType.Damage:
                return baseData.baseDamage;
            case StatType.MoveSpeed:
                return baseData.baseSpeed;
            case StatType.Defense:
                return baseData.baseDefense;
            case StatType.AttackSpeed:
                return baseData.baseAttackSpeed;
            case StatType.AttackRange:
                return baseData.baseAttackRange;
            case StatType.ExpCollectionRadius:
                return baseData.baseExpCollectionRadius;
            case StatType.HpRegenRate:
                return baseData.baseHpRegenRate;
            default:
                return 0f;
        }
    }

    public void ResetToBase()
    {
        activeEffects.Clear();
        foreach (SourceType source in System.Enum.GetValues(typeof(SourceType)))
        {
            if (IsPermanentSource(source))
            {
                baseData.RemovePermanentStatsBySource(source);
            }
        }
        InitializeStats();
    }

    public void SetCurrentHp(float value)
    {
        currentStats[StatType.CurrentHp] = Mathf.Clamp(value, 0, currentStats[StatType.MaxHp]);
    }

    // 특정 소스의 스탯 효과만 임시로 비활성화/활성화
    public void ToggleSourceEffects(SourceType source, bool enable)
    {
        if (!enable)
        {
            if (activeEffects.ContainsKey(source))
            {
                activeEffects[source].Clear();
            }

            // 영구적인 효과라면 다시 적용
            if (IsPermanentSource(source))
            {
                foreach (var stat in baseData.GetPermanentStats(source))
                {
                    if (!activeEffects.ContainsKey(source))
                        activeEffects[source] = new List<StatContainer>();
                    activeEffects[source].Add(stat);
                }
            }
        }

        RecalculateStats();
    }

    // 장비 장착
    public void EquipItem(List<StatContainer> itemStats, EquipmentSlot slot)
    {
        // 해당 슬롯의 기존 장비 효과 제거
        UnequipItem(slot);

        // 새 장비 효과 적용
        equippedItems[slot] = itemStats;
        foreach (var stat in itemStats)
        {
            AddStatModifier(stat.statType, stat.buffType, stat.incType, stat.amount);
        }
    }

    // 장비 해제
    public void UnequipItem(EquipmentSlot slot)
    {
        if (equippedItems.TryGetValue(slot, out var existingStats))
        {
            foreach (var stat in existingStats)
            {
                RemoveSpecificStat(stat);
            }
            equippedItems.Remove(slot);
        }
    }

    // 특정 장비 슬롯의 효과만 제거/활성화
    public void ToggleEquipmentSlot(EquipmentSlot slot, bool enable)
    {
        if (equippedItems.TryGetValue(slot, out var stats))
        {
            if (!enable)
            {
                foreach (var stat in stats)
                {
                    RemoveSpecificStat(stat);
                }
            }
            else
            {
                foreach (var stat in stats)
                {
                    AddStatModifier(stat.statType, stat.buffType, stat.incType, stat.amount);
                }
            }
        }
    }

    private void RemoveSpecificStat(StatContainer stat)
    {
        if (activeEffects.TryGetValue(stat.buffType, out var effects))
        {
            effects.RemoveAll(x =>
                x.statType == stat.statType &&
                x.incType == stat.incType &&
                x.amount == stat.amount &&
                x.equipSlot == stat.equipSlot);
        }
        RecalculateStats();
    }

    // 현재 활성화된 효과들 가져오기
    public List<StatContainer> GetActiveEffects(SourceType source)
    {
        if (activeEffects.TryGetValue(source, out var effects))
        {
            return new List<StatContainer>(effects);
        }
        return new List<StatContainer>();
    }
}