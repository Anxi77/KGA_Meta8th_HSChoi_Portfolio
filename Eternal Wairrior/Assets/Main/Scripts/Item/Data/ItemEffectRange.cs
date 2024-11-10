using System.Collections.Generic;

[System.Serializable]
public class ItemEffectRange
{
    public string effectId;
    public string effectName;
    public string description;
    public EffectType effectType;
    public float minValue;
    public float maxValue;
    public float weight = 1f;
    public ItemRarity minRarity = ItemRarity.Common;
    public ItemType[] applicableTypes;
    public SkillType[] applicableSkills;
    public ElementType[] applicableElements;
}

[System.Serializable]
public class ItemEffectRangeData
{
    public string itemId;
    public ItemType itemType;
    public List<ItemEffectRange> possibleEffects = new List<ItemEffectRange>();
    public int minEffectCount = 1;
    public int maxEffectCount = 3;

    // 레어리티별 추가 효과 개수
    public Dictionary<ItemRarity, int> additionalEffectsByRarity = new Dictionary<ItemRarity, int>
    {
        { ItemRarity.Common, 0 },
        { ItemRarity.Uncommon, 1 },
        { ItemRarity.Rare, 2 },
        { ItemRarity.Epic, 3 },
        { ItemRarity.Legendary, 4 }
    };
}
