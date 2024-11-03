using UnityEngine;

public class RangeUpgradeSkill : PermanentPassiveSkill
{
    protected override void ApplyEffectToPlayer(Player player)
    {
        if (_attackRangeIncrease > 0)
        {
            player.IncreaseAttackRange(_attackRangeIncrease);
            Debug.Log($"Applied permanent attack range increase: {_attackRangeIncrease}%");
        }

        if (_expAreaIncrease > 0)
        {
            player.IncreaseExpArea(_expAreaIncrease);
            Debug.Log($"Applied permanent exp collection range increase: {_expAreaIncrease}%");
        }
    }

    protected override void RemoveEffectFromPlayer(Player player)
    {
        if (_attackRangeIncrease > 0)
        {
            player.IncreaseAttackRange(-_attackRangeIncrease);
            Debug.Log($"Removed attack range increase: {_attackRangeIncrease}%");
        }

        if (_expAreaIncrease > 0)
        {
            player.IncreaseExpArea(-_expAreaIncrease);
            Debug.Log($"Removed exp collection range increase: {_expAreaIncrease}%");
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

    // 스킬 설명을 위한 오버라이드
    protected override SkillData CreateDefaultSkillData()
    {
        var data = base.CreateDefaultSkillData();
        data.metadata.Name = "Range Mastery";
        data.metadata.Description = GetDetailedDescription();
        data.metadata.Type = SkillType.Passive;
        return data;
    }

    // 상세 설명을 위한 public 메서드
    public override string GetDetailedDescription()
    {
        string baseDesc = "Permanently increases attack range and experience collection range";
        if (skillData?.GetCurrentTypeStat() != null)
        {
            baseDesc += $"\n\nCurrent Effects:" +
                       $"\nAttack Range: +{_attackRangeIncrease:F1}%" +
                       $"\nExp Collection Range: +{_expAreaIncrease:F1}%";
        }
        return baseDesc;
    }

    protected override string GetDefaultSkillName() => "Range Mastery";
    protected override string GetDefaultDescription() => "Permanently increases attack range and experience collection range";
    protected override SkillType GetSkillType() => SkillType.Passive;
}