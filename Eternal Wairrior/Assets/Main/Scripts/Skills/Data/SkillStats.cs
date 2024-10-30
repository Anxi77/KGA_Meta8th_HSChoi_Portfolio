// 스탯 인터페이스
public interface ISkillStat
{
    BaseSkillStat baseStat { get; set; }
}

// 모든 스킬이 공통적으로 가지는 기본 스탯
[System.Serializable]
public struct BaseSkillStat
{
    public float damage;
    public string skillName;
    public int skillLevel;
    public int maxSkillLevel;
    public ElementType element;        // 스킬 속성
    public float elementalPower;       // 속성 효과 계수
}

// 발사체 스킬 전용 스탯
[System.Serializable]
public struct ProjectileSkillStat : ISkillStat
{
    private BaseSkillStat _baseStat;
    public BaseSkillStat baseStat
    {
        get => _baseStat;
        set => _baseStat = value;
    }

    public float projectileSpeed;
    public float projectileScale;
    public float shotInterval;
    public int pierceCount;
    public float attackRange;
    public float homingRange;
    public bool isHoming;
    public float explosionRad;
    public int projectileCount;
    public float innerInterval;
}

// 영역 스킬 전용 스탯
[System.Serializable]
public struct AreaSkillStat : ISkillStat
{
    private BaseSkillStat _baseStat;
    public BaseSkillStat baseStat
    {
        get => _baseStat;
        set => _baseStat = value;
    }

    public float radius;
    public float duration;
    public float tickRate;
    public bool isPersistent;
    public float moveSpeed;
}

// 패시브 스킬 전용 스탯
[System.Serializable]
public struct PassiveSkillStat : ISkillStat
{
    private BaseSkillStat _baseStat;
    public BaseSkillStat baseStat
    {
        get => _baseStat;
        set => _baseStat = value;
    }

    public float effectDuration;
    public float cooldown;
    public float triggerChance;
}