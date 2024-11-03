using UnityEngine;

public class SpeedUpgradeSkill : PermanentPassiveSkill
{
    protected override void ApplyEffectToPlayer(Player player)
    {
        if (_moveSpeedIncrease > 0)
        {
            player.IncreaseMoveSpeed(_moveSpeedIncrease);
            Debug.Log($"Applied permanent move speed increase: {_moveSpeedIncrease}%");
        }

        if (_attackSpeedIncrease > 0)
        {
            player.IncreaseAttackSpeed(_attackSpeedIncrease);
            Debug.Log($"Applied permanent attack speed increase: {_attackSpeedIncrease}%");
        }
    }

    protected override void RemoveEffectFromPlayer(Player player)
    {
        float moveSpeedPercent = _moveSpeedIncrease / 100f;
        float attackSpeedPercent = _attackSpeedIncrease / 100f;

        if (_moveSpeedIncrease > 0)
        {
            player.IncreaseMoveSpeed(-moveSpeedPercent);
        }

        if (_attackSpeedIncrease > 0)
        {
            player.IncreaseAttackSpeed(-attackSpeedPercent);
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
        string baseDesc = "Permanently increases movement speed and attack speed";
        if (skillData?.GetCurrentTypeStat() != null)
        {
            baseDesc += $"\n\nCurrent Effects:" +
                       $"\nMovement Speed: +{_moveSpeedIncrease:F1}%" +
                       $"\nAttack Speed: +{_attackSpeedIncrease:F1}%";
        }
        return baseDesc;
    }

    protected override string GetDefaultSkillName() => "Speed Mastery";
    protected override string GetDefaultDescription() => "Permanently increases movement speed and attack speed";
    protected override SkillType GetSkillType() => SkillType.Passive;
}