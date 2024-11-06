using System.Collections.Generic;

[System.Serializable]
public class ItemStatData
{
    public string itemId;
    public string name;
    public ItemType type;
    public ItemRarity rarity;
    public float dropRate;

    // ±âº» ½ºÅÈ
    public float damage;
    public float defense;
    public float hp;
    public float moveSpeed;
    public float attackSpeed;
    public float attackRange;
    public float hpRegen;
    public float expGainRate;
    public float goldGainRate;

    // Æ¯¼ö ½ºÅÈ
    public float criticalChance;
    public float criticalDamage;
    public float lifeSteal;
    public float reflectDamage;
    public float dodgeChance;

    // ÀúÇ× ½ºÅÈ
    public float fireResistance;
    public float iceResistance;
    public float lightningResistance;
    public float poisonResistance;
    public float stunResistance;
    public float slowResistance;

    public List<StatContainer> ConvertToStatContainers()
    {
        var containers = new List<StatContainer>();

        // ±âº» ½ºÅÈ
        if (damage > 0) containers.Add(new StatContainer(StatType.Damage, SourceType.Equipment_Weapon, IncreaseType.Add, damage));
        if (defense > 0) containers.Add(new StatContainer(StatType.Defense, SourceType.Equipment_Armor, IncreaseType.Add, defense));
        if (hp > 0) containers.Add(new StatContainer(StatType.MaxHp, SourceType.Equipment_Armor, IncreaseType.Add, hp));
        if (moveSpeed > 0) containers.Add(new StatContainer(StatType.MoveSpeed, SourceType.Equipment_Accessory, IncreaseType.Mul, moveSpeed));
        if (attackSpeed > 0) containers.Add(new StatContainer(StatType.AttackSpeed, SourceType.Equipment_Weapon, IncreaseType.Mul, attackSpeed));
        if (attackRange > 0) containers.Add(new StatContainer(StatType.AttackRange, SourceType.Equipment_Weapon, IncreaseType.Mul, attackRange));
        if (hpRegen > 0) containers.Add(new StatContainer(StatType.HpRegenRate, SourceType.Equipment_Accessory, IncreaseType.Add, hpRegen));

        // Æ¯¼ö ½ºÅÈ
        if (criticalChance > 0) containers.Add(new StatContainer(StatType.CriticalChance, SourceType.Equipment_Weapon, IncreaseType.Add, criticalChance));
        if (criticalDamage > 0) containers.Add(new StatContainer(StatType.CriticalDamage, SourceType.Equipment_Weapon, IncreaseType.Add, criticalDamage));
        if (lifeSteal > 0) containers.Add(new StatContainer(StatType.LifeSteal, SourceType.Equipment_Weapon, IncreaseType.Add, lifeSteal));
        if (reflectDamage > 0) containers.Add(new StatContainer(StatType.ReflectDamage, SourceType.Equipment_Armor, IncreaseType.Add, reflectDamage));
        if (dodgeChance > 0) containers.Add(new StatContainer(StatType.DodgeChance, SourceType.Equipment_Accessory, IncreaseType.Add, dodgeChance));

        // ÀúÇ× ½ºÅÈ
        if (fireResistance > 0) containers.Add(new StatContainer(StatType.FireResistance, SourceType.Equipment_Armor, IncreaseType.Add, fireResistance));
        if (iceResistance > 0) containers.Add(new StatContainer(StatType.IceResistance, SourceType.Equipment_Armor, IncreaseType.Add, iceResistance));
        if (lightningResistance > 0) containers.Add(new StatContainer(StatType.LightningResistance, SourceType.Equipment_Armor, IncreaseType.Add, lightningResistance));
        if (poisonResistance > 0) containers.Add(new StatContainer(StatType.PoisonResistance, SourceType.Equipment_Armor, IncreaseType.Add, poisonResistance));
        if (stunResistance > 0) containers.Add(new StatContainer(StatType.StunResistance, SourceType.Equipment_Armor, IncreaseType.Add, stunResistance));
        if (slowResistance > 0) containers.Add(new StatContainer(StatType.SlowResistance, SourceType.Equipment_Armor, IncreaseType.Add, slowResistance));

        return containers;
    }
}