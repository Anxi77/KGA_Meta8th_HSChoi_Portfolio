using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class ItemEffectData
{
    public string effectId;
    public string effectName;
    public EffectType effectType;
    public float value;
    public ItemRarity minRarity;
    public ItemType[] applicableTypes;
    public SkillType[] applicableSkills;
    public ElementType[] applicableElements;
    public float weight;

    public bool CanApplyTo(ItemData item, SkillType skillType = SkillType.None, ElementType element = ElementType.None)
    {
        if (item.rarity < minRarity) return false;
        if (applicableTypes != null && !applicableTypes.Contains(item.type)) return false;
        if (skillType != SkillType.None && applicableSkills != null && !applicableSkills.Contains(skillType)) return false;
        if (element != ElementType.None && applicableElements != null && !applicableElements.Contains(element)) return false;
        return true;
    }
}

public enum EffectType
{
    None,
    DamageBonus,
    CooldownReduction,
    ProjectileSpeed,
    ProjectileRange,
    HomingEffect,
    AreaRadius,
    AreaDuration,
    ElementalPower
}

[System.Serializable]
public class ItemEffectPool
{
    public ItemType itemType;
    public ItemRarity minRarity;
    public List<ItemEffectData> effects = new();
}