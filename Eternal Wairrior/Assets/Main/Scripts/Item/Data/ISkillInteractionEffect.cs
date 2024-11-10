public interface ISkillInteractionEffect
{
    void OnSkillCast(Skill skill, Player player);
    void OnSkillHit(Skill skill, Player player, Enemy target);
    void OnSkillKill(Skill skill, Player player, Enemy target);
    void ModifySkillStats(Skill skill);
}
