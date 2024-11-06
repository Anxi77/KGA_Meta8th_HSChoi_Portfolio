using UnityEngine;

public class RangeUpgradeSkill : PermanentPassiveSkill
{
    protected override void UpdateInspectorValues(PassiveSkillStat stats)
    {
        if (stats == null)
        {
            Debug.LogError($"{GetType().Name}: Received null stats");
            return;
        }

        base.UpdateInspectorValues(stats);
        _attackRangeIncrease = stats.attackRangeIncrease;
        _expAreaIncrease = stats.expAreaIncrease;

        if (GameManager.Instance?.player != null)
        {
            RemoveEffectFromPlayer(GameManager.Instance.player);
            ApplyEffectToPlayer(GameManager.Instance.player);
        }
    }

    public override string GetDetailedDescription()
    {
        var playerStat = GameManager.Instance.playerStat;
        string baseDesc = "Permanently increases attack range and experience collection radius";

        if (skillData?.GetCurrentTypeStat() != null)
        {
            float currentRange = playerStat.GetStat(StatType.AttackRange);
            float currentExpRadius = playerStat.GetStat(StatType.ExpCollectionRadius);

            baseDesc += $"\n\nCurrent Effects:" +
                       $"\nAttack Range: +{_attackRangeIncrease:F1}% (Current: {currentRange:F1})" +
                       $"\nExp Collection Radius: +{_expAreaIncrease:F1}% (Current: {currentExpRadius:F1})";
        }
        return baseDesc;
    }

    protected override string GetDefaultSkillName() => "Range Mastery";
    protected override string GetDefaultDescription() => "Permanently increases attack range and experience collection radius";
    protected override SkillType GetSkillType() => SkillType.Passive;
}