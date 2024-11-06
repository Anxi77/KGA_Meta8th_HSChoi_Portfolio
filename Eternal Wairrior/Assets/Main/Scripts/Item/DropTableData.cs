using System.Collections.Generic;

[System.Serializable]
public class DropTableData
{
    public EnemyType enemyType;
    public List<DropTableEntry> dropEntries = new();
    public float guaranteedDropRate = 0.1f; // 최소 드롭 확률
    public int maxDrops = 3; // 최대 드롭 개수
}

[System.Serializable]
public class DropTableEntry
{
    public string itemId;
    public float dropRate;
    public ItemRarity rarity;
    public int minAmount = 1;
    public int maxAmount = 1;
}