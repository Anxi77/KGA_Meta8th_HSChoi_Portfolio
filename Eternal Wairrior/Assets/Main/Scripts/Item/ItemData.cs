using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemData
{
    public string id;
    public string name;
    public string description;
    public ItemType type;
    public ItemRarity rarity;
    public List<StatContainer> stats;
    public int maxStack = 99;
    public float dropRate;
    public int amount = 1;

    public string iconPath;
    public string prefabPath;
    public string iconGuid;
    public string prefabGuid;

    [System.NonSerialized]
    public Sprite icon;
    [System.NonSerialized]
    public GameObject prefab;

    public ItemData Clone()
    {
        return new ItemData
        {
            id = this.id,
            name = this.name,
            description = this.description,
            type = this.type,
            rarity = this.rarity,
            stats = new List<StatContainer>(this.stats),
            icon = this.icon,
            prefab = this.prefab,
            maxStack = this.maxStack,
            dropRate = this.dropRate,
            amount = this.amount,
            iconPath = this.iconPath,
            prefabPath = this.prefabPath,
            iconGuid = this.iconGuid,
            prefabGuid = this.prefabGuid
        };
    }
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
