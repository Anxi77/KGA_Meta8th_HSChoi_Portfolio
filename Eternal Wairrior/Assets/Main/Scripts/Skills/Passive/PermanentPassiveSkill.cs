using UnityEngine;
using System.Collections;

public abstract class PermanentPassiveSkill : PassiveSkills
{
    protected override void Awake()
    {
        base.Awake();
        StartCoroutine(WaitForPlayerAndInitialize());
    }

    private IEnumerator WaitForPlayerAndInitialize()
    {
        while (GameManager.Instance == null || GameManager.Instance.player == null)
        {
            yield return null;
        }

        ApplyPermanentEffect();
        Debug.Log($"Applied permanent effect for {skillData?.metadata?.Name ?? "Unknown Skill"}");
    }

    protected override void Start()
    {
        // Start는 제거하고 Awake의 코루틴으로 처리
    }

    protected virtual void ApplyPermanentEffect()
    {
        if (GameManager.Instance?.player == null)
        {
            Debug.LogError("Failed to apply permanent effect: Player not found");
            return;
        }

        Player player = GameManager.Instance.player;
        ApplyEffectToPlayer(player);
        Debug.Log($"Successfully applied {skillData?.metadata?.Name ?? "Unknown Skill"} effect to player");
    }

    protected override void OnDestroy()
    {
        if (GameManager.Instance?.player != null)
        {
            RemoveEffectFromPlayer(GameManager.Instance.player);
            Debug.Log($"Removed permanent effect for {skillData?.metadata?.Name ?? "Unknown Skill"}");
        }
    }

    protected abstract void ApplyEffectToPlayer(Player player);

    protected abstract void RemoveEffectFromPlayer(Player player);

    public override bool SkillLevelUpdate(int newLevel)
    {
        if (GameManager.Instance.player == null) return false;

        Player player = GameManager.Instance.player;

        RemoveEffectFromPlayer(player);

        bool success = base.SkillLevelUpdate(newLevel);

        if (success)
        {
            ApplyEffectToPlayer(player);
        }
        else
        {
            ApplyEffectToPlayer(player);
        }

        return success;
    }

    protected void LogCurrentStats()
    {
        var stats = TypedStats;
        Debug.Log($"Current stats for {skillData?.metadata?.Name ?? "Unknown Skill"}:");
        Debug.Log($"Move Speed Increase: {stats.moveSpeedIncrease}");
        Debug.Log($"Attack Speed Increase: {stats.attackSpeedIncrease}");
        Debug.Log($"HP Regen Increase: {stats.hpRegenIncrease}");
        Debug.Log($"Attack Range Increase: {stats.attackRangeIncrease}");
    }
}