using UnityEngine;

public class AttackRegenUpgradeSkill : PermanentPassiveSkill
{
    protected override void ApplyEffectToPlayer(Player player)
    {
        var playerStat = player.GetComponent<PlayerStat>();

        if (_damageIncrease > 0)
        {
            playerStat.AddStatModifier(StatType.Damage, SourceType.Passive, IncreaseType.Mul, _damageIncrease / 100f);
            Debug.Log($"Applied permanent damage increase: {_damageIncrease}%");
        }

        if (_hpRegenIncrease > 0)
        {
            playerStat.AddStatModifier(StatType.HpRegenRate, SourceType.Passive, IncreaseType.Mul, _hpRegenIncrease / 100f);
            Debug.Log($"Applied permanent HP regen rate increase: {_hpRegenIncrease}%");
        }
    }

    protected override void RemoveEffectFromPlayer(Player player)
    {
        var playerStat = player.GetComponent<PlayerStat>();

        if (_damageIncrease > 0)
        {
            playerStat.RemoveStatModifier(StatType.Damage, SourceType.Passive);
        }

        if (_hpRegenIncrease > 0)
        {
            playerStat.RemoveStatModifier(StatType.HpRegenRate, SourceType.Passive);
        }
    }

    public override string GetDetailedDescription()
    {
        var playerStat = GameManager.Instance.player?.GetComponent<PlayerStat>();
        if (playerStat == null) return "Permanently increases attack damage and HP regeneration rate";

        string baseDesc = "Permanently increases attack damage and HP regeneration rate";

        if (skillData?.GetCurrentTypeStat() != null)
        {
            float currentDamage = playerStat.GetStat(StatType.Damage);
            float currentHPRegen = playerStat.GetStat(StatType.HpRegenRate);

            baseDesc += $"\n\nCurrent Effects:" +
                       $"\nAttack Damage: +{_damageIncrease:F1}% (Current: {currentDamage:F1})" +
                       $"\nHP Regen Rate: +{_hpRegenIncrease:F1}% (Current: {currentHPRegen:F1}/s)";
        }
        return baseDesc;
    }

    protected override void UpdateInspectorValues(PassiveSkillStat stats)
    {
        if (stats == null)
        {
            Debug.LogError($"{GetType().Name}: Received null stats");
            return;
        }

        base.UpdateInspectorValues(stats);
        _damageIncrease = stats.damageIncrease;
        _hpRegenIncrease = stats.hpRegenIncrease;

        if (GameManager.Instance?.player != null)
        {
            RemoveEffectFromPlayer(GameManager.Instance.player);
            ApplyEffectToPlayer(GameManager.Instance.player);
        }
    }

    protected override SkillData CreateDefaultSkillData()
    {
        var data = base.CreateDefaultSkillData();
        data.metadata.Name = "Combat Mastery";
        data.metadata.Description = GetDetailedDescription();
        data.metadata.Type = SkillType.Passive;
        return data;
    }

    protected override string GetDefaultSkillName() => "Combat Mastery";
    protected override string GetDefaultDescription() => "Permanently increases attack damage and HP regeneration rate";
    protected override SkillType GetSkillType() => SkillType.Passive;
}