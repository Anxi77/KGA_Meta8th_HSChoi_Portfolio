using System.Collections.Generic;

public class ItemEffectApplier
{
    private PlayerStat playerStat;

    public void ApplyItemEffects(ItemData item, EquipmentSlot slot)
    {
        foreach (var stat in item.stats)
        {
            var sourceType = GetSourceTypeForSlot(slot);
            var modifiedStat = new StatContainer(
                stat.statType,
                sourceType,
                stat.incType,
                stat.amount,
                slot
            );
            playerStat.AddStatModifier(
                modifiedStat.statType,
                modifiedStat.buffType,
                modifiedStat.incType,
                modifiedStat.amount
            );
        }
    }

    private SourceType GetSourceTypeForSlot(EquipmentSlot slot)
    {
        return slot switch
        {
            EquipmentSlot.Weapon => SourceType.Equipment_Weapon,
            EquipmentSlot.Armor => SourceType.Equipment_Armor,
            EquipmentSlot.Accessory1 or EquipmentSlot.Accessory2 => SourceType.Equipment_Accessory,
            EquipmentSlot.Special => SourceType.Equipment_Special,
            _ => SourceType.Consumable
        };
    }
}