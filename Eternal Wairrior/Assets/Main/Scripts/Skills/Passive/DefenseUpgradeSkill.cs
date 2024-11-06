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
            player.IncreaseHP(_hpIncrease);
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
            player.IncreaseHP(-_hpIncrease);
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
        _defenseIncrease = stats.defenseIncrease;
        _hpIncrease = stats.hpIncrease;

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
        var playerStat = GameManager.Instance.playerStat;
        string baseDesc = "Permanently increases defense and maximum HP";

        if (skillData?.GetCurrentTypeStat() != null)
        {
            float currentDefense = playerStat.GetStat(StatType.Defense);
            float currentMaxHp = playerStat.GetStat(StatType.MaxHp);

            baseDesc += $"\n\nCurrent Effects:" +
                       $"\nDefense: +{_defenseIncrease:F1}% (Current: {currentDefense:F1})" +
                       $"\nMax HP: +{_hpIncrease:F1}% (Current: {currentMaxHp:F0})";
        }
        return baseDesc;
    }
}