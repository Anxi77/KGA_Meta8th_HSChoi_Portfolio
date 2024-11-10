using UnityEngine;

public abstract class SkillInteractionEffectBase : ISkillInteractionEffect
{
    protected ItemEffectData effectData;
    protected float procChance;
    protected float cooldown;
    protected float lastProcTime;

    public SkillInteractionEffectBase(ItemEffectData effectData, float procChance = 1f, float cooldown = 0f)
    {
        this.effectData = effectData;
        this.procChance = procChance;
        this.cooldown = cooldown;
        this.lastProcTime = 0f;
    }

    protected bool CanTriggerEffect()
    {
        if (Time.time < lastProcTime + cooldown) return false;
        if (Random.value > procChance) return false;

        lastProcTime = Time.time;
        return true;
    }

    public virtual void OnSkillCast(Skill skill, Player player) { }

    public virtual void OnSkillHit(Skill skill, Player player, Enemy target) { }

    public virtual void OnSkillKill(Skill skill, Player player, Enemy target) { }

    public virtual void ModifySkillStats(Skill skill) { }
}
