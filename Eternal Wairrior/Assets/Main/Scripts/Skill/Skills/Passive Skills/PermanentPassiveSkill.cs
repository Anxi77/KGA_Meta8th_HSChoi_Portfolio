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
        while (GameManager.Instance?.player == null)
        {
            yield return null;
        }

        ApplyPermanentEffect();
        Debug.Log($"Applied permanent effect for {skillData?.metadata?.Name ?? "Unknown Skill"}");
    }

    protected virtual void ApplyPermanentEffect()
    {
        var player = GameManager.Instance?.player;
        if (player == null)
        {
            Debug.LogError("Failed to apply permanent effect: Player not found");
            return;
        }

        ApplyEffectToPlayer(player);
        Debug.Log($"Successfully applied {skillData?.metadata?.Name ?? "Unknown Skill"} effect to player");
    }

    protected override void OnDestroy()
    {
        var player = GameManager.Instance?.player;
        if (player != null)
        {
            RemoveEffectFromPlayer(player);
            Debug.Log($"Removed permanent effect for {skillData?.metadata?.Name ?? "Unknown Skill"}");
        }
        base.OnDestroy();
    }

    protected abstract void ApplyEffectToPlayer(Player player);
    protected abstract void RemoveEffectFromPlayer(Player player);

    public override bool SkillLevelUpdate(int newLevel)
    {
        var player = GameManager.Instance?.player;
        if (player == null) return false;

        RemoveEffectFromPlayer(player);
        bool success = base.SkillLevelUpdate(newLevel);
        ApplyEffectToPlayer(player);

        return success;
    }
}