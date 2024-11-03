using UnityEngine;

public class AttackRegenUpgradeSkill : PermanentPassiveSkill
{
    protected override void ApplyEffectToPlayer(Player player)
    {
        if (_damageIncrease > 0)
        {
            player.IncreaseDamage(_damageIncrease);
            Debug.Log($"Applied permanent damage increase: {_damageIncrease}%");
        }

        if (_hpRegenIncrease > 0)
        {
            player.IncreaseHPRegenRate(_hpRegenIncrease);
            Debug.Log($"Applied permanent HP regen rate increase: {_hpRegenIncrease}%");
        }
    }

    protected override void RemoveEffectFromPlayer(Player player)
    {
        if (_damageIncrease > 0)
        {
            player.IncreaseDamage(-_damageIncrease);
            Debug.Log($"Removed damage increase: {_damageIncrease}%");
        }

        if (_hpRegenIncrease > 0)
        {
            player.IncreaseHPRegenRate(-_hpRegenIncrease);
            Debug.Log($"Removed HP regen rate increase: {_hpRegenIncrease}%");
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
        data.metadata.Name = "Combat Mastery";
        data.metadata.Description = GetDetailedDescription();
        data.metadata.Type = SkillType.Passive;
        return data;
    }

    // 상세 설명을 위한 public 메서드
    public override string GetDetailedDescription()
    {
        string baseDesc = "Permanently increases attack damage and HP regeneration rate";
        if (skillData?.GetCurrentTypeStat() != null)
        {
            float currentHPRegen = GameManager.Instance.player.baseHpRegenRate;
            baseDesc += $"\n\nCurrent Effects:" +
                       $"\nAttack Damage: +{_damageIncrease:F1}%" +
                       $"\nHP Regen Rate: +{_hpRegenIncrease:F1}% (Current: {currentHPRegen:F1}/s)";
        }
        return baseDesc;
    }

    protected override string GetDefaultSkillName() => "Combat Mastery";
    protected override string GetDefaultDescription() => "Permanently increases attack damage and HP regeneration rate";
    protected override SkillType GetSkillType() => SkillType.Passive;
}