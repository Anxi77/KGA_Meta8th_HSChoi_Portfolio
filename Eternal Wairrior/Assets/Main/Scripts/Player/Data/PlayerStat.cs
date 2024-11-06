using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum StatType
{
    // ⺻ 
    MaxHp,              // ִ ü
    CurrentHp,          // ü
    Damage,             // ݷ
    Defense,            // 
    MoveSpeed,          // ̵ ӵ
    AttackSpeed,        // ӵ
    AttackRange,        // 
    AttackAngle,        // 

    // Ư
    ExpCollectionRadius,// ġ ȹ
    HpRegenRate,       // HP 
    ExpGainRate,       // ġ ȹ淮
    GoldGainRate,      // ȹ淮
    CriticalChance,    // ġŸ Ȯ
    CriticalDamage,    // ġŸ 

    // 
    FireResistance,    // ȭ
    IceResistance,     // 
    LightningResistance,// 
    PoisonResistance,  // 

    // ̻
    StunResistance,    // 
    SlowResistance,    // ο

    // Ÿ
    Luck,              //  (  )
    DodgeChance,       // ȸ
    ReflectDamage,     // 
    LifeSteal,         // 
}

public enum SourceType
{
    Base,       // ⺻ 
    Level,      // ȿ 
    Passive,    // нú 
    Active,     // Ƽ 
    Equipment_Weapon,    // 
    Equipment_Armor,     // 
    Equipment_Accessory, // 
    Equipment_Special,   // Ư
    Consumable, // Һ
    Buff,       // Ͻ
    Debuff      // 
}

public enum IncreaseType
{
    Add,    // ϱ
    Mul     // ϱ
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
    public StatType statType;      //  
    public SourceType buffType;    //  ȿ
    public IncreaseType incType;   // ϱ ϱ
    public float amount;           // ġ
    public EquipmentSlot equipSlot;  //  ߰

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

public class PlayerStat : MonoBehaviour
{
    [SerializeField] private PlayerStatData baseData;
    private Dictionary<EquipmentSlot, List<StatContainer>> equippedItems = new();
    private Player player;

    private Dictionary<StatType, float> currentStats = new();
    private Dictionary<SourceType, List<StatContainer>> activeEffects = new();

    private void Awake()
    {
        player = GetComponent<Player>();
        if (baseData == null)
        {
            baseData = GameManager.Instance.PlayerStatData;
        }
        InitializeStats();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneUnloaded(Scene scene)
    {
        SaveCurrentState();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RestoreState();
    }

    public void SaveCurrentState()
    {
        baseData.SavePermanentStats();
    }

    public void LoadSavedState(PlayerStatData savedData)
    {
        baseData = savedData;
        InitializeStats();
    }

    public void RestoreState()
    {
        InitializeStats();
    }

    public void InitializeStats()
    {
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

    public void ToggleSourceEffects(SourceType source, bool enable)
    {
        if (!enable)
        {
            if (activeEffects.ContainsKey(source))
            {
                activeEffects[source].Clear();
            }

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

    public void EquipItem(List<StatContainer> itemStats, EquipmentSlot slot)
    {
        UnequipItem(slot);

        equippedItems[slot] = itemStats;
        foreach (var stat in itemStats)
        {
            AddStatModifier(stat.statType, stat.buffType, stat.incType, stat.amount);
        }
    }

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

    public List<StatContainer> GetActiveEffects(SourceType source)
    {
        if (activeEffects.TryGetValue(source, out var effects))
        {
            return new List<StatContainer>(effects);
        }
        return new List<StatContainer>();
    }

    public void RemoveStatModifier(StatType statType, SourceType source)
    {
        if (activeEffects.TryGetValue(source, out var effects))
        {
            effects.RemoveAll(x => x.statType == statType);
            RecalculateStats();
        }
    }

    // 레벨과 경험치 관련
    public int level { get; set; } = 1;
    public float currentExp { get; set; } = 0f;
    public float currentHp
    {
        get => currentStats[StatType.CurrentHp];
        set => SetCurrentHp(value);
    }

    // 새로운 메서드들
    public void RestoreFullHealth()
    {
        float maxHp = GetStat(StatType.MaxHp);
        SetCurrentHp(maxHp);
    }

    public void LoadStats(PlayerStatData data)
    {
        if (data == null) return;
        baseData = data;
        InitializeStats();
    }

    public float GetFinalStatValue(StatType statType)
    {
        return GetStat(statType);
    }

    public void RemoveStatModifier(StatType statType, SourceType source, IncreaseType incType, float amount)
    {
        if (activeEffects.TryGetValue(source, out var effects))
        {
            effects.RemoveAll(x =>
                x.statType == statType &&
                x.incType == incType &&
                x.amount == amount);
            RecalculateStats();
        }
    }
}