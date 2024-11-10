using System.Collections.Generic;

[System.Serializable]
public class ItemStatRange
{
    public StatType statType;
    public float minValue;
    public float maxValue;
    public float weight = 1f;
    public ItemRarity minRarity = ItemRarity.Common;
    public IncreaseType increaseType = IncreaseType.Add;
    public SourceType sourceType = SourceType.Equipment_Weapon;
}

[System.Serializable]
public class ItemStatRangeData
{
    public string itemId;
    public ItemType itemType;
    public List<ItemStatRange> possibleStats = new();
    public int minStatCount = 1;
    public int maxStatCount = 4;
    // 레어리티별 추가 스탯 개수
    public Dictionary<ItemRarity, int> additionalStatsByRarity = new()
    {
        { ItemRarity.Common, 0 },
        { ItemRarity.Uncommon, 1 },
        { ItemRarity.Rare, 2 },
        { ItemRarity.Epic, 3 },
        { ItemRarity.Legendary, 4 }
    };
}

// JSON 직렬화를 위한 래퍼 클래스
[System.Serializable]
public class SerializableItemStatRanges
{
    public Dictionary<string, ItemStatRangeData> items = new();
}

public enum ItemType
{
    None,
    Weapon,
    Armor,
    Accessory,
    Consumable,
    Material
}

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}
