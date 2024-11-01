public class AreaStats
{
    public float damage { get; set; }
    public float radius { get; set; }
    public float tickRate { get; set; }
    public float moveSpeed { get; set; }
    public ElementType elementType { get; set; }
    public float elementalPower { get; set; }
    public AreaPersistenceData persistenceData { get; set; }

    public AreaStats()
    {
        persistenceData = new AreaPersistenceData();
    }
}