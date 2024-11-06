using UnityEngine;

public class DefenseUpgradeSkill : PermanentPassiveSkill
{
    protected override void ApplyEffectToPlayer(Player player)
    {
        var playerStat = player.GetComponent<PlayerStat>();

        if (_defenseIncrease > 0)
        {
            playerStat.AddStatModifier(StatType.Defense, SourceType.Passive, IncreaseType.Mul, _defenseIncrease / 100f);
            Debug.Log($"Applied permanent defense increase: {_defenseIncrease}%");
        }

        if (_hpIncrease > 0)
        {
            playerStat.AddStatModifier(StatType.MaxHp, SourceType.Passive, IncreaseType.Mul, _hpIncrease / 100f);
            Debug.Log($"Applied permanent max HP increase: {_hpIncrease}%");
        }
    }

    protected override void RemoveEffectFromPlayer(Player player)
    {
        var playerStat = player.GetComponent<PlayerStat>();

        if (_defenseIncrease > 0)
        {
            playerStat.RemoveStatModifier(StatType.Defense, SourceType.Passive);
        }

        if (_hpIncrease > 0)
        {
            playerStat.RemoveStatModifier(StatType.MaxHp, SourceType.Passive);
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

    public override string GetDetailedDescription()
    {
        var playerStat = GameManager.Instance.player?.GetComponent<PlayerStat>();
        if (playerStat == null) return "Permanently increases defense and maximum HP";

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

    protected override string GetDefaultSkillName() => "Defense Mastery";
    protected override string GetDefaultDescription() => "Permanently increases defense and maximum HP";
    protected override SkillType GetSkillType() => SkillType.Passive;
}