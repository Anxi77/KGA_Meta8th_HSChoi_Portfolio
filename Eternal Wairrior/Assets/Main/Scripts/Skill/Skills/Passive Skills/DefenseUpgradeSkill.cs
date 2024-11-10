using UnityEngine;

public class DefenseUpgradeSkill : PermanentPassiveSkill
{
    public override void ApplyEffectToPlayer(Player player)
    {
        var playerStat = player.GetComponent<PlayerStat>();
        if (playerStat == null) return;

        float currentHpRatio = playerStat.GetStat(StatType.CurrentHp) / playerStat.GetStat(StatType.MaxHp);

        if (_defenseIncrease > 0)
        {
            ApplyStatModifier(playerStat, StatType.Defense, _defenseIncrease);
        }

        if (_hpIncrease > 0)
        {
            ApplyStatModifier(playerStat, StatType.MaxHp, _hpIncrease);
            float newMaxHp = playerStat.GetStat(StatType.MaxHp);
            float newCurrentHp = Mathf.Max(1f, newMaxHp * currentHpRatio);
            playerStat.SetCurrentHp(newCurrentHp);
        }
    }

    public override void RemoveEffectFromPlayer(Player player)
    {
        var playerStat = player.GetComponent<PlayerStat>();
        if (playerStat == null) return;

        float currentHpRatio = playerStat.GetStat(StatType.CurrentHp) / playerStat.GetStat(StatType.MaxHp);

        if (_defenseIncrease > 0)
        {
            playerStat.RemoveStatModifier(StatType.Defense, SourceType.Passive);
        }

        if (_hpIncrease > 0)
        {
            playerStat.RemoveStatModifier(StatType.MaxHp, SourceType.Passive);
            
            float newMaxHp = playerStat.GetStat(StatType.MaxHp);
            float newCurrentHp = newMaxHp * currentHpRatio;
            playerStat.SetCurrentHp(newCurrentHp);
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
    public override SkillType GetSkillType() => SkillType.Passive;
}