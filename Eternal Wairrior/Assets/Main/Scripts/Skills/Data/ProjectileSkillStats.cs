public class ProjectileStats
{
    // 기본 스탯
    public float damage { get; set; }
    public float moveSpeed { get; set; }
    public float scale { get; set; }
    public ElementType elementType { get; set; }
    public float elementalPower { get; set; }

    // 투사체 동작 관련
    public int pierceCount { get; set; }
    public float maxTravelDistance { get; set; }

    // 지속 효과 관련
    public ProjectilePersistenceData persistenceData { get; set; }

    public ProjectileStats()
    {
        persistenceData = new ProjectilePersistenceData();
    }
}