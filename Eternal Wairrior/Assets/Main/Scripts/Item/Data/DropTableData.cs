using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DropTableData
{
    public EnemyType enemyType;
    [SerializeField]
    public List<DropTableEntry> dropEntries = new();
    [SerializeField]
    public float guaranteedDropRate = 0.1f;
    [SerializeField]
    public int maxDrops = 3;
}

[System.Serializable]
public class DropTableEntry
{
    [SerializeField]
    public string itemId;
    [SerializeField]
    public float dropRate;
    [SerializeField]
    public ItemRarity rarity;
    [SerializeField]
    public int minAmount = 1;
    [SerializeField]
    public int maxAmount = 1;
}