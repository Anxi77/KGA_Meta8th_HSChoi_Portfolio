using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Unity.VisualScripting;

[System.Serializable]
public class ItemMetadata
{
    public string ID;
    public string Name;
    public string Description;
    public ItemType Type;
    public ItemRarity Rarity;
    public ElementType Element;
    public int MaxStack = 1;
    public float DropRate;
    public int MinAmount = 1;
    public int MaxAmount = 1;
    public List<StatType> baseStatTypes = new();

    // 아이콘 관련 필드만 유지
    [System.NonSerialized]
    private Sprite _icon;
    public string iconPath;
    public string iconGuid;

    public Sprite Icon
    {
        get => _icon;
        set
        {
            _icon = value;
#if UNITY_EDITOR
            if (value != null)
            {
                string assetPath = UnityEditor.AssetDatabase.GetAssetPath(value);
                iconPath = GetResourcePath(assetPath);
                iconGuid = UnityEditor.AssetDatabase.AssetPathToGUID(assetPath);
            }
#endif
        }
    }

    private string GetResourcePath(string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath)) return string.Empty;
        int resourcesIndex = fullPath.IndexOf("Resources/");
        if (resourcesIndex == -1) return fullPath;
        string relativePath = fullPath.Substring(resourcesIndex + 10);
        return relativePath;
    }
}

[System.Serializable]
public class ItemData
{
    public ItemMetadata metadata = new();
    public ItemStatRangeData statRanges = new()
    {
        possibleStats = new List<ItemStatRange>(),
        minStatCount = 1,
        maxStatCount = 4,
        additionalStatsByRarity = new Dictionary<ItemRarity, int>()
        {
            { ItemRarity.Common, 0 },
            { ItemRarity.Uncommon, 1 },
            { ItemRarity.Rare, 2 },
            { ItemRarity.Epic, 3 },
            { ItemRarity.Legendary, 4 }
        }
    };
    private List<StatContainer> _stats = new();
    public List<StatContainer> stats
    {
        get => _stats;
        set => _stats = value ?? new List<StatContainer>();
    }
    public ItemEffectRangeData effectRanges = new()
    {
        possibleEffects = new List<ItemEffectRange>(),
        minEffectCount = 1,
        maxEffectCount = 3,
        additionalEffectsByRarity = new Dictionary<ItemRarity, int>()
        {
            { ItemRarity.Common, 0 },
            { ItemRarity.Uncommon, 1 },
            { ItemRarity.Rare, 2 },
            { ItemRarity.Epic, 3 },
            { ItemRarity.Legendary, 4 }
        }
    };
    private List<ItemEffectData> _effects = new();
    public List<ItemEffectData> effects
    {
        get => _effects;
        set => _effects = value ?? new List<ItemEffectData>();
    }
    public List<string> effectTypes = new();
    public Dictionary<string, float> effectValues = new();

    // 직렬화를 위한 필드들
    public string iconPath { get; set; }
    public string prefabPath { get; set; }
    public string iconGuid { get; set; }
    public string prefabGuid { get; set; }

    // 프로퍼티들
    public string id { get => metadata.ID; set => metadata.ID = value; }
    public string name { get => metadata.Name; set => metadata.Name = value; }
    public string description { get => metadata.Description; set => metadata.Description = value; }
    public ItemType type { get => metadata.Type; set => metadata.Type = value; }
    public ItemRarity rarity { get => metadata.Rarity; set => metadata.Rarity = value; }
    public ElementType element { get => metadata.Element; set => metadata.Element = value; }
    public int maxStack { get => metadata.MaxStack; set => metadata.MaxStack = value; }
    public float dropRate { get => metadata.DropRate; set => metadata.DropRate = value; }
    public int minAmount { get => metadata.MinAmount; set => metadata.MinAmount = value; }
    public int maxAmount { get => metadata.MaxAmount; set => metadata.MaxAmount = value; }
    public Sprite icon { get => metadata.Icon; set => metadata.Icon = value; }
    public int amount { get; set; } = 1;

    // 스탯 관련 메서드들
    public void AddStat(StatContainer stat)
    {
        if (stat == null) return;
        // 같은 타입의 스탯이 있다면 제거
        _stats.RemoveAll(s => s.statType == stat.statType);
        _stats.Add(stat);
    }
    public StatContainer GetStat(StatType statType)
    {
        return _stats.FirstOrDefault(s => s.statType == statType);
    }
    public float GetStatValue(StatType statType)
    {
        var stat = GetStat(statType);
        return stat?.amount ?? 0f;
    }
    public List<StatContainer> GetAllStats()
    {
        return new List<StatContainer>(_stats);
    }
    public void ClearStats()
    {
        _stats.Clear();
    }

    // 효과 관련 메서드들
    public void AddEffect(ItemEffectData effect)
    {
        if (effect == null) return;
        _effects.Add(effect);
    }
    public void RemoveEffect(string effectId)
    {
        _effects.RemoveAll(e => e.effectId == effectId);
    }
    public ItemEffectData GetEffect(string effectId)
    {
        return _effects.FirstOrDefault(e => e.effectId == effectId);
    }
    public List<ItemEffectData> GetEffectsByType(EffectType type)
    {
        return _effects.Where(e => e.effectType == type).ToList();
    }
    public List<ItemEffectData> GetEffectsForSkill(SkillType skillType)
    {
        return _effects.Where(e => e.applicableSkills?.Contains(skillType) ?? false).ToList();
    }
    public List<ItemEffectData> GetEffectsForElement(ElementType elementType)
    {
        return _effects.Where(e => e.applicableElements?.Contains(elementType) ?? false).ToList();
    }

    // 직렬화/역직렬화
    public string Serialize()
    {
        var data = new SerializedItemData
        {
            metadata = this.metadata,
            stats = this.stats,
            statRanges = this.statRanges,
            effects = this.effects,
            effectTypes = this.effectTypes,
            effectValues = this.effectValues,
            iconPath = this.metadata.iconPath,
            iconGuid = this.metadata.iconGuid
        };
        return JsonUtility.ToJson(data);
    }
    public static ItemData Deserialize(string json)
    {
        var data = JsonUtility.FromJson<SerializedItemData>(json);
        return new ItemData
        {
            metadata = data.metadata,
            stats = data.stats,
            statRanges = data.statRanges,
            effects = data.effects,
            effectTypes = data.effectTypes,
            effectValues = data.effectValues,
            iconPath = data.iconPath,
            prefabPath = data.prefabPath,
            iconGuid = data.iconGuid,
            prefabGuid = data.prefabGuid
        };
    }

    // 복제
    public ItemData Clone()
    {
        var clone = new ItemData
        {
            metadata = new ItemMetadata
            {
                ID = this.metadata.ID,
                Name = this.metadata.Name,
                Description = this.metadata.Description,
                Type = this.metadata.Type,
                Rarity = this.metadata.Rarity,
                Element = this.metadata.Element,
                MaxStack = this.metadata.MaxStack,
                DropRate = this.metadata.DropRate,
                MinAmount = this.metadata.MinAmount,
                MaxAmount = this.metadata.MaxAmount,
                baseStatTypes = new List<StatType>(this.metadata.baseStatTypes),
                iconPath = this.metadata.iconPath,
                iconGuid = this.metadata.iconGuid
            },
            statRanges = this.statRanges,
            stats = new List<StatContainer>(this.stats),
            effectRanges = this.effectRanges,
            effects = new List<ItemEffectData>(this.effects),
            effectTypes = new List<string>(this.effectTypes)
        };

        // 아이콘 참조도 복사
        clone.metadata.Icon = this.metadata.Icon;
        clone.icon = this.metadata.Icon;  // ItemData의 icon 필드도 설정

        return clone;
    }
}

[System.Serializable]
public class SerializedItemData
{
    public ItemMetadata metadata;
    public List<StatContainer> stats;
    public ItemStatRangeData statRanges;
    public List<ItemEffectData> effects;
    public List<string> effectTypes;
    public Dictionary<string, float> effectValues;
    public string iconPath;
    public string prefabPath;
    public string iconGuid;
    public string prefabGuid;
}
