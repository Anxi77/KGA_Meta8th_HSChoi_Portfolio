using UnityEngine;

public class SpeedUpgradeSkill : PermanentPassiveSkill
{
    protected override void ApplyEffectToPlayer(Player player)
    {
        var playerStat = player.GetComponent<PlayerStat>();

        if (_moveSpeedIncrease > 0)
        {
            playerStat.AddStatModifier(StatType.MoveSpeed, SourceType.Passive, IncreaseType.Mul, _moveSpeedIncrease / 100f);
            Debug.Log($"Applied permanent move speed increase: {_moveSpeedIncrease}%");
        }

        if (_attackSpeedIncrease > 0)
        {
            playerStat.AddStatModifier(StatType.AttackSpeed, SourceType.Passive, IncreaseType.Mul, _attackSpeedIncrease / 100f);
            Debug.Log($"Applied permanent attack speed increase: {_attackSpeedIncrease}%");
        }
    }

    protected override void RemoveEffectFromPlayer(Player player)
    {
        var playerStat = player.GetComponent<PlayerStat>();
        playerStat.RemoveStatsBySource(SourceType.Passive);
    }

    protected override void UpdateInspectorValues(PassiveSkillStat stats)
    {
        if (stats == null)
        {
            Debug.LogError($"{GetType().Name}: Received null stats");
            return;
        }

        base.UpdateInspectorValues(stats);
        _moveSpeedIncrease = stats.moveSpeedIncrease;
        _attackSpeedIncrease = stats.attackSpeedIncrease;

        if (GameManager.Instance?.player != null)
        {
            RemoveEffectFromPlayer(GameManager.Instance.player);
            ApplyEffectToPlayer(GameManager.Instance.player);
        }
    }

    protected override SkillData CreateDefaultSkillData()
    {
        var data = base.CreateDefaultSkillData();
        data.metadata.Name = "Speed Mastery";
        data.metadata.Description = GetDetailedDescription();
        data.metadata.Type = SkillType.Passive;
        return data;
    }

    public override string GetDetailedDescription()
    {
        var playerStat = GameManager.Instance.playerStat;
        string baseDesc = "Permanently increases movement and attack speed";

        if (skillData?.GetCurrentTypeStat() != null)
        {
            float currentMoveSpeed = playerStat.GetStat(StatType.MoveSpeed);
            float currentAttackSpeed = playerStat.GetStat(StatType.AttackSpeed);

            baseDesc += $"\n\nCurrent Effects:" +
                       $"\nMove Speed: +{_moveSpeedIncrease:F1}% (Current: {currentMoveSpeed:F1})" +
                       $"\nAttack Speed: +{_attackSpeedIncrease:F1}% (Current: {currentAttackSpeed:F1}/s)";
        }
        return baseDesc;
    }

    protected override string GetDefaultSkillName() => "Speed Mastery";
    protected override string GetDefaultDescription() => "Permanently increases movement and attack speed";
    protected override SkillType GetSkillType() => SkillType.Passive;
}