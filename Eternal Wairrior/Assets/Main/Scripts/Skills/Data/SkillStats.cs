

using Unity.VisualScripting;

public interface ISkillStat
{
    BaseSkillStat baseStat { get; set; }
}


[System.Serializable]
public class BaseSkillStat
{
    public float damage;
    public string skillName;
    public int skillLevel;
    public int maxSkillLevel;
    public SkillType skillType;
    public ElementType element;     
    public float elementalPower;      

    public BaseSkillStat()
    {
        damage = 10f;
        skillLevel = 1;
        maxSkillLevel = 5;
    }
}

// 발사체 스킬 전용 스탯

[System.Serializable]
public class ProjectileSkillStat : ISkillStat
{
    private BaseSkillStat _baseStat = new BaseSkillStat();

    public BaseSkillStat baseStat
    {
        get => _baseStat;
        set => _baseStat = value;
    }

    public SkillType skillType = SkillType.Projectile;
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


[System.Serializable]
public class AreaSkillStat : ISkillStat
{
    private BaseSkillStat _baseStat = new BaseSkillStat();

    public BaseSkillStat baseStat
    {
        get => _baseStat;
        set => _baseStat = value;
    }

    public SkillType skillType = SkillType.Area;
    public float radius;
    public float duration;
    public float tickRate;
    public bool isPersistent;
    public float moveSpeed;
}

[System.Serializable]
public class PassiveSkillStat : ISkillStat
{
    private BaseSkillStat _baseStat = new BaseSkillStat();

    public BaseSkillStat baseStat
    {
        get => _baseStat;
        set => _baseStat = value;
    }

    public SkillType skillType = SkillType.Passive;
    public float effectDuration;
    public float cooldown;
    public float triggerChance;
}
