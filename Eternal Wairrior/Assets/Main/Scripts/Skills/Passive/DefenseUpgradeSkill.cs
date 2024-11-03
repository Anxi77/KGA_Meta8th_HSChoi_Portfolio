using UnityEngine;

public class DefenseUpgradeSkill : PermanentPassiveSkill
{
    protected override void ApplyEffectToPlayer(Player player)
    {
        if (_defenseIncrease > 0)
        {
            player.IncreaseDefense(_defenseIncrease);
            Debug.Log($"Applied permanent defense increase: {_defenseIncrease}%");
        }

        if (_hpIncrease > 0)
        {
            player.IncreaseMaxHP(_hpIncrease);
            Debug.Log($"Applied permanent max HP increase: {_hpIncrease}%");
        }
    }

    protected override void RemoveEffectFromPlayer(Player player)
    {
        if (_defenseIncrease > 0)
        {
            player.IncreaseDefense(-_defenseIncrease);
            Debug.Log($"Removed defense increase: {_defenseIncrease}%");
        }

        if (_hpIncrease > 0)
        {
            player.IncreaseMaxHP(-_hpIncrease);
            Debug.Log($"Removed max HP increase: {_hpIncrease}%");
        }
    }

    protected override void UpdateInspectorValues(PassiveSkillStat stats)
    {
        if (stats == null)
        {
            Debug.LogError($"{GetType().Name}: Received null stats");
            return;
        }

        base.UpdateInspectorValues(stats);
        LogCurrentStats();

        if (GameManager.Instance?.player != null)
        {
            RemoveEffectFromPlayer(GameManager.Instance.player);
            ApplyEffectToPlayer(GameManager.Instance.player);
        }
    }

    protected override string GetDefaultSkillName() => "Defense Mastery";
    protected override string GetDefaultDescription() => "Permanently increases defense and maximum HP";
    protected override SkillType GetSkillType() => SkillType.Passive;

    public override string GetDetailedDescription()
    {
        string baseDesc = "Permanently increases defense and maximum HP";
        if (skillData?.GetCurrentTypeStat() != null)
        {
            baseDesc += $"\n\nCurrent Effects:" +
                       $"\nDefense: +{_defenseIncrease:F1}%" +
                       $"\nMax HP: +{_hpIncrease:F1}%";
        }
        return baseDesc;
    }
}