[System.Serializable]
public class AreaSkillStat : ISkillStat
{
    public BaseSkillStat baseStat { get; set; }

    // 扁夯 康开 加己
    public float radius { get; set; }
    public float tickRate { get; set; }
    public float moveSpeed { get; set; }

    // 瘤加 瓤苞 包访 加己
    public AreaPersistenceData persistenceData { get; set; }

    public AreaSkillStat()
    {
        baseStat = new BaseSkillStat();
        persistenceData = new AreaPersistenceData();
    }
}

[System.Serializable]
public class AreaPersistenceData
{
    public bool isPersistent { get; set; }
    public float duration { get; set; }
    public float effectInterval { get; set; }
}